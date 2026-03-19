using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common
{
    public class PaxDetailDTO
    {
        public string bookingid { get; set; }
        public int paxtype { get; set; }
        public string title { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }

        public DateTime? dateofbirth { get; set; }
        public string age { get; set; }
        public int gender { get; set; }
        public string nationality { get; set; }

        public string seat_preference { get; set; }
        public string meal_preference { get; set; }

        public string passport_number { get; set; }
        public string passport_country { get; set; }
        public DateTime? passport_expirydate { get; set; }
        public DateTime? passport_issuedate { get; set; }
        public string passport_city { get; set; }

        public string api_traveller_id { get; set; }

        //public string contactee_title { get; set; }
        //public string contactee_firstname { get; set; }
        //public string contactee_lastname { get; set; }
        public bool isleadpax { get; set; }
        public string contactee_phone_country_code { get; set; }
        public string contactee_phone_number { get; set; }
        public string contactee_email { get; set; }
        public string PNR { get; set; }

        public int id { get; set; }
    }

    public class FlightDetailDTO
    {
        public string bookingid { get; set; }

        public string departure_code { get; set; }
        public string departure_name { get; set; }
        public string departure_city { get; set; }
        public DateTime? departure_datetime { get; set; }

        public string arrival_code { get; set; }
        public string arrival_name { get; set; }
        public string arrival_city { get; set; }
        public DateTime? arrival_datetime { get; set; }

        public string operating_airline_code { get; set; }
        public string operating_airline_name { get; set; }

        public string flightnumber { get; set; }
        public string equipment { get; set; }

        public string cabin_class_code { get; set; }
        public string cabin_class_text { get; set; }

        public string flighttime { get; set; }
        public string flightminute { get; set; }

        public string marketing_airline_code { get; set; }
        public string marketing_airline_name { get; set; }

        public string connectiontime { get; set; }

        public string resbooking_code { get; set; }
        public string resbooking_text { get; set; }
        public string DepartureTerminal { get; set; }
        public string ArrivalTerminal { get; set; }
        public string technicalStopminute { get; set; }

        public string api_segmentid { get; set; }

        public int id { get; set; }
    }

    public class FlightBaggageDTO
    {
        public string bookingid { get; set; }
        public string departure_iata { get; set; }
        public string arrival_iata { get; set; }
        public string paxtype { get; set; }
        public string baggage_type { get; set; }
        public string value { get; set; }
        public int id { get; set; }
    }

    public class PriceBreakdownDTO
    {
        public string bookingid { get; set; }
        public string paxtype { get; set; }

        public double api_basefare { get; set; }
        public double api_tax { get; set; }
        public double api_total { get; set; }

        public double basefare { get; set; }
        public double tax { get; set; }
        public double total { get; set; }

        public string api_currency { get; set; }
        public string currency { get; set; }

        public int? quantity { get; set; }

        public int id { get; set; }
    }

    public class FlightBookingDTO
    {
        public int id { get; set; }

        public string departure_code { get; set; }
        public string departure_city { get; set; }
        public string departure_name { get; set; }
        public DateTime? departure_datetime { get; set; }

        public string arrival_code { get; set; }
        public string arrival_city { get; set; }
        public string arrival_name { get; set; }
        public DateTime? arrival_datetime { get; set; }

        public string system_payment_reference { get; set; }

        public string triptype { get; set; }
        public string faretype { get; set; }
        public string searchcode { get; set; }

        public int? userid { get; set; }

        public string api_currency { get; set; }
        public string currency { get; set; }
        public decimal? api_baseprice { get; set; }
        public decimal? api_totalprice { get; set; }
        public decimal? api_taxprice { get; set; }

        public decimal? baseprice { get; set; }
        public decimal? totalprice { get; set; }
        public decimal? taxprice { get; set; }
        public decimal? extraserviceprice { get; set; }

        public decimal? total_adult { get; set; }
        public decimal? total_child { get; set; }
        public decimal? total_infant { get; set; }

        public string api_faresourcecode { get; set; }
        public string system_faresourcecode { get; set; }
        public string system_searchcode { get; set; }
        public string system_reference { get; set; }

        public string phone_country_code { get; set; }
        public string phone_area_code { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public string postalcode { get; set; }

        public int status { get; set; }
        public bool? HoldAllowed { get; set; }
        public bool? IsRefundable { get; set; }

        public DateTime? createddate { get; set; }
        public int paymenttype { get; set; }

        public decimal? api_extraserviceprice { get; set; }
        public string api_tkt_time_limit { get; set; }

        public string api_SecurityToken { get; set; }
        public string api_SessionId { get; set; }

        public bool is_lcc_airline { get; set; }
        public int paymentstatus { get; set; }

        public int searchchanel { get; set; }
        public string ipaddress { get; set; }
        public string pcc { get; set; }

        public int? supplier { get; set; }
        public string PNR { get; set; }
    }

    public class FlightBookingResponseDTO
    {
        public FlightBookingDTO Booking { get; set; }
        public List<FlightDetailDTO> FlightDetails { get; set; }
        public List<PaxDetailDTO> PaxDetails { get; set; }
        public List<PriceBreakdownDTO> PriceBreakdowns { get; set; }
        public List<FlightBaggageDTO> Baggages { get; set; }
    }



}
