using Amadeus_wsdl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.ThirdPartyAPIs.Models.Flight
{
    public class CalendarPrice
    {
        public class calendarPriceResponse
        {
            public string Origin { get; set; }
            public string Destination { get; set; }
            public List<Searchresult> SearchResults { get; set; }
        }
        public class Searchresult
        {
            //public string AirlineCode { get; set; }
            //public string AirlineName { get; set; }
            //public DateTime DepartureDate { get; set; }
            public DateTime? DepartureDatetime { get; set; }
            public double base_price { get; set; }
            public double tax_price { get; set; }
            public double total_price { get; set; }
            public string Currency { get; set; }
            public string Currency_sign { get; set; }
            public string marketing_airline_code { get; set; }
            public string marketing_airline_name { get; set; }
            public string operating_airline_code { get; set; }
            public string operating_airline_name { get; set; }
        }
    }
}
