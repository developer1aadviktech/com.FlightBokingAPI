using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.ThirdPartyAPIs.Models.Flight
{
    public class FareRule
    {
        public class FareRuleResponse
        {
            public Boolean success { get; set; }
            public string error { get; set; }
            public List<Data> data { get; set; }
        }
        public class Data
        {
            public string segmentid { get; set; }
            public string departure_iata { get; set; }
            public string arrival_iata { get; set; }
            public List<farerule_data> farerule { get; set; }
        }

        public class farerule_data
        {
            public string title { get; set; }
            public string description { get; set; }
        }
    }
}
