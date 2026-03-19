create proc dbo.sp_UpdateTempFlightBooking
@system_reference nvarchar(max),
@api_SecurityToken nvarchar(max),
@api_SessionId nvarchar(max),
@status int

as begin

update Temp_flight_booking set api_SecurityToken=@api_SecurityToken,api_SessionId=@api_SessionId,status=@status where system_reference=@system_reference  

end