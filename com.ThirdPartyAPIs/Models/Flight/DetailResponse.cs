using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace com.ThirdPartyAPIs.Models.Flight
{
    public class DetailResponse
    {
        public class FlightDetailResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string parameter { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public List<ResultResponse.OutboundInbounddata> Flight_Data { get; set; }
            public List<ResultResponse.Connection> Connection { get; set; }
            //public required_field required_field { get; set; }
            public Price_Data price { get; set; }
            public string id { get; set; }
            public string outboundid { get; set; }
            public string inboundid { get; set; }
            public string pcc { get; set; }
            public bool isRefundable { get; set; }
            public string offercode { get; set; }
            public int supplier { get; set; }
            //public string userid { get; set; }
            //public leadpax_detail leadpax_detail { get; set; }
            public string sc { get; set; }
            //public expire expire { get; set; }
            //  public List<Pax_Data> Pax_Data { get; set; }
            public List<Request_passengers> RequestPax { get; set; }
            //public double agencyamount { get; set; }
            public List<farefamilydata> farefamilydata { get; set; }
            public string faresourcecode { get; set; }
            public string faresourcename { get; set; }
            public string Fare_type { get; set; }
            //public string cardHandlingfee { get; set; }
            public string transid { get; set; }
            public List<ResultResponse.familylistdata> fareFamiliesList { get; set; }
            public List<fareroute> fareroute { get; set; }

        }

        public class service
        {
            public string segmentcode { get; set; }
            public List<legseatmap> legseatmap { get; set; }
            public List<meallist> meallist { get; set; }
            public List<meallist> baggagelist { get; set; }
        }

        public class meallist
        {
            public string price { get; set; }
            public string value { get; set; }
            public string currency { get; set; }
            public string type { get; set; }

            public string Description { get; set; }
            public string CategoryCode { get; set; }
            public string imageurl { get; set; }
            public string available { get; set; }

            public string defaultMeal { get; set; }
        }


        public class legseatmap
        {

            public string segmentcode { get; set; }
            public string cabin_class { get; set; }
            public string currency { get; set; }
            public string price { get; set; }
            public string seatnumber { get; set; }
            public string rowseatnumber { get; set; }
            public string SeatAvailability { get; set; }
            public string MaxNumberOfSeats { get; set; }
            public string RowNumber { get; set; }
        }

        public class fareroute
        {
            public string routeid { get; set; }
            public string route { get; set; }
            public List<fareoption> fareoption { get; set; }

        }

        public class fareoption
        {
            public List<string> incluedsevice { get; set; }
            public string fareid { get; set; }
            public string faresourcecode { get; set; }
            public string description { get; set; }
            public string faresourcename { get; set; }
            public string routeid { get; set; }
            public double price { get; set; }
        }



        public class farefamilydata
        {
            public List<ResultResponse.OutboundInbounddata> Flight_Data { get; set; }
            public Price_Data Price_Data { get; set; }
            public string fareid { get; set; }
            public string faresourcecode { get; set; }
            public string faresourcename { get; set; }
            public string isrefundable { get; set; }
            public bool isselect { get; set; }
        }

        public class expire
        {
            public Int64 seconds_time_limit { get; set; }
            public string expire_datetime { get; set; }
            public int page_valid { get; set; }
        }

        public class required_field
        {
            public bool ContactNumber { get; set; }
            public bool Email { get; set; }
        }
        public class leadpax_detail
        {
            public string phone_countrycode { get; set; }
            public string phone_number { get; set; }
            //public string whatsappcode { get; set; }
            //public string whatsappnumber { get; set; }
            //public bool iswhatsapp { get; set; }
            public string email { get; set; }
        }
        public class Price_Data
        {
            public double Discount_markup { get; set; }
            public double Airline_markup { get; set; }
            public double Admin_markup { get; set; }
            public double website_markup { get; set; }


            public double payment_gateway_price { get; set; }
            public double seat_price { get; set; }
            public double net_seat_price { get; set; }
            public double api_seat_price { get; set; }
            public double total_price { get; set; }
            public double add_ons_price { get; set; }
            public double net_add_ons_price { get; set; }
            public double tax_price { get; set; }
            public double base_price { get; set; }
            public double api_total_price { get; set; }
            public double api_tax_price { get; set; }
            public double api_base_price { get; set; }
            public double net_total_price { get; set; }
            public double net_tax_price { get; set; }
            public double net_base_price { get; set; }
            public double net_extra_price { get; set; }
            public string currency { get; set; }
            public string currency_sign { get; set; }
            public string api_currency { get; set; }
            public List<tarriffinfo> tarriffinfo { get; set; }
        }

        public class tarriffinfo
        {
            public double seat_price { get; set; }
            public double net_seat_price { get; set; }
            public double api_seat_price { get; set; }
            public double per_pax_total_price { get; set; }
            public double totalprice { get; set; }
            public double baseprice { get; set; }
            public double tax { get; set; }
            public string currency { get; set; }
            public string currency_sign { get; set; }
            public string paxid { get; set; }
            public int quantity { get; set; }
            public string paxtype_text { get; set; }
            public int paxtype { get; set; }
            public double api_totalprice { get; set; }
            public double api_baseprice { get; set; }
            public double api_tax { get; set; }
            public double net_totalprice { get; set; }
            public double net_baseprice { get; set; }
            public double net_tax { get; set; }
            public string api_currency { get; set; }

        }
        public class pax_fareDetailsBySegment
        {
            public string segmentid { get; set; }
            public List<segment_pax_detail> segment_pax_detail { get; set; }
        }

        public class segment_pax_detail
        {
            public string cabin { get; set; }
            public string fareBasis { get; set; }
            public string class_code { get; set; }
            public string paxtype { get; set; }
            public string paxid { get; set; }
            public string baggage { get; set; }
        }
        public class Pax_Data
        {
            public int index { get; set; }
            public string api_traveller_id { get; set; }
            public string associatedAdultId { get; set; }
            public Passport_details Passport { get; set; }
            public passenger_basic_details basic_details { get; set; }
            public double mindays { get; set; }
            public double maxdays { get; set; }
            public required_field_passenger required { get; set; }
            public passengercontact contact { get; set; }

            public List<selectedservice> selectedservice { get; set; }
            public List<service> servicelist { get; set; }
        }


        public class selectedservice
        {
            public string seatnumber { get; set; }
            public string seat { get; set; }
            public string segmentcode { get; set; }
            public string mealcode { get; set; }
            public string mealname { get; set; }
            public string baggagecode { get; set; }
            public string baggagename { get; set; }
        }



        public class required_field_passenger
        {
            public bool passport_mandotary { get; set; }
            public bool redressRequiredIfAny { get; set; }
            public bool residenceRequired { get; set; }
            public bool passport_issue_city { get; set; }
            public bool contact { get; set; }
            public int PaxNameCharacterLimits { get; set; }
        }
        public class passenger_basic_details
        {
            public string paxtype { get; set; }
            public string paxtype_text { get; set; }
            public string title { get; set; }
            public string firstname { get; set; }
            public string middlename { get; set; }
            public string lastname { get; set; }
            public string gender { get; set; }
            public string dateofbirth { get; set; }
            public string age { get; set; }
            public string PassengerNationality { get; set; }
            public string meal_prefrence { get; set; }
            public string seat_prefrence { get; set; }
            public string frequent_flyer_number { get; set; }

        }
        public class passengercontact
        {
            public string title { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string phone_countrycode { get; set; }
            public string phone_number { get; set; }
            public string email { get; set; }
        }
        public class Passport_details
        {
            public string number { get; set; }
            public string city { get; set; }
            public string expiry_date { get; set; }
            public string issue_date { get; set; }
            public string country { get; set; }
            public double expiry_mindays { get; set; }
            public double expiry_maxdays { get; set; }
        }

        public class Request_passengers
        {
            public int paxtype { get; set; }
            public int index { get; set; }
            public string associatedAdultId { get; set; }
            public int quantity { get; set; }

        }



    }
}