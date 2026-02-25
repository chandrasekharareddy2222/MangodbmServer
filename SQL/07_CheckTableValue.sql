CREATE OR ALTER PROCEDURE sp_CheckTableValueExists
(
    @CheckTableName VARCHAR(50),
    @KeyValue VARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

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
    FROM Check_Table_Values
    WHERE CheckTableName = @CheckTableName
      AND (@KeyValue IS NULL OR KeyValue = @KeyValue)
      AND IsActive = 1
      AND GETDATE() BETWEEN ValidFrom AND ValidTo
    ORDER BY KeyValue;
END

EXEC sp_CheckTableValueExists 'T134'

