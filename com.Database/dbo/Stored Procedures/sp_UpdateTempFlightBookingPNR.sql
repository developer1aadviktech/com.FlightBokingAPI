create proc dbo.sp_UpdateTempFlightBookingPNR
@id int,  
@status int,
@PNR nvarchar(300)
as begin 
update Temp_flight_booking set status=@status,PNR=@PNR where id=@id
end