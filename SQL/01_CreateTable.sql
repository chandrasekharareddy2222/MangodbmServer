-- =============================================
-- Field Metadata API - Database Setup Script
-- Database: DatabridgeDB
-- =============================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DatabridgeDB')
BEGIN
    CREATE DATABASE DatabridgeDB;
    PRINT 'Database DatabridgeDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database DatabridgeDB already exists.';
END
GO

-- Use the database
USE DatabridgeDB;
GO

-- Drop table if exists (for clean setup)
IF OBJECT_ID('dbo.Field_Metadata', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Field_Metadata;
    PRINT 'Existing Field_Metadata table dropped.';
END
GO

-- Create Field_Metadata table
CREATE TABLE dbo.Field_Metadata
(
    FieldName       NVARCHAR(100)   NOT NULL PRIMARY KEY,
    DataElement     NVARCHAR(100)   NULL,
    Description     NVARCHAR(500)   NULL,
    KeyField        NVARCHAR(1)     NULL,
    CheckTable      NVARCHAR(100)   NULL,
    DataType        NVARCHAR(50)    NULL,
    FieldLength     INT             NULL,
    Decimals        INT             NULL,
    
    -- Computed Column: ValidationType
    ValidationType  AS (
        CASE 
            WHEN CheckTable IS NOT NULL AND CheckTable <> '' THEN 'LOOKUP'
            WHEN DataType = 'DATS' THEN 'DATE'
            WHEN DataType = 'NUMC' THEN 'NUMERIC'
            ELSE 'TEXT'
        END
    ) PERSISTED,
    
    HasDropdown     NVARCHAR(1)     NULL,
    
    -- Computed Column: IsMandatory
    IsMandatory     AS (
        CASE 
            WHEN KeyField = 'X' THEN 1
            ELSE 0
        END
    ) PERSISTED,
    
    TableGroup      NVARCHAR(100)   NULL,
    
    -- Computed Column: UIControlType
    UIControlType   AS (
        CASE 
            WHEN HasDropdown = 'X' THEN 'DROPDOWN'
            WHEN DataType = 'DATS' THEN 'DATEPICKER'
            WHEN FieldLength > 255 THEN 'TEXTAREA'
            ELSE 'TEXTBOX'
        END
    ) PERSISTED,
    
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedDate     DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Create indexes for better query performance
CREATE NONCLUSTERED INDEX IX_Field_Metadata_IsActive 
    ON dbo.Field_Metadata(IsActive);
GO

CREATE NONCLUSTERED INDEX IX_Field_Metadata_TableGroup 
    ON dbo.Field_Metadata(TableGroup) 
    WHERE IsActive = 1;
GO

CREATE NONCLUSTERED INDEX IX_Field_Metadata_DataType 
    ON dbo.Field_Metadata(DataType) 
    WHERE IsActive = 1;
GO

PRINT 'Field_Metadata table created successfully with computed columns and indexes.';
GO
