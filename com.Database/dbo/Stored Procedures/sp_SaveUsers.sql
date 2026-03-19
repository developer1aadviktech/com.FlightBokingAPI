CREATE PROCEDURE dbo.sp_SaveUsers
(
    @Users dbo.UsersType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO Users (UserName, Email, CreatedBy)
        SELECT UserName, Email, CreatedBy
        FROM @Users;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END