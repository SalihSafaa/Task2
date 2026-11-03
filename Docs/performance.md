# Performance notes

How the database runs three of the API's queries, measured on real SQL Server (Docker).

**Test data:** 100,004 products in about 50 categories (synthetic rows, deleted afterwards).
The `Products` table is 1,592 pages (`sys.dm_db_partition_stats`).

**How it was measured:** `SET STATISTICS IO ON` for logical reads, and the actual
execution plan (`SET STATISTICS PROFILE ON` for the plan as text). The SQL is exactly what
EF Core generated, copied from the API log, with the parameter values filled in.

**Reading the numbers:** a *logical read* is one 8 KB page touched. It is a count we can
compare before and after a change, independent of how fast the machine is.

---

## 1. Products by category

`GET /api/products?categoryId=...` runs a `COUNT(*)` and then this page query:

```sql
SELECT ... FROM Products AS p
WHERE p.CategoryId = @cat
ORDER BY p.Id
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;
```

An index on `CategoryId` already exists (`IX_Products_CategoryId`, created by EF for the
foreign key). The same query got two different plans, depending on the category.

**Common category (about 2,000 products): 15 logical reads**

```
Top (10)
 └─ Clustered Index Scan  PK_Products   (predicate: CategoryId = 3002, ordered forward)
```

This is a **scan**, and it never touched `IX_Products_CategoryId`. The clustered index is
already sorted by `Id`, which is the `ORDER BY`, so SQL Server walked the table in order,
kept the rows with the right category, and stopped after 10 matches. No sort was needed.

**Rare category (1 product): 5 logical reads**

```
Top (10)
 └─ Nested Loops (Inner Join)
     ├─ Index Seek           IX_Products_CategoryId   (seek: CategoryId = 2)
     └─ Clustered Index Seek PK_Products              (Key Lookup)
```

This is an **index seek**, followed by a **Key Lookup** to fetch the columns the index does
not hold (`Name`, `Price`, `Stock` and so on).

**Why the plans differ.** SQL Server picks a plan from its statistics about how many rows
it expects to match. For a common value, a seek plus about ten Key Lookups costs more than
walking the table in `Id` order and stopping early, so it scans. For a rare value, the seek
is far cheaper.

**The caveat.** The scan is cheap here only because matching rows are frequent (in this test
data every 50th row belongs to the category), so `TOP 10` is satisfied almost at once. If a
category had fewer than 10 products and the optimizer still chose that scan, `TOP` would
never be satisfied and the whole table would be read. A scan that stops early is not
automatically bad, but it depends on how the data is distributed, so it is not a guarantee.

---

## 2. Product search by name

`GET /api/products?search=...` becomes `WHERE Name LIKE @search ESCAPE N'\'`, where EF builds
the pattern `%text%` in C# and sends it as a parameter.

| Search | Matches | Plan | Logical reads |
|---|---|---|---|
| `lap` | about 100 rows, spread through the table | Clustered Index Scan, stops after 10 rows | 132 |
| `zzzzqq` | none | Clustered Index Scan, whole table | **1,592** |

```
Top (10)
 └─ Clustered Index Scan  PK_Products   (predicate: Name LIKE '%lap%' ESCAPE '\', ordered forward)
```

**It is always a scan.** The pattern starts with a wildcard, so SQL Server cannot know where
in the name the text will appear, and the predicate is checked against every row it reads.

**Why the first search was cheap.** About every thousandth product in the test data is named
"Laptop...", so `TOP 10` found its 10 matches after reading roughly a tenth of the table. The
second search has no matches, so `TOP` is never satisfied and every page is read: 1,592 reads,
the full table. A typo in a search box produces that case, and its cost grows in direct
proportion to the size of the table.

**Can `LIKE '%text%'` ever use an index seek? No.** An index keeps values sorted from their
first character. When the pattern begins with `%`, the matching values are scattered across
the sorted list, so there is no position to seek to. Even an index on `Name` would only be
scanned, not seeked.

**What about `LIKE 'text%'`? Yes, it can.** All values that start with the same text sit next
to each other in the sorted index. SQL Server seeks to the first entry with that prefix, reads
the contiguous range, and stops as soon as the prefix changes. (This is explained from how a
sorted index works; it was not measured in this project.) Two caveats:

- On a non-covering index every match still needs a Key Lookup, so for a very common prefix
  the optimizer may prefer a scan anyway.
- Changing the endpoint from "contains" to "starts with" changes what users can find. That is
  a product decision, not only a performance one, so it was not changed here.

---

## 3. Report: most expensive product

`GetMostExpensiveProductAsync` runs:

```sql
SELECT TOP(1) ... FROM Products AS p ORDER BY p.Price DESC;
```

**Before the index: 1,670 logical reads** (scan count 9)

```
Top (1)
 └─ Parallelism (Gather Streams)
     └─ Sort (TopN Sort, ORDER BY Price DESC)
         └─ Clustered Index Scan  PK_Products   (100,004 rows read)
```

`Price` has no index, so SQL Server had to read every one of the 100,004 rows to find the
highest price. It read the whole table across 8 parallel threads, merged the results, and
returned a single row. That is a full scan to produce one row, and 1,670 reads is slightly
more than the table's 1,592 pages.

**After the index: 6 logical reads** (scan count 1)

```
Top (1)
 └─ Nested Loops (Inner Join)
     ├─ Index Scan  IX_Products_Price   (ordered forward, 1 row)
     └─ Clustered Index Seek PK_Products (Key Lookup, 1 row)
```

The index holds the prices already sorted from highest to lowest, so SQL Server reads the
first entry and stops. The Sort and Parallelism steps are gone. One Key Lookup fetches the
remaining columns for that single product.

In the new plan the operator is an *Index Scan*, not a *Seek*. A seek needs a condition to
jump to, such as `Price = 5`, and this query has none. What matters is the number of rows
read: 1, instead of 100,004.

---

## The index change

`IX_Products_Price` on `Products (Price DESC)`, added through the migration
`AddProductPriceIndex` (so it is versioned like everything else, not created by hand).

**Result:** 1,670 → 6 logical reads for the report, about 280 times fewer.

**Why it is justified**

- The report reads exactly the end of a sorted list that the index provides.
- `Price` rarely changes, so the index is cheap to maintain.
- A stock adjustment (`usp_AdjustStock`) only updates `Stock`, which is not part of this
  index, so the concurrency-critical write path does not pay for it.

**What it costs**

- Extra storage for the index.
- Every `INSERT` and every change to `Price` must also update the index.
- It only helps queries that sort or filter by `Price`. It does nothing for the category
  query or the name search.

**Why not an index on `Stock` for the out-of-stock report?** That column changes on every
stock adjustment, so such an index would be rewritten on the hottest write path, in the same
procedure that holds a row lock. That is a worse trade-off, and no index was added for it.

## Index suggestions

No suggested index was applied blindly. The one index added was chosen after reading the
plan and measuring the reads before and after. A suggested index only helps the query it was
generated for, and it adds write cost and storage for every other statement on the table, so
each suggestion has to be checked against how often that query runs and how often the data
changes.

## Notes and limits

- The data is synthetic and uniform (every 50th row in the same category, every thousandth
  product a "Laptop"). Real data would give different match frequencies, and so different
  reads for the "stops early" cases. The no-match search and the full scan are the stable
  worst cases.
- Logical reads were measured with the data already in memory (physical reads were 0), so
  the figures describe the work SQL Server does, not disk speed.