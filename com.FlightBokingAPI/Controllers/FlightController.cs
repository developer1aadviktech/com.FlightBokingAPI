using Azure.Core;
using com.Services.ServiceInterface;
using com.Services.Services;
using com.ThirdPartyAPIs.Models;
using com.ThirdPartyAPIs.Models.Flight;
using Com.AuthProvider;
using Com.Common.DTO;
using Com.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using static com.ThirdPartyAPIs.Models.Flight.SearchRequest;

namespace com.FlightBokingAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _flightServices;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IGenericRepository _genericRepository;

        public FlightController(IFlightService flightServices, IErrorLogRepository errorLogRepository, IGenericRepository genericRepository)
        {
            _flightServices = flightServices;
            _errorLogRepository = errorLogRepository;
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
        }


        [HttpGet("GetCountryList")]
        public async Task<ActionResult> GetCountryList()
        {
            try
            {
                CommonResponse Response = await _flightServices.GetCountryList();
                return Ok(Response);
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->GetCountryList", "");
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }


        [HttpGet("GetAirportSearch")]
        public async Task<ActionResult> GetAirportSearch(string Search)
        {
            try
            {
                if (Search != null)
                {
                    CommonResponse Response = _flightServices.GetAirportSearch(Search);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->GetAirportSearch", Search);
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }

        [HttpPost("SearchFlight")]
        public async Task<ActionResult> SearchFlight([FromBody] SearchRequest.RootObject Search)
        {
            try
            {
                if (Search != null)
                {
                    int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (userid == null || userid == 0)
                    {
                        return Ok(new CommonResponse { Status = HttpStatusCode.Unauthorized, Data = "Login First!" });
                    }
                    CommonResponse Response = await _flightServices.SearchFlight(Search,userid);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->SearchFlight", JsonConvert.SerializeObject(Search));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }

        [HttpPost("GetCalendarPrice")]
        public async Task<ActionResult> GetCalendarPrice([FromBody] SearchRequest.RootObject Search)
        {
            try
            {
                if (Search != null)
                {
                    int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (userid == null || userid == 0)
                    {
                        return Ok(new CommonResponse { Status = HttpStatusCode.Unauthorized, Data = "Login First!" });
                    }
                    CommonResponse Response = await _flightServices.CalendarPrice(Search,userid);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->GetCalendarPrice", JsonConvert.SerializeObject(Search));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }


        [HttpPost("Fare_PriceUpsellWithoutPNR")]
        public async Task<ActionResult> Fare_PriceUpsellWithoutPNR([FromBody] SearchRequest.FarePriceUpsellRequest request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.sc) && !string.IsNullOrEmpty(request.id))
                {
                    int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (userid == null || userid == 0)
                    {
                        return Ok(new CommonResponse { Status = HttpStatusCode.Unauthorized, Data = "Login First!" });
                    }
                    CommonResponse Response = await _flightServices.Fare_PriceUpsellWithoutPNR(request.sc, request.id,userid);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->Fare_PriceUpsellWithoutPNR", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }

        [HttpPost("FlightDetail")]
        public async Task<ActionResult> FlightDetail([FromBody] SearchRequest.FlightDetailRequest request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.sc) && !string.IsNullOrEmpty(request.id))
                {
                    int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (userid == null || userid == 0)
                    {
                        return Ok(new CommonResponse { Status = HttpStatusCode.NonAuthoritativeInformation, Data = "Login First!" });
                    }
                    CommonResponse Response = await _flightServices.FlightDetail(request,userid);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->FlightDetail", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }

        [HttpPost("Book")]
        public async Task<ActionResult> Book([FromBody] FlightBook.BookRequest request)
        {
            try
            {
                if (request != null)
                {
                    int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (userid == null || userid == 0)
                    {
                        return Ok(new CommonResponse { Status = HttpStatusCode.NonAuthoritativeInformation, Data = "Login First!" });
                    }
                    string ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    CommonResponse Response = await _flightServices.FillPaxData(request, ipaddress,userid);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->Book", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }


        [HttpPost("BookingPageDirect")]
        public async Task<ActionResult> BookingPageDirect([FromBody] string systemrefrence)
        {
            try
            {
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                CommonResponse Response = await _flightServices.BookingPageDirect(systemrefrence);
                return Ok(Response);

            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->BookingPageDirect", systemrefrence);
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }

        [HttpPost("BookingList")]
        public async Task<ActionResult> BookingList()
        {
            try
            {
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userid == null || userid == 0)
                {
                    return Ok(new CommonResponse { Status = HttpStatusCode.NonAuthoritativeInformation, Data = "Login First!" });
                }
                CommonResponse Response = await _flightServices.BookingList(userid);
                return Ok(Response);

            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->BookingList", "");
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }

        [HttpPost("BookingDetail")]
        public async Task<ActionResult> BookingDetail([FromBody] SearchRequest.BookingDetailRequest request)
        {
            try
            {
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userid == null || userid == 0)
                {
                    return Ok(new CommonResponse { Status = HttpStatusCode.NonAuthoritativeInformation, Data = "Login First!" });
                }
                CommonResponse Response = await _flightServices.BookingDetail(request,userid);
                return Ok(Response);

            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Flight->BookingDetail", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }

        [HttpPost("TestSaveData")]
        public async Task<ActionResult> TestSaveData()
        {
            try
            {
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                CommonResponse Response = await _flightServices.testsavedata(ipAddress);
                return Ok(Response);

            }
            catch (Exception e)
            {
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Error Found!" });
            }
        }


    }
}
