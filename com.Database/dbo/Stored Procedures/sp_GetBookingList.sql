CREATE proc dbo.sp_GetBookingList
as begin 
select * from Flight_booking order by 1 desc
end