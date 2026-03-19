	 CREATE FUNCTION dbo.fn_GetTableColumns (@TableName SYSNAME)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @Columns NVARCHAR(MAX);

    SELECT @Columns = STRING_AGG(
        QUOTENAME(c.name) + ' ' +
        t.name +
        CASE 
            WHEN t.name IN ('nvarchar','varchar') THEN '(' + IIF(c.max_length = -1,'MAX',CAST(c.max_length AS VARCHAR)) + ')'
            ELSE ''
        END,
        ','
    )
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID(@TableName)
      AND c.is_identity = 0;

    RETURN @Columns;
END