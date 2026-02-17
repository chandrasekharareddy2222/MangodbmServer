-- =============================================
-- Create Check_Table_Values and Passable_Values Tables
-- =============================================

USE DatabridgeDB;
GO

-- Create Check_Table_Values table
IF OBJECT_ID('dbo.Check_Table_Values', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Check_Table_Values;
    PRINT 'Existing Check_Table_Values table dropped.';
END
GO

CREATE TABLE dbo.Check_Table_Values
(
    CheckTableID    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CheckTableName  NVARCHAR(100)     NOT NULL,
    KeyValue        NVARCHAR(100)     NOT NULL,
    Description     NVARCHAR(500)     NULL,
    Language        NVARCHAR(10)      NULL DEFAULT 'EN',
    IsActive        BIT               NOT NULL DEFAULT 1,
    SortOrder       INT               NULL,
    AdditionalInfo  NVARCHAR(MAX)     NULL,
    CreatedDate     DATETIME2         NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE NONCLUSTERED INDEX IX_Check_Table_Values_TableName 
    ON dbo.Check_Table_Values(CheckTableName, IsActive);
GO

PRINT 'Check_Table_Values table created successfully.';
GO

-- Create Passable_Values table
IF OBJECT_ID('dbo.Passable_Values', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Passable_Values;
    PRINT 'Existing Passable_Values table dropped.';
END
GO

CREATE TABLE dbo.Passable_Values
(
    PassableID          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FieldName           NVARCHAR(100)     NOT NULL,
    KeyValue            NVARCHAR(100)     NOT NULL,
    DisplayValue        NVARCHAR(200)     NULL,
    Description         NVARCHAR(500)     NULL,
    DisplayOrder        INT               NOT NULL DEFAULT 0,
    IsDefault           BIT               NOT NULL DEFAULT 0,
    IsActive            BIT               NOT NULL DEFAULT 1,
    ValidationPattern   NVARCHAR(500)     NULL,
    CreatedDate         DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy           NVARCHAR(50)      NULL DEFAULT 'SYSTEM'
);
GO

CREATE NONCLUSTERED INDEX IX_Passable_Values_FieldName 
    ON dbo.Passable_Values(FieldName, IsActive);
GO

PRINT 'Passable_Values table created successfully.';
GO
