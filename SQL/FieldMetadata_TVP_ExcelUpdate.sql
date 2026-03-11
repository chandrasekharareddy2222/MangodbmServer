CREATE TYPE FieldMandatoryUpdateType AS TABLE
(
    FieldName NVARCHAR(100),
    IsMandatory BIT
);

CREATE PROCEDURE sp_BulkUpdateMandatoryFields
(
    @Updates FieldMandatoryUpdateType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE FM
    SET 
        FM.KeyField = CASE 
                        WHEN U.IsMandatory = 1 THEN 'X'
                        ELSE NULL
                      END
    FROM Field_Metadata FM
    INNER JOIN @Updates U
        ON FM.FieldName = U.FieldName
    WHERE FM.IsActive = 1;

END