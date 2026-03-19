CREATE proc [dbo].[sp_temp_flight_booking]
@refr nvarchar(500)
as begin

 SET NOCOUNT ON;

declare @id nvarchar(50);

IF EXISTS (select id from Temp_flight_booking where system_reference=@refr and status='1')
begin
select @id=id from Temp_flight_booking where system_reference=@refr and status='1'
select * from Temp_flight_booking where id=@id
select * from Temp_flight_detail where bookingid=@id
select * from Temp_pax_detail where bookingid=@id
--select * from tbl_temp_flight_seat where bookingid=@id
select * from Temp_price_breakdown where bookingid=@id
select * from Temp_flight_baggage where bookingid=@id

end
end