create proc dbo.Insertusers
@UserName nvarchar(200),
@Email nvarchar(200),
@CreatedBy int
as begin
insert into Users(UserName,Email,CreatedBy) values (@UserName,@Email,@CreatedBy)
end