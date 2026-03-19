using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.ThirdPartyAPIs.Models.Flight
{
    public class airportsearch
    {
        public class airportlist
        {
            public string AirportCode { get; set; }
            public string AirportName { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }
        public class airlinefile
        {
            public string Airlinecode { get; set; }
            public string Airlinename { get; set; }
            public string id { get; set; }
        }
    }
}
