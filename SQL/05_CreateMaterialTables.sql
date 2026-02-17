-- =============================================
-- Create Material_Master and Material_Attributes Tables
-- =============================================

USE DatabridgeDB;
GO

-- Drop existing tables
IF OBJECT_ID('dbo.Material_Attributes', 'U') IS NOT NULL
    DROP TABLE dbo.Material_Attributes;
IF OBJECT_ID('dbo.Material_Master', 'U') IS NOT NULL
    DROP TABLE dbo.Material_Master;
GO

PRINT 'Creating Material_Master table...';
GO

-- Create Material_Master table
CREATE TABLE dbo.Material_Master (
    -- Primary Key
    MATNR CHAR(18) NOT NULL,
    
    -- Optional Core Fields (all optional except MATNR)
    MTART CHAR(4),                     -- Material Type (CHECKABLE → T134)
    MEINS CHAR(3),                     -- Base Unit of Measure (CHECKABLE → T006)
    MBRSH CHAR(1),                     -- Industry Sector (CHECKABLE → T137)
    MATKL CHAR(9),                     -- Material Group (CHECKABLE → T023)
    
    -- Administrative Fields
    ERSDA DATE DEFAULT GETDATE(),      -- Created On
    ERNAM VARCHAR(12) DEFAULT SYSTEM_USER, -- Created By
    LAEDA DATE,                        -- Last Changed Date
    AENAM VARCHAR(12),                 -- Changed By
    
    -- Status Fields
    LVORM CHAR(1) DEFAULT ' ',        -- Deletion Flag (PASSABLE: ' '=Active, 'X'=Deleted)
    Status VARCHAR(20) DEFAULT 'ACTIVE', -- Internal status
    
    -- Audit Fields
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2,
    RowVersion ROWVERSION,
    
    -- Constraints
    CONSTRAINT PK_Material_Master PRIMARY KEY (MATNR),
    CONSTRAINT CK_Status CHECK (Status IN ('ACTIVE', 'BLOCKED', 'DELETED', 'PENDING')),
    CONSTRAINT CK_LVORM CHECK (LVORM IN (' ', 'X'))
);
GO

-- Indexes for Performance on Material_Master
CREATE NONCLUSTERED INDEX IX_Material_Type 
    ON dbo.Material_Master(MTART) 
    INCLUDE (MEINS, Status);
GO

CREATE NONCLUSTERED INDEX IX_Material_Group 
    ON dbo.Material_Master(MATKL) 
    WHERE Status = 'ACTIVE';
GO

CREATE NONCLUSTERED INDEX IX_Material_Status 
    ON dbo.Material_Master(Status, ERSDA);
GO

CREATE NONCLUSTERED INDEX IX_Material_Dates 
    ON dbo.Material_Master(ERSDA, LAEDA) 
    INCLUDE (ERNAM, AENAM);
GO

PRINT 'Material_Master table created successfully.';
GO

PRINT 'Creating Material_Attributes table...';
GO

-- Create Material_Attributes table
CREATE TABLE dbo.Material_Attributes (
    AttributeID BIGINT IDENTITY(1,1) NOT NULL,
    MATNR CHAR(18) NOT NULL,
    FieldName VARCHAR(50) NOT NULL,
    FieldValue NVARCHAR(MAX),         -- Stores any data type as string
    DataType VARCHAR(20),              -- CHAR, NUMC, DATS, QUAN, DEC, UNIT
    FieldLength INT,
    Decimals INT,
    LastModified DATETIME2 DEFAULT GETDATE(),
    ModifiedBy VARCHAR(50) DEFAULT SYSTEM_USER,
    
    CONSTRAINT PK_Material_Attributes PRIMARY KEY (AttributeID),
    CONSTRAINT FK_Material_Attributes_Master 
        FOREIGN KEY (MATNR) REFERENCES dbo.Material_Master(MATNR) 
        ON DELETE CASCADE,
    CONSTRAINT UQ_Material_Field UNIQUE (MATNR, FieldName)
);
GO

-- Indexes for Performance on Material_Attributes
CREATE NONCLUSTERED INDEX IX_Attributes_MATNR 
    ON dbo.Material_Attributes(MATNR) 
    INCLUDE (FieldName, FieldValue);
GO

CREATE NONCLUSTERED INDEX IX_Attributes_Field 
    ON dbo.Material_Attributes(FieldName) 
    INCLUDE (FieldValue, DataType);
GO

CREATE NONCLUSTERED INDEX IX_Attributes_Composite 
    ON dbo.Material_Attributes(MATNR, FieldName) 
    INCLUDE (FieldValue);
GO

-- Columnstore index for analytical queries (SQL Server 2016+)
CREATE NONCLUSTERED COLUMNSTORE INDEX IX_Attributes_Columnstore
    ON dbo.Material_Attributes (MATNR, FieldName, DataType);
GO

PRINT 'Material_Attributes table created successfully.';
GO

-- Create sequence for MATNR auto-generation
IF EXISTS (SELECT * FROM sys.sequences WHERE name = 'SEQ_MATNR')
    DROP SEQUENCE dbo.SEQ_MATNR;
GO

CREATE SEQUENCE dbo.SEQ_MATNR
    AS BIGINT
    START WITH 100000000
    INCREMENT BY 1
    MINVALUE 100000000
    MAXVALUE 999999999999999999
    NO CYCLE;
GO

PRINT 'MATNR sequence created (starting from 100000000).';
PRINT 'All material tables created successfully!';
GO
