USE DatabridgeDB;
GO

CREATE OR ALTER PROCEDURE sp_GetActiveCheckTablesOnly
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        CheckTable
    FROM dbo.Field_Metadata
    WHERE CheckTable IS NOT NULL
      AND LTRIM(RTRIM(CheckTable)) <> ''
      AND IsActive = 1
    ORDER BY CheckTable;
END
GO

--EXEC sp_GetActiveCheckTablesOnly;