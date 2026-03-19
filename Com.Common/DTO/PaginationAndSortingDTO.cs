using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.DTO
{
    public class PaginationAndSortingDTO
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public string? SearchString { get; set; }
    }
    public class AirportListFilterDTO:PaginationAndSortingDTO
    {
        public string AirportName { get; set; }
        public string AirportCode { get; set; }
    }
    public class AirlineListFilterDTO:PaginationAndSortingDTO
    {
        public string AirlineName { get; set; }
        public string AirlineCode { get; set; }
    }
    public class UserListFilterDTO:PaginationAndSortingDTO
    {
        public int UserStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
