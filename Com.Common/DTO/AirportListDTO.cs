using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.DTO
{
    public class AirportDTO
    {
        public int Id { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
    public class AirportListDTO
    {
        public int rno { get; set; }
        public int Id { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class AirlineListDTO
    {
        public int rno { get; set; }
        public int Id { get; set; }
        public string AirlineCode { get; set; }
        public string AirlineName { get; set; }
    }

    public class AirlineDTO
    {
        public int Id { get; set; }
        public string AirlineCode { get; set; }
        public string AirlineName { get; set; }
    }
    public class CountryMasterDTO
    {
        public int Id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
    }

}
