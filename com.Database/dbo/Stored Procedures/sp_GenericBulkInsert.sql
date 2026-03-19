CREATE PROCEDURE sp_GenericBulkInsert  
(  
    @TableName SYSNAME,  
    @JsonData NVARCHAR(MAX)  
)  
AS  
BEGIN  
    SET NOCOUNT ON;  

    DECLARE @SQL NVARCHAR(MAX);  

    BEGIN TRY  
        BEGIN TRAN;  

        SET @SQL = N'
        INSERT INTO ' + QUOTENAME(@TableName) + '
        SELECT *
        FROM OPENJSON(@JsonData)
        WITH (' + dbo.fn_GetTableColumns(@TableName) + ')
        ';

        EXEC sp_executesql  
            @SQL,
            N'@JsonData NVARCHAR(MAX)',
            @JsonData = @JsonData;

        COMMIT;  
    END TRY  
    BEGIN CATCH  
        ROLLBACK;  
        THROW;  
    END CATCH  
END;