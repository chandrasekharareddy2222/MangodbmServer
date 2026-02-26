USE [DatabridgeDB];
GO
--renaming the column name TableGroup to UIAssignmentBlock
EXEC sp_rename 'dbo.Field_Metadata.TableGroup', 'UIAssignmentBlock', 'COLUMN';
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Field_Metadata' AND COLUMN_NAME = 'Subject'
)
BEGIN
    ALTER TABLE dbo.Field_Metadata
    ADD [Subject] NVARCHAR(100) NULL;
    
    PRINT 'Column [Subject] added successfully.';
END
ELSE
BEGIN
    PRINT 'Column [Subject] already exists.';
END
GO

SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH AS Length,
    COLUMNPROPERTY(OBJECT_ID('dbo.Field_Metadata'), COLUMN_NAME, 'IsComputed') AS IsComputed
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Field_Metadata'
AND COLUMN_NAME IN ('UIAssignmentBlock', 'Subject');
GO

--Added Execl 
GO

CREATE OR ALTER PROCEDURE sp_LoadFieldMetadata
    @FilePath NVARCHAR(500),
    @SheetName NVARCHAR(100) = 'Sheet1'   -- Optional sheet name
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        DECLARE @Extension NVARCHAR(10);
        DECLARE @RowsInserted INT = 0;

        -- Extract extension
        SET @Extension = UPPER(RIGHT(@FilePath, CHARINDEX('.', REVERSE(@FilePath)) - 1));

        -- Validate extension
        IF @Extension NOT IN ('CSV','XLSX','XLS','XLSM','XLSB')
        BEGIN
            RAISERROR('Only CSV and Excel files are supported.',16,1);
            RETURN;
        END

        ------------------------------------------------------------------
        -- CSV SECTION
        ------------------------------------------------------------------
        IF @Extension = 'CSV'
        BEGIN
            IF OBJECT_ID('tempdb..#FieldStaging') IS NOT NULL 
                DROP TABLE #FieldStaging;

            CREATE TABLE #FieldStaging (
                [Field] VARCHAR(100), 
                [Key Field] VARCHAR(10), 
                [Initial Values] VARCHAR(100),
                [Data element] VARCHAR(100), 
                [Data Type] VARCHAR(50), 
                [Length] INT,
                [Decimals] INT, 
                [Coordinate] VARCHAR(100), 
                [Short Description] NVARCHAR(255),
                [Check Table] VARCHAR(100), 
                [Possible values] NVARCHAR(MAX),
                [UI Assignment Block] NVARCHAR(100), 
                [Subject] NVARCHAR(100)
            );

            DECLARE @CSVSQL NVARCHAR(MAX);

            SET @CSVSQL = '
            BULK INSERT #FieldStaging
            FROM ''' + @FilePath + '''
            WITH (
                FIRSTROW = 2,
                FIELDTERMINATOR = '','',
                ROWTERMINATOR = ''\n'',
                TABLOCK,
                CODEPAGE = ''65001''
            );';

            EXEC sp_executesql @CSVSQL;

            INSERT INTO dbo.Field_Metadata
            (
                FieldName, KeyField, DataElement, DataType,
                FieldLength, Decimals, Description,
                CheckTable, HasDropdown, UIAssignmentBlock,
                Subject, IsActive
            )
            SELECT
                UPPER(LTRIM(RTRIM([Field]))),
                CASE WHEN UPPER(LTRIM(RTRIM([Key Field]))) IN ('X','1','TRUE') THEN 'X' END,
                NULLIF(LTRIM(RTRIM([Data element])), ''),
                UPPER(LTRIM(RTRIM([Data Type]))),
                ISNULL([Length],0),
                ISNULL([Decimals],0),
                NULLIF(LTRIM(RTRIM([Short Description])), ''),
                NULLIF(LTRIM(RTRIM([Check Table])), ''),
                CASE WHEN [Possible values] IS NOT NULL 
                     AND LTRIM(RTRIM([Possible values])) <> '' THEN 'X' END,
                NULLIF(LTRIM(RTRIM([UI Assignment Block])), ''),
                NULLIF(LTRIM(RTRIM([Subject])), ''),
                1
            FROM #FieldStaging;

            SET @RowsInserted = @@ROWCOUNT;
        END

        ------------------------------------------------------------------
        -- EXCEL SECTION (Stable Version)
        ------------------------------------------------------------------
        ELSE
        BEGIN
            DECLARE @Provider NVARCHAR(300);

            IF @Extension = 'XLS'
                SET @Provider = 'Excel 8.0; HDR=NO; IMEX=1; Database=' + @FilePath;
            ELSE
                SET @Provider = 'Excel 12.0 Xml; HDR=NO; IMEX=1; Database=' + @FilePath;

            DECLARE @ExcelSQL NVARCHAR(MAX);

            SET @ExcelSQL = '
            INSERT INTO dbo.Field_Metadata
            (
                FieldName, KeyField, DataElement, DataType,
                FieldLength, Decimals, Description,
                CheckTable, HasDropdown, UIAssignmentBlock,
                Subject, IsActive
            )
            SELECT
                UPPER(LTRIM(RTRIM(F1))),
                CASE WHEN UPPER(LTRIM(RTRIM(F2))) IN (''X'',''1'',''TRUE'') THEN ''X'' END,
                NULLIF(LTRIM(RTRIM(F4)), ''''),
                UPPER(LTRIM(RTRIM(F5))),
                ISNULL(TRY_CAST(F6 AS INT),0),
                ISNULL(TRY_CAST(F7 AS INT),0),
                NULLIF(LTRIM(RTRIM(F9)), ''''),
                NULLIF(LTRIM(RTRIM(F10)), ''''),
                CASE WHEN F11 IS NOT NULL 
                     AND LTRIM(RTRIM(F11)) <> '''' THEN ''X'' END,
                NULLIF(LTRIM(RTRIM(F12)), ''''),
                NULLIF(LTRIM(RTRIM(F13)), ''''),
                1
            FROM OPENROWSET(
                ''Microsoft.ACE.OLEDB.12.0'',
                ''' + @Provider + ''',
                ''SELECT * FROM [' + @SheetName + '$]''
            )
            WHERE F1 IS NOT NULL
            AND F1 <> ''Field'';';

            EXEC sp_executesql @ExcelSQL;

            SET @RowsInserted = @@ROWCOUNT;
        END

        SELECT 'Success' AS Status, @RowsInserted AS RowsInserted;

    END TRY
    BEGIN CATCH
        SELECT 
            ERROR_MESSAGE() AS ErrorMessage,
            ERROR_LINE() AS ErrorLine;
    END CATCH
END;
GO