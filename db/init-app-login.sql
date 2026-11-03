-- Runs once at setup, as sa, after migrations. Safe to run again.
IF NOT EXISTS (SELECT 1
               FROM   sys.server_principals
               WHERE  name = N'catalog_app')
    CREATE LOGIN catalog_app
        WITH PASSWORD = N'$(APP_DB_PASSWORD)';
ELSE
    ALTER LOGIN catalog_app WITH PASSWORD = N'$(APP_DB_PASSWORD)';


GO
USE ProductCatalogDb;


GO
IF NOT EXISTS (SELECT 1
               FROM   sys.database_principals
               WHERE  name = N'catalog_app')
    CREATE USER catalog_app FOR LOGIN catalog_app;


GO
ALTER ROLE db_datareader ADD MEMBER catalog_app;

ALTER ROLE db_datawriter ADD MEMBER catalog_app;

GRANT EXECUTE
    ON OBJECT::dbo.usp_AdjustStock TO catalog_app;