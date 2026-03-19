CREATE proc [dbo].[sp_flight_booking]
@bookingid int,
@userid int,
@pnr nvarchar(500)
as begin  
  
 SET NOCOUNT ON;  
  
declare @id nvarchar(50);  
  
IF EXISTS (select id from Flight_booking where id=@bookingid and userid=@userid and PNR=@pnr)
begin  
select @id=id from Flight_booking where id=@bookingid and userid=@userid and PNR=@pnr
select * from Flight_booking where id=@id  
select * from Flight_detail where bookingid=@id  
select * from Flight_pax_detail where bookingid=@id  
--select * from tbl_temp_flight_seat where bookingid=@id  
select * from Flight_price_breakdown where bookingid=@id  
select * from Flight_baggage where bookingid=@id  
  
end  
end