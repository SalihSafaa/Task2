using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdjustStockProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE PROCEDURE dbo.usp_AdjustStock
    @ProductId INT,
    @Delta INT,
    @Reason NVARCHAR(200),
    @CreatedBy NVARCHAR(100),
    @NewStock INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OwnsTran BIT = CASE WHEN @@TRANCOUNT = 0 THEN 1 ELSE 0 END;
    IF @OwnsTran = 1 BEGIN TRAN;

    DECLARE @Stock INT;

    SELECT @Stock = Stock
    FROM dbo.Products WITH (UPDLOCK, ROWLOCK)
    WHERE Id = @ProductId;

    IF @Stock IS NULL
        THROW 50002, 'Product not found', 1;

    IF @Stock + @Delta < 0
        THROW 50001, 'Insufficient stock', 1;

    SET @NewStock = @Stock + @Delta;

    UPDATE dbo.Products
    SET Stock = @NewStock,
        LastUpdate = SYSUTCDATETIME(),
        LastUpdatedBy = @CreatedBy
    WHERE Id = @ProductId;

    INSERT INTO dbo.StockMovements (ProductId, Delta, StockAfter, Reason, CreatedBy, CreatedAt)
    VALUES (@ProductId, @Delta, @NewStock, @Reason, @CreatedBy, SYSUTCDATETIME());

    IF @OwnsTran = 1 COMMIT;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.usp_AdjustStock;");
        }
    }
}
