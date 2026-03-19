using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.ThirdPartyAPIs.Models.Flight
{
    public class FlightBook
    {
        public class BookRequest
        {
            public string sc { get; set; }
            public string id { get; set; }
            public List<paxdata> paxdata { get; set; }
            //public leadpax_details leadpax_details { get; set; }
        }

        //public class leadpax_details
        //{
        //    public string phone_code { get; set; }
        //    public string phone_number { get; set; }
        //    public string email { get; set; }
        //}

        public class paxdata
        {

            public int paxtype { get; set; }
            public bool IsLeadPax { get; set; }
            public string paxindex { get; set; }
            //Title(Adult Mr/Mrs Child Miss/Mstr Infant Miss/Mstr)
            public string title { get; set; }
            public string firstname { get; set; }
            public string middlename { get; set; }
            public string lastname { get; set; }
            public string dob { get; set; }
            public int gender { get; set; }
            public string nationality { get; set; }
            public passport_details? passport_details { get; set; }
            public contactdetails? contact_details { get; set; }
            public List<service>? service { get; set; }

        }

        public class service
        {
            public string segmentcode { get; set; }
            public string baggagecode { get; set; }
            public string mealcode { get; set; }
            public string seat { get; set; }

        }

        public class passport_details
        {
            public string number { get; set; }
            public string country { get; set; }
            public string expiry_date { get; set; }
            public string issue_date { get; set; }
            public string issue_city { get; set; }
        }

        public class contactdetails
        {
            public string phone_code { get; set; }
            public string phone_number { get; set; }
            public string email { get; set; }
        }

    }
}