-- =============================================
-- Field Metadata API - Sample Seed Data
-- =============================================

USE DatabridgeDB;
GO

-- Insert sample data
INSERT INTO dbo.Field_Metadata 
(
    FieldName,
    DataElement,
    Description,
    KeyField,
    CheckTable,
    DataType,
    FieldLength,
    Decimals,
    HasDropdown,
    TableGroup,
    IsActive,
    CreatedDate
)
VALUES
(
    'CUSTOMER_ID',
    'KUNNR',
    'Customer Number',
    'X',                    -- KeyField = 'X' means IsMandatory = 1 (computed)
    NULL,
    'CHAR',
    10,
    NULL,
    NULL,
    'CUSTOMER',
    1,
    GETUTCDATE()
),
(
    'CUSTOMER_NAME',
    'NAME1',
    'Customer Name',
    NULL,
    NULL,
    'CHAR',
    40,
    NULL,
    NULL,
    'CUSTOMER',
    1,
    GETUTCDATE()
),
(
    'COUNTRY_CODE',
    'LAND1',
    'Country Code',
    NULL,
    'T005',                 -- CheckTable exists, so ValidationType = 'LOOKUP' (computed)
    'CHAR',
    3,
    NULL,
    'X',                    -- HasDropdown = 'X', so UIControlType = 'DROPDOWN' (computed)
    'ADDRESS',
    1,
    GETUTCDATE()
),
(
    'ORDER_DATE',
    'AUDAT',
    'Order Date',
    NULL,
    NULL,
    'DATS',                 -- DataType = 'DATS', so ValidationType = 'DATE', UIControlType = 'DATEPICKER' (computed)
    8,
    NULL,
    NULL,
    'ORDER',
    1,
    GETUTCDATE()
),
(
    'ORDER_AMOUNT',
    'NETWR',
    'Net Order Value',
    NULL,
    NULL,
    'NUMC',                 -- DataType = 'NUMC', so ValidationType = 'NUMERIC' (computed)
    15,
    2,
    NULL,
    'ORDER',
    1,
    GETUTCDATE()
);
GO

PRINT 'Sample seed data inserted successfully.';
PRINT '';
PRINT 'Verification - Display all records:';
PRINT '';

-- Verify the data
SELECT 
    FieldName,
    Description,
    DataType,
    KeyField,
    CheckTable,
    HasDropdown,
    ValidationType,     -- Computed column
    IsMandatory,        -- Computed column
    UIControlType,      -- Computed column
    TableGroup,
    IsActive,
    CreatedDate
FROM dbo.Field_Metadata
ORDER BY FieldName;
GO
