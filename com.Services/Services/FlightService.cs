using Azure;
using Azure.Core;
using com.Services.ServiceInterface;
using com.ThirdPartyAPIs.Models;
using com.ThirdPartyAPIs.Models.Flight;
using Com.AuthProvider;
using Com.Common;
using Com.Common.DTO;
using Com.Common.Utility;
using Com.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static com.ThirdPartyAPIs.Models.Flight.DetailResponse;
using static Com.Common.Utility.AllEnums;

namespace com.Services.Services
{
    public class FlightService : IFlightService
    {
        public IConfiguration _configuration;
        //private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository _genericRepository;
        private readonly IErrorLogRepository _errorLogRepository;
        public FlightService(IConfiguration configuration, IWebHostEnvironment env, IGenericRepository genericRepository, IErrorLogRepository errorLogRepository)//IHttpClientFactory httpClientFactory,
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _configuration = configuration;
            //_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _env = env;
            _errorLogRepository = errorLogRepository;
        }

        public async Task<CommonResponse> GetCountryList()
        {
            CommonResponse response = new CommonResponse();

            List<CountryMasterDTO> result = (await _genericRepository.LoadData<CountryMasterDTO, Object>("GetCountryMasterList", null)).ToList();

            if (result == null)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = "An error occurred!";
                return response;
            }
            response.Status = HttpStatusCode.OK;
            response.Data = result;
            return response;
        }
        public CommonResponse GetAirportSearch(string search)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");

                List<AirportListDTO> Airportlist = new List<AirportListDTO>();
                string response1 = System.IO.File.ReadAllText(importantfiles + "Airportlist.json");

                List<AirportListDTO> alllist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AirportListDTO>>(response1);
                Airportlist = alllist.FindAll(x => x.AirportCode.ToString().ToLower().StartsWith(search.ToLower().Trim()) || x.AirportName.ToString().ToLower().StartsWith(search.ToLower().Trim()) || x.City.ToString().ToLower().StartsWith(search.ToLower().Trim()) || x.Country.ToString().ToLower().StartsWith(search.ToLower().Trim()));

                response.Status = HttpStatusCode.OK;
                response.Data = Airportlist;
            }
            catch(Exception ex)
            { 
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = ex.Message.ToString();

                _errorLogRepository.AddErrorLog(ex, "FlightService->GetAirportSearch", search);
            }
            return response;
        }


        public async Task<CommonResponse> SearchFlight(SearchRequest.RootObject request, int userid)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                if (request == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid request payload";
                    return response;
                }

                #region Validation
                UserDetailDTO userdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", new { UserId = userid });
                if (userdetail==null||userdetail.UserId==null||userdetail.UserId==0|| string.IsNullOrEmpty(userdetail.UserName))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid User!";
                    return response;
                }

                if (request.segments == null || request.segments.Length == 0 || request.adults == null || request.adults == 0 || request.cabin == null || request.cabin.Replace(" ", "") == "")
                {
                    response.Data = "Bad Request";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }
                
                if (Convert.ToInt16(request.adults) > 9)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Max. 9 Adults Allowed!";
                    return response;
                }
                if (Convert.ToInt16(request.children) > 9)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Max. 9 Children Allowed!";
                    return response;
                }
                if (Convert.ToInt16(request.infant) > Convert.ToInt16(request.adults))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Infant should less or equal to adult count!";
                    return response;
                }
                if (!Enum.IsDefined(typeof(JourneyTypeEnum), request.JourneyType))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "JourneyType is Invalid!";
                    return response;
                }
                if (request.JourneyType == (int)JourneyTypeEnum.OneWay && request.segments.Length != 1)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "One way must have 1 segment";
                    return response;
                }

                if (request.JourneyType == (int)JourneyTypeEnum.RoundTrip && request.segments.Length != 2)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Return must have 2 segments";
                    return response;
                }
                if (request.JourneyType == (int)JourneyTypeEnum.MultiCity && (request.segments.Length < 2 || request.segments.Length > 3))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Segments must be 2 and 3 in MultiCity!";
                    return response;
                }

                DateTime today = DateTime.Today;
                DateTime prevDate = DateTime.MinValue;

                for (int i = 0; i < request.segments.Length; i++)
                {
                    var seg = request.segments[i];

                    if (string.IsNullOrWhiteSpace(seg.depcode))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} depcode required";
                        return response;
                    }

                    if (string.IsNullOrWhiteSpace(seg.arrcode))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} arrcode required";
                        return response;
                    }

                    if (!DateTime.TryParseExact(seg.depdate,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime depDate))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} date format must be yyyy-MM-dd";
                        return response;
                    }

                    if (depDate.Date < today)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} date must be greater than today";
                        return response;
                    }

                    if (i > 0 && depDate <= prevDate)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} date must be greater than previous segment date";
                        return response;
                    }

                    prevDate = depDate;
                }

                if (request.cabin.ToLower().Replace(" ", "") != "c" && request.cabin.ToLower().Replace(" ", "") != "f" && request.cabin.ToLower().Replace(" ", "") != "m" && request.cabin.ToLower().Replace(" ", "") != "w" && request.cabin.ToLower().Replace(" ", "") != "y")
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid Cabin Class!";
                    return response;
                }

                string pathroot = Path.Combine(_env.ContentRootPath, "XmlFiles/");
                string SC = System.Guid.NewGuid().ToString();
                #endregion;
                               

                #region Call API
                var tasks = new List<Task<int>>();
                com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse API_RESULT = new ResultResponse.FlightResponse();
                if (System.IO.File.Exists(pathroot + SC + "_api_call.txt") == false)
                {
                    System.IO.File.WriteAllText(pathroot + SC + "_api_call.txt", "call", System.Text.ASCIIEncoding.UTF8);

                    tasks.Add(Task.Run(async () =>
                    {
                        // Create a client from the factory inside the background task
                        //var client = _httpClientFactory.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                        var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env,_genericRepository, _errorLogRepository);
                        //var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env,_genericRepository);
                        API_RESULT = await AmadeusService.Result(request, SC, userdetail).ConfigureAwait(false);
                        return 0;
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                #endregion;

                if (API_RESULT != null && API_RESULT.success == true)
                {
                    response.Data = API_RESULT;
                    response.Status = HttpStatusCode.OK;
                }
                else {
                    response.Status = HttpStatusCode.InternalServerError;
                    response.Data = "An Internal Server Error found!";
                }
                    return response;
            }
            catch (Exception e)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = e.Message.ToString();

                _errorLogRepository.AddErrorLog(e, "FlightService->SearchFlight", JsonConvert.SerializeObject(request));
                return response;
            }
        }

        public async Task<CommonResponse> CalendarPrice(SearchRequest.RootObject request,int userid)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                if (request == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid request payload";
                    return response;
                }

                #region Validation
                UserDetailDTO userdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", new { UserId = userid });
                if (userdetail == null || userdetail.UserId == null || userdetail.UserId == 0 || string.IsNullOrEmpty(userdetail.UserName))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid User!";
                    return response;
                }

                if (request.segments == null || request.segments.Length == 0 || request.adults == null || request.adults == 0 || request.cabin == null || request.cabin.Replace(" ", "") == "")
                {
                    response.Data = "Bad Request";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }
                if (Convert.ToInt16(request.adults) > 9)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Max. 9 Adults Allowed!";
                    return response;
                }
                if (Convert.ToInt16(request.children) > 9)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Max. 9 Children Allowed!";
                    return response;
                }
                if (Convert.ToInt16(request.infant) > Convert.ToInt16(request.adults))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Infant should less or equal to adult count!";
                    return response;
                }
                if (!Enum.IsDefined(typeof(JourneyTypeEnum), request.JourneyType))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "JourneyType is Invalid!";
                    return response;
                }
                if (request.JourneyType == (int)JourneyTypeEnum.OneWay && request.segments.Length != 1)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "One way must have 1 segment";
                    return response;
                }

                if (request.JourneyType == (int)JourneyTypeEnum.RoundTrip && request.segments.Length != 2)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Return must have 2 segments";
                    return response;
                }
                if (request.JourneyType == (int)JourneyTypeEnum.MultiCity && (request.segments.Length < 2 || request.segments.Length > 3))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Segments must be in MultiCity!";
                    return response;
                }

                DateTime today = DateTime.Today;
                DateTime prevDate = DateTime.MinValue;

                for (int i = 0; i < request.segments.Length; i++)
                {
                    var seg = request.segments[i];

                    if (string.IsNullOrWhiteSpace(seg.depcode))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} depcode required";
                        return response;
                    }

                    if (string.IsNullOrWhiteSpace(seg.arrcode))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} arrcode required";
                        return response;
                    }

                    if (!DateTime.TryParseExact(seg.depdate,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime depDate))
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} date format must be yyyy-MM-dd";
                        return response;
                    }

                    if (depDate.Date < today)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} date must be greater than today";
                        return response;
                    }

                    if (i > 0 && depDate <= prevDate)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.Data = $"Segment {i + 1} date must be greater than previous segment date";
                        return response;
                    }

                    prevDate = depDate;
                }

                if (request.cabin.ToLower().Replace(" ", "") != "c" && request.cabin.ToLower().Replace(" ", "") != "f" && request.cabin.ToLower().Replace(" ", "") != "m" && request.cabin.ToLower().Replace(" ", "") != "w" && request.cabin.ToLower().Replace(" ", "") != "y")
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid Cabin Class!";
                    return response;
                }

                string pathroot = Path.Combine(_env.ContentRootPath, "XmlFiles/");
                string SC = System.Guid.NewGuid().ToString();
                #endregion;

                #region Call API
                var tasks = new List<Task<int>>();
                com.ThirdPartyAPIs.Models.Flight.CalendarPrice.calendarPriceResponse API_RESULT = new CalendarPrice.calendarPriceResponse();
                if (System.IO.File.Exists(pathroot + SC + "_api_call.txt") == false)
                {
                    System.IO.File.WriteAllText(pathroot + SC + "_api_call.txt", "call", System.Text.ASCIIEncoding.UTF8);

                    tasks.Add(Task.Run(async () =>
                    {
                        // Create a client from the factory inside the background task
                        //var client = _httpClientFactory.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                        //var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, client, _env,_genericRepository);
                        
                        var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env,_genericRepository, _errorLogRepository);
                        API_RESULT = await AmadeusService.CalendarPrice(request, SC,userdetail).ConfigureAwait(false);
                        return 0;
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                #endregion;

                response.Data = API_RESULT;
                response.Status = HttpStatusCode.OK;
                return response;
            }
            catch (Exception e)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = e.Message.ToString();

                _errorLogRepository.AddErrorLog(e, "FlightService->CalendarPrice", JsonConvert.SerializeObject(request));
                return response;
            }
        }

        public async Task<CommonResponse> Fare_PriceUpsellWithoutPNR(string sc, string id,int userid)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                #region Validation
                UserDetailDTO userdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", new { UserId = userid });
                if (userdetail == null || userdetail.UserId == null || userdetail.UserId == 0 || string.IsNullOrEmpty(userdetail.UserName))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid User!";
                    return response;
                }

                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");

                if (sc == null || sc == "" || id == null || id == "" || File.Exists(xmlfiles + sc + "_flight_result.json") == false)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid error!";
                    return response;
                }

                var file_text = "";
                using (var fileStream = new FileStream(xmlfiles + sc + "_flight_result.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    file_text = streamReader.ReadToEnd();
                }

                //com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse File_data = Newtonsoft.Json.JsonConvert.DeserializeObject<com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse>(file_text);
                ResultResponse.FlightResponse File_data = System.Text.Json.JsonSerializer.Deserialize<ResultResponse.FlightResponse>(file_text);

                ResultResponse.Flightdata selected_flight = File_data.Data.Find(x => x.id == id);
                if (selected_flight == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid selected flight!";
                    return response;
                }

                #endregion;

                #region Call API
                var tasks = new List<Task<int>>();
                List<FareOptionResopnse.FareUpsellResponse> API_RESULT = new List<FareOptionResopnse.FareUpsellResponse>();

                if (selected_flight.supplier == (int)SupplierEnum.Amadeus)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        // Create a client from the factory inside the background task
                        var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env, _genericRepository, _errorLogRepository);
                        API_RESULT = await AmadeusService.Fare_PriceUpsellWithoutPNR(selected_flight, sc, File_data.totaladult, File_data.totalchild, File_data.totalinfant,userdetail).ConfigureAwait(false);
                        return 0;
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                #endregion;

                response.Data = API_RESULT;
                response.Status = HttpStatusCode.OK;
                return response;
            }
            catch (Exception e)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = e.Message.ToString();
                _errorLogRepository.AddErrorLog(e, "FlightService->Fare_PriceUpsellWithoutPNR", sc + "|~|" + id);
                return response;
            }
        }

        public async Task<CommonResponse> FlightDetail(SearchRequest.FlightDetailRequest request, int userid)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                #region Validation

                UserDetailDTO userdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", new { UserId = userid });
                if (userdetail == null || userdetail.UserId == null || userdetail.UserId == 0 || string.IsNullOrEmpty(userdetail.UserName))
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid User!";
                    return response;
                }

                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");

                if (request.sc == null || request.sc == "" || request.id == null || request.id == "" || File.Exists(xmlfiles + request.sc + "_flight_result.json") == false)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid error!";
                    return response;
                }

                var file_text = "";
                using (var fileStream = new FileStream(xmlfiles + request.sc + "_flight_result.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    file_text = streamReader.ReadToEnd();
                }

                //com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse File_data = Newtonsoft.Json.JsonConvert.DeserializeObject<com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse>(file_text);
                ResultResponse.FlightResponse File_data = System.Text.Json.JsonSerializer.Deserialize<ResultResponse.FlightResponse>(file_text);

                ResultResponse.Flightdata selected_flight = File_data.Data.Find(x => x.id == request.id);
                if (selected_flight == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid selected flight!";
                    return response;
                }

                #endregion;

                #region Currency
                com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer.Rootobject Rootobject_curency_list = new ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer.Rootobject();
                com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer currency_exchange_rate_fixer = new ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer(_configuration, _env, _genericRepository);
                //Rootobject_curency_list = await currency_exchange_rate_fixer.get_exchage_rate();
                string currency = "USD";
                #endregion;

                #region Call API
                var tasks = new List<Task<int>>();
                DetailResponse.FlightDetailResponse API_RESULT = new DetailResponse.FlightDetailResponse();

                if (selected_flight.supplier == (int)SupplierEnum.Amadeus)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env, _genericRepository,_errorLogRepository);
                        API_RESULT = await AmadeusService.FlightDetail(selected_flight, request, File_data.totaladult, File_data.totalchild, File_data.totalinfant,userdetail).ConfigureAwait(false);
                        return 0;
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                #endregion;

                response.Data = API_RESULT;
                response.Status = HttpStatusCode.OK;
                return response;
            }
            catch (Exception e)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = e.Message.ToString();
                _errorLogRepository.AddErrorLog(e, "FlightService->FlightDetailc ", request.sc + "|~|" + request.id);
                return response;
            }
        }

        public async Task<CommonResponse> FillPaxData(FlightBook.BookRequest data, string ipaddress,int userid)
        {
            CommonResponse Response = new CommonResponse();
            try
            {
                DetailResponse.FlightDetailResponse basket_data = new DetailResponse.FlightDetailResponse();
                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");

                FlightBookingDTO FlightBookingDTO = new FlightBookingDTO();
                List<PaxDetailDTO> Pax_Data = new List<PaxDetailDTO>();
                List<FlightDetailDTO> FlightDetailDTO = new List<FlightDetailDTO>();
                List<FlightBaggageDTO> FlightBaggageDTO = new List<FlightBaggageDTO>();
                List<PriceBreakdownDTO> PriceBreakdownDTO = new List<PriceBreakdownDTO>();
                string temp_bookingid = "jagdishjaat_replace_id";
                #region main Validation
                UserDetailDTO userdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", new { UserId = userid });
                if (userdetail == null || userdetail.UserId == null || userdetail.UserId == 0 || string.IsNullOrEmpty(userdetail.UserName))
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Invalid User!";
                    return Response;
                }
                if (data == null || data.paxdata == null)
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Invalid data!";
                    return Response;
                }

                if (data.sc == null || data.sc == "" || data.id == null || data.id == "" || File.Exists(xmlfiles + data.sc + data.id + "_flight_revalidate.json") == false)
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Invalid data!";
                    return Response;
                }

                string Revalidate_file_text = File.ReadAllText(xmlfiles + data.sc + data.id + "_flight_revalidate.json", System.Text.ASCIIEncoding.UTF8);

                basket_data = System.Text.Json.JsonSerializer.Deserialize<DetailResponse.FlightDetailResponse>(Revalidate_file_text);

                #endregion;

                #region Pax Validation and pax data

                if (data.paxdata.FindAll(x => x.IsLeadPax == true && x.paxtype== (int)PaxtypeEnum.Adult).Count == 0 || data.paxdata.FindAll(x => x.IsLeadPax == true).Count > 1)
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Define 1 adult passenger as the Lead Pax.";
                    return Response;
                }

                if (data.paxdata.Count != basket_data.data.RequestPax.Sum(x => x.quantity))
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Passenger data is incorrect!";
                    return Response;
                }
                
                int total_adult = basket_data?.data?.RequestPax?.FirstOrDefault(x => x.paxtype == (int)PaxtypeEnum.Adult)?.quantity ?? 0;
                int total_child = basket_data?.data?.RequestPax?.FirstOrDefault(x => x.paxtype == (int)PaxtypeEnum.Child)?.quantity ?? 0;
                int total_infant = basket_data?.data?.RequestPax?.FirstOrDefault(x => x.paxtype == (int)PaxtypeEnum.Infant)?.quantity ?? 0;

                if (data.paxdata.FindAll(x => x.paxtype == (int)PaxtypeEnum.Adult).Count != total_adult)
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Adult passenger data is incorrect!";
                    return Response;
                }

                if (data.paxdata.FindAll(x => x.paxtype == (int)PaxtypeEnum.Child).Count != total_child)
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Child passenger data is incorrect!";
                    return Response;
                }

                if (data.paxdata.FindAll(x => x.paxtype == (int)PaxtypeEnum.Infant).Count != total_infant)
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Infant passenger data is incorrect!";
                    return Response;
                }

                int paxindex = 1;
                foreach (var pax_array in data.paxdata)
                {
                    //var pax_array = data.paxdata.Find(x => x.paxindex == Pax_Data.index.ToString());
                    if (pax_array == null)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Passenger Detail Not Found!";
                        return Response;
                    }

                    if (pax_array.title == null || pax_array.title.Trim().Replace(" ", "") == "")
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Select Passengers " + paxindex + " title!";
                        return Response;
                    }

                    int PaxType = pax_array.paxtype;
                    string paxtype_text = "";
                    if (PaxType == (int)PaxtypeEnum.Adult)
                    {
                        paxtype_text = "Adult";
                    }
                    else if (PaxType == (int)PaxtypeEnum.Child)
                    {
                        paxtype_text = "Child";
                    }
                    else if (PaxType == (int)PaxtypeEnum.Infant)
                    {
                        paxtype_text = "Infant";
                    }
                    else
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "PaxType is incorrect!";
                        return Response;
                    }

                    if (pax_array.firstname == null || pax_array.firstname.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.firstname.ToString(), @"([a-zA-Z])\w+") == false)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter Passengers " + paxindex + " first name correctly!";
                        return Response;
                    }

                    if (pax_array.middlename != null && (pax_array.middlename.Trim().Replace(" ", "") != "" && Regex.IsMatch(pax_array.middlename.ToString(), @"([a-zA-Z])\w+") == false))
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxindex + " middle name correctly!";
                        return Response;
                    }

                    if (pax_array.lastname == null || pax_array.lastname.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.lastname.ToString(), @"([a-zA-Z])\w+") == false)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxindex + " last name correctly!";
                        return Response;
                    }

                    pax_array.middlename = pax_array.middlename == null ? "" : pax_array.middlename;

                    //if ((pax_array.firstname.Length + pax_array.lastname.Length + pax_array.middlename.Length) > Pax_Data.required.PaxNameCharacterLimits)
                    //{
                    //    Response.message = "Enter " + Pax_Data.basic_details.paxtype_text + " name less than " + Pax_Data.required.PaxNameCharacterLimits + " characters!";
                    //    return Response;
                    //}


                    //if (pax_array.dob == null || pax_array.dob.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.dob.ToString(), @"^[0-3]{1}[0-9]{1}/[0|1][0-9]{1}/[0-9]{4}$") == false)
                    //{
                    //    Response.message = "Enter " + Pax_Data.basic_details.paxtype_text + " date of birth correctly!";
                    //    return Response;
                    //}

                    if (pax_array.dob == null || pax_array.dob.Trim().Replace(" ", "") == "" ||
        !Regex.IsMatch(pax_array.dob.ToString(), @"^\d{4}-\d{2}-\d{2}$"))
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter" + paxtype_text + " date of birth correctly!";
                        return Response;
                    }

                    string dateofbirth = "";
                    try
                    {
                        dateofbirth = pax_array.dob;// Convert.ToDateTime(pax_array.dob.Split('/')[1] + "/" + pax_array.dob.Split('/')[0] + "/" + pax_array.dob.Split('/')[2]).ToString();
                        if (Convert.ToDateTime(dateofbirth) > DateTime.Now)
                        {
                            Response.Status = HttpStatusCode.BadRequest;
                            Response.Data = "Enter " + paxtype_text + " date of birth in past dates only";
                            return Response;
                        }
                    }
                    catch
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxtype_text + " date of birth in YYYY-MM-DD Format only";
                        return Response;
                    }

                    //string gender = "M";
                    int gender = 0;
                    if (pax_array.title.ToLower() == "mr" || pax_array.title.ToLower() == "mstr")
                    {
                        gender = 1;
                    }
                    else
                    {
                        gender = 2;
                    }                                                                                                                                                                                                                                                                           

                    //if (pax_array.gender == null || pax_array.gender.Trim().Replace(" ", "") == "" || (pax_array.gender != "M" && pax_array.gender != "F"))
                    //{
                    //    Response.message = "Select " + Pax_Data.basic_details.paxtype_text + " gender correctly!";
                    //    return Response;
                    //}

                    if (pax_array.nationality == null || pax_array.nationality.Trim().Replace(" ", "") == "" || pax_array.nationality.Length != 2 || Regex.IsMatch(pax_array.firstname.ToString(), @"([a-zA-Z])\w+") == false)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Select " + paxtype_text + " nationality correctly!";
                        return Response;
                    }

                    #region Passport Details

                    //if (Pax_Data.required.passport_mandotary && (pax_array.passport_details == null || pax_array.passport_details.number == null || pax_array.passport_details.number.Trim().Replace(" ", "") == "" || pax_array.passport_details.number.Length > 200))

                    if (pax_array.passport_details == null || pax_array.passport_details.number == null || pax_array.passport_details.number.Trim().Replace(" ", "") == "" || pax_array.passport_details.number.Length > 200)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxtype_text + " passport number correctly!";
                        return Response;
                    }

                    //if (Pax_Data.required.passport_mandotary && (pax_array.passport_details.country == null || pax_array.passport_details.country.Trim().Replace(" ", "") == "" || pax_array.passport_details.country.Length > 2))
                    if (pax_array.passport_details.country == null || pax_array.passport_details.country.Trim().Replace(" ", "") == "" || pax_array.passport_details.country.Length > 2)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Select " + paxtype_text + " passport country correctly!";
                        return Response;
                    }

                    //if (pax_array.passport_details.expiry_date == null || pax_array.passport_details.expiry_date.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.passport_details.expiry_date.ToString(), @"^[0-3]{1}[0-9]{1}/[0|1][0-9]{1}/[0-9]{4}$") == false)
                    //{
                    //    Response.Status = HttpStatusCode.BadRequest;
                    //    Response.Data = "Enter " + paxtype_text + " passport expire date correctly!";
                    //    return Response;
                    //}
                    
                    string expirydate = "";
                    if (pax_array.passport_details.expiry_date != null && pax_array.passport_details.expiry_date != "")
                    {
                        try
                        {
                            if (pax_array.passport_details.expiry_date != "")
                            {
                                expirydate = pax_array.passport_details.expiry_date;// Convert.ToDateTime(pax_array.passport_details.expiry_date.Split('/')[1] + "/" + pax_array.passport_details.expiry_date.Split('/')[0] + "/" + pax_array.passport_details.expiry_date.Split('/')[2]).ToString();
                                if (Convert.ToDateTime(expirydate) < DateTime.Now)
                                {
                                    Response.Status = HttpStatusCode.BadRequest;
                                    Response.Data = "Enter " + paxtype_text + " passport expire date correctly in YYYY-MM-DD format!";
                                    return Response;
                                }
                            }
                        }
                        catch
                        {
                            Response.Status = HttpStatusCode.BadRequest;
                            Response.Data = "Enter " + paxtype_text + " passport expire date correctly  in YYYY-MM-DD format!";
                            return Response;
                        }
                    }

                    //if (Pax_Data.required.passport_mandotary && (pax_array.passport_details.issue_date == null || pax_array.passport_details.issue_date.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.passport_details.issue_date.ToString(), @"^[0-3]{1}[0-9]{1}/[0|1][0-9]{1}/[0-9]{4}$") == false))

                    //if (pax_array.passport_details.issue_date == null || pax_array.passport_details.issue_date.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.passport_details.issue_date.ToString(), @"^[0-3]{1}[0-9]{1}/[0|1][0-9]{1}/[0-9]{4}$") == false)
                    //{
                    //    Response.Status = HttpStatusCode.BadRequest;
                    //    Response.Data = "Enter " + paxtype_text + " passport expire date correctly!";
                    //    return Response;
                    //}

                    string issuedate = "";
                    if (pax_array.passport_details.issue_date != null && pax_array.passport_details.issue_date != "")
                    {
                        try
                        {
                            if (pax_array.passport_details.issue_date != "")
                            {
                                issuedate = pax_array.passport_details.issue_date;
                                //issuedate = Convert.ToDateTime(pax_array.passport_details.issue_date.Split('/')[1] + "/" + pax_array.passport_details.issue_date.Split('/')[0] + "/" + pax_array.passport_details.issue_date.Split('/')[2]).ToString();
                                if (Convert.ToDateTime(issuedate) > DateTime.Now)
                                {
                                    Response.Status = HttpStatusCode.BadRequest;
                                    Response.Data = "Enter " + paxtype_text + " passport issue date correctly in YYYY-MM-DD format!";
                                    return Response;
                                }
                            }
                        }
                        catch
                        {
                            Response.Status = HttpStatusCode.BadRequest;
                            Response.Data = "Enter " + paxtype_text + " passport issue date correctly in YYYY-MM-DD format!";
                            return Response;
                        }
                    }

                    if (pax_array.passport_details.issue_city == null || pax_array.passport_details.issue_city.Trim().Replace(" ", "") == "" || pax_array.passport_details.issue_city.Length > 300)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxtype_text + " passport issue city correctly!";
                        return Response;
                    }

                    #endregion;

                    #region Contact Details
                    //Pax_Data.required.contact &&
                    if ((pax_array.IsLeadPax || !string.IsNullOrEmpty(pax_array.contact_details?.phone_code))  && (pax_array.contact_details == null || pax_array.contact_details.phone_code == null || pax_array.contact_details.phone_code.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.contact_details.phone_code.ToString(), @"([0-9])+") == false || pax_array.contact_details.phone_code.Length > 4))
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxtype_text + " contactee phone country correctly!";
                        return Response;
                    }

                    if ((pax_array.IsLeadPax || !string.IsNullOrEmpty(pax_array.contact_details?.phone_number)) && (pax_array.contact_details.phone_number == null || pax_array.contact_details.phone_number.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.contact_details.phone_number.ToString(), @"([0-9])+") == false))
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxtype_text + " contactee phone country correctly!";
                        return Response;
                    }

                    if ((pax_array.IsLeadPax || !string.IsNullOrEmpty(pax_array.contact_details?.email)) && (pax_array.contact_details.email == null || pax_array.contact_details.email.Trim().Replace(" ", "") == "" || Regex.IsMatch(pax_array.contact_details.email.ToString(), @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase) == false || pax_array.contact_details.email.Length > 200))
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Enter " + paxtype_text + " contactee email correctly!";
                        return Response;
                    }
                    #endregion;

                    #region serivce
                    List<DetailResponse.selectedservice> selectedservice = new List<selectedservice>();
                    if (pax_array.service != null && pax_array.service.Count > 0)
                    {
                        foreach (var ser in pax_array.service)
                        {
                            selectedservice.Add(new DetailResponse.selectedservice()
                            {
                                baggagecode = ser.baggagecode,
                                mealcode = ser.mealcode,
                                segmentcode = ser.segmentcode,
                                seatnumber = ser.seat,
                            });
                        }
                    }
                    //Pax_Data.selectedservice = selectedservice;

                    #endregion

                    Pax_Data.Add(new PaxDetailDTO() 
                    {
                        bookingid = temp_bookingid,
                        paxtype = PaxType,
                        title = pax_array.title.Trim(),
                        firstname = pax_array.firstname.Trim(),
                        middlename = pax_array.middlename.Trim(),
                        lastname = pax_array.lastname.Trim(),
                        dateofbirth = Convert.ToDateTime(dateofbirth),
                        gender = gender,
                        nationality = pax_array.nationality,
                        isleadpax = pax_array.IsLeadPax,

                        passport_number = pax_array.passport_details?.number,
                        passport_country = pax_array.passport_details?.country,
                        passport_expirydate = Convert.ToDateTime(expirydate),
                        passport_issuedate = Convert.ToDateTime(issuedate),
                        passport_city = pax_array.passport_details?.issue_city,

                        contactee_email = pax_array.contact_details?.email,
                        contactee_phone_country_code = pax_array.contact_details?.phone_code,
                        contactee_phone_number = pax_array.contact_details?.phone_number,
                    });
                }
                #endregion;

                //basket_data.data.expire.page_valid = 1;
                // basket_data.data.user_data = Member_user_configuration.agent;

                //string basket_data_text = Newtonsoft.Json.JsonConvert.SerializeObject(basket_data);

                //using (var fileStream = new FileStream(xmlfiles + data.sc + data.id + "_flight_revalidate.json", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                //using (var streamWriter = new StreamWriter(fileStream, System.Text.ASCIIEncoding.UTF8))
                //{
                //    streamWriter.WriteLine(basket_data_text);
                //}

                string paymentrefrence = System.Guid.NewGuid().ToString();
                string systemrefrence = System.Guid.NewGuid().ToString();


                #region Air_SellFromRecommendation
                string SecurityToken = "";
                string SessionId = "";

                com.ThirdPartyAPIs.Amadeus.Flight.Air_SellFromRecommendation_response.System_air_sellfromrecommendation IS_FLIGHT_AVAILABLE_FOR_BOOK = new com.ThirdPartyAPIs.Amadeus.Flight.Air_SellFromRecommendation_response.System_air_sellfromrecommendation();

                if (basket_data.data.supplier == (int)SupplierEnum.Amadeus)
                {
                    //var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env, _genericRepository);
                    //IS_FLIGHT_AVAILABLE_FOR_BOOK = await AmadeusService.Air_SellFromRecommendation(selected_flight, sc, File_data.totaladult, File_data.totalchild, File_data.totalinfant).ConfigureAwait(false);

                    var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env, _genericRepository, _errorLogRepository);
                    IS_FLIGHT_AVAILABLE_FOR_BOOK = AmadeusService.Air_SellFromRecommendation(basket_data).GetAwaiter().GetResult();

                    if (IS_FLIGHT_AVAILABLE_FOR_BOOK.success == false)
                    {
                        Response.Status = HttpStatusCode.BadRequest;
                        Response.Data = "Flight not available anymore!";
                        return Response;
                    }

                    SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken;
                    SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId;
                }

                #endregion;


                #region Flight Details and Baggage Query
                Boolean Lccairlines = false;
                //List<string> Lcc_airline_list = new List<string>() { "nk", "f9", "gk", "sy", "3k", "pd" };
                List<string> Lcc_airline_list = new List<string>();
                
                foreach (var OutboundInbounddata in basket_data.data.Flight_Data)
                {
                    foreach (var flightlist in OutboundInbounddata.flightlist)
                    {
                        if (Lcc_airline_list.IndexOf(flightlist.MarketingAirline.code.ToLower()) > -1)
                        {
                            Lccairlines = true;
                        }

                        string ResBookDesigCode = "";
                        string ResBookDesigText = "";
                        string api_segmentid = flightlist.api_segment_id;
                        string DepartureTerminal = flightlist.Departure.Terminal;
                        string ArrivalTerminal = flightlist.Arrival.Terminal;
                        string stop_totalminutes = "";
                        if (flightlist.technicalStop != null && flightlist.technicalStop.Count > 0)
                        {
                            stop_totalminutes = flightlist.technicalStop[0].stop_totalminutes.ToString();
                        }

                        FlightDetailDTO.Add(new Com.Common.FlightDetailDTO()
                        {
                            bookingid = temp_bookingid,
                            api_segmentid = flightlist.api_segment_id,
                            departure_code = flightlist.Departure.Iata,
                            departure_name = flightlist.Departure.name,
                            departure_city = flightlist.Departure.city,
                            departure_datetime = Convert.ToDateTime(flightlist.Departure.Datetime),
                            arrival_code = flightlist.Arrival.Iata,
                            arrival_name = flightlist.Arrival.name,
                            arrival_city = flightlist.Arrival.city,
                            arrival_datetime = Convert.ToDateTime(flightlist.Arrival.Datetime),
                            operating_airline_code = flightlist.OperatingAirline.code,
                            operating_airline_name = flightlist.OperatingAirline.name,
                            flightnumber = flightlist.MarketingAirline.number,
                            equipment = flightlist.Aircraft,
                            cabin_class_code = flightlist.CabinClassText,
                            cabin_class_text = flightlist.CabinClassText,
                            flighttime = flightlist.FlightTime,
                            flightminute = flightlist.FlightMinutes,
                            marketing_airline_code = flightlist.MarketingAirline.code,
                            marketing_airline_name = flightlist.MarketingAirline.name,
                            connectiontime = flightlist.connectiontime,
                            resbooking_code = ResBookDesigCode,
                            resbooking_text = ResBookDesigText,
                            DepartureTerminal = DepartureTerminal,
                            ArrivalTerminal = ArrivalTerminal,
                            technicalStopminute = stop_totalminutes
                        });


                        if (flightlist.CheckinBaggage != null && flightlist.CheckinBaggage.Count > 0)
                        {
                            foreach (var CheckinBaggage in flightlist.CheckinBaggage)
                            {
                                FlightBaggageDTO.Add(new Com.Common.FlightBaggageDTO()
                                {
                                    bookingid = temp_bookingid,
                                    departure_iata = flightlist.Departure.Iata,
                                    arrival_iata = flightlist.Arrival.Iata,
                                    paxtype = CheckinBaggage.Type,
                                    baggage_type = "CheckinBaggage",
                                    value = CheckinBaggage.Value
                                });
                            }
                        }

                    }
                }

                #endregion;

                #region Price Breakdown Query
                
                foreach (var tarriffinfo in basket_data.data.price.tarriffinfo)
                {                    
                    PriceBreakdownDTO.Add(new Com.Common.PriceBreakdownDTO()
                    {
                        bookingid = temp_bookingid,
                        paxtype = tarriffinfo.paxtype.ToString(),
                        api_basefare = tarriffinfo.api_baseprice,
                        api_tax = tarriffinfo.api_tax,
                        api_total = tarriffinfo.api_totalprice,
                        basefare = tarriffinfo.baseprice,
                        tax = tarriffinfo.tax,
                        total = tarriffinfo.totalprice,
                        api_currency = tarriffinfo.api_currency,
                        currency = tarriffinfo.currency,
                        quantity = tarriffinfo.quantity
                    });

                }

                #endregion;


                #region Insert temp data

                #region Insert in main table

                string triptype = "";

                if (basket_data.data.Flight_Data.Count == 1)
                {
                    triptype = "oneway";
                }
                else if (basket_data.data.Flight_Data.Count == 2)
                {
                    triptype = "roundtrip";
                }
                else
                {
                    triptype = "multitrip";
                }

                bool HoldAllowed = true;
                bool IsRefundable = true;
                string api_tkt_time_limit = "";

                //int total_adult = basket_data.data.RequestPax.FindAll(x => x.paxtype == (int)PaxtypeEnum.Adult).Count;
                //int total_child = basket_data.data.RequestPax.FindAll(x => x.paxtype == (int)PaxtypeEnum.Child).Count;
                //int total_infant = basket_data.data.RequestPax.FindAll(x => x.paxtype == (int)PaxtypeEnum.Infant).Count;
                //int userid= 0;

                double extraservice_price = basket_data.data.price.net_seat_price; //add ons price
                double api_extraserviceprice = basket_data.data.price.net_seat_price;

                string phone_country_code = "";
                string phone_number = "";
                string email = "";
                if (data.paxdata != null)
                {
                    var leadpax = data.paxdata.Find(x => x.IsLeadPax == true);
                    if (leadpax != null)
                    {
                        phone_country_code = (leadpax.contact_details != null) ? leadpax.contact_details.phone_code : "";
                        phone_number = (leadpax.contact_details!= null) ? leadpax.contact_details.phone_number : "";
                        email = (leadpax.contact_details != null) ? leadpax.contact_details.email : "";
                    }
                }

                FlightBookingDTO.id = 0;
                FlightBookingDTO.departure_code = basket_data.data.Flight_Data[0].flightlist[0].Departure.Iata;
                FlightBookingDTO.departure_city = basket_data.data.Flight_Data[0].flightlist[0].Departure.city;
                FlightBookingDTO.departure_name = basket_data.data.Flight_Data[0].flightlist[0].Departure.name;
                FlightBookingDTO.departure_datetime = Convert.ToDateTime(basket_data.data.Flight_Data[0].flightlist[0].Departure.Datetime);
                FlightBookingDTO.arrival_code = basket_data.data.Flight_Data[0].flightlist[basket_data.data.Flight_Data[0].flightlist.Count - 1].Arrival.Iata;
                FlightBookingDTO.arrival_city = basket_data.data.Flight_Data[0].flightlist[basket_data.data.Flight_Data[0].flightlist.Count - 1].Arrival.city;
                FlightBookingDTO.arrival_name = basket_data.data.Flight_Data[0].flightlist[basket_data.data.Flight_Data[0].flightlist.Count - 1].Arrival.name;
                FlightBookingDTO.arrival_datetime = Convert.ToDateTime(basket_data.data.Flight_Data[0].flightlist[basket_data.data.Flight_Data[0].flightlist.Count - 1].Arrival.Datetime);
                FlightBookingDTO.system_payment_reference = paymentrefrence;
                FlightBookingDTO.triptype = triptype;
                FlightBookingDTO.faretype = basket_data.data.Fare_type;
                FlightBookingDTO.searchcode = basket_data.data.sc;
                FlightBookingDTO.userid = userid;
                FlightBookingDTO.currency = basket_data.data.price.currency;
                FlightBookingDTO.api_currency = basket_data.data.price.api_currency;
                FlightBookingDTO.api_baseprice = Convert.ToDecimal(basket_data.data.price.api_base_price);
                FlightBookingDTO.api_totalprice = Convert.ToDecimal(basket_data.data.price.api_total_price);
                FlightBookingDTO.api_taxprice = Convert.ToDecimal(basket_data.data.price.api_tax_price);
                FlightBookingDTO.baseprice = Convert.ToDecimal(basket_data.data.price.base_price);
                FlightBookingDTO.totalprice = Convert.ToDecimal(basket_data.data.price.total_price);
                FlightBookingDTO.taxprice = Convert.ToDecimal(basket_data.data.price.tax_price);
                FlightBookingDTO.extraserviceprice = Convert.ToDecimal(extraservice_price);
                FlightBookingDTO.total_adult = total_adult;
                FlightBookingDTO.total_child = total_child;
                FlightBookingDTO.total_infant = total_infant;
                FlightBookingDTO.api_faresourcecode = basket_data.data.faresourcecode;
                FlightBookingDTO.system_faresourcecode = basket_data.data.id;
                FlightBookingDTO.system_searchcode = basket_data.data.sc;
                FlightBookingDTO.system_reference = systemrefrence;
                FlightBookingDTO.phone_country_code = phone_country_code;
                FlightBookingDTO.phone_number = phone_number;
                FlightBookingDTO.email = email;
                FlightBookingDTO.status = (int)TempBookingStatusEnum.Pending;
                FlightBookingDTO.HoldAllowed = HoldAllowed;
                FlightBookingDTO.IsRefundable = IsRefundable;
                FlightBookingDTO.createddate = DateTime.Now;
                FlightBookingDTO.paymenttype = (int)PaymentTypeEnum.Wallet;
                FlightBookingDTO.api_extraserviceprice = Convert.ToDecimal(api_extraserviceprice);
                FlightBookingDTO.api_tkt_time_limit = api_tkt_time_limit;
                FlightBookingDTO.api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken;
                FlightBookingDTO.api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId;
                FlightBookingDTO.is_lcc_airline = Lccairlines;
                FlightBookingDTO.paymentstatus = (int)PaymentStatusEnum.Pending;
                FlightBookingDTO.searchchanel = (int)SearchChanelEnum.Direct;
                FlightBookingDTO.ipaddress = ipaddress;
                FlightBookingDTO.pcc = basket_data.data.pcc;
                FlightBookingDTO.supplier = basket_data.data.supplier;


                var parameters = new 
                {
                    id = 0,

                    departure_code = FlightBookingDTO.departure_code,
                    departure_city = FlightBookingDTO.departure_city,
                    departure_name = FlightBookingDTO.departure_name,
                    departure_datetime = FlightBookingDTO.departure_datetime,

                    arrival_code = FlightBookingDTO.arrival_code,
                    arrival_city = FlightBookingDTO.arrival_city,
                    arrival_name = FlightBookingDTO.arrival_name,
                    arrival_datetime = FlightBookingDTO.arrival_datetime,

                    system_payment_reference = FlightBookingDTO.system_payment_reference,
                    triptype = FlightBookingDTO.triptype,
                    faretype = FlightBookingDTO.faretype,
                    searchcode = FlightBookingDTO.searchcode,
                    userid = FlightBookingDTO.userid,

                    api_currency = FlightBookingDTO.api_currency,
                    api_baseprice = FlightBookingDTO.api_baseprice,
                    api_totalprice = FlightBookingDTO.api_totalprice,
                    api_taxprice = FlightBookingDTO.api_taxprice,

                    baseprice = FlightBookingDTO.baseprice,
                    totalprice = FlightBookingDTO.totalprice,
                    taxprice = FlightBookingDTO.taxprice,
                    extraserviceprice = FlightBookingDTO.extraserviceprice,
                    currency = FlightBookingDTO.currency,

                    total_adult = FlightBookingDTO.total_adult,
                    total_child = FlightBookingDTO.total_child,
                    total_infant = FlightBookingDTO.total_infant,

                    api_faresourcecode = FlightBookingDTO.api_faresourcecode,
                    system_faresourcecode = FlightBookingDTO.system_faresourcecode,
                    system_searchcode = FlightBookingDTO.system_searchcode,
                    system_reference = FlightBookingDTO.system_reference,

                    phone_country_code = FlightBookingDTO.phone_country_code,
                    phone_number = FlightBookingDTO.phone_number,
                    email = FlightBookingDTO.email,

                    status = FlightBookingDTO.status,
                    HoldAllowed = FlightBookingDTO.HoldAllowed,
                    IsRefundable = FlightBookingDTO.IsRefundable,
                    createddate = FlightBookingDTO.createddate,

                    paymenttype = FlightBookingDTO.paymenttype,
                    api_extraserviceprice = FlightBookingDTO.api_extraserviceprice,

                    api_tkt_time_limit = FlightBookingDTO.api_tkt_time_limit,
                    api_SecurityToken = FlightBookingDTO.api_SecurityToken,
                    api_SessionId = FlightBookingDTO.api_SessionId,

                    is_lcc_airline = FlightBookingDTO.is_lcc_airline,
                    paymentstatus = FlightBookingDTO.paymentstatus,
                    searchchanel = FlightBookingDTO.searchchanel,

                    ipaddress = FlightBookingDTO.ipaddress,
                    pcc = FlightBookingDTO.pcc,
                    supplier = FlightBookingDTO.supplier
                };
                #endregion Insert in main table

                var bookingid = await _genericRepository.Save<int, object>("sp_InsertTempFlightBooking", parameters);

                if (bookingid == null || bookingid == 0)
                {
                    Response.Status = HttpStatusCode.InternalServerError;
                    Response.Data = "An error occurred!";
                    return Response;
                }
                #region Insert in Sub tables
                string paxdetailqry = "";
                string flightdetailqry = "";
                string flight_baggage_qry = "";
                string price_breakdown = "";

                paxdetailqry = JsonConvert.SerializeObject(Pax_Data).Replace(temp_bookingid, bookingid.ToString());
                if (Pax_Data != null && Pax_Data.Count > 0 && !string.IsNullOrEmpty(paxdetailqry))
                {
                    var parameter = new
                    {
                        TableName = "Temp_pax_detail",
                        JsonData = paxdetailqry
                    };
                    var paxsave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", parameter);
                }

                flightdetailqry = JsonConvert.SerializeObject(FlightDetailDTO).Replace(temp_bookingid, bookingid.ToString());
                if (FlightDetailDTO != null && FlightDetailDTO.Count > 0 && !string.IsNullOrEmpty(flightdetailqry))
                {
                    var parameter = new
                    {
                        TableName = "Temp_flight_detail",
                        JsonData = flightdetailqry
                    };
                    var detailsave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", parameter);
                }

                flight_baggage_qry = JsonConvert.SerializeObject(FlightBaggageDTO).Replace(temp_bookingid, bookingid.ToString());
                if (FlightBaggageDTO != null && FlightBaggageDTO.Count > 0 && !string.IsNullOrEmpty(flight_baggage_qry))
                {
                    var parameter = new
                    {
                        TableName = "Temp_flight_baggage",
                        JsonData = flight_baggage_qry
                    };
                    var baggagesave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", parameter);
                }

                price_breakdown = JsonConvert.SerializeObject(PriceBreakdownDTO).Replace(temp_bookingid, bookingid.ToString());
                if (PriceBreakdownDTO != null && PriceBreakdownDTO.Count > 0 && !string.IsNullOrEmpty(price_breakdown))
                {
                    var parameter = new
                    {
                        TableName = "Temp_price_breakdown",
                        JsonData = price_breakdown
                    };
                    var pricesave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", parameter);
                }
                #endregion Insert in Sub tables

                #endregion Insert in temp


                #region Call API

                var tasks = new List<Task<int>>();
                DetailResponse.FlightDetailResponse API_RESULT = new DetailResponse.FlightDetailResponse();

                #region Fare_PricePNRWithBookingClass
                if (basket_data.data.supplier == (int)SupplierEnum.Amadeus)
                {
                    #region  Getting Temp Data from database
                    //SqlParameter[] para = new SqlParameter[3];
                    //para[0] = new SqlParameter("@ref", SqlDbType.NVarChar);
                    //para[0].Value = systemrefrence;
                    //para[1] = new SqlParameter("@btype", SqlDbType.NVarChar);
                    //para[1].Value = "1";
                    //DataSet ds = SqlHelper.ExecuteDataset(CommandType.StoredProcedure, "sp_temp_flight_booking", para);
                    #endregion;    

                    string SequenceNumber = "";
                    SequenceNumber = "2";
                    // Checking Price of the pnr
                    //App_Code.Flight.Amadeus.basic_function basic_function = new Amadeus.basic_function();
                    //basic_function.get_api_variable(ds_flight_configuration, Amadeus_wsdl);

                    var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env, _genericRepository, _errorLogRepository);
                    com.ThirdPartyAPIs.Amadeus.Flight.Fare_PricePNRWithBookingClass_response.Envelope Fare_PricePNRWithBookingClass_obj = AmadeusService.Fare_PricePNRWithBookingClass(basket_data, SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK).GetAwaiter().GetResult();
                    if (Fare_PricePNRWithBookingClass_obj == null || Fare_PricePNRWithBookingClass_obj.Body == null || Fare_PricePNRWithBookingClass_obj.Body.Fare_PricePNRWithBookingClassReply == null || Fare_PricePNRWithBookingClass_obj.Body.Fare_PricePNRWithBookingClassReply.Length == 0)
                    {
                        SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                        //com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = basic_function.Security_SignOut(ds.Tables[0].Rows[0]["api_SessionId"].ToString(), ds.Tables[0].Rows[0]["api_SecurityToken"].ToString(), SequenceNumber);
                        com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = AmadeusService.Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK).GetAwaiter().GetResult();

                        Response.Status = HttpStatusCode.InternalServerError;
                        Response.Data = "Flight not available anymore!";
                        return Response;
                    }
                    SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                    com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_obj = AmadeusService.Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK).GetAwaiter().GetResult();

                    // Price Change  Cancel the PNR
                    var price_change_obj = Array.Find(Fare_PricePNRWithBookingClass_obj.Body.Fare_PricePNRWithBookingClassReply[0].fareDataInformation.fareDataSupInformation, item => item.fareDataQualifier == "712");

                    //if (price_change_obj == null || price_change_obj.fareAmount == null || Convert.ToDouble(price_change_obj.fareAmount) != basket_data.data.price.api_total_price)
                    //{

                    //    Response.Status = HttpStatusCode.InternalServerError;
                    //    Response.Data = "Flight price change please rebook again!";
                    //    return Response;
                    //}
                    try
                    {
                        CommonResponse ResponseData = AmadeusService.BookingPageDirect(systemrefrence).GetAwaiter().GetResult();
                        return ResponseData;
                    }
                    catch
                    {
                        Response.Status = HttpStatusCode.InternalServerError;
                        Response.Data = "An Error found please contact website administration!";
                        return Response;
                    }
                }
                #endregion;

                Task.WaitAll(tasks.ToArray());
                #endregion;

                Response.Data = API_RESULT;
                Response.Status = HttpStatusCode.OK;
                return Response;
            }
            catch (Exception e)
            {
                Response.Status = HttpStatusCode.InternalServerError;
                Response.Data = e.Message.ToString();
                _errorLogRepository.AddErrorLog(e, "FlightService->FillPaxData", JsonConvert.SerializeObject(data));
                return Response;
            }
        }

        public async Task<CommonResponse> BookingPageDirect(string systemrefrence)
        {
            CommonResponse Response = new CommonResponse();
            try
            {
                var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, _env, _genericRepository, _errorLogRepository);
                CommonResponse ResponseData = AmadeusService.BookingPageDirect(systemrefrence).GetAwaiter().GetResult();
                
            }
            catch (Exception ex)
            {
                Response.Status = HttpStatusCode.InternalServerError;
                Response.Data = ex.Message.ToString();
                _errorLogRepository.AddErrorLog(ex, "FlightService->BookingPageDirect", systemrefrence);
            }
            return Response;
         }

        
        public async Task<CommonResponse> BookingList(int userid)
        {
            CommonResponse Response = new CommonResponse();
            try
            {
                UserDetailDTO userdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", new { UserId = userid });
                if (userdetail == null || userdetail.UserId == null || userdetail.UserId == 0 || string.IsNullOrEmpty(userdetail.UserName))
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Invalid User!";
                    return Response;
                }
                //var client = _httpClientFactory.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                //var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, client, _env, _genericRepository);
                //CommonResponse Fare_PricePNRWithBookingClass_obj = AmadeusService.testingsavedata().GetAwaiter().GetResult();
                var parameters = new
                {
                    UserId = userdetail.UserId,   
                };
                List<FlightBookingDTO> result = (await _genericRepository.LoadData<FlightBookingDTO, Object>("sp_GetBookingList", parameters)).ToList();
                Response.Data = result;
                Response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Response.Status = HttpStatusCode.InternalServerError;
                Response.Data = ex.Message.ToString();
                _errorLogRepository.AddErrorLog(ex, "FlightService->BookingList", "");
            }
            return Response;
         }
        
        public async Task<CommonResponse> BookingDetail(SearchRequest.BookingDetailRequest request,int userid)
        {
            CommonResponse Response = new CommonResponse();
            try
            {
                UserDetailDTO userdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", new { UserId = userid });
                if (userdetail == null || userdetail.UserId == null || userdetail.UserId == 0 || string.IsNullOrEmpty(userdetail.UserName))
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Invalid User!";
                    return Response;
                }
                if (request == null || request.Bookingid == null || request.PNR == null || Convert.ToInt32(request.Bookingid) == null || Convert.ToInt32(request.Bookingid) == 0)
                {
                    Response.Status = HttpStatusCode.BadRequest;
                    Response.Data = "Invalid request payload";
                    return Response;
                }

                var parameters = new
                {
                    bookingid = Convert.ToInt32(request.Bookingid),
                    userid = userdetail.UserId,
                    pnr = request.PNR
                };
                
                var result = await _genericRepository.LoadMultipleResultSets<FlightBookingDTO, FlightDetailDTO, PaxDetailDTO, PriceBreakdownDTO, FlightBaggageDTO, object>("sp_flight_booking", parameters);

                if (result == null)
                {
                    Response.Status = HttpStatusCode.NotFound;
                    Response.Data = "Booking not Found!";
                    return Response;
                }

                FlightBookingResponseDTO ds = new FlightBookingResponseDTO
                {
                    // Single booking (usually one row)
                    Booking = result.List1?.FirstOrDefault(),
                    // Multiple rows
                    FlightDetails = result.List2 ?? new List<FlightDetailDTO>(),
                    PaxDetails = result.List3 ?? new List<PaxDetailDTO>(),
                    PriceBreakdowns = result.List4 ?? new List<PriceBreakdownDTO>(),
                    Baggages = result.List5 ?? new List<FlightBaggageDTO>()
                };
                
                Response.Data = ds;
                Response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Response.Status = HttpStatusCode.InternalServerError;
                Response.Data = ex.Message.ToString();
                _errorLogRepository.AddErrorLog(ex, "FlightService->BookingList", JsonConvert.SerializeObject(request));
            }
            return Response;
         }

        public async Task<CommonResponse> testsavedata(string ipaddress)
        {
            CommonResponse Response = new CommonResponse();
            try
            {
                var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");
                var airlinefilelistfile = "";
                using (var fileStream = new FileStream(importantfiles + "Airlinelist.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    airlinefilelistfile = streamReader.ReadToEnd();
                }
                List<AirlineListDTO> airlinefilelist = System.Text.Json.JsonSerializer.Deserialize<List<AirlineListDTO>>(airlinefilelistfile);

                string airlinescript = "";

                if (airlinefilelist!=null && airlinefilelist.Count > 0)
                {
                    foreach (var item in airlinefilelist) {
                        airlinescript += "  insert into AirlineList(Airlinecode,Airlinename) values ('" + item.AirlineCode + "','" + item.AirlineName + "')   ";

                    }

                    File.WriteAllText(Path.Combine(importantfiles+ "airlinescript.txt"), airlinescript ?? string.Empty, Encoding.UTF8);
                }


                //var client = _httpClientFactory.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                //var AmadeusService = new com.ThirdPartyAPIs.Amadeus.AmadeusConfig(_configuration, client, _env, _genericRepository);
                //CommonResponse Fare_PricePNRWithBookingClass_obj = AmadeusService.testingsavedata().GetAwaiter().GetResult();
                //var parameters = new
                //{
                //    TableName = "Users",
                //    JsonData = ""
                //};
                //if (parameters!=null)
                //{
                //    Response.Status = HttpStatusCode.BadRequest;
                //    Response.Data = "Define 1 passenger as the Lead Pax.";
                //    return Response;
                //}

                //int result = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", parameters);
            }
            catch (Exception ex)
            {
                Response.Status = HttpStatusCode.InternalServerError;
                Response.Data = ex.Message.ToString();

            }
            return Response;
         }

        

    }
}