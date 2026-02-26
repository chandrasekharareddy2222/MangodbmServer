-- Stored Procedure to fetch all field metadata with check table and passable values
-- Returns 3 result sets for efficient data loading
CREATE OR ALTER PROCEDURE sp_GetAllFieldMetadataWithValues
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result Set 1: All active field metadata
    SELECT 
        FieldName,
        DataElement,
        Description,
        KeyField,
        CheckTable,
        DataType,
        FieldLength,
        Decimals,
        ValidationType,
        HasDropdown,
        IsMandatory,
        UIAssignmentBlock, 
        Subject,             
        UIControlType,
        IsActive,
        CreatedDate
    FROM dbo.Field_Metadata
    WHERE IsActive = 1
    ORDER BY FieldName;
    
    -- Result Set 2: All check table values for active check tables
    SELECT 
        CheckTableID,
        CheckTableName,
        KeyValue,
        Description,
        AdditionalInfo,
        IsActive,
        ValidFrom,
        ValidTo,
        CreatedDate,
        CreatedBy
    FROM dbo.Check_Table_Values
    WHERE IsActive = 1
    AND CheckTableName IN (
        SELECT DISTINCT CheckTable 
        FROM dbo.Field_Metadata 
        WHERE IsActive = 1 
        AND CheckTable IS NOT NULL
    )
    ORDER BY CheckTableName, KeyValue;
    
    -- Result Set 3: All passable values for active fields
    SELECT 
        PassableID,
        FieldName,
        KeyValue,
        DisplayValue,
        Description,
        DisplayOrder,
        IsDefault,
        IconClass,
        ColorCode,
        IsActive,
        CreatedDate
    FROM dbo.Passable_Values
    WHERE IsActive = 1
    AND FieldName IN (
        SELECT FieldName 
        FROM dbo.Field_Metadata 
        WHERE IsActive = 1
    )
    ORDER BY FieldName, DisplayOrder;
    
END;
