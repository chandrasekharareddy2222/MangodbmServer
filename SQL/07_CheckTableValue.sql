
CREATE OR ALTER PROCEDURE sp_InsertCheckTableValue
(
    @CheckTableName VARCHAR(50),
    @KeyValue VARCHAR(100),
    @Description NVARCHAR(200),
    @AdditionalInfo NVARCHAR(MAX),
    @IsActive BIT,
    @ValidFrom DATETIME,
    @ValidTo DATETIME,
    @CreatedBy NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO Check_Table_Values
        (
            CheckTableName,
            KeyValue,
            Description,
            AdditionalInfo,
            IsActive,
            ValidFrom,
            ValidTo,
            CreatedDate,
            CreatedBy
        )
        OUTPUT INSERTED.CheckTableID   -- <<< returns the new ID
        VALUES
        (
            @CheckTableName,
            @KeyValue,
            @Description,
            @AdditionalInfo,
            @IsActive,
            @ValidFrom,
            @ValidTo,
            GETDATE(),
            @CreatedBy
        );
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ERROR;
    END CATCH
END
EXEC sp_CheckTableValueExists 'T134'

GO
/* Updates Check Table Value details by CheckTableID. */
CREATE OR ALTER PROCEDURE sp_UpdateCheckTableValue
(
    @CheckTableID INT,
    @KeyValue VARCHAR(100),
    @Description VARCHAR(200),
    @AdditionalInfo NVARCHAR(MAX),
    @IsActive BIT,
    @ValidFrom DATETIME,
    @ValidTo DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TableName VARCHAR(50);

    SELECT @TableName = CheckTableName
    FROM Check_Table_Values
    WHERE CheckTableID = @CheckTableID;

    IF EXISTS (
        SELECT 1
        FROM Check_Table_Values
        WHERE CheckTableName = @TableName
        AND KeyValue = @KeyValue
        AND CheckTableID <> @CheckTableID
    )
    BEGIN
        RAISERROR('KeyValue already exists',16,1);
        RETURN;
    END

    UPDATE Check_Table_Values
    SET
        KeyValue = @KeyValue,
        Description = @Description,
        AdditionalInfo = @AdditionalInfo,
        IsActive = @IsActive,
        ValidFrom = @ValidFrom,
        ValidTo = @ValidTo
    WHERE CheckTableID = @CheckTableID;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

/*SoftDeleteCheckTableValue*/
CREATE OR ALTER PROCEDURE sp_SoftDeleteCheckTableValue
(
    @CheckTableID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Check_Table_Values
    SET
        IsActive = 0,
        ValidTo = GETDATE()
    WHERE CheckTableID = @CheckTableID
      AND IsActive = 1;

    SELECT @@ROWCOUNT;
END


