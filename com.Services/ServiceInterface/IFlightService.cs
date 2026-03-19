using com.ThirdPartyAPIs.Models;
using com.ThirdPartyAPIs.Models.Flight;
using Com.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.Services.ServiceInterface
{
    public interface IFlightService
    {
        Task<CommonResponse> GetCountryList();
        CommonResponse GetAirportSearch(string Search);
        //CommonResponse SearchFlight(SearchRequest.RootObject Search,int userid);
        Task<CommonResponse> SearchFlight(SearchRequest.RootObject Search,int userid);
        Task<CommonResponse> CalendarPrice(SearchRequest.RootObject Search, int userid);
        Task<CommonResponse> Fare_PriceUpsellWithoutPNR(string sc, string id, int userid);
        Task<CommonResponse> FlightDetail(SearchRequest.FlightDetailRequest request, int userid);
        Task<CommonResponse> FillPaxData(FlightBook.BookRequest request, string ipaddress, int userid);
        Task<CommonResponse> testsavedata(string ipaddress);
        Task<CommonResponse> BookingPageDirect(string systemrefrence);
        Task<CommonResponse> BookingList(int userid);
        Task<CommonResponse> BookingDetail(SearchRequest.BookingDetailRequest request, int userid);
    }
}
