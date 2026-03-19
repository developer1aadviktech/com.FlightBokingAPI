create proc dbo.sp_insert_user
@UserName nvarchar(100),
@Email	nvarchar(100),
@CreatedBy	int
as begin

insert into Users(UserName,Email,CreatedBy) values(@UserName,@Email,@CreatedBy)

end