using Amadeus_wsdl;
using Azure;
using Azure.Core;
using com.ThirdPartyAPIs.Amadeus.Flight;
using com.ThirdPartyAPIs.Models;
using com.ThirdPartyAPIs.Models.Flight;
using Com.Common;
using Com.Common.DTO;
using Com.Common.Utility;
using Com.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static com.ThirdPartyAPIs.Models.Flight.DetailResponse;
using static com.ThirdPartyAPIs.Models.Flight.FareOptionResopnse;
using static Com.Common.Utility.AllEnums;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Com.AuthProvider;

namespace com.ThirdPartyAPIs.Amadeus
{
    public class AmadeusConfig
    {
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository _genericRepository;
        //private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _env;
        private readonly IErrorLogRepository _errorLogRepository;
        public AmadeusConfig(IConfiguration configuration, IWebHostEnvironment env, IGenericRepository genericRepository, IErrorLogRepository errorLogRepository) //HttpClient httpClient,
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            //_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _errorLogRepository = errorLogRepository;
        }

        //string APIcurrency = "KES";
        string APIcurrency = "KES";
        string APIcurrency_sign = "KES";
        // Public result entry — now performs session create then sends pricing request with session header

        #region Reslut comment code

        public async Task<com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse> Result_old(com.ThirdPartyAPIs.Models.Flight.SearchRequest.RootObject searchcriteria)
        {
            if (searchcriteria == null) throw new ArgumentNullException(nameof(searchcriteria));
            if (searchcriteria.segments == null) throw new ArgumentException("searchcriteria.segments is null", nameof(searchcriteria));

            var flightdatamain = new Models.Flight.ResultResponse.FlightResponse();

            // Read password from configuration (avoid hard-coding). Fallback to previous literal if missing.
            string password = _configuration["FlightSettings:AirPassword"] ?? "U9MbJZjzR^EP";
            string url = _configuration["FlightSettings:AirProductionURL"] ?? throw new InvalidOperationException("FlightSettings:AirProductionURL missing");

            // Build security header values
            Guid Messageguid = Guid.NewGuid();
            string guidString = Messageguid.ToString();

            byte[] nonceBytes = new byte[16];
            RandomNumberGenerator.Fill(nonceBytes);
            string encodedNonce = Convert.ToBase64String(nonceBytes);

            string created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss+00:00");

            // Password Digest (WS-Security style). SHA1 used to match remote requirement.
            string passSHA;
            //using (var sha1 = SHA1.Create())
            //{
            //    var passwordHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            //    var createdBytes = Encoding.ASCII.GetBytes(created);

            //    var combined = new byte[nonceBytes.Length + createdBytes.Length + passwordHash.Length];
            //    Buffer.BlockCopy(nonceBytes, 0, combined, 0, nonceBytes.Length);
            //    Buffer.BlockCopy(createdBytes, 0, combined, nonceBytes.Length, createdBytes.Length);
            //    Buffer.BlockCopy(passwordHash, 0, combined, nonceBytes.Length + createdBytes.Length, passwordHash.Length);

            //    var digest = sha1.ComputeHash(combined);
            //    passSHA = Convert.ToBase64String(digest);
            //}
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(created);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }

            // Build XML body (use StringBuilder-like behaviour via string interpolation for readability)
            var sb = new StringBuilder();
            sb.Append("<Fare_MasterPricerTravelBoardSearch xmlns=\"http://xml.amadeus.com/FMPTBQ_23_4_1A\">");

            sb.Append("<numberOfUnit>");
            sb.Append("<unitNumberDetail><numberOfUnits>" + _configuration["FlightSettings:NoOfCombinations"] + "</numberOfUnits><typeOfUnit>RC</typeOfUnit></unitNumberDetail>");
            sb.Append("<unitNumberDetail><numberOfUnits>" + (searchcriteria.adults + searchcriteria.children) + "</numberOfUnits><typeOfUnit>PX</typeOfUnit></unitNumberDetail>");
            sb.Append("</numberOfUnit>");

            // Adult pax
            sb.Append("<paxReference><ptc>IIT</ptc><ptc>ADT</ptc>");
            for (int i = 0; i < searchcriteria.adults; i++)
                sb.Append("<traveller><ref>" + (i + 1) + "</ref></traveller>");
            sb.Append("</paxReference>");

            // Children
            if (searchcriteria.children > 0)
            {
                sb.Append("<paxReference><ptc>CNN</ptc><ptc>INN</ptc>");
                for (int i = searchcriteria.adults; i < searchcriteria.adults + searchcriteria.children; i++)
                    sb.Append("<traveller><ref>" + (i + 1) + "</ref></traveller>");
                sb.Append("</paxReference>");
            }

            // Infants - ensure closing traveller tag and closing paxReference
            if (searchcriteria.infant > 0)
            {
                sb.Append("<paxReference><ptc>ITF</ptc><ptc>IN</ptc>");
                for (int i = 0; i < searchcriteria.infant; i++)
                    sb.Append("<traveller><ref>" + (i + 1) + "</ref><infantIndicator>1</infantIndicator></traveller>");
                sb.Append("</paxReference>");
            }

            // Fare options (kept same as your original content)
            sb.Append("<fareOptions>");
            sb.Append("<pricingTickInfo><pricingTicketing>");
            sb.Append("<priceType>TAC</priceType><priceType>RP</priceType><priceType>ET</priceType><priceType>RU</priceType><priceType>CUC</priceType><priceType>MNR</priceType><priceType>RW</priceType>");
            sb.Append("</pricingTicketing></pricingTickInfo>");
            sb.Append("<corporate><corporateId><corporateQualifier>RW</corporateQualifier><identity>387833</identity></corporateId></corporate>");
            sb.Append("<feeIdDescription><feeId><feeType>FFI</feeType><feeIdNumber>1</feeIdNumber></feeId></feeIdDescription>");
            sb.Append("<conversionRate><conversionRateDetail><currency>" + APIcurrency + "</currency></conversionRateDetail></conversionRate>");
            sb.Append("</fareOptions>");

            sb.Append("<travelFlightInfo><cabinId><cabinQualifier>RC</cabinQualifier>");
            sb.Append("<cabin>" + (searchcriteria.cabin ?? string.Empty).ToUpper() + "</cabin>");
            sb.Append("</cabinId></travelFlightInfo>");

            // Segments — ensure segRef increments
            int index = 1;
            foreach (var seg in searchcriteria.segments)
            {
                sb.Append("<itinerary>");
                sb.Append("<requestedSegmentRef><segRef>" + index + "</segRef></requestedSegmentRef>");
                sb.Append("<departureLocalization><departurePoint><locationId>" + (seg.depcode ?? string.Empty).ToUpper() + "</locationId></departurePoint></departureLocalization>");
                sb.Append("<arrivalLocalization><arrivalPointDetails><locationId>" + (seg.arrcode ?? string.Empty).ToUpper() + "</locationId></arrivalPointDetails></arrivalLocalization>");
                sb.Append("<timeDetails><firstDateTimeDetail><date>" + Convert.ToDateTime(seg.depdate).ToString("ddMMyy") + "</date></firstDateTimeDetail></timeDetails>");
                sb.Append("</itinerary>");
                index++;
            }

            sb.Append("</Fare_MasterPricerTravelBoardSearch>");

            // Final SOAP envelope
            string finalXML =
$@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""
 xmlns:awsse=""http://xml.amadeus.com/2010/06/Session_v3""
 xmlns:sec=""http://xml.amadeus.com/2010/06/Security_v1""
 xmlns:typ=""http://xml.amadeus.com/2010/06/Types_v1""
 xmlns:app=""http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3"">
<s:Header>
    <a:MessageID xmlns:a=""http://www.w3.org/2005/08/addressing"">{guidString}</a:MessageID>
    <a:Action xmlns:a=""http://www.w3.org/2005/08/addressing"">{_configuration["FlightSettings:AirSoapAction"]}FMPTBQ_23_4_1A</a:Action>
    <a:To xmlns:a=""http://www.w3.org/2005/08/addressing"">{url}</a:To>
<Security xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">
<oas:UsernameToken xmlns:oas=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:oas1=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" oas1:Id=""UsernameToken-1""><oas:Username>{_configuration["FlightSettings:AirUserName"]}</oas:Username><oas:Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">{encodedNonce}</oas:Nonce> <oas:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest"">{passSHA}</oas:Password><oas1:Created>{created}</oas1:Created></oas:UsernameToken>
</Security>

<h:AMA_SecurityHostedUser xmlns:h=""http://xml.amadeus.com/2010/06/Security_v1"">
        <h:UserID POS_Type=""1"" 
                  PseudoCityCode=""{_configuration["FlightSettings:AirOfficeID"]}"" 
                  AgentDutyCode=""{_configuration["FlightSettings:AirDutyCode"]}"" 
                  RequestorType=""U"" />
    </h:AMA_SecurityHostedUser>
</s:Header>
<s:Body>
{sb}
</s:Body>
</s:Envelope>";

            // Call API using modern HttpClient (async). Avoid blocking and deprecated APIs.
            string actionHeader = _configuration["FlightSettings:AirSoapAction"] + "FMPTBQ_23_4_1A";
            string result;
            try
            {
                result = await CallApiAsync(finalXML, actionHeader).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Surface exceptions instead of swallowing — caller can decide how to handle.
                throw new InvalidOperationException("Amadeus request failed", ex);
            }

            // Save logs if configured
            try
            {
                var xmlfiles = _configuration["StaticFiles:xmlfiles"];
                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    Directory.CreateDirectory(xmlfiles);
                    File.WriteAllText(Path.Combine(xmlfiles, "Request.xml"), finalXML, Encoding.UTF8);
                    File.WriteAllText(Path.Combine(xmlfiles, "Response.xml"), result ?? string.Empty, Encoding.UTF8);
                }
            }
            catch
            {
                // Swallow logging I/O errors — do not break main flow
            }

            // TODO: parse result to populate flightdatamain (kept out so behavior unchanged)
            return flightdatamain;
        }

        // Replaced synchronous WebRequest usage with async HttpClient usage.
        public async Task<string> CallApiAsync(string requestBody, string actionUrl)
        {
            if (string.IsNullOrWhiteSpace(requestBody)) throw new ArgumentException("requestBody is empty", nameof(requestBody));
            var baseUrl = _configuration["FlightSettings:AirProductionURL"] ?? throw new InvalidOperationException("FlightSettings:AirProductionURL missing");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, baseUrl);
            // SOAPAction header may be required by the server; add it.
            httpRequest.Headers.Remove("SOAPAction");
            httpRequest.Headers.Add("SOAPAction", actionUrl);
            httpRequest.Content = new StringContent(requestBody, Encoding.UTF8, "text/xml");

            //var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            //var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var body = "";

            // Return body (could be SOAP fault); caller handles success vs fault
            return body;
        }

        // kept for compatibility; thin wrapper that uses CallApiAsync
        public Task<string> CallSoapAsync(string url, string action, string xmlBody)
        {
            // Note: original CallSoapAsync used _httpClient and identical behavior.
            return CallApiAsync(xmlBody, action);
        }

        //public async Task<com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse> Result(com.ThirdPartyAPIs.Models.Flight.SearchRequest.RootObject data)
        //{


        #endregion Reslut comment code

        #region Result
        public async Task<com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse> Result(com.ThirdPartyAPIs.Models.Flight.SearchRequest.RootObject data, string SC, UserDetailDTO userdetail)
        {
            //ConcurrentBag<FlightPriceData> concurrentPricing = [];
            var flightdatamain = new Models.Flight.ResultResponse.FlightResponse();

            try
            {
                string password = "U9MbJZjzR^EP";
                //string searchkey = GetSearchKey(data);
                //CBTFlightBooking.Models.Flight.cachedresponse.Rootobject casheresponse = new Models.Flight.cachedresponse.Rootobject();

                //casheresponse = GetCachedResponse(searchkey);

                //if (!string.IsNullOrEmpty(casheresponse?.response))
                //{
                //    lstPricing = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FlightPriceData>>(casheresponse.response);
                //    return lstPricing;
                //}

                #region Request
                var url = _configuration["FlightSettings:AirProductionURL"];
                Guid Messageguid = Guid.NewGuid();
                string guidString = Messageguid.ToString();
                byte[] nonce = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(nonce);
                }
                DateTime timestamp = DateTime.UtcNow;
                string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
                string encodedNonce = Convert.ToBase64String(nonce);
                string passSHA = "";
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                    byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                    byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                    Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                    Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                    Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                    byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                    passSHA = Convert.ToBase64String(passSHABytes);
                }


                StringBuilder sb = new();
                sb.Append("<Fare_MasterPricerTravelBoardSearch xmlns=\"http://xml.amadeus.com/FMPTBQ_23_4_1A\" >");
                sb.Append("<numberOfUnit>");
                sb.Append("<unitNumberDetail>");
                sb.Append("<numberOfUnits>" + _configuration["FlightSettings:NoOfCombinations"] + "</numberOfUnits>");
                sb.Append("<typeOfUnit>RC</typeOfUnit>");
                sb.Append("</unitNumberDetail>");
                sb.Append("<unitNumberDetail>");
                sb.Append("<numberOfUnits>" + (data.adults + data.children) + "</numberOfUnits>");
                sb.Append("<typeOfUnit>PX</typeOfUnit>");
                sb.Append("</unitNumberDetail>");
                sb.Append("</numberOfUnit>");
                sb.Append("<paxReference>");
                sb.Append("<ptc>IIT</ptc>");
                sb.Append("<ptc>ADT</ptc>");
                for (int a = 0; a < data.adults; a++)
                {
                    sb.Append("<traveller>");
                    sb.Append("<ref>" + (a + 1) + "</ref>");
                    sb.Append("</traveller>");
                }
                sb.Append("</paxReference>");
                if (data.children > 0)
                {
                    sb.Append("<paxReference>");
                    sb.Append("<ptc>CNN</ptc>");
                    sb.Append("<ptc>INN</ptc>");
                }
                for (int c = data.adults; c < (data.adults + data.children); c++)
                {
                    sb.Append("<traveller>");
                    sb.Append("<ref>" + (c + 1) + "</ref>");
                    sb.Append("</traveller>");
                }
                if (data.children > 0)
                    sb.Append("</paxReference>");
                if (data.infant > 0)
                {
                    sb.Append("<paxReference>");
                    sb.Append("<ptc>ITF</ptc>");
                    sb.Append("<ptc>IN</ptc>");
                }
                for (int c = 0; c < data.infant; c++)
                {
                    sb.Append("<traveller>");
                    sb.Append("<ref>" + (c + 1) + "</ref>");
                    sb.Append("<infantIndicator>1</infantIndicator>");
                    sb.Append("</traveller>");
                }
                if (data.infant > 0)
                    sb.Append("</paxReference>");
                sb.Append("<fareOptions>");
                sb.Append("<pricingTickInfo>");
                sb.Append("<pricingTicketing>");
                sb.Append("<priceType>TAC</priceType>");
                sb.Append("<priceType>RP</priceType>");
                sb.Append("<priceType>ET</priceType>");
                sb.Append("<priceType>RU</priceType>");
                sb.Append("<priceType>CUC</priceType>");
                sb.Append("<priceType>MNR</priceType>");
                sb.Append("<priceType>RW</priceType>");
                //if (data.isRF == "1")
                //sb.Append("<priceType>RF</priceType>");
                //if (data.isRF == "2")
                //    sb.Append("<priceType>NRE</priceType>");
                sb.Append("</pricingTicketing>");
                sb.Append("</pricingTickInfo>");
                sb.Append("<corporate>");
                sb.Append("<corporateId>");
                sb.Append("<corporateQualifier>RW</corporateQualifier>");
                sb.Append("<identity>387833</identity>");
                //sb.Append("<identity>SEATONLY</identity>");
                sb.Append("</corporateId>");
                sb.Append("</corporate>");
                sb.Append("<feeIdDescription>");
                sb.Append("<feeId>");
                sb.Append("<feeType>FFI</feeType>");
                sb.Append("<feeIdNumber>1</feeIdNumber>");
                sb.Append("</feeId>");
                sb.Append("</feeIdDescription>");
                sb.Append("<conversionRate>");
                sb.Append("<conversionRateDetail>");
                sb.Append("<currency>" + APIcurrency + "</currency>");
                sb.Append("</conversionRateDetail>");
                sb.Append("</conversionRate>");
                sb.Append("</fareOptions>");
                sb.Append("<travelFlightInfo>");
                sb.Append("<cabinId>");
                sb.Append("<cabinQualifier>RC</cabinQualifier>");
                sb.Append("<cabin>" + data.cabin + "</cabin>");
                sb.Append("</cabinId>");
                sb.Append("</travelFlightInfo>");
                if (data.JourneyType != (int)JourneyTypeEnum.MultiCity)
                {
                    for (int i = 1; i <= data.JourneyType; i++)
                    {
                        sb.Append("<itinerary>");
                        sb.Append("<requestedSegmentRef>");
                        sb.Append("<segRef>" + i + "</segRef>");
                        sb.Append("</requestedSegmentRef>");
                        sb.Append("<departureLocalization>");
                        sb.Append("<departurePoint>");
                        sb.Append("<locationId>" + data.segments[i - 1].depcode + "</locationId>");
                        sb.Append("</departurePoint>");
                        sb.Append("</departureLocalization>");
                        sb.Append("<arrivalLocalization>");
                        sb.Append("<arrivalPointDetails>");
                        sb.Append("<locationId>" + data.segments[i - 1].arrcode + "</locationId>");
                        sb.Append("</arrivalPointDetails>");
                        sb.Append("</arrivalLocalization>");
                        sb.Append("<timeDetails>");
                        sb.Append("<firstDateTimeDetail>");
                        sb.Append("<date>" + Convert.ToDateTime(data.segments[i - 1].depdate).ToString("ddMMyy") + "</date>");
                        sb.Append("</firstDateTimeDetail>");
                        sb.Append("</timeDetails>");
                        sb.Append("</itinerary>");
                    }
                }
                else if (data.JourneyType == (int)JourneyTypeEnum.MultiCity)
                {
                    int segno = 1;
                    //for (int i = 1; i <= data.JourneyType; i++)
                    foreach(var i in data.segments)
                    {
                        sb.Append("<itinerary>");
                        sb.Append("<requestedSegmentRef>");
                        sb.Append("<segRef>" + segno + "</segRef>");
                        sb.Append("</requestedSegmentRef>");
                        sb.Append("<departureLocalization>");
                        sb.Append("<depMultiCity>");
                        sb.Append("<locationId>" + i.depcode + "</locationId>");
                        sb.Append("</depMultiCity>");
                        sb.Append("</departureLocalization>");
                        sb.Append("<arrivalLocalization>");
                        sb.Append("<arrivalMultiCity>");
                        sb.Append("<locationId>" + i.arrcode + "</locationId>");
                        sb.Append("</arrivalMultiCity>");
                        sb.Append("</arrivalLocalization>");
                        sb.Append("<timeDetails>");
                        sb.Append("<firstDateTimeDetail>");
                        sb.Append("<date>" + Convert.ToDateTime(i.depdate).ToString("ddMMyy") + "</date>");
                        sb.Append("</firstDateTimeDetail>");
                        sb.Append("</timeDetails></itinerary>");

                        segno++;
                    }
                }

                sb.Append("</Fare_MasterPricerTravelBoardSearch>");

                var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                + "<s:Header>"
                + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID>"
                + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "FMPTBQ_23_4_1A</a:Action>"
                + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                + "<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">"
                + "<oas:UsernameToken xmlns:oas=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\""
                + " xmlns:oas1=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" oas1:Id=\"UsernameToken-1\">"
                + "<oas:Username>" + _configuration["FlightSettings:AirUserName"] + "</oas:Username>"
                + "<oas:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" + encodedNonce + "</oas:Nonce>"
                + "<oas:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">" + passSHA + "</oas:Password>"
                + "<oas1:Created>" + formattedTimestamp + "</oas1:Created>"
                + "</oas:UsernameToken>"
                + "</Security>"
                + "<h:AMA_SecurityHostedUser xmlns:h=\"http://xml.amadeus.com/2010/06/Security_v1\">"
                + "<h:UserID POS_Type=\"1\" PseudoCityCode=\"" + _configuration["FlightSettings:AirOfficeID"]
                + "\" AgentDutyCode=\"" + _configuration["FlightSettings:AirDutyCode"] + "\" RequestorType=\"U\"/>"
                + "</h:AMA_SecurityHostedUser>"
                + "</s:Header>"
                + "<s:Body>"
                + sb.ToString()
                + "</s:Body>"
                + "</s:Envelope>", null, "application/xml");
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //var reqt = new HttpRequestMessage(HttpMethod.Post, url);
                //reqt.Headers.Add("soapAction", _configuration["FlightSettings:AirSoapAction"] + "FMPTBQ_23_4_1A");
                //reqt.Content = content;
                var requestContent = content.ReadAsStringAsync().Result;

                #endregion Request

                #region Response

                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                //var response = await _httpClient.SendAsync(reqt);

                string soapAction = _configuration["FlightSettings:AirSoapAction"] + "FMPTBQ_23_4_1A";
                var response = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

                //var re = response.Content.ReadAsStringAsync().Result;
                var re = response;

                //SaveAmadeusLog( , re, "Response Fare_MasterPricerTravelBoardSearch");
                //System.IO.File.WriteAllText("C:\\xmlfiles\\Fare_MasterPricerTravelBoardSearch Request.xml", requestContent);
                //System.IO.File.WriteAllText("C:\\xmlfiles\\Fare_MasterPricerTravelBoardSearch Response.xml", re);

                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
                var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");
                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    if (!Directory.Exists(xmlfiles))
                        Directory.CreateDirectory(xmlfiles);
                    File.WriteAllText(Path.Combine(xmlfiles, SC + "_Flight_amadeus_request.xml"), requestContent, Encoding.UTF8);
                    File.WriteAllText(Path.Combine(xmlfiles, SC + "_Flight_amadeus_response.xml"), re ?? string.Empty, Encoding.UTF8);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerTravelBoardSearch_response.Envelope));
                StringReader rdr = new StringReader(re);
                com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerTravelBoardSearch_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerTravelBoardSearch_response.Envelope)serializer.Deserialize(rdr);
                rdr.Close();

                //File.WriteAllText(xml_files + "Fare_MasterPricerTravelBoardSearch_response_" + sc + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);
                System.IO.File.WriteAllText(xmlfiles + SC + "_Flight_amadeus_response.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);


                if (result == null || result.Body == null || result.Body.Fare_MasterPricerTravelBoardSearchReply == null || result.Body.Fare_MasterPricerTravelBoardSearchReply.errorMessage != null || result.Body.Fare_MasterPricerTravelBoardSearchReply.recommendation == null || result.Body.Fare_MasterPricerTravelBoardSearchReply.recommendation.Length == 0)
                {
                    return flightdatamain;
                }

                bool AgencyFixed_allow = userdetail.MarkupType == (int)MrkupTypeEnum.Fixed ? true : false;
                bool AgencyPercentage_allow = userdetail.MarkupType == (int)MrkupTypeEnum.Percentage ? true : false;

                double AgencyFixed_value = userdetail.MarkupType != null ? Convert.ToDouble(userdetail.MarkupValue) : 0;
                double AgencyPercentage_value = userdetail.MarkupValue != null ? Convert.ToDouble(userdetail.MarkupValue) : 0;

                #region result bind

                #region Currency
                double currency_exchangerate = 1;
                string currency = userdetail.Currency;
                string currency_sign = userdetail.CurrencySign;
                try
                {
                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer.Rootobject Rootobject_curency_list = new CurrencyExchange.currencyexchange_rate_fixer.Rootobject();
                    //var client = _httpClient.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer currency_exchange_rate_fixer = new CurrencyExchange.currencyexchange_rate_fixer(_configuration, _env, _genericRepository);

                    //string currency = currency_exchange_rate_fixer.Currency("");

                    Rootobject_curency_list = await currency_exchange_rate_fixer.get_exchage_rate();
                    currency_exchangerate = currency_exchange_rate_fixer.currency_exchange_rate(Rootobject_curency_list, currency, APIcurrency);
                    if (currency_exchangerate == 0)
                    {
                        currency = APIcurrency;
                        currency_sign = APIcurrency_sign;
                        currency_exchangerate = 1;
                    }
                }
                catch (Exception ex)
                {
                    currency = APIcurrency;
                    currency_sign = APIcurrency_sign;
                    currency_exchangerate = 1;
                }
                #endregion

                #region Itinerary Bound

                var airlinefilelistfile = "";
                using (var fileStream = new FileStream(importantfiles + "Airlinelist.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    airlinefilelistfile = streamReader.ReadToEnd();
                }
                List<AirlineListDTO> airlinefilelist = System.Text.Json.JsonSerializer.Deserialize<List<AirlineListDTO>>(airlinefilelistfile);

                string listairportjson = System.IO.File.ReadAllText(importantfiles + "Airportlist.json", System.Text.ASCIIEncoding.UTF8);
                List<AirportListDTO> airportfilelist = System.Text.Json.JsonSerializer.Deserialize<List<AirportListDTO>>(listairportjson);

                //List<Models.Flight.Result.response.AirPortInfo> Airport_Filter = new List<Models.Flight.Result.response.AirPortInfo>();
                //List<Models.Flight.Result.response.FlightFilter> Airline_Filter = new List<Models.Flight.Result.response.FlightFilter>();
                //List<Models.Flight.Result.response.FlightFilter_price> FlightFilter_price = new List<Models.Flight.Result.response.FlightFilter_price>();


                //List<string> Outbound_Destination_Filter = new List<string>();
                //List<string> Inbound_Destination_Filter = new List<string>();
                //Models.Flight.Result.response.stopsdata Outboundstops = new Models.Flight.Result.response.stopsdata();
                //Models.Flight.Result.response.stopsdata Inboundstops = new Models.Flight.Result.response.stopsdata();
                List<com.ThirdPartyAPIs.Models.Flight.ResultResponse.Flightdata> Flightdata = new List<Models.Flight.ResultResponse.Flightdata>();
                List<com.ThirdPartyAPIs.Models.Flight.ResultResponse.Flightdata> Flightdata_secret = new List<Models.Flight.ResultResponse.Flightdata>();

                List<string> flight_uniqueids = new List<string>();

                #region manage pcc code

                //if (ds_flight_configuration.Tables.Count > 2)
                //{
                //    DataTable dt_pcc = ds_flight_configuration.Tables[2];

                //    if (dt_pcc.Rows.Count > 0)
                //    {
                //        int searchindex = 1;
                //        foreach (DataRow dr in dt_pcc.Rows)
                //        {
                //
                //            Models.Flight.Result.response.Flight pccresult = new Models.Flight.Result.response.Flight();

                //            pccresult = otherpcccall(ds_flight_configuration, currency, currency_exchangerate, dr["pcc_code"].ToString(), searchindex, admin_fixed_allow, admin_percentage_allow, admin_fixed, admin_percentage, Dt_airline_discount_markup, dt_airlinemarkup, sc, searchcriteria);


                //            if (pccresult.success == true)
                //            {
                //                if (Airline_Filter.Count == 0)
                //                {
                //                    Airline_Filter = pccresult.data.Filter.Airline;
                //                }
                //                else
                //                {
                //                    Airline_Filter.AddRange(pccresult.data.Filter.Airline);
                //                }
                //                if (Airport_Filter.Count == 0)
                //                {
                //                    Airport_Filter = pccresult.data.Filter.AirportFilter;
                //                }
                //                else
                //                {
                //                    Airport_Filter.AddRange(pccresult.data.Filter.AirportFilter);
                //                }
                //                if (Flightdata.Count == 0)
                //                {
                //                    Flightdata = pccresult.data.Data;
                //                }
                //                else
                //                {
                //                    Flightdata.AddRange(pccresult.data.Data);
                //                }

                //                if (flight_uniqueids.Count == 0)
                //                {
                //                    flight_uniqueids = pccresult.data.flight_unique_ids;
                //                }
                //                else
                //                {
                //                    flight_uniqueids.AddRange(pccresult.data.flight_unique_ids);
                //                }
                //            }
                //            searchindex++;
                //        }
                //    }
                //}

                #endregion

                int result_index = 1;

                foreach (Amadeus_wsdl.Fare_MasterPricerTravelBoardSearchReplyRecommendation recommendation in result.Body.Fare_MasterPricerTravelBoardSearchReply.recommendation)
                {
                    bool isRefundable = false;
                    #region Tarriff info
                    double apitotalprice = 0;
                    double totalprice = 0;
                    double baseprice = 0;
                    double taxprice = 0;
                    List<Models.Flight.ResultResponse.tarriffinfo> tarriffinfo = new List<Models.Flight.ResultResponse.tarriffinfo>();
                    List<Models.Flight.ResultResponse.pax_fareDetailsBySegment> pax_fareDetailsBySegment = new List<Models.Flight.ResultResponse.pax_fareDetailsBySegment>();

                    List<Models.Flight.ResultResponse.fareDetailsgroup> fareDetailsgroup = new List<Models.Flight.ResultResponse.fareDetailsgroup>();

                    foreach (Amadeus_wsdl.Fare_MasterPricerTravelBoardSearchReplyRecommendationPaxFareProduct paxFareProduct in recommendation.paxFareProduct)
                    {
                        int quantity = paxFareProduct.paxReference[0].traveller.Length;

                        double api_total_price_per_leg = Convert.ToDouble(paxFareProduct.paxFareDetail.totalFareAmount) * quantity;
                        double api_tax_price_per_leg = Convert.ToDouble(paxFareProduct.paxFareDetail.totalTaxAmount) * quantity;
                        double api_base_price_per_leg = api_total_price_per_leg - api_tax_price_per_leg;

                        double net_total_price_per_leg = api_total_price_per_leg;
                        double net_tax_price_per_leg = api_tax_price_per_leg;
                        double net_base_price_per_leg = api_base_price_per_leg;


                        double base_price_per_leg = Convert.ToDouble((net_base_price_per_leg).ToString("#0.00"));
                        double total_price_per_leg = Convert.ToDouble((net_total_price_per_leg).ToString("#0.00"));
                        double tax_price_per_leg = total_price_per_leg - base_price_per_leg;

                        int typeofpax = 0;
                        string PaxType = paxFareProduct.paxReference[0].ptc[0];
                        string PaxType_text = paxFareProduct.paxReference[0].ptc[0];
                        if (PaxType.ToLower() == "adt" || PaxType.ToLower() == "iit")
                        {
                            typeofpax = (int)PaxtypeEnum.Adult;
                            PaxType_text = "Adult";
                        }
                        else if (PaxType.ToLower() == "cnn")
                        {
                            typeofpax = (int)PaxtypeEnum.Child;
                            PaxType_text = "Child";
                        }
                        else if (PaxType.ToLower() == "inf")
                        {
                            typeofpax = (int)PaxtypeEnum.Infant;
                            PaxType_text = "Infant";
                        }

                        totalprice = totalprice + net_total_price_per_leg;
                        baseprice = baseprice + net_base_price_per_leg;
                        taxprice = taxprice + net_tax_price_per_leg;


                        apitotalprice = apitotalprice + net_total_price_per_leg;

                        tarriffinfo.Add(new Models.Flight.ResultResponse.tarriffinfo()
                        {
                            api_baseprice = api_base_price_per_leg,
                            api_tax = api_tax_price_per_leg,
                            api_totalprice = api_total_price_per_leg,
                            net_baseprice = net_base_price_per_leg,
                            net_tax = net_tax_price_per_leg,
                            net_totalprice = net_total_price_per_leg,
                            baseprice = Convert.ToDouble(base_price_per_leg.ToString("#0.00")),
                            currency = currency,
                            paxtype = typeofpax,
                            paxtype_text = PaxType_text,
                            paxid = "",// travelerPricings.travelerId,
                            per_pax_total_price = Convert.ToDouble(total_price_per_leg.ToString("#0.00")),
                            quantity = quantity,
                            tax = Convert.ToDouble(tax_price_per_leg.ToString("#0.00")),
                            totalprice = Convert.ToDouble((total_price_per_leg).ToString("#0.00"))
                        });




                        int segindex = 0;
                        foreach (Amadeus_wsdl.Fare_MasterPricerTravelBoardSearchReplyRecommendationPaxFareProductFareDetails fareDetails in paxFareProduct.fareDetails)
                        {
                            int leg_index = 1;
                            foreach (Amadeus_wsdl.Fare_MasterPricerTravelBoardSearchReplyRecommendationPaxFareProductFareDetailsGroupOfFares groupOfFares in fareDetails.groupOfFares)
                            {
                                var pax_fareDetailsBySegment_find_obj = pax_fareDetailsBySegment.Find(x => x.segmentid == leg_index.ToString());
                                if (pax_fareDetailsBySegment_find_obj == null)
                                {
                                    pax_fareDetailsBySegment.Add(new Models.Flight.ResultResponse.pax_fareDetailsBySegment()
                                    {
                                        segmentid = leg_index.ToString(),
                                        segment_pax_detail = new List<Models.Flight.ResultResponse.segment_pax_detail>() { new Models.Flight.ResultResponse.segment_pax_detail() {
                                    baggage = null,
                                    cabin = groupOfFares.productInformation.cabinProduct[0].cabin,
                                    class_code = groupOfFares.productInformation.cabinProduct[0].rbd,
                                    fareBasis =groupOfFares.productInformation.fareProductDetail.fareBasis,
                                    paxid = "",//travelerPricings.travelerId,
                                    //paxtype = PaxType,
                                    paxtype = typeofpax,
                                }},
                                    });
                                }
                                else
                                {
                                    pax_fareDetailsBySegment_find_obj.segment_pax_detail.Add(new Models.Flight.ResultResponse.segment_pax_detail()
                                    {
                                        baggage = null,
                                        cabin = groupOfFares.productInformation.cabinProduct[0].cabin,
                                        class_code = groupOfFares.productInformation.cabinProduct[0].rbd,
                                        fareBasis = groupOfFares.productInformation.fareProductDetail.fareBasis,
                                        paxid = "",//travelerPricings.travelerId,
                                        paxtype = typeofpax,
                                    });
                                }

                                var pax_fareDetailsgroup_obj = fareDetailsgroup.Find(x => x.segmentid == segindex);
                                if (pax_fareDetailsgroup_obj == null)
                                {
                                    fareDetailsgroup.Add(new Models.Flight.ResultResponse.fareDetailsgroup()
                                    {
                                        segmentid = segindex,
                                        groupfare = new List<Models.Flight.ResultResponse.groupfare>() { new Models.Flight.ResultResponse.groupfare() {

                                    cabin = groupOfFares.productInformation.cabinProduct[0].cabin,
                                    rbd = groupOfFares.productInformation.cabinProduct[0].rbd,
                                    fareBasis =groupOfFares.productInformation.fareProductDetail.fareBasis,
                                    groupindex=leg_index,
                                }},
                                    });
                                }
                                else
                                {
                                    pax_fareDetailsgroup_obj.groupfare.Add(new Models.Flight.ResultResponse.groupfare()
                                    {
                                        cabin = groupOfFares.productInformation.cabinProduct[0].cabin,
                                        rbd = groupOfFares.productInformation.cabinProduct[0].rbd,
                                        fareBasis = groupOfFares.productInformation.fareProductDetail.fareBasis,
                                        groupindex = leg_index,
                                    });
                                }
                                leg_index++;
                            }
                            segindex++;

                        }
                        try
                        {
                            var inforType = paxFareProduct.fare.FirstOrDefault(x => x.pricingMessage.freeTextQualification.informationType == "73");

                            if (inforType != null && inforType.pricingMessage.freeTextQualification.informationType == "73")
                            {
                                isRefundable = true;
                            }
                            else { isRefundable = true; }
                        }
                        catch { }

                    }

                    #endregion;

                    foreach (Amadeus_wsdl.ReferenceInfoType22 Itinerary in recommendation.segmentFlightRef)
                    //foreach (Amadeus_wsdl.ReferenceInformationType11 Itinerary in recommendation.segmentFlightRef)
                    {
                        bool calculate_markup = true;
                        int totaltime = 0;
                        Amadeus_wsdl.ReferencingDetailsType_191583C2[] Out_in_bound_list = Array.FindAll(Itinerary.referencingDetail, item => item.refQualifier == "S");

                        if (Out_in_bound_list == null || Out_in_bound_list.Length == 0)
                        {
                            continue;
                        }

                        #region Slice and Dice
                        Amadeus_wsdl.ReferencingDetailsType_191583C2[] slice_dice_list = Array.FindAll(Itinerary.referencingDetail, item => item.refQualifier == "A");
                        List<Models.Flight.ResultResponse.spirit_airline> Slice_dice_value = new List<Models.Flight.ResultResponse.spirit_airline>();
                        if (slice_dice_list != null && slice_dice_list.Length > 0)
                        {
                            foreach (var slice_dice_list_each in slice_dice_list)
                            {
                                var specificRecDetails = Array.Find(recommendation.specificRecDetails, item => item.specificRecItem.referenceType == slice_dice_list_each.refQualifier && item.specificRecItem.refNumber == slice_dice_list_each.refNumber);
                                if (specificRecDetails != null)
                                {
                                    foreach (var specificProductDetails in specificRecDetails.specificProductDetails)
                                    {
                                        foreach (var fareContextDetails in specificProductDetails.fareContextDetails)
                                        {
                                            int leg = 1;
                                            if (fareContextDetails.cnxContextDetails.Length > 0)
                                            {
                                                foreach (var cnxContextDetails in fareContextDetails.cnxContextDetails)
                                                {
                                                    if (cnxContextDetails.fareCnxInfo.contextDetails.Length > 0)
                                                    {
                                                        foreach (var contextDetails in cnxContextDetails.fareCnxInfo.contextDetails)
                                                        {
                                                            Slice_dice_value.Add(new Models.Flight.ResultResponse.spirit_airline()
                                                            {
                                                                leg_ref = leg.ToString(),
                                                                segment_ref = fareContextDetails.requestedSegmentInfo.segRef,
                                                                code = contextDetails,
                                                            });
                                                            leg++;
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion;

                        List<Models.Flight.ResultResponse.OutboundInbounddata> OutboundInbounddata = new List<Models.Flight.ResultResponse.OutboundInbounddata>();
                        List<string> airlinelists = new List<string>();
                        int index_out_in_bound = 0;

                        string flight_uniqueid = "";

                        foreach (Amadeus_wsdl.ReferencingDetailsType_191583C2 Out_in_bound in Out_in_bound_list)
                        {
                            int leg_index = 1;
                            List<Models.Flight.ResultResponse.flightlist> flightlist = new List<Models.Flight.ResultResponse.flightlist>();

                            Amadeus_wsdl.Fare_MasterPricerTravelBoardSearchReplyFlightIndex Fare_MasterPricerTravelBoardSearchReplyFlightIndex = result.Body.Fare_MasterPricerTravelBoardSearchReply.flightIndex[index_out_in_bound];

                            Amadeus_wsdl.Fare_MasterPricerTravelBoardSearchReplyFlightIndexGroupOfFlights legs = Array.Find(Fare_MasterPricerTravelBoardSearchReplyFlightIndex.groupOfFlights, item => item.propFlightGrDetail.flightProposal[0].@ref == Out_in_bound.refNumber);


                            string previous_leg_datetime = "";
                            int totalminutes = 0;
                            foreach (Amadeus_wsdl.Fare_MasterPricerTravelBoardSearchReplyFlightIndexGroupOfFlightsFlightDetails leg_details in legs.flightDetails)
                            {
                                #region MARKUP AND DISCOUNT
                                if (calculate_markup)
                                {

                                    #region Getting Airline Discount
                                    //Mode  true = B2B , false = B2C
                                    //Origin  true = Algeria , false = other
                                    //discount_type  true = Fixed , false = Percentage
                                    //markup_type true = Fixed , false = Percentage

                                    string airline_code = leg_details.flightInformation.companyId.marketingCarrier;
                                    string airport_code = leg_details.flightInformation.location[0].locationId;
                                    string class_code = recommendation.paxFareProduct[0].fareDetails[0].groupOfFares[0].productInformation.cabinProduct[0].cabin;

                                    //  Models.Admin.airportdata.airlinejsonlist Airline_discount_found = new Models.Admin.airportdata.airlinejsonlist();

                                    #region varibale
                                    bool discountallow = false;
                                    double discountperc = 0;
                                    double discountvalue = 0;
                                    double markupvalue = 0;
                                    bool ismarkuppercentage = false;
                                    #endregion

                                    //if (dt_airlinemarkup.Rows.Count > 0)
                                    //{
                                    //    foreach (DataRow dr in dt_airlinemarkup.Select("airlinecode='" + airline_code + "'"))
                                    //    {
                                    //        discountperc = dr["discountvalue"].ToString() != null && dr["discountvalue"].ToString() != "" ? Convert.ToDouble(dr["discountvalue"].ToString()) : 0;
                                    //        if (dr["destinationcode"].ToString() == searchcriteria.destinations[0].departureairportcode || dr["destinationcode"].ToString().ToLower() == "xx")
                                    //        {
                                    //            discountallow = true;
                                    //            discountvalue = (baseprice * discountperc) / 100;
                                    //        }
                                    //        markupvalue = dr["markupvalue"].ToString() != null && dr["markupvalue"].ToString() != "" ? Convert.ToDouble(dr["markupvalue"].ToString()) : 0;
                                    //        ismarkuppercentage = dr["markuptype"].ToString().ToLower() == "fixed" ? false : true;

                                    //    }
                                    //}

                                    totalprice = 0;
                                    baseprice = 0;

                                    //#region apply on total price
                                    //#region discount apply
                                    //totalprice = apitotalprice - discountvalue;
                                    //#endregion

                                    //#region markup
                                    //// markup formula:  markupvalue= ((baseprice - discount) + tax) * markuppercentage / 100;

                                    //if(ismarkuppercentage==true)
                                    //{
                                    //    double markupval = ((baseprice - discountvalue) + taxprice)  * markupvalue / 100;

                                    //    double markupapply = ((baseprice - discountvalue) + markupval);
                                    //    totalprice = markupapply + taxprice;
                                    //}
                                    //else
                                    //{
                                    //    totalprice = totalprice + markupvalue;
                                    //}
                                    //#endregion
                                    //#endregion

                                    //foreach (DataRow dr in Dt_airline_discount_markup.Select("code='" + airline_code + "'"))
                                    //{
                                    //    /*Airline_discount_found.discount_type = Convert.ToBoolean(dr["discount_type"].ToString());
                                    //    Airline_discount_found.discount = Convert.ToDouble(dr["discount"].ToString());*/

                                    //    Airline_discount_found.ispercentage = dr["ispercentage"].ToString() != null && dr["ispercentage"].ToString() != "" ? Convert.ToBoolean(dr["ispercentage"].ToString()) : false;
                                    //    Airline_discount_found.markup_percentage = dr["markup_percentage"].ToString() != null && dr["markup_percentage"].ToString() != "" ? Convert.ToDouble(dr["markup_percentage"].ToString()) : 0;

                                    //    Airline_discount_found.isfixed = dr["isfixed"].ToString() != null && dr["isfixed"].ToString() != "" ? Convert.ToBoolean(dr["isfixed"].ToString()) : false;
                                    //    Airline_discount_found.markup_fixed = dr["markup_fixed"].ToString() != null && dr["markup_fixed"].ToString() != "" ? Convert.ToDouble(dr["markup_fixed"].ToString()) : 0;

                                    //    break;
                                    //}
                                    #endregion;

                                    #region TOTAL PRICE
                                    //#region Appling Discount
                                    ///*double discount_amount = 0;
                                    //if (Airline_discount_found.discount > 0)
                                    //{
                                    //    if (Airline_discount_found.discount_type)
                                    //    {
                                    //        discount_amount = Airline_discount_found.discount * (Search_criteria_obj.totaladult + Search_criteria_obj.totalchild + Search_criteria_obj.totalinfant);
                                    //    }
                                    //    else if (Airline_discount_found.discount_type == false && Airline_discount_found.discount != 100)
                                    //    {
                                    //        discount_amount = ((totalprice * Airline_discount_found.discount) / 100);
                                    //    }

                                    //    if (discount_amount > totalprice || discount_amount < 0)
                                    //    {
                                    //        discount_amount = 0;
                                    //    }
                                    //}
                                    //totalprice = totalprice - discount_amount;*/
                                    //#endregion;

                                    #region Applying Markup
                                    double markup_amount = 0;
                                    /*if (Airline_discount_found.markup > 0)
                                    {*/
                                    //if (Airline_discount_found.isfixed == true)
                                    //{
                                    //    markup_amount = markup_amount + Airline_discount_found.markup_fixed * (Search_criteria_obj.totaladult + Search_criteria_obj.totalchild + Search_criteria_obj.totalinfant);
                                    //}
                                    //if (Airline_discount_found.ispercentage == true && Airline_discount_found.markup_percentage != 100)
                                    //{
                                    //    markup_amount = markup_amount + ((totalprice * Airline_discount_found.markup_percentage) / 100);
                                    //}

                                    //if (markup_amount > totalprice || markup_amount < 0)
                                    //{
                                    //    markup_amount = 0;
                                    //}
                                    /*}*/

                                    /*totalprice = totalprice + markup_amount;*/


                                    //if (admin_fixed_allow)
                                    //{
                                    //    markup_amount = markup_amount + (admin_fixed * (Search_criteria_obj.totaladult + Search_criteria_obj.totalchild + Search_criteria_obj.totalinfant));
                                    //}
                                    //if (admin_percentage_allow)
                                    //{
                                    //    markup_amount = markup_amount + ((totalprice * admin_percentage) / 100);
                                    //}

                                    //totalprice = totalprice + markup_amount;

                                    //if (AgencyFixed_allow)
                                    //{
                                    //    markup_amount = markup_amount + (AgencyFixed_value * (data.adults + data.children + data.infant));
                                    //}
                                    //if (AgencyPercentage_allow)
                                    //{
                                    //    markup_amount = markup_amount + ((totalprice * AgencyPercentage_value) / 100);
                                    //}

                                    //totalprice = totalprice + markup_amount;
                                    #endregion;

                                    #endregion;

                                    #region BREAKDOWN
                                    foreach (var breakdown in tarriffinfo)
                                    {

                                        #region apply on total price

                                        #region discount apply

                                        double breakdiscountvalue = 0;

                                        if (discountallow == true)
                                        {
                                            breakdiscountvalue = (breakdown.api_baseprice * discountperc) / 100;

                                            breakdown.net_baseprice = breakdown.api_baseprice - breakdiscountvalue;
                                            breakdown.net_totalprice = breakdown.net_baseprice + breakdown.net_tax;
                                        }

                                        #endregion


                                        #region markup
                                        // markup formula:  markupvalue= ((baseprice - discount) + tax) * markuppercentage / 100;

                                        if (ismarkuppercentage == true)
                                        {
                                            double markupval = ((breakdown.api_baseprice - breakdiscountvalue) + breakdown.api_tax) * markupvalue / 100;

                                            double markupapply = (breakdown.api_baseprice + markupval);

                                            breakdown.net_baseprice = markupapply;
                                            breakdown.net_totalprice = markupapply + breakdown.api_tax;
                                        }
                                        else
                                        {
                                            breakdown.net_baseprice = (breakdown.api_baseprice - breakdiscountvalue) + markupvalue;
                                            breakdown.net_totalprice = breakdown.net_baseprice + breakdown.net_tax;
                                        }

                                        #endregion

                                        #endregion


                                        #region Appling Discount
                                        //double base_price_discount_amount_pax = 0;
                                        //double tax_price_discount_amount_pax = 0;
                                        //double total_price_discount_amount_pax = 0;
                                        //if (Airline_discount_found.discount > 0)
                                        //{
                                        //    if (Airline_discount_found.discount_type)
                                        //    {
                                        //        base_price_discount_amount_pax = Airline_discount_found.discount;
                                        //        total_price_discount_amount_pax = Airline_discount_found.discount;
                                        //    }
                                        //    else if (Airline_discount_found.discount_type == false)
                                        //    {
                                        //        base_price_discount_amount_pax = ((breakdown.net_baseprice * Airline_discount_found.discount) / 100);
                                        //        tax_price_discount_amount_pax = ((breakdown.net_tax * Airline_discount_found.discount) / 100);
                                        //        total_price_discount_amount_pax = ((breakdown.net_totalprice * Airline_discount_found.discount) / 100);
                                        //    }

                                        //    if (base_price_discount_amount_pax > breakdown.net_baseprice || base_price_discount_amount_pax < 0)
                                        //    {
                                        //        base_price_discount_amount_pax = 0;
                                        //        tax_price_discount_amount_pax = 0;
                                        //        total_price_discount_amount_pax = 0;
                                        //    }
                                        //}

                                        //breakdown.net_baseprice = breakdown.net_baseprice - base_price_discount_amount_pax;
                                        //breakdown.net_totalprice = breakdown.net_totalprice - total_price_discount_amount_pax;
                                        //breakdown.net_tax = breakdown.net_tax - tax_price_discount_amount_pax;
                                        #endregion;

                                        #region Applying Markup
                                        //double base_price_markup_amount_pax = 0;
                                        //double tax_price_markup_amount_pax = 0;
                                        //double total_price_markup_amount_pax = 0;
                                        ///*if (Airline_discount_found.markup > 0)
                                        //{
                                        //    if (Airline_discount_found.markup_type)
                                        //    {*/
                                        ////  base_price_markup_amount_pax = Airline_discount_found.markup / total_pax;
                                        //// total_price_markup_amount_pax = Airline_discount_found.markup / total_pax;

                                        ////base_price_markup_amount_pax = Airline_discount_found.markup;
                                        ////total_price_markup_amount_pax = Airline_discount_found.markup;

                                        //base_price_markup_amount_pax = markup_amount;
                                        //total_price_markup_amount_pax = markup_amount;

                                        ///*}
                                        //else if (Airline_discount_found.markup_type == false)
                                        //{
                                        //    base_price_markup_amount_pax = ((breakdown.net_baseprice * Airline_discount_found.markup) / 100);
                                        //    tax_price_markup_amount_pax = ((breakdown.net_tax * Airline_discount_found.markup) / 100);
                                        //    total_price_markup_amount_pax = ((breakdown.net_totalprice * Airline_discount_found.markup) / 100);
                                        //}*/

                                        //if (base_price_markup_amount_pax > breakdown.net_baseprice || base_price_markup_amount_pax < 0)
                                        //{
                                        //    base_price_markup_amount_pax = 0;
                                        //    tax_price_markup_amount_pax = 0;
                                        //    total_price_markup_amount_pax = 0;
                                        //}
                                        ///*}*/
                                        //breakdown.net_baseprice = breakdown.net_baseprice + base_price_markup_amount_pax;
                                        //breakdown.net_totalprice = breakdown.net_totalprice + total_price_markup_amount_pax;
                                        //breakdown.net_tax = breakdown.net_tax + tax_price_markup_amount_pax;

                                        //if (admin_fixed_allow)
                                        //{
                                        //    breakdown.net_baseprice = breakdown.net_baseprice + admin_fixed;
                                        //    breakdown.net_totalprice = breakdown.net_totalprice + admin_fixed;
                                        //}
                                        //if (admin_percentage_allow)
                                        //{
                                        //    breakdown.net_baseprice = breakdown.net_baseprice + ((breakdown.net_baseprice * admin_percentage) / 100);
                                        //    breakdown.net_totalprice = breakdown.net_totalprice + ((breakdown.net_totalprice * admin_percentage) / 100);
                                        //    breakdown.net_tax = breakdown.net_tax + ((breakdown.net_tax * admin_percentage) / 100);
                                        //}

                                        if (AgencyFixed_allow)
                                        {
                                            breakdown.net_baseprice = breakdown.net_baseprice + AgencyFixed_value;
                                            breakdown.net_totalprice = breakdown.net_totalprice + AgencyFixed_value;
                                        }
                                        if (AgencyPercentage_allow)
                                        {
                                            breakdown.net_baseprice = breakdown.net_baseprice + ((breakdown.net_baseprice * AgencyPercentage_value) / 100);
                                            breakdown.net_totalprice = breakdown.net_totalprice + ((breakdown.net_totalprice * AgencyPercentage_value) / 100);
                                            breakdown.net_tax = breakdown.net_tax + ((breakdown.net_tax * AgencyPercentage_value) / 100);
                                        }
                                        #endregion;

                                        double per_pax_total_price = breakdown.net_totalprice / breakdown.quantity;
                                        breakdown.totalprice = Convert.ToDouble((breakdown.net_totalprice * currency_exchangerate).ToString("#0.00"));
                                        breakdown.baseprice = Convert.ToDouble((breakdown.net_baseprice * currency_exchangerate).ToString("#0.00"));
                                        breakdown.tax = Convert.ToDouble((breakdown.net_tax * currency_exchangerate).ToString("#0.00"));
                                        //breakdown.per_pax_total_price = Convert.ToDouble((breakdown.net_totalprice * currency_exchangerate).ToString("#0.00"));
                                        breakdown.per_pax_total_price = Convert.ToDouble((per_pax_total_price * currency_exchangerate).ToString("#0.00"));
                                        //breakdown.totalprice = Convert.ToDouble((breakdown.net_totalprice).ToString("#0.00"));
                                        //breakdown.baseprice = Convert.ToDouble((breakdown.net_baseprice).ToString("#0.00"));
                                        //breakdown.tax = Convert.ToDouble((breakdown.net_tax).ToString("#0.00"));
                                        //breakdown.per_pax_total_price = Convert.ToDouble((breakdown.net_totalprice).ToString("#0.00"));

                                        totalprice += breakdown.totalprice;
                                        baseprice += breakdown.baseprice;
                                    }
                                    #endregion;

                                    calculate_markup = false;
                                }
                                #endregion;


                                Models.Flight.ResultResponse.Arrivaldeparture Departure = new Models.Flight.ResultResponse.Arrivaldeparture();
                                Models.Flight.ResultResponse.Arrivaldeparture Arrival = new Models.Flight.ResultResponse.Arrivaldeparture();
                                List<Models.Flight.ResultResponse.Checkinbaggage> Checkinbaggage = new List<Models.Flight.ResultResponse.Checkinbaggage>();

                                #region Departure

                                DateTime departure_datetime = DateTime.ParseExact(leg_details.flightInformation.productDateTime.dateOfDeparture + " " + leg_details.flightInformation.productDateTime.timeOfDeparture, "ddMMyy HHmm", CultureInfo.InvariantCulture);

                                Departure.time = departure_datetime.ToString("HH:mm");
                                Departure.Date = departure_datetime.ToString("dd-MM-yyyy");
                                Departure.Datetime = departure_datetime.ToString();
                                Departure.city = leg_details.flightInformation.location[0].locationId;
                                Departure.Iata = leg_details.flightInformation.location[0].locationId;
                                Departure.name = leg_details.flightInformation.location[0].locationId;
                                Departure.Terminal = leg_details.flightInformation.location[0].terminal == null ? "" : leg_details.flightInformation.location[0].terminal;

                                var departure_airport_obj = airportfilelist.Find(x => x.AirportCode == leg_details.flightInformation.location[0].locationId);
                                if (departure_airport_obj != null)
                                {
                                    Departure.city = departure_airport_obj.City;
                                    Departure.name = departure_airport_obj.AirportName;
                                }

                                //if (Airport_Filter.Find(x => x.Iata == Departure.Iata) == null)
                                //{
                                //    Airport_Filter.Add(new Models.Flight.Result.response.AirPortInfo()
                                //    {
                                //        Iata = Departure.Iata,
                                //        City = Departure.city,
                                //        Name = Departure.name,
                                //    });
                                //}

                                #endregion;

                                #region Arrival

                                DateTime arrival_datetime = DateTime.ParseExact(leg_details.flightInformation.productDateTime.dateOfArrival + " " + leg_details.flightInformation.productDateTime.timeOfArrival, "ddMMyy HHmm", CultureInfo.InvariantCulture);

                                Arrival.time = arrival_datetime.ToString("HH:mm");
                                Arrival.Date = arrival_datetime.ToString("dd-MM-yyyy");
                                Arrival.Datetime = arrival_datetime.ToString();
                                Arrival.city = leg_details.flightInformation.location[1].locationId;
                                Arrival.Iata = leg_details.flightInformation.location[1].locationId;
                                Arrival.name = leg_details.flightInformation.location[1].locationId;
                                Arrival.Terminal = leg_details.flightInformation.location[1].terminal == null ? "" : leg_details.flightInformation.location[1].terminal;

                                var arrival_airport_obj = airportfilelist.Find(x => x.AirportCode == leg_details.flightInformation.location[1].locationId);
                                if (arrival_airport_obj != null)
                                {
                                    Arrival.city = arrival_airport_obj.City;
                                    Arrival.name = arrival_airport_obj.AirportName;
                                }
                                //if (Airport_Filter.Find(x => x.Iata == Arrival.Iata) == null)
                                //{
                                //    Airport_Filter.Add(new Models.Flight.Result.response.AirPortInfo()
                                //    {
                                //        Iata = Arrival.Iata,
                                //        City = Arrival.city,
                                //        Name = Arrival.name,
                                //    });
                                //}
                                #endregion;

                                #region Airline
                                string Aircraft = "";
                                if (leg_details.flightInformation.productDetail != null && leg_details.flightInformation.productDetail.equipmentType != null && leg_details.flightInformation.productDetail.equipmentType != "")
                                {
                                    Aircraft = leg_details.flightInformation.productDetail.equipmentType;
                                    //if (Amadeus_response_obj.dictionaries != null && Amadeus_response_obj.dictionaries.aircraft != null && Amadeus_response_obj.dictionaries.aircraft.Count != 0)
                                    //{
                                    //    var aircraft_obj = Amadeus_response_obj.dictionaries.aircraft.FirstOrDefault(x => x.Key == Aircraft);
                                    //    Aircraft = aircraft_obj.Value;
                                    //}
                                }


                                Models.Flight.ResultResponse.operating_airline operating_airline = new Models.Flight.ResultResponse.operating_airline();
                                Models.Flight.ResultResponse.operating_airline marketing_airline = new Models.Flight.ResultResponse.operating_airline();

                                if (leg_details.flightInformation.companyId.marketingCarrier != null && leg_details.flightInformation.companyId.marketingCarrier != "")
                                {
                                    if (airlinelists.IndexOf(leg_details.flightInformation.companyId.marketingCarrier) < 0)
                                    {
                                        airlinelists.Add(leg_details.flightInformation.companyId.marketingCarrier);
                                    }
                                    marketing_airline.code = leg_details.flightInformation.companyId.marketingCarrier;
                                    marketing_airline.name = leg_details.flightInformation.companyId.marketingCarrier;
                                    marketing_airline.number = leg_details.flightInformation.flightOrtrainNumber;
                                    var airline_obj = airlinefilelist.Find(x => x.AirlineCode == leg_details.flightInformation.companyId.marketingCarrier);
                                    if (airline_obj != null)
                                    {
                                        marketing_airline.name = airline_obj.AirlineName;
                                        //if (Airline_Filter.Find(x => x.Code == leg_details.flightInformation.companyId.marketingCarrier) == null)
                                        //{
                                        //    Airline_Filter.Add(new Models.Flight.Result.response.FlightFilter()
                                        //    {
                                        //        Code = leg_details.flightInformation.companyId.marketingCarrier,
                                        //        price = totalprice.ToString("#0"),
                                        //        Value = marketing_airline.name,
                                        //    });
                                        //}
                                        //else if (Convert.ToDouble(Airline_Filter.Find(x => x.Code == leg_details.flightInformation.companyId.marketingCarrier).price) > totalprice)
                                        //{
                                        //    Airline_Filter.Find(x => x.Code == leg_details.flightInformation.companyId.marketingCarrier).price = totalprice.ToString("#0");
                                        //}
                                    }
                                }

                                if (leg_details.flightInformation.companyId.operatingCarrier != null && leg_details.flightInformation.companyId.operatingCarrier != null && leg_details.flightInformation.companyId.operatingCarrier != "")
                                {
                                    operating_airline.code = leg_details.flightInformation.companyId.operatingCarrier;
                                    operating_airline.name = leg_details.flightInformation.companyId.operatingCarrier;
                                    operating_airline.number = leg_details.flightInformation.flightOrtrainNumber;
                                    var airline_obj = airlinefilelist.Find(x => x.AirlineCode == leg_details.flightInformation.companyId.operatingCarrier);
                                    if (airline_obj != null)
                                    {
                                        operating_airline.name = airline_obj.AirlineName; //leg_details.flightInformation.companyId.operatingCarrier;
                                    }
                                }
                                #endregion;

                                string CabinClassText = "";
                                string CabinClassCode = "";
                                string RBD = "";

                                List<string> RBDLIST = new List<string>();

                                #region Baggage

                                Models.Flight.ResultResponse.pax_fareDetailsBySegment baggage_segement_obj = pax_fareDetailsBySegment.Find(x => x.segmentid == leg_index.ToString());
                                if (baggage_segement_obj != null)
                                {

                                    foreach (Models.Flight.ResultResponse.segment_pax_detail segment_pax_detail in baggage_segement_obj.segment_pax_detail)
                                    {
                                        //if (RBD == "")
                                        //{
                                        //    RBD = segment_pax_detail.class_code;
                                        //}

                                        //if (RBDLIST.FindAll(x => x.ToString() == segment_pax_detail.class_code).Count == 0)
                                        //{
                                        //    RBDLIST.Add(segment_pax_detail.class_code);
                                        //}
                                        //if (CabinClassText == "")
                                        //{
                                        //    CabinClassText = segment_pax_detail.cabin;
                                        //}

                                        var bagdata = Array.Find(Itinerary.referencingDetail, item => item.refQualifier == "B");

                                        var serviceFeesGrp = Array.Find(result.Body.Fare_MasterPricerTravelBoardSearchReply.serviceFeesGrp, x => x.freeBagAllowanceGrp != null);

                                        var bagageobjdata = Array.Find(serviceFeesGrp.freeBagAllowanceGrp, x => x.itemNumberInfo[0].number == bagdata.refNumber);

                                        if (bagageobjdata != null && bagageobjdata.freeBagAllownceInfo != null)
                                        {

                                            //var bagageobjdata = Array.Find(result.Body.Fare_MasterPricerTravelBoardSearchReply.serviceFeesGrp[0].freeBagAllowanceGrp, x => x.itemNumberInfo[0].number == bagdata.refNumber);

                                            if (bagageobjdata != null && bagageobjdata.freeBagAllownceInfo != null)
                                            {
                                                var baggagevalue = bagageobjdata.freeBagAllownceInfo.baggageDetails.freeAllowance + (bagageobjdata.freeBagAllownceInfo.baggageDetails.unitQualifier != null && bagageobjdata.freeBagAllownceInfo.baggageDetails.unitQualifier != "" ? bagageobjdata.freeBagAllownceInfo.baggageDetails.unitQualifier : "PC");

                                                if (baggagevalue == "0K")
                                                {
                                                    baggagevalue = "No Baggage";
                                                }
                                                Checkinbaggage.Add(new Models.Flight.ResultResponse.Checkinbaggage()
                                                {
                                                    Type = segment_pax_detail.paxtype + " " + segment_pax_detail.paxid,
                                                    Value = baggagevalue,
                                                });
                                            }
                                        }
                                        //if (segment_pax_detail.baggage != null && segment_pax_detail.baggage != "")
                                        //{
                                        //    Checkinbaggage.Add(new Models.Flight.Result.response.Checkinbaggage()
                                        //    {
                                        //        Type = segment_pax_detail.paxtype + " " + segment_pax_detail.paxid,
                                        //        Value = segment_pax_detail.baggage,
                                        //    });
                                        //}
                                    }
                                }
                                #endregion;

                                #region rbd class

                                var fareDetailsgroup_obj = fareDetailsgroup.Find(x => x.segmentid == index_out_in_bound);

                                if (fareDetailsgroup_obj != null)
                                {
                                    var gropfare_obj = fareDetailsgroup_obj.groupfare.Find(x => x.groupindex == leg_index);

                                    if (gropfare_obj != null)
                                    {
                                        if (RBD == "")
                                        {
                                            RBD = gropfare_obj.rbd;
                                        }
                                        if (CabinClassCode == "")
                                        {
                                            CabinClassCode = gropfare_obj.cabin;
                                            if (CabinClassCode.ToUpper() == "W")
                                            {
                                                CabinClassText = "Premium Economy";
                                            }
                                            else if (CabinClassCode.ToUpper() == "Y" || CabinClassCode.ToUpper() == "M")
                                            {
                                                CabinClassText = "Economy";
                                            }
                                            else if (CabinClassCode.ToUpper() == "C")
                                            {
                                                CabinClassText = "Business";
                                            }
                                            else if (CabinClassCode.ToUpper() == "F")
                                            {
                                                CabinClassText = "First";
                                            }
                                        }

                                        if (RBDLIST.FindAll(x => x.ToString() == gropfare_obj.rbd).Count == 0)
                                        {
                                            RBDLIST.Add(gropfare_obj.rbd);
                                        }
                                    }
                                }
                                #endregion

                                TimeSpan ts = arrival_datetime.Subtract(departure_datetime);
                                string connectiontime = "";
                                if (previous_leg_datetime == "")
                                {
                                    previous_leg_datetime = arrival_datetime.ToString();
                                }
                                else
                                {
                                    TimeSpan Ts = (departure_datetime - Convert.ToDateTime(previous_leg_datetime));
                                    connectiontime = Convert.ToInt32(Ts.TotalMinutes).ToString();
                                    previous_leg_datetime = arrival_datetime.ToString();

                                    totalminutes = totalminutes + Convert.ToInt32(connectiontime);
                                }

                                totalminutes = totalminutes + Convert.ToInt32(ts.TotalMinutes);

                                string availabilityCnxType_slice_dice = "";

                                if (Slice_dice_value != null && Slice_dice_value.Count > 0)
                                {
                                    var availabilityCnxType_slice_dice_find = Slice_dice_value.Find(x => x.segment_ref == (OutboundInbounddata.Count + 1).ToString() && x.leg_ref == (flightlist.Count + 1).ToString());
                                    if (availabilityCnxType_slice_dice_find != null)
                                    {
                                        availabilityCnxType_slice_dice = availabilityCnxType_slice_dice_find.code;
                                    }
                                }

                                #region technical stop

                                List<Models.Flight.ResultResponse.technicalStop> technicalStop = new List<Models.Flight.ResultResponse.technicalStop>();

                                if (leg_details.technicalStop != null && leg_details.technicalStop.Length > 0)
                                {
                                    foreach (var technical in leg_details.technicalStop)
                                    {
                                        int stop_totalminutes = 0;
                                        string stop_previous_leg_datetime = "";

                                        DateTime depstoptime = new DateTime();
                                        DateTime arrstoptime = new DateTime();

                                        List<Models.Flight.ResultResponse.stopDetails> stopDetails = new List<Models.Flight.ResultResponse.stopDetails>();
                                        if (technical.stopDetails != null && technical.stopDetails.Length > 0)
                                        {

                                            int stopindex = 0;
                                            foreach (var stop in technical.stopDetails)
                                            {
                                                #region stop date time
                                                string stoptime = "";
                                                string stopdate = "";
                                                string stopdatetime = "";
                                                string location_city = "";
                                                string location_Iata = "";
                                                string location_name = "";


                                                if (stop.date != null && stop.date != "" && stop.firstTime != null && stop.firstTime != "")
                                                {
                                                    if (stopindex == 0)
                                                    {
                                                        depstoptime = DateTime.ParseExact(stop.date + " " + stop.firstTime, "ddMMyy HHmm", CultureInfo.InvariantCulture);

                                                    }
                                                    else
                                                    {
                                                        arrstoptime = DateTime.ParseExact(stop.date + " " + stop.firstTime, "ddMMyy HHmm", CultureInfo.InvariantCulture);


                                                    }
                                                    DateTime stop_date_time = DateTime.ParseExact(stop.date + " " + stop.firstTime, "ddMMyy HHmm", CultureInfo.InvariantCulture);
                                                    stoptime = stop_date_time.ToString("HH:mm");
                                                    stopdate = stop_date_time.ToString("dd-MM-yyyy");
                                                    stopdatetime = stop_date_time.ToString();
                                                }
                                                if (stop.locationId != null && stop.locationId != "")
                                                {
                                                    //var location_airport_obj = airportfilelist.Find(x => x.AirportCode == stop.locationId);
                                                    //if (departure_airport_obj != null)
                                                    //{
                                                    location_city = stop.locationId;
                                                    location_name = stop.locationId;
                                                    location_Iata = stop.locationId;
                                                    //}
                                                }

                                                #endregion;

                                                stopDetails.Add(new Models.Flight.ResultResponse.stopDetails()
                                                {
                                                    date = stop.date == null || stop.date == "" ? "" : stopdate,
                                                    dateTime = stopdatetime,
                                                    dateQualifier = stop.dateQualifier == null || stop.dateQualifier == "" ? "" : stop.dateQualifier,
                                                    equipementType = stop.equipementType == null || stop.equipementType == "" ? "" : stop.equipementType,
                                                    firstTime = stop.firstTime == null || stop.firstTime == "" ? "" : stoptime,
                                                    locationId = stop.locationId == null || stop.locationId == "" ? "" : stop.locationId,
                                                    location_city = location_city,
                                                    location_name = location_name,
                                                });
                                                stopindex++;
                                            }
                                        }

                                        TimeSpan tss = arrstoptime.Subtract(depstoptime);
                                        string stopconnectiontime = "";
                                        if (stop_previous_leg_datetime == "")
                                        {
                                            stop_previous_leg_datetime = arrstoptime.ToString();
                                        }
                                        else
                                        {
                                            TimeSpan Ts = (depstoptime - Convert.ToDateTime(stop_previous_leg_datetime));
                                            stopconnectiontime = Convert.ToInt32(Ts.TotalMinutes).ToString();
                                            stop_previous_leg_datetime = arrstoptime.ToString();
                                        }

                                        stop_totalminutes = stop_totalminutes + Convert.ToInt32(tss.TotalMinutes);

                                        technicalStop.Add(new Models.Flight.ResultResponse.technicalStop()
                                        {
                                            stopDetails = stopDetails,
                                            stopconnectiontime = stopconnectiontime,
                                            stop_totalminutes = stop_totalminutes,
                                        });
                                    }
                                }

                                #endregion

                                flight_uniqueid += "|" + Departure.Iata + "-" + Convert.ToDateTime(Departure.Datetime).ToString("dd/MM/yyyy-HH:mm") + "-" + operating_airline.code + "-" + operating_airline.number;

                                flightlist.Add(new Models.Flight.ResultResponse.flightlist()
                                {
                                    Aircraft = Aircraft,
                                    Arrival = Arrival,
                                    Departure = Departure,
                                    CheckinBaggage = Checkinbaggage,
                                    CabinClassText = CabinClassText,
                                    CabinClassCode = CabinClassCode,
                                    FlightMinutes = Convert.ToInt32(ts.TotalMinutes).ToString(),
                                    FlightTime = ts.ToString(@"hh\:mm"),
                                    MarketingAirline = marketing_airline,
                                    OperatingAirline = operating_airline,
                                    connectiontime = connectiontime,
                                    RBD = RBD,
                                    RBDLIST = RBDLIST,
                                    availabilityCnxType_slice_dice = availabilityCnxType_slice_dice,
                                    technicalStop = technicalStop,
                                });

                                leg_index++;
                            }

                            totaltime = totaltime + totalminutes;

                            OutboundInbounddata.Add(new Models.Flight.ResultResponse.OutboundInbounddata()
                            {
                                flightlist = flightlist,
                                totaltime = totalminutes.ToString(),
                            });

                            index_out_in_bound++;
                        }

                        #region Filter Bind
                        List<string> Inbound_airport = new List<string>();
                        List<string> Outbound_airport = new List<string>();

                        #region outbound filter

                        //stops filter
                        //if (OutboundInbounddata[0].flightlist.Count == 1)
                        //{
                        //    if (Outboundstops.direct == null)
                        //    {
                        //        Outboundstops.direct = totalprice;
                        //    }
                        //    else if (Convert.ToDouble(Outboundstops.direct) > totalprice)
                        //    {
                        //        Outboundstops.direct = totalprice;
                        //    }
                        //}
                        //else if (OutboundInbounddata[0].flightlist.Count == 2)
                        //{
                        //    if (Outboundstops.onestop == null)
                        //    {
                        //        Outboundstops.onestop = totalprice;
                        //    }
                        //    else if (Convert.ToDouble(Outboundstops.onestop) > totalprice)
                        //    {
                        //        Outboundstops.onestop = totalprice;
                        //    }
                        //}
                        //else if (OutboundInbounddata[0].flightlist.Count > 2)
                        //{
                        //    if (Outboundstops.morethanonestop == null)
                        //    {
                        //        Outboundstops.morethanonestop = totalprice;
                        //    }
                        //    else if (Convert.ToDouble(Outboundstops.morethanonestop) > totalprice)
                        //    {
                        //        Outboundstops.morethanonestop = totalprice;
                        //    }
                        //}
                        ////stops filter

                        ////duration filter
                        //if (Outboundstops.journeymin == null)
                        //{
                        //    Outboundstops.journeymin = Convert.ToInt32(OutboundInbounddata[0].totaltime);
                        //}
                        //else if (Convert.ToInt16(Outboundstops.journeymin) > Convert.ToInt16(OutboundInbounddata[0].totaltime))
                        //{
                        //    Outboundstops.journeymin = Convert.ToInt32(OutboundInbounddata[0].totaltime);
                        //}

                        //if (Outboundstops.journeymax == null)
                        //{
                        //    Outboundstops.journeymax = Convert.ToInt32(OutboundInbounddata[0].totaltime);
                        //}
                        //else if (Convert.ToInt16(Outboundstops.journeymax) < Convert.ToInt16(OutboundInbounddata[0].totaltime))
                        //{
                        //    Outboundstops.journeymax = Convert.ToInt32(OutboundInbounddata[0].totaltime);
                        //}
                        ////duration filter

                        //// airport Filter
                        //if (Outbound_Destination_Filter.IndexOf(OutboundInbounddata[0].flightlist[0].Departure.Iata) < 0)
                        //{
                        //    Outbound_Destination_Filter.Add(OutboundInbounddata[0].flightlist[0].Departure.Iata);
                        //}

                        //if (Inbound_Destination_Filter.IndexOf(OutboundInbounddata[0].flightlist[OutboundInbounddata[0].flightlist.Count - 1].Arrival.Iata) < 0)
                        //{
                        //    Inbound_Destination_Filter.Add(OutboundInbounddata[0].flightlist[OutboundInbounddata[0].flightlist.Count - 1].Arrival.Iata);
                        //}

                        //if (Outbound_airport.IndexOf(OutboundInbounddata[0].flightlist[0].Departure.Iata) < 0)
                        //{
                        //    Outbound_airport.Add(OutboundInbounddata[0].flightlist[0].Departure.Iata);
                        //}

                        //if (Inbound_airport.IndexOf(OutboundInbounddata[0].flightlist[OutboundInbounddata[0].flightlist.Count - 1].Arrival.Iata) < 0)
                        //{
                        //    Inbound_airport.Add(OutboundInbounddata[0].flightlist[OutboundInbounddata[0].flightlist.Count - 1].Arrival.Iata);
                        //}


                        // airport Filter


                        #endregion;

                        #region Inbound filter
                        //if (OutboundInbounddata.Count > 1)
                        //{
                        //    //stops filter
                        //    if (OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist.Count == 1)
                        //    {
                        //        if (Inboundstops.direct == null)
                        //        {
                        //            Inboundstops.direct = totalprice;
                        //        }
                        //        else if (Convert.ToDouble(Inboundstops.direct) > totalprice)
                        //        {
                        //            Inboundstops.direct = totalprice;
                        //        }
                        //    }
                        //    else if (OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist.Count == 2)
                        //    {
                        //        if (Inboundstops.onestop == null)
                        //        {
                        //            Inboundstops.onestop = totalprice;
                        //        }
                        //        else if (Convert.ToDouble(Inboundstops.onestop) > totalprice)
                        //        {
                        //            Inboundstops.onestop = totalprice;
                        //        }
                        //    }
                        //    else if (OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist.Count > 2)
                        //    {
                        //        if (Inboundstops.morethanonestop == null)
                        //        {
                        //            Inboundstops.morethanonestop = totalprice;
                        //        }
                        //        else if (Convert.ToDouble(Inboundstops.morethanonestop) > totalprice)
                        //        {
                        //            Inboundstops.morethanonestop = totalprice;
                        //        }
                        //    }
                        //    //stops filter

                        //    //duration filter
                        //    if (Inboundstops.journeymin == null)
                        //    {
                        //        Inboundstops.journeymin = Convert.ToInt32(OutboundInbounddata[OutboundInbounddata.Count - 1].totaltime);
                        //    }
                        //    else if (Convert.ToInt16(Inboundstops.journeymin) > Convert.ToInt32(OutboundInbounddata[OutboundInbounddata.Count - 1].totaltime))
                        //    {
                        //        Inboundstops.journeymin = Convert.ToInt32(OutboundInbounddata[OutboundInbounddata.Count - 1].totaltime);
                        //    }

                        //    if (Inboundstops.journeymax == null)
                        //    {
                        //        Inboundstops.journeymax = Convert.ToInt32(OutboundInbounddata[OutboundInbounddata.Count - 1].totaltime);
                        //    }
                        //    else if (Convert.ToInt16(Inboundstops.journeymax) < Convert.ToInt16(OutboundInbounddata[OutboundInbounddata.Count - 1].totaltime))
                        //    {
                        //        Inboundstops.journeymax = Convert.ToInt32(OutboundInbounddata[OutboundInbounddata.Count - 1].totaltime);
                        //    }
                        //    //duration filter


                        //    // airport Filter
                        //    if (Inbound_Destination_Filter.IndexOf(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[0].Departure.Iata) < 0)
                        //    {
                        //        Inbound_Destination_Filter.Add(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[0].Departure.Iata);
                        //    }

                        //    if (Outbound_Destination_Filter.IndexOf(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist.Count - 1].Arrival.Iata) < 0)
                        //    {
                        //        Outbound_Destination_Filter.Add(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist.Count - 1].Arrival.Iata);
                        //    }


                        //    if (Inbound_airport.IndexOf(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[0].Departure.Iata) < 0)
                        //    {
                        //        Inbound_airport.Add(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[0].Departure.Iata);
                        //    }

                        //    if (Outbound_airport.IndexOf(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist.Count - 1].Arrival.Iata) < 0)
                        //    {
                        //        Outbound_airport.Add(OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist[OutboundInbounddata[OutboundInbounddata.Count - 1].flightlist.Count - 1].Arrival.Iata);
                        //    }

                        //}

                        #endregion;

                        #endregion;

                        double net_totalprice = baseprice;
                        //double price_total = totalprice * currency_exchangerate;
                        double price_total = totalprice;

                        //string guid = System.Guid.NewGuid().ToString() + System.Guid.NewGuid().ToString() + result_index;
                        string guid = System.Guid.NewGuid().ToString() + result_index;
                        //   Itinerary.refrence = guid;
                        flight_uniqueids.Add(flight_uniqueid);

                        #region Matrix Airline Filter
                        //if (airlinelists.Count == 1)
                        //{
                        //    var flight_matric = FlightFilter_price.Find(x => x.Code == airlinelists[0]);
                        //    if (flight_matric == null)
                        //    {
                        //        double airline_matric_price_direct = 0;
                        //        double airline_matric_price_stop = 0;
                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count == 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count == 1 && OutboundInbounddata[1].flightlist.Count == 1))
                        //        {
                        //            airline_matric_price_direct = net_totalprice;
                        //        }

                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count != 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count != 1 && OutboundInbounddata[1].flightlist.Count != 1))
                        //        {
                        //            airline_matric_price_stop = net_totalprice;
                        //        }

                        //        var airline_obj = airlinefilelist.Find(x => x.Airlinecode == airlinelists[0]);
                        //        FlightFilter_price.Add(new Models.Flight.Result.response.FlightFilter_price()
                        //        {
                        //            Code = airlinelists[0],
                        //            Value = airline_obj.Airlinename,
                        //            direct = airline_matric_price_direct,
                        //            stops = airline_matric_price_stop,
                        //        });
                        //    }
                        //    else
                        //    {
                        //        double airline_matric_price_direct = flight_matric.direct;
                        //        double airline_matric_price_stop = flight_matric.stops;
                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count == 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count == 1 && OutboundInbounddata[1].flightlist.Count == 1))
                        //        {
                        //            airline_matric_price_direct = net_totalprice;
                        //        }

                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count != 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count != 1 && OutboundInbounddata[1].flightlist.Count != 1))
                        //        {
                        //            airline_matric_price_stop = net_totalprice;
                        //        }


                        //        flight_matric.direct = (flight_matric.direct == 0 || flight_matric.direct > airline_matric_price_direct) ? airline_matric_price_direct : flight_matric.direct;

                        //        flight_matric.stops = (flight_matric.stops == 0 || flight_matric.stops > airline_matric_price_stop) ? airline_matric_price_stop : flight_matric.stops;


                        //    }
                        //}

                        //List<string> distinct_airline = airlinelists.Distinct().ToList();
                        //foreach (var airline_code in distinct_airline)
                        //{
                        //    var flight_matric = FlightFilter_price.Find(x => x.Code == airline_code);
                        //    if (flight_matric == null)
                        //    {
                        //        double airline_matric_price_direct = 0;
                        //        double airline_matric_price_stop = 0;
                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count == 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count == 1 && OutboundInbounddata[1].flightlist.Count == 1))
                        //        {
                        //            airline_matric_price_direct = net_totalprice;
                        //        }

                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count != 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count != 1 && OutboundInbounddata[1].flightlist.Count != 1))
                        //        {
                        //            airline_matric_price_stop = net_totalprice;
                        //        }

                        //        var airline_obj = airlinefilelist.Find(x => x.Airlinecode == airline_code);

                        //        string airlinename = airline_code;
                        //        if (airline_obj != null)
                        //        {
                        //            airlinename = airline_obj.Airlinename;
                        //        }

                        //        FlightFilter_price.Add(new Models.Flight.Result.response.FlightFilter_price()
                        //        {
                        //            Code = airline_code,
                        //            Value = airlinename,
                        //            direct = airline_matric_price_direct,
                        //            stops = airline_matric_price_stop,
                        //        });
                        //    }
                        //    else
                        //    {
                        //        double airline_matric_price_direct = flight_matric.direct;
                        //        double airline_matric_price_stop = flight_matric.stops;
                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count == 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count == 1 && OutboundInbounddata[1].flightlist.Count == 1))
                        //        {
                        //            airline_matric_price_direct = net_totalprice;
                        //        }

                        //        if ((OutboundInbounddata.Count == 1 && OutboundInbounddata[0].flightlist.Count != 1) || (OutboundInbounddata.Count == 2 && OutboundInbounddata[0].flightlist.Count != 1 && OutboundInbounddata[1].flightlist.Count != 1))
                        //        {
                        //            airline_matric_price_stop = net_totalprice;
                        //        }


                        //        flight_matric.direct = (flight_matric.direct == 0 || flight_matric.direct > airline_matric_price_direct) ? airline_matric_price_direct : flight_matric.direct;

                        //        flight_matric.stops = (flight_matric.stops == 0 || flight_matric.stops > airline_matric_price_stop) ? airline_matric_price_stop : flight_matric.stops;


                        //    }
                        //}
                        #endregion;

                        string faretype = "";
                        string faretypename = "";

                        List<Models.Flight.ResultResponse.familylistdata> farefamilylistdata = new List<Models.Flight.ResultResponse.familylistdata>();

                        if (recommendation.fareFamilyRef != null && Array.FindAll(recommendation.fareFamilyRef, x => x.refQualifier == "F").Length > 0)
                        {
                            string refNumber = Array.Find(recommendation.fareFamilyRef, x => x.refQualifier == "F").refNumber;


                            string farematchno = Array.Find(recommendation.fareFamilyRef, x => x.refQualifier != "F") != null ? Array.Find(recommendation.fareFamilyRef, x => x.refQualifier != "F").refNumber : "";

                            if (farematchno == null || farematchno == "")
                            {
                                farematchno = "";
                            }

                            var faretypeobj = Array.Find(result.Body.Fare_MasterPricerTravelBoardSearchReply.familyInformation, x => x.refNumber == refNumber);

                            if (faretypeobj == null)
                            {
                                goto fareend;
                            }
                            faretype = faretypeobj.fareFamilyname;
                            faretypename = faretypeobj.description;

                            string carrier = faretypeobj.carrier;

                            var familylist = Array.FindAll(result.Body.Fare_MasterPricerTravelBoardSearchReply.familyInformation, x => x.carrier == carrier);

                            if (familylist != null && familylist.Length > 0)
                            {
                                //  faretypeobj.services[0].reference
                                //var matchdata = Array.FindAll(familylist, x => x.services != null && x.services.Length > 0 && Array.FindAll(x.services, y => y.reference == farematchno).Length > 0);

                                int matchCount = 0;
                                foreach (var otherServices in familylist)
                                {
                                    if (faretypeobj.services != null && faretypeobj.services.Length > 0)
                                    {
                                        foreach (var ref1Service in faretypeobj.services)
                                        {
                                            if (otherServices.services != null && otherServices.services.Length > 0)
                                            {
                                                foreach (var otherService in otherServices.services)
                                                {
                                                    if (ref1Service.reference == otherService.reference &&
                            ref1Service.status == otherService.status)
                                                    {

                                                        if (farefamilylistdata.FindAll(x => x.code.ToString() == otherServices.fareFamilyname).Count == 0)
                                                        {
                                                            farefamilylistdata.Add(new Models.Flight.ResultResponse.familylistdata()
                                                            {
                                                                code = otherServices.fareFamilyname,
                                                                name = otherServices.description,
                                                            });
                                                        }
                                                        matchCount++;
                                                    }
                                                }
                                            }

                                        }
                                    }

                                }

                                //if (matchdata.Length > 0)
                                //{
                                //    foreach (var match in matchdata)
                                //    {
                                //        farefamilylistdata.Add(match.fareFamilyname);
                                //    }
                                //}
                            }
                        fareend:;
                        }

                        Models.Flight.ResultResponse.price price = new Models.Flight.ResultResponse.price();

                        price.currency = currency;
                        price.currency_sign = currency_sign;
                        price.base_price = net_totalprice;
                        price.total_price = price_total;
                        price.tax_price = Convert.ToDouble((taxprice * currency_exchangerate).ToString("#0.00"));
                        price.tarriffinfo = tarriffinfo;


                        Flightdata.Add(new Models.Flight.ResultResponse.Flightdata()
                        {
                            //Airlinelists = airlinelists,
                            //Arrivalcityairports = Inbound_airport,
                            //Departurecityairports = Outbound_airport,
                            faretype = faretype,
                            faretypename = faretypename,
                            id = guid,
                            price = price,
                            fareindex = 0,
                            isRefundable = isRefundable,
                            pcc = _configuration["FlightSettings:AirOfficeID"],
                            Offercode = guid,
                            unique_id = flight_uniqueid,
                            searchindex = 0,
                            api_response_ref = recommendation.itemNumber.itemNumberId.number,
                            // refundable = 1,
                            supplier = (int)SupplierEnum.Amadeus,
                            totaltime = totaltime,
                            OutboundInboundlist = OutboundInbounddata,
                            farefamilylistdata = farefamilylistdata
                        });
                        result_index++;
                    }
                }
                #endregion;


                List<string> flight_unique_ids = new List<string>();
                List<com.ThirdPartyAPIs.Models.Flight.ResultResponse.Flightdata> Flight_Data_new = new List<Models.Flight.ResultResponse.Flightdata>();

                flight_unique_ids = flight_uniqueids.Distinct().ToList();
                Flightdata = Flightdata.OrderBy(x => x.price.total_price).ToList();

                foreach (var flight_unique_id in flight_unique_ids)
                {
                    var flight_find = Flightdata.FindAll(x => x.unique_id == flight_unique_id);

                    if (flight_find.Count > 1)
                    {
                        string asasas = "";
                    }
                    // Flight_Data_new.AddRange(flight_find);

                    var flight_find_obj = flight_find.OrderBy(x => x.searchindex).FirstOrDefault();
                    Flight_Data_new.Add(flight_find_obj);
                }

                //List<com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightFilter> Airline_Filter_new = new List<com.ThirdPartyAPIs..Models.Flight.Result.response.FlightFilter>();
                //List< com.ThirdPartyAPIs..Models.Flight.Result.response.AirPortInfo> Airport_Filter_new = new List<com.ThirdPartyAPIs..Models.Flight.Result.response.AirPortInfo>();

                //List<com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightFilter_price> FlightFilter_price_new = new List<com.ThirdPartyAPIs..Models.Flight.ResultResponse.FlightFilter_price>();

                List<string> Outbound_Destination_Filter_new = new List<string>();
                List<string> Inbound_Destination_Filter_new = new List<string>();
                //com.ThirdPartyAPIs.Models.Flight.ResultResponse.stopsdata Outboundstops_new = new com.ThirdPartyAPIs.Models.Flight.ResultResponse.stopsdata();
                //com.ThirdPartyAPIs.Models.Flight.ResultResponse.stopsdata Inboundstops_new = new com.ThirdPartyAPIs.Models.Flight.ResultResponse.stopsdata();

                //com.ThirdPartyAPIs.Models.Flight.Result.response.flexDate flexDate = new Models.Flight.Result.response.flexDate();

                //List<com.ThirdPartyAPIs.Models.Flight.Result.response.flexdata> flexdata = new List<Models.Flight.Result.response.flexdata>();

                com.ThirdPartyAPIs.Models.Flight.ResultResponse.FlightResponse newdata = new Models.Flight.ResultResponse.FlightResponse();

                //newdata.flight_unique_ids = flight_unique_ids;
                newdata.totalflight = Flight_Data_new.Count;
                newdata.Currency = currency;
                newdata.Currency = currency_sign;
                newdata.Data = Flight_Data_new;
                newdata.sc = SC;
                newdata.totaladult = data.adults;
                newdata.totalchild = data.children;
                newdata.totalinfant = data.infant;
                newdata.success = true;
                //flightdatamain.Data = newdata;
                flightdatamain = newdata;

                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    //Directory.CreateDirectory(xmlfiles);
                    //File.WriteAllText(Path.Combine(xmlfiles, Token + "_Flight_amadeus_request.xml"), requestContent, Encoding.UTF8);
                    //File.WriteAllText(Path.Combine(xmlfiles, "_Flight_amadeus_response.xml"), re ?? string.Empty, Encoding.UTF8);

                    System.IO.File.WriteAllText(xmlfiles + SC + "flightavailable_secret.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);

                    //System.IO.File.WriteAllText(xmlfiles + Token + "_flight_result_Amadeus.json", Newtonsoft.Json.JsonConvert.SerializeObject(flightdatamain), System.Text.ASCIIEncoding.UTF8);

                    System.IO.File.WriteAllText(xmlfiles + SC + "_flight_result_Amadeus.json", System.Text.Json.JsonSerializer.Serialize(flightdatamain), System.Text.ASCIIEncoding.UTF8);


                    System.IO.File.WriteAllText(xmlfiles + SC + "_flight_result.json", System.Text.Json.JsonSerializer.Serialize(flightdatamain), System.Text.ASCIIEncoding.UTF8);
                }

                #endregion

                #endregion

                return flightdatamain;
            }
            catch (Exception ex)
            {
                //SaveAmadeusLog("", "", ex.Message + " /n error Fare_MasterPricerTravelBoardSearch");
                //return [.. concurrentPricing];
                _errorLogRepository.AddErrorLog(ex, "AmadeusConfig->Result|~|" + SC + "", JsonConvert.SerializeObject(data));
                return flightdatamain;
            }

        }

        #endregion Result

        #region CalendarPrice
        public async Task<com.ThirdPartyAPIs.Models.Flight.CalendarPrice.calendarPriceResponse> CalendarPrice(com.ThirdPartyAPIs.Models.Flight.SearchRequest.RootObject data, string SC, UserDetailDTO userdetail)
        {
            //ConcurrentBag<FlightPriceData> concurrentPricing = [];
            var calendarResponse = new Models.Flight.CalendarPrice.calendarPriceResponse();

            try
            {
                #region Request

                string password = "U9MbJZjzR^EP";
                //string searchkey = GetSearchKey(data);
                //CBTFlightBooking.Models.Flight.cachedresponse.Rootobject casheresponse = new Models.Flight.cachedresponse.Rootobject();

                //casheresponse = GetCachedResponse(searchkey);

                //if (!string.IsNullOrEmpty(casheresponse?.response))
                //{
                //    lstPricing = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FlightPriceData>>(casheresponse.response);
                //    return lstPricing;
                //}


                var url = _configuration["FlightSettings:AirProductionURL"];
                Guid Messageguid = Guid.NewGuid();
                string guidString = Messageguid.ToString();
                byte[] nonce = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(nonce);
                }
                DateTime timestamp = DateTime.UtcNow;
                string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
                string encodedNonce = Convert.ToBase64String(nonce);
                string passSHA = "";
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                    byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                    byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                    Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                    Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                    Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                    byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                    passSHA = Convert.ToBase64String(passSHABytes);
                }
                StringBuilder sb = new();
                sb.Append("<Fare_MasterPricerCalendar>");
                sb.Append("<numberOfUnit>");
                sb.Append("<unitNumberDetail>");
                sb.Append("<numberOfUnits>" + _configuration["FlightSettings:NoOfCombinations"] + "</numberOfUnits>");
                //sb.Append("<typeOfUnit>R</typeOfUnit>");
                sb.Append("<typeOfUnit>RC</typeOfUnit>");
                sb.Append("</unitNumberDetail>");
                sb.Append("<unitNumberDetail>");
                sb.Append("<numberOfUnits>" + (data.adults + data.children) + "</numberOfUnits>");
                sb.Append("<typeOfUnit>PX</typeOfUnit>");
                sb.Append("</unitNumberDetail>");
                sb.Append("</numberOfUnit>");
                sb.Append("<paxReference>");
                sb.Append("<ptc>IIT</ptc>");
                sb.Append("<ptc>ADT</ptc>");
                for (int a = 0; a < data.adults; a++)
                {
                    sb.Append("<traveller>");
                    sb.Append("<ref>" + (a + 1) + "</ref>");
                    sb.Append("</traveller>");
                }
                sb.Append("</paxReference>");
                if (data.children > 0)
                {
                    sb.Append("<paxReference>");
                    sb.Append("<ptc>CNN</ptc>");
                    sb.Append("<ptc>INN</ptc>");
                }
                for (int c = data.adults; c < (data.adults + data.children); c++)
                {
                    sb.Append("<traveller>");
                    sb.Append("<ref>" + (c + 1) + "</ref>");
                    sb.Append("</traveller>");
                }
                if (data.children > 0)
                    sb.Append("</paxReference>");
                if (data.infant > 0)
                {
                    sb.Append("<paxReference>");
                    sb.Append("<ptc>ITF</ptc>");
                    sb.Append("<ptc>IN</ptc>");
                }
                for (int c = 0; c < data.infant; c++)
                {
                    sb.Append("<traveller>");
                    sb.Append("<ref>" + (c + 1) + "</ref>");
                    sb.Append("<infantIndicator>1</infantIndicator>");
                    sb.Append("</traveller>");
                }
                if (data.infant > 0)
                    sb.Append("</paxReference>");
                sb.Append("<fareOptions>");
                sb.Append("<pricingTickInfo>");
                sb.Append("<pricingTicketing>");
                sb.Append("<priceType>ET</priceType>");
                sb.Append("<priceType>RP</priceType>");
                sb.Append("<priceType>RU</priceType>");
                sb.Append("<priceType>TAC</priceType>");
                sb.Append("<priceType>CUC</priceType>");

                //if (data.isRF == "1")
                //    sb.Append("<priceType>RF</priceType>");
                //if (data.isRF == "2")
                //    sb.Append("<priceType>NRE</priceType>");
                sb.Append("</pricingTicketing>");
                sb.Append("</pricingTickInfo>");
                sb.Append("<conversionRate>");
                sb.Append("<conversionRateDetail>");
                sb.Append("<currency>" + APIcurrency + "</currency>");
                sb.Append("</conversionRateDetail>");
                sb.Append("</conversionRate>");
                sb.Append("</fareOptions>");
                if (data.JourneyType != 3)
                {
                    for (int i = 1; i <= data.JourneyType; i++)
                    {
                        sb.Append("<itinerary>");
                        sb.Append("<requestedSegmentRef>");
                        sb.Append("<segRef>" + i + "</segRef>");
                        sb.Append("</requestedSegmentRef>");
                        sb.Append("<departureLocalization>");
                        sb.Append("<departurePoint>");
                        sb.Append("<locationId>" + data.segments[i - 1].depcode + "</locationId>");
                        sb.Append("</departurePoint>");
                        sb.Append("</departureLocalization>");
                        sb.Append("<arrivalLocalization>");
                        sb.Append("<arrivalPointDetails>");
                        sb.Append("<locationId>" + data.segments[i - 1].arrcode + "</locationId>");
                        sb.Append("</arrivalPointDetails>");
                        sb.Append("</arrivalLocalization>");
                        sb.Append("<timeDetails>");
                        sb.Append("<firstDateTimeDetail>");
                        sb.Append("<date>" + Convert.ToDateTime(data.segments[i - 1].depdate).ToString("ddMMyy") + "</date>");
                        sb.Append("</firstDateTimeDetail>");
                        sb.Append("<rangeOfDate><rangeQualifier>C</rangeQualifier><dayInterval>3</dayInterval></rangeOfDate>");
                        sb.Append("</timeDetails>");
                        sb.Append("</itinerary>");
                        //if (data.isRound == 2)
                        //    continue;
                        //else
                        //    break;
                    }
                }

                //if (data.JourneyType != 3)
                //{
                //    for (int i = 1; i <= data.JourneyType; i++)
                //    {
                //        sb.Append("<itinerary>");
                //        sb.Append("<requestedSegmentRef>");
                //        sb.Append("<segRef>" + i + "</segRef>");
                //        sb.Append("</requestedSegmentRef>");
                //        sb.Append("<departureLocalization>");
                //        sb.Append("<departurePoint>");
                //        sb.Append("<locationId>" + data.segments[i - 1].depcode + "</locationId>");
                //        sb.Append("</departurePoint>");
                //        sb.Append("</departureLocalization>");
                //        sb.Append("<arrivalLocalization>");
                //        sb.Append("<arrivalPointDetails>");
                //        sb.Append("<locationId>" + data.segments[i - 1].arrcode + "</locationId>");
                //        sb.Append("</arrivalPointDetails>");
                //        sb.Append("</arrivalLocalization>");
                //        sb.Append("<timeDetails>");
                //        sb.Append("<firstDateTimeDetail>");
                //        sb.Append("<date>" + Convert.ToDateTime(data.segments[i - 1].depdate).ToString("ddMMyy") + "</date>");
                //        sb.Append("</firstDateTimeDetail>");
                //        sb.Append("</timeDetails>");
                //        sb.Append("</itinerary>");
                //    }
                //}
                sb.Append("</Fare_MasterPricerCalendar>");

                var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                + "<s:Header>"
                + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID>"
                + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "FMPCAQ_20_2_1A</a:Action>"
                + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                + "<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">"
                + "<oas:UsernameToken xmlns:oas=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\""
                + " xmlns:oas1=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" oas1:Id=\"UsernameToken-1\">"
                + "<oas:Username>" + _configuration["FlightSettings:AirUserName"] + "</oas:Username>"
                + "<oas:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" + encodedNonce + "</oas:Nonce>"
                + "<oas:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">" + passSHA + "</oas:Password>"
                + "<oas1:Created>" + formattedTimestamp + "</oas1:Created>"
                + "</oas:UsernameToken>"
                + "</Security>"
                + "<h:AMA_SecurityHostedUser xmlns:h=\"http://xml.amadeus.com/2010/06/Security_v1\">"
                + "<h:UserID POS_Type=\"1\" PseudoCityCode=\"" + _configuration["FlightSettings:AirOfficeID"]
                + "\" AgentDutyCode=\"" + _configuration["FlightSettings:AirDutyCode"] + "\" RequestorType=\"U\"/>"
                + "</h:AMA_SecurityHostedUser>"
                + "</s:Header>"
                + "<s:Body>"
                + sb.ToString()
                + "</s:Body>"
                + "</s:Envelope>", null, "application/xml");

                #endregion Request

                #region Response

                var requestContent = content.ReadAsStringAsync().Result;

                string soapAction = _configuration["FlightSettings:AirSoapAction"] + "FMPCAQ_20_2_1A";
                var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);
                //var re = response.Content.ReadAsStringAsync().Result;

                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
                var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");
                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    if (!Directory.Exists(xmlfiles))
                        Directory.CreateDirectory(xmlfiles);
                    File.WriteAllText(Path.Combine(xmlfiles, SC + "Fare_MasterPricerCalendar_request.xml"), requestContent, Encoding.UTF8);
                    File.WriteAllText(Path.Combine(xmlfiles, SC + "_Fare_MasterPricerCalendar_response.xml"), re ?? string.Empty, Encoding.UTF8);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerCalendar_response.Envelope));
                StringReader rdr = new StringReader(re);
                com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerCalendar_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerCalendar_response.Envelope)serializer.Deserialize(rdr);

                //File.WriteAllText(xml_files + "Fare_MasterPricerTravelBoardSearch_response_" + sc + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);
                System.IO.File.WriteAllText(xmlfiles + SC + "_Fare_MasterPricerCalendar_response.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);

                // Parse the JSON string into a JObject
                //string jsonResponse = rdr.ToString();
                //JObject responseObject = JObject.Parse(jsonResponse);
                rdr.Close();

                if (result == null || result.Body == null || result.Body.Fare_MasterPricerCalendarReply == null || result.Body.Fare_MasterPricerCalendarReply.errorMessage != null || result.Body.Fare_MasterPricerCalendarReply.recommendation == null || result.Body.Fare_MasterPricerCalendarReply.recommendation.Length == 0)
                {
                    return calendarResponse;
                }
                calendarResponse.Origin = data.segments[0].depcode;
                calendarResponse.Destination = data.segments[0].arrcode;
                List<Models.Flight.CalendarPrice.Searchresult> searchResults = new List<CalendarPrice.Searchresult>();

                #region Currency
                double currency_exchangerate = 1;
                string currency = userdetail.Currency;
                string currency_sign = userdetail.CurrencySign;
                try
                {
                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer.Rootobject Rootobject_curency_list = new CurrencyExchange.currencyexchange_rate_fixer.Rootobject();
                    //var client = _httpClient.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer currency_exchange_rate_fixer = new CurrencyExchange.currencyexchange_rate_fixer(_configuration, _env, _genericRepository);

                    //string currency = currency_exchange_rate_fixer.Currency("");

                    Rootobject_curency_list = await currency_exchange_rate_fixer.get_exchage_rate();
                    currency_exchangerate = currency_exchange_rate_fixer.currency_exchange_rate(Rootobject_curency_list, currency, APIcurrency);
                    if (currency_exchangerate == 0)
                    {
                        currency = APIcurrency;
                        currency_sign = APIcurrency_sign;
                        currency_exchangerate = 1;
                    }
                }
                catch (Exception ex)
                {
                    currency = APIcurrency;
                    currency_sign = APIcurrency_sign;
                    currency_exchangerate = 1;
                }
                #endregion
                int result_index = 1;
                if (result.Body.Fare_MasterPricerCalendarReply.recommendation.Length > 0)
                {

                    var airlinefilelistfile = "";
                    using (var fileStream = new FileStream(importantfiles + "Airlinelist.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        airlinefilelistfile = streamReader.ReadToEnd();
                    }
                    List<AirlineListDTO> airlinefilelist = System.Text.Json.JsonSerializer.Deserialize<List<AirlineListDTO>>(airlinefilelistfile);

                    foreach (Amadeus_wsdl.Fare_MasterPricerCalendarReplyRecommendation recommendation in result.Body.Fare_MasterPricerCalendarReply.recommendation)
                    {
                        double totalprice = 0;
                        double baseprice = 0;
                        double taxprice = 0;
                        string DepartureDatetime = "";
                        string marketing_airline_code = "";
                        string marketing_airline_name = "";
                        string operating_airline_code = "";
                        string operating_airline_name = "";
                        foreach (Amadeus_wsdl.Fare_MasterPricerCalendarReplyRecommendationPaxFareProduct paxFareProduct in recommendation.paxFareProduct)
                        {
                            int quantity = paxFareProduct.paxReference[0].traveller.Length;

                            double api_total_price_per_leg = Convert.ToDouble(paxFareProduct.paxFareDetail.totalFareAmount) * quantity;
                            double api_tax_price_per_leg = Convert.ToDouble(paxFareProduct.paxFareDetail.totalTaxAmount) * quantity;
                            double api_base_price_per_leg = api_total_price_per_leg - api_tax_price_per_leg;

                            double net_total_price_per_leg = api_total_price_per_leg;
                            double net_tax_price_per_leg = api_tax_price_per_leg;
                            double net_base_price_per_leg = api_base_price_per_leg;


                            totalprice = totalprice + net_total_price_per_leg;
                            baseprice = baseprice + net_base_price_per_leg;
                            taxprice = taxprice + net_tax_price_per_leg;

                        }

                        foreach (Amadeus_wsdl.ReferenceInfoType18 Itinerary in recommendation.segmentFlightRef)
                        {
                            Amadeus_wsdl.ReferencingDetailsType_191583C[] Out_in_bound_list = Array.FindAll(Itinerary.referencingDetail, item => item.refQualifier == "S");

                            if (Out_in_bound_list == null || Out_in_bound_list.Length == 0)
                            {
                                continue;
                            }
                            int index_out_in_bound = 0;

                            foreach (Amadeus_wsdl.ReferencingDetailsType_191583C Out_in_bound in Out_in_bound_list)
                            {
                                int leg_index = 1;
                                List<Models.Flight.ResultResponse.flightlist> flightlist = new List<Models.Flight.ResultResponse.flightlist>();

                                Amadeus_wsdl.Fare_MasterPricerCalendarReplyFlightIndex Fare_MasterPricerTravelBoardSearchReplyFlightIndex = result.Body.Fare_MasterPricerCalendarReply.flightIndex[index_out_in_bound];

                                Amadeus_wsdl.Fare_MasterPricerCalendarReplyFlightIndexGroupOfFlights legs = Array.Find(Fare_MasterPricerTravelBoardSearchReplyFlightIndex.groupOfFlights, item => item.propFlightGrDetail.flightProposal[0].@ref == Out_in_bound.refNumber);


                                string previous_leg_datetime = "";
                                int totalminutes = 0;

                                foreach (Amadeus_wsdl.Fare_MasterPricerCalendarReplyFlightIndexGroupOfFlightsFlightDetails leg_details in legs.flightDetails)
                                {
                                    #region Departure

                                    DateTime departure_datetime = DateTime.ParseExact(leg_details.flightInformation.productDateTime.dateOfDeparture + " " + leg_details.flightInformation.productDateTime.timeOfDeparture, "ddMMyy HHmm", CultureInfo.InvariantCulture);

                                    //Departure.time = departure_datetime.ToString("HH:mm");
                                    //Departure.Date = departure_datetime.ToString("dd-MM-yyyy");
                                    DepartureDatetime = departure_datetime.ToString();

                                    #endregion

                                    #region Airline
                                    string Aircraft = "";
                                    if (leg_details.flightInformation.productDetail != null && leg_details.flightInformation.productDetail.equipmentType != null && leg_details.flightInformation.productDetail.equipmentType != "")
                                    {
                                        Aircraft = leg_details.flightInformation.productDetail.equipmentType;
                                    }

                                    if (leg_details.flightInformation.companyId.marketingCarrier != null && leg_details.flightInformation.companyId.marketingCarrier != "")
                                    {
                                        marketing_airline_code = leg_details.flightInformation.companyId.marketingCarrier;
                                        marketing_airline_name = leg_details.flightInformation.companyId.marketingCarrier;

                                        var airline_obj = airlinefilelist.Find(x => x.AirlineCode == leg_details.flightInformation.companyId.marketingCarrier);
                                        if (airline_obj != null)
                                        {
                                            marketing_airline_name = airline_obj.AirlineName;
                                        }
                                    }

                                    if (leg_details.flightInformation.companyId.operatingCarrier != null && leg_details.flightInformation.companyId.operatingCarrier != null && leg_details.flightInformation.companyId.operatingCarrier != "")
                                    {
                                        operating_airline_code = leg_details.flightInformation.companyId.operatingCarrier;
                                        operating_airline_name = leg_details.flightInformation.companyId.operatingCarrier;
                                        var airline_obj = airlinefilelist.Find(x => x.AirlineCode == leg_details.flightInformation.companyId.operatingCarrier);
                                        if (airline_obj != null)
                                        {
                                            operating_airline_name = airline_obj.AirlineName; //leg_details.flightInformation.companyId.operatingCarrier;
                                        }
                                    }
                                    #endregion;

                                    break;
                                }

                                break;
                            }
                        }

                        searchResults.Add(new Models.Flight.CalendarPrice.Searchresult()
                        {
                            //total_price = Convert.ToDouble(totalprice.ToString("#0.00")),
                            total_price = Convert.ToDouble((totalprice * currency_exchangerate).ToString("#0.00")),
                            base_price = Convert.ToDouble((baseprice * currency_exchangerate).ToString("#0.00")),
                            tax_price = Convert.ToDouble((taxprice * currency_exchangerate).ToString("#0.00")),
                            Currency = currency,
                            Currency_sign = currency_sign,
                            DepartureDatetime = !string.IsNullOrEmpty(DepartureDatetime) ? Convert.ToDateTime(DepartureDatetime) : null,
                            marketing_airline_code = marketing_airline_code,
                            marketing_airline_name = marketing_airline_name,
                            operating_airline_code = operating_airline_code,
                            operating_airline_name = operating_airline_name,
                        });
                    }
                }

                /*// Extract flight details for price and date
                var flightIndex = responseObject.SelectTokens("$.Body.Fare_MasterPricerCalendarReply.flightIndex[*].groupOfFlights[*].flightDetails[*].flightInformation.productDateTime.dateOfDeparture");
                var prices = responseObject.SelectTokens("$.Body.Fare_MasterPricerCalendarReply.recommendation[*].recPriceInfo[*].amount");

                // Loop through the flightIndex and prices to build the result list
                int p = 0;
                foreach (var date in flightIndex)
                {
                    // Get the date of departure (as string) and price
                    string dateOfDeparture = date.ToString();
                    if (prices != null && p < prices.Count()) 
                    {
                        //decimal price = prices[i].Value<decimal>(); // Assuming price is a decimal
                        decimal price = 0;
                        searchResults.Add(new CalendarPrice.Searchresult
                        {
                            DepartureDate = dateOfDeparture,
                            TotalPrice = price,
                            BasePrice = price
                        });
                    }
                    p++;
                }*/


                //var reply = result.Body.Fare_MasterPricerCalendarReply;

                //// Prices
                //decimal totalPrice = reply.recommendation[0].recPriceInfo[0].amount;
                //decimal basePrice = reply.recommendation[0].recPriceInfo[1].amount;

                //// Currency
                //string currency = reply.conversionRate[0].currency;

                //foreach (var group in reply.flightIndex[0].groupOfFlights)
                //{
                //    string rawDate =
                //        group.flightDetails[0]
                //             .flightInformation
                //             .productDateTime
                //             .dateOfDeparture;

                //    DateTime departureDate = DateTime.ParseExact(
                //        rawDate,
                //        "ddMMyy",
                //        CultureInfo.InvariantCulture
                //    );

                //    searchResults.Add(new CalendarPrice.Searchresult
                //    {
                //        DepartureDate = departureDate,
                //        BasePrice = basePrice,
                //        TotalPrice = totalPrice,
                //        Currency = currency
                //    });
                //}

                calendarResponse.SearchResults = searchResults;

                #endregion

                return calendarResponse;
            }
            catch (Exception ex)
            {
                //SaveAmadeusLog("", "", ex.Message + " /n error Fare_MasterPricerTravelBoardSearch");
                //return [.. concurrentPricing];
                _errorLogRepository.AddErrorLog(ex, "AmadeusConfig->CalendarPrice|~|" + SC + "", JsonConvert.SerializeObject(data));
                return calendarResponse;
            }

        }

        #endregion calendar price 

        #region Fare PriceUpsellWithoutPNR
        public async Task<List<FareOptionResopnse.FareUpsellResponse>> Fare_PriceUpsellWithoutPNR(ResultResponse.Flightdata Selected_flight_result, string sc, int totaladult, int totalchild, int totalinfant, UserDetailDTO userdetail)
        {
            List<FareOptionResopnse.FareUpsellResponse> lstdta = new List<FareUpsellResponse>();
            try
            {
                #region Request
                string password = "U9MbJZjzR^EP";

                var url = _configuration["FlightSettings:AirProductionURL"];
                Guid guid = Guid.NewGuid();
                string guidString = guid.ToString();
                byte[] nonce = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(nonce);
                }
                DateTime timestamp = DateTime.UtcNow;
                string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
                string encodedNonce = Convert.ToBase64String(nonce);
                string passSHA = "";
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                    byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                    byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                    Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                    Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                    Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                    byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                    passSHA = Convert.ToBase64String(passSHABytes);
                }
                StringBuilder dbps = new();
                dbps.Append("<passengersGroup>");
                dbps.Append("<segmentRepetitionControl>");
                dbps.Append("<segmentControlDetails>");
                dbps.Append("<quantity>1</quantity>");
                dbps.Append("<numberOfUnits>" + totaladult + "</numberOfUnits>");
                dbps.Append("</segmentControlDetails>");
                dbps.Append("</segmentRepetitionControl>");
                dbps.Append("<travellersID>");
                for (int i = 0; i < totaladult; i++)
                {
                    dbps.Append("<travellerDetails>");
                    dbps.Append("<measurementValue>" + (i + 1) + "</measurementValue>");
                    dbps.Append("</travellerDetails>");
                }
                dbps.Append("</travellersID>");
                dbps.Append("<discountPtc>");
                dbps.Append("<valueQualifier>ADT</valueQualifier>");
                dbps.Append("</discountPtc>");
                dbps.Append("</passengersGroup>");
                if (totalchild > 0)
                {
                    dbps.Append("<passengersGroup>");
                    dbps.Append("<segmentRepetitionControl>");
                    dbps.Append("<segmentControlDetails>");
                    dbps.Append("<quantity>2</quantity>");
                    dbps.Append("<numberOfUnits>" + totalchild + "</numberOfUnits>");
                    dbps.Append("</segmentControlDetails>");
                    dbps.Append("</segmentRepetitionControl>");
                    dbps.Append("<travellersID>");
                    for (int i = 0; i < totalchild; i++)
                    {
                        dbps.Append("<travellerDetails>");
                        dbps.Append("<measurementValue>" + (totaladult + i + 1) + "</measurementValue>");
                        dbps.Append("</travellerDetails>");
                    }
                    dbps.Append("</travellersID>");
                    dbps.Append("<discountPtc>");
                    dbps.Append("<valueQualifier>CNN</valueQualifier>");
                    dbps.Append("</discountPtc>");
                    dbps.Append("</passengersGroup>");
                }
                if (totalinfant > 0)
                {
                    dbps.Append("<passengersGroup>");
                    dbps.Append("<segmentRepetitionControl>");
                    dbps.Append("<segmentControlDetails>");
                    dbps.Append("<quantity>3</quantity>");
                    dbps.Append("<numberOfUnits>" + totalinfant + "</numberOfUnits>");
                    dbps.Append("</segmentControlDetails>");
                    dbps.Append("</segmentRepetitionControl>");
                    dbps.Append("<travellersID>");
                    for (int i = 0; i < totalinfant; i++)
                    {
                        dbps.Append("<travellerDetails>");
                        dbps.Append("<measurementValue>" + (i + 1) + "</measurementValue>");
                        dbps.Append("</travellerDetails>");
                    }
                    dbps.Append("</travellersID>");
                    dbps.Append("<discountPtc>");
                    dbps.Append("<valueQualifier>INF</valueQualifier>");
                    dbps.Append("<fareDetails>");
                    dbps.Append("<qualifier>766</qualifier>");
                    dbps.Append("</fareDetails>");
                    dbps.Append("</discountPtc>");
                    dbps.Append("</passengersGroup>");
                }

                StringBuilder sb = new();

                int flightIndicator = 1;
                int itemNumber = 1;
                int itemNumberindex = 0;
                string carrierInformation_otherCompany = "";

                foreach (var OutboundInbounddata in Selected_flight_result.OutboundInboundlist)
                {
                    string FlightIndicator_str = flightIndicator.ToString();

                    foreach (var flightlist in OutboundInbounddata.flightlist)
                    {
                        carrierInformation_otherCompany = flightlist.MarketingAirline.code;

                        string RBD = flightlist.RBD;

                        sb.AppendLine("  <segmentGroup><segmentInformation><flightDate><departureDate>" + Convert.ToDateTime(flightlist.Departure.Datetime).ToString("ddMMyy") + "</departureDate><departureTime>" + Convert.ToDateTime(flightlist.Departure.Datetime).ToString("HHmm") + "</departureTime><arrivalDate>" + Convert.ToDateTime(flightlist.Arrival.Datetime).ToString("ddMMyy") + "</arrivalDate><arrivalTime>" + Convert.ToDateTime(flightlist.Arrival.Datetime).ToString("HHmm") + "</arrivalTime></flightDate><boardPointDetails><trueLocationId>" + flightlist.Departure.Iata + "</trueLocationId></boardPointDetails><offpointDetails><trueLocationId>" + flightlist.Arrival.Iata + "</trueLocationId></offpointDetails><companyDetails><marketingCompany>" + flightlist.MarketingAirline.code + "</marketingCompany><operatingCompany>" + flightlist.OperatingAirline.code + "</operatingCompany></companyDetails><flightIdentification><flightNumber>" + flightlist.MarketingAirline.number + "</flightNumber><bookingClass>" + RBD + "</bookingClass></flightIdentification><flightTypeDetails><flightIndicator>" + FlightIndicator_str + "</flightIndicator></flightTypeDetails><itemNumber>" + itemNumber + "</itemNumber></segmentInformation></segmentGroup> ");
                        itemNumber++;
                    }
                    itemNumberindex++;
                    flightIndicator++;
                }

                //sb.Append("<segmentGroup>");
                //sb.Append("<segmentInformation>");
                //sb.Append("<flightDate>");
                //sb.Append("<departureDate>" + data.depDt.ToString("ddMMyy") + "</departureDate>");
                //sb.Append("<departureTime>" + data.departureTime.Replace(":", "") + "</departureTime>");
                //sb.Append("<arrivalDate>" + data.arrDt.ToString("ddMMyy") + "</arrivalDate>");
                //sb.Append("<arrivalTime>" + data.arrivalTime.Replace(":", "") + "</arrivalTime>");
                //sb.Append("</flightDate>");
                //sb.Append("<boardPointDetails>");
                //sb.Append("<trueLocationId>" + data.depCode.Split(',')[0] + "</trueLocationId>");
                //sb.Append("</boardPointDetails>");
                //sb.Append("<offpointDetails>");
                //sb.Append("<trueLocationId>" + data.arrCode.Split(',')[0] + "</trueLocationId>");
                //sb.Append("</offpointDetails>");
                //sb.Append("<companyDetails>");
                //sb.Append("<marketingCompany>" + data.marketingCompany.Split('|')[0] + "</marketingCompany>");
                //sb.Append("<operatingCompany></operatingCompany>");
                //sb.Append("</companyDetails>");
                //sb.Append("<flightIdentification>");
                //sb.Append("<flightNumber>" + data.flightNumber + "</flightNumber>");
                //sb.Append("<bookingClass>" + data.bookingClass + "</bookingClass>");
                //sb.Append("</flightIdentification>");
                //sb.Append("<flightTypeDetails>");
                //sb.Append("<flightIndicator>1</flightIndicator>");
                //sb.Append("</flightTypeDetails>");
                //sb.Append("<itemNumber>" + seg + "</itemNumber>");
                //sb.Append("</segmentInformation>");
                //sb.Append("</segmentGroup>");


                sb.Append("<pricingOptionGroup>");
                sb.Append("<pricingOptionKey>");
                sb.Append("<pricingOptionKey>VC</pricingOptionKey>");
                sb.Append("</pricingOptionKey>");
                sb.Append("<carrierInformation>");
                sb.Append("<companyIdentification>");
                sb.Append("<otherCompany>" + carrierInformation_otherCompany + "</otherCompany>");
                sb.Append("</companyIdentification>");
                sb.Append("</carrierInformation>");
                sb.Append("</pricingOptionGroup>");
                sb.Append("<pricingOptionGroup>");
                sb.Append("<pricingOptionKey>");
                sb.Append("<pricingOptionKey>RP</pricingOptionKey>");
                sb.Append("</pricingOptionKey>");
                sb.Append("</pricingOptionGroup>");
                sb.Append("<pricingOptionGroup>");
                sb.Append("<pricingOptionKey>");
                sb.Append("<pricingOptionKey>RU</pricingOptionKey>");
                sb.Append("</pricingOptionKey>");
                sb.Append("</pricingOptionGroup>");
                sb.Append("<pricingOptionGroup>");
                sb.Append("<pricingOptionKey>");
                sb.Append("<pricingOptionKey>RLO</pricingOptionKey>");
                sb.Append("</pricingOptionKey>");
                sb.Append("</pricingOptionGroup>");
                sb.Append("<pricingOptionGroup>");
                sb.Append("<pricingOptionKey>");
                sb.Append("<pricingOptionKey>FFH</pricingOptionKey>");
                sb.Append("</pricingOptionKey>");
                sb.Append("</pricingOptionGroup>");
                //sb.Append("<pricingOptionGroup>");
                //sb.Append("<pricingOptionKey>");
                //sb.Append("<pricingOptionKey>PFH</pricingOptionKey>");
                //sb.Append("</pricingOptionKey>");
                //sb.Append("</pricingOptionGroup>");
                sb.Append("</Fare_PriceUpsellWithoutPNR>");

                var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                    + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                    + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                    + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                    + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                    + "<s:Header>"
                    + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID>"
                    + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "TIUNRQ_23_1_1A</a:Action>"
                    + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                    + "<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">"
                    + "<oas:UsernameToken xmlns:oas=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\""
                    + " xmlns:oas1=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" oas1:Id=\"UsernameToken-1\">"
                    + "<oas:Username>" + _configuration["FlightSettings:AirUserName"] + "</oas:Username>"
                    + "<oas:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" + encodedNonce + "</oas:Nonce>"
                    + "<oas:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">" + passSHA + "</oas:Password>"
                    + "<oas1:Created>" + formattedTimestamp + "</oas1:Created>"
                    + "</oas:UsernameToken>"
                    + "</Security>"
                    + "<h:AMA_SecurityHostedUser xmlns:h=\"http://xml.amadeus.com/2010/06/Security_v1\">"
                    + "<h:UserID POS_Type=\"1\" PseudoCityCode=\"" + _configuration["FlightSettings:AirOfficeID"] + "\" AgentDutyCode=\"" + _configuration["FlightSettings:AirDutyCode"]
                    + "\" RequestorType=\"U\"/>"
                    + "</h:AMA_SecurityHostedUser>"
                    + "<awsse:Session TransactionStatusCode=\"Start\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\"/>"
                    + "</s:Header>"
                    + "<s:Body>"
                    + "<Fare_PriceUpsellWithoutPNR>"
                    + dbps.ToString()
                    + sb.ToString()
                    + "</s:Body>"
                    + "</s:Envelope>", null, "application/xml");

                #endregion Request

                #region Response

                var requestContent = content.ReadAsStringAsync().Result;

                string soapAction = _configuration["FlightSettings:AirSoapAction"] + "TIUNRQ_23_1_1A";
                var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
                var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");
                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    if (!Directory.Exists(xmlfiles))
                        Directory.CreateDirectory(xmlfiles);
                    File.WriteAllText(Path.Combine(xmlfiles, Selected_flight_result.id + "_Fare_PriceUpsellWithoutPNR_Request.xml"), requestContent, Encoding.UTF8);
                    File.WriteAllText(Path.Combine(xmlfiles, Selected_flight_result.id + "_Fare_PriceUpsellWithoutPNR_Response.xml"), re ?? string.Empty, Encoding.UTF8);
                }

                //XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerTravelBoardSearch_response.Envelope));
                //StringReader rdr = new StringReader(re);
                //com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerTravelBoardSearch_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_MasterPricerTravelBoardSearch_response.Envelope)serializer.Deserialize(rdr);
                //rdr.Close();

                //XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_PriceUpsellWithoutPNR_response.Envelope));
                //StringReader rdr = new StringReader(re);
                //com.ThirdPartyAPIs.Amadeus.Flight.Fare_PriceUpsellWithoutPNR_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_PriceUpsellWithoutPNR_response.Envelope)serializer.Deserialize(rdr);
                //rdr.Close();

                //var result = DeserializeXml<com.ThirdPartyAPIs.Amadeus.Flight.Fare_PriceUpsellWithoutPNR_response.Envelope>(re);

                XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_PriceUpsellWithoutPNR_response.Envelope));
                StringReader rdr = new StringReader(re);
                com.ThirdPartyAPIs.Amadeus.Flight.Fare_PriceUpsellWithoutPNR_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_PriceUpsellWithoutPNR_response.Envelope)serializer.Deserialize(rdr);
                rdr.Close();


                System.IO.File.WriteAllText(xmlfiles + Selected_flight_result.id + "_Fare_PriceUpsellWithoutPNR_Response.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);



                if (result == null || result.Body == null || result.Body.Fare_PriceUpsellWithoutPNRReply == null || result.Body.Fare_PriceUpsellWithoutPNRReply.Length == 0)
                {
                    return lstdta;
                    //return fareUpsellResponse;
                }
                #region Currency

                double currency_exchangerate = 1;
                string currency = userdetail.Currency;
                string currency_sign = userdetail.CurrencySign;
                try
                {

                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer.Rootobject Rootobject_curency_list = new CurrencyExchange.currencyexchange_rate_fixer.Rootobject();
                    //var client = _httpClient.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer currency_exchange_rate_fixer = new CurrencyExchange.currencyexchange_rate_fixer(_configuration, _env, _genericRepository);

                    //string currency = currency_exchange_rate_fixer.Currency("");

                    Rootobject_curency_list = await currency_exchange_rate_fixer.get_exchage_rate();
                    currency_exchangerate = currency_exchange_rate_fixer.currency_exchange_rate(Rootobject_curency_list, currency, APIcurrency);
                    if (currency_exchangerate == 0)
                    {
                        currency = APIcurrency;
                        currency_sign = APIcurrency_sign;
                        currency_exchangerate = 1;
                    }
                }
                catch (Exception ex)
                {
                    currency = APIcurrency;
                    currency_sign = APIcurrency_sign;
                    currency_exchangerate = 1;
                }
                #endregion

                var airlinefilelistfile = "";
                using (var fileStream = new FileStream(importantfiles + "Airlinelist.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    airlinefilelistfile = streamReader.ReadToEnd();
                }
                List<AirlineListDTO> airlinefilelist = System.Text.Json.JsonSerializer.Deserialize<List<AirlineListDTO>>(airlinefilelistfile);

                string listairportjson = System.IO.File.ReadAllText(importantfiles + "Airportlist.json", System.Text.ASCIIEncoding.UTF8);
                List<AirportListDTO> airportfilelist = System.Text.Json.JsonSerializer.Deserialize<List<AirportListDTO>>(listairportjson);
                string sessionData = "";
                var sessionid = Convert.ToString(result.Header.Session.SessionId);
                var sequence = Convert.ToString(result.Header.Session.SequenceNumber);
                var secToken = Convert.ToString(result.Header.Session.SecurityToken);
                sessionData = sessionid + "|" + sequence + "|" + secToken;
                int fareindex = 1;

                foreach (var fare in result.Body.Fare_PriceUpsellWithoutPNRReply)
                {
                    FareUpsellResponse d = new()
                    {
                        sessionData = sessionData
                    };

                    d.uniqueRef = fare.offerReferences?.offerIdentifier?.uniqueOfferReference;
                    d.fareindex = fareindex;
                    //d.year = fare.lastTktDate?.dateTime?.year;
                    //d.month = fare.lastTktDate?.dateTime?.month;
                    //d.day = fare.lastTktDate?.dateTime?.day;
                    string year = fare.lastTktDate?.dateTime?.year ?? "00";
                    string month = fare.lastTktDate?.dateTime?.month ?? "00";
                    string day = fare.lastTktDate?.dateTime?.day ?? "00";
                    string hour = fare.lastTktDate?.dateTime?.hour ?? "00";
                    string min = fare.lastTktDate?.dateTime?.minutes ?? "00";
                    string sec = fare.lastTktDate?.dateTime?.seconds ?? "00";
                    string ms = fare.lastTktDate?.dateTime?.milliseconds ?? "000";

                    //d.lastTktDate= year"-"+month+"-"+day+"-"+hour}-{min}-{sec}-{ms}";
                    d.lastTktDate = year + "-" + month + "-" + day + "T" + hour + ":" + min + ":" + sec;

                    if (fare.originDestination?.Length >= 2)
                    {
                        d.origincode = fare.originDestination[0];
                        d.destinationcode = fare.originDestination[1];
                        var departure_airport_obj = airportfilelist.Find(x => x.AirportCode == fare.originDestination[0]);
                        if (departure_airport_obj != null)
                        {
                            d.origin = departure_airport_obj.City;
                        }
                        var arrival_airport_obj = airportfilelist.Find(x => x.AirportCode == fare.originDestination[1]);
                        if (departure_airport_obj != null)
                        {
                            d.destination = departure_airport_obj.City;
                        }
                    }
                    d.carriercode = fare.validatingCarrier?.carrierInformation?.carrierCode;
                    var carrier_airline_obj = airlinefilelist.Find(x => x.AirlineCode == fare.validatingCarrier?.carrierInformation?.carrierCode);
                    if (carrier_airline_obj != null)
                    {
                        d.carrier = carrier_airline_obj.AirlineName;
                    }

                    var seg = fare.segmentInformation?.FirstOrDefault();
                    if (seg != null)
                    {
                        d.primarycode = seg.fareQualifier?.fareBasisDetails?.primaryCode;
                        d.farebasis = seg.fareQualifier?.fareBasisDetails?.fareBasisCode;
                        d.typequalifier =
                            seg.cabinGroup?.FirstOrDefault()?
                            .cabinSegment?.bookingClassDetails?.FirstOrDefault()?
                            .designator;
                        d.RBD = seg.segDetails.segmentDetail.classOfService;
                    }

                    if (seg.bagAllowanceInformation?.bagAllowanceDetails != null)
                    {
                        d.baggageDetail = seg.bagAllowanceInformation.bagAllowanceDetails.baggageWeight +
                            seg.bagAllowanceInformation.bagAllowanceDetails.measureUnit;
                    }

                    // 🔹 Amount (fareDataQualifier = 712)
                    var price712 = fare.fareDataInformation?.fareDataSupInformation?.FirstOrDefault(x => x.fareDataQualifier == "712");

                    if (price712 != null)
                    {
                        //var rate = APIcurrency == "KES" ? 1 : await Fare_ConvertCurrency(data.currency);
                        //var rate = APIcurrency == "KES" ? 1 : 1;
                        var rate = currency_exchangerate;
                        d.amount = Math.Round(Convert.ToDecimal(price712.fareAmount) * Convert.ToDecimal(rate), 2).ToString();
                        d.currency = currency;
                        d.currency_sign = currency_sign;
                    }

                    var fareFamily =
                        fare.fareComponentDetailsGroup?
                        .FirstOrDefault()?
                        .fareFamilyDetails?.fareFamilyname;

                    if (!string.IsNullOrEmpty(fareFamily))
                    {
                        d.farebasis += "|" + fareFamily;

                        // 🔹 Mini Rule (same as before)
                        string miniRule = await Fare_GetFareFamilyDescription(
                            Selected_flight_result,
                            fareFamily,
                            d.carriercode,
                            sessionData, fareindex
                        ).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(miniRule))
                        {
                            d.miniRule = miniRule.Split("~")[1];
                            d.sessionData = miniRule.Split("~")[0];
                        }
                        else { continue; }
                    }

                    lstdta.Add(d);
                    fareindex++;
                }

                System.IO.File.WriteAllText(xmlfiles + Selected_flight_result.id + "_Fare_PricedetailList.json", System.Text.Json.JsonSerializer.Serialize(lstdta), System.Text.ASCIIEncoding.UTF8);
                #endregion Response 
            }
            catch (Exception ex)
            {
                _errorLogRepository.AddErrorLog(ex, "AmadeusConfig->Fare_PriceUpsellWithoutPNR|~|" + sc + "", JsonConvert.SerializeObject(Selected_flight_result));
                return lstdta;
            }
            return lstdta;
        }


        public async Task<string> Fare_GetFareFamilyDescription(ResultResponse.Flightdata Selected_flight_result, string fare, string carrier, string sessionData, int fareindex)
        {
            try
            {
                #region Request
                string password = "U9MbJZjzR^EP";
                var url = _configuration["FlightSettings:AirProductionURL"];
                Guid guid = Guid.NewGuid();
                string guidString = guid.ToString();
                byte[] nonce = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(nonce);
                }
                DateTime timestamp = DateTime.UtcNow;
                string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
                string encodedNonce = Convert.ToBase64String(nonce);
                string passSHA = "";
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                    byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                    byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                    Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                    Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                    Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                    byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                    passSHA = Convert.ToBase64String(passSHABytes);
                }

                StringBuilder sb = new();
                sb.Append("<Fare_GetFareFamilyDescription>");
                foreach (var obib in Selected_flight_result.OutboundInboundlist)
                {
                    string depCode = obib.flightlist[0].Departure.Iata;
                    string arrCode = obib.flightlist[obib.flightlist.Count - 1].Arrival.Iata;

                    sb.Append("<standaloneDescriptionRequest>");
                    sb.Append("<fareInformation>");
                    sb.Append("<discountDetails>");
                    sb.Append("<fareQualifier>FF</fareQualifier>");
                    sb.Append("<rateCategory>" + fare + "</rateCategory>");
                    sb.Append("</discountDetails>");
                    sb.Append("</fareInformation>");
                    sb.Append("<itineraryInformation>");
                    sb.Append("<origin>" + depCode + "</origin>");
                    sb.Append("<destination>" + arrCode + "</destination>");
                    sb.Append("</itineraryInformation>");
                    sb.Append("<carrierInformation>");
                    sb.Append("<companyIdentification>");
                    sb.Append("<otherCompany>" + carrier + "</otherCompany>");
                    sb.Append("</companyIdentification>");
                    sb.Append("</carrierInformation>");
                    sb.Append("</standaloneDescriptionRequest>");
                }
                sb.Append("</Fare_GetFareFamilyDescription>");

                var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                    + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                    + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                    + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                    + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                    + "<s:Header>"
                    + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID>"
                    + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "TFQFRQ_18_1_1A</a:Action>"
                    + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                    + "<link:TransactionFlowLink xmlns:link=\"http://wsdl.amadeus.com/2010/06/ws/Link_v1\" />"
                    + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                    + "<awsse:SessionId>" + sessionData.Split('|')[0]
                    + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(Convert.ToInt32(sessionData.Split('|')[1]) + 1) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                    + sessionData.Split('|')[2] + "</awsse:SecurityToken> </awsse:Session>"
                    + "</s:Header>"
                    + "<s:Body>"
                    + sb.ToString()
                    + "</s:Body>"
                    + "</s:Envelope>", null, "application/xml");

                #endregion Request

                var requestContent = content.ReadAsStringAsync().Result;

                string soapAction = _configuration["FlightSettings:AirSoapAction"] + "TFQFRQ_18_1_1A";
                var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    if (!Directory.Exists(xmlfiles))
                        Directory.CreateDirectory(xmlfiles);
                    File.WriteAllText(Path.Combine(xmlfiles, Selected_flight_result.id + "_" + fareindex + "_Fare_GetFareFamilyDescription_request.xml"), requestContent, Encoding.UTF8);
                    File.WriteAllText(Path.Combine(xmlfiles, Selected_flight_result.id + "_" + fareindex + "_Fare_GetFareFamilyDescription_response.xml"), re ?? string.Empty, Encoding.UTF8);
                }
                XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_GetFareFamilyDescription_response.Envelope));
                StringReader rdr = new StringReader(re);
                com.ThirdPartyAPIs.Amadeus.Flight.Fare_GetFareFamilyDescription_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_GetFareFamilyDescription_response.Envelope)serializer.Deserialize(rdr);
                rdr.Close();

                System.IO.File.WriteAllText(xmlfiles + Selected_flight_result.id + "_" + fareindex + "_Fare_GetFareFamilyDescription_response.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);
                string sessionDataa = "";
                if (result == null || result.Body == null || result.Body.Fare_GetFareFamilyDescriptionReply == null || result.Body.Fare_GetFareFamilyDescriptionReply.fareFamilyDescriptionGroup == null || result.Body.Fare_GetFareFamilyDescriptionReply.fareFamilyDescriptionGroup.Length == 0)
                {
                    return sessionDataa;
                    //return fareUpsellResponse;
                }
                string freetext = "";

                var groups = result.Body
                    ?.Fare_GetFareFamilyDescriptionReply
                    ?.fareFamilyDescriptionGroup;

                if (groups != null)
                {
                    foreach (var group in groups)
                    {
                        string chaText = "", incText = "", notOfferedText = "";

                        string chaicon = "<li> <img src=\"/assets/images/dollar-sign.png\" class=\"mx-1\"> ";
                        string incicon = "<li> <img src=\"/assets/images/check.png\" class=\"mx-1\"> ";
                        string notOfferedicon = "<li> <img src=\"/assets/images/minus.png\" class=\"mx-1\"> ";

                        //foreach (var oc in group.ocFeeInformation ?? new List<OcFeeInformation>())
                        //foreach (var oc in group.ocFeeInformation ?? new List<Amadeus_wsdl.Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformation>())
                        //{
                        foreach (var oc in group.ocFeeInformation ??
       Array.Empty<Amadeus_wsdl.Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformation>())
                        {
                            string indicator = oc.feeDescription?.dataInformation?.FirstOrDefault()?.indicator;
                            var options = oc.feeFreeFlowDescription?.freeText;

                            if (options == null) continue;

                            foreach (var option in options)
                            {
                                if (indicator == "CHA")
                                    chaText += chaicon + option.ToLower() + " </li>";
                                else if (indicator == "INC")
                                    incText += incicon + option.ToLower() + " </li>";
                                else
                                    notOfferedText += notOfferedicon + option.ToLower() + " </li>";
                            }
                        }

                        if (!string.IsNullOrEmpty(chaText))
                            freetext += "<div><ul> At Additional Cost: " + chaText + "</ul></div>";

                        if (!string.IsNullOrEmpty(incText))
                            freetext += "<div><ul> Included: " + incText + "</ul></div>";

                        if (!string.IsNullOrEmpty(notOfferedText))
                            freetext += "<div><ul> Not Offered: " + notOfferedText + "</ul></div>";
                    }
                }


                return sessionDataa + "~" + freetext;
            }
            catch (Exception ex)
            {
                //SaveAmadeusLog("", "", ex.Message + " /n error Fare_GetFareFamilyDescription");
                _errorLogRepository.AddErrorLog(ex, "AmadeusConfig->Fare_GetFareFamilyDescription", JsonConvert.SerializeObject(Selected_flight_result));
                return null;
            }
        }

        #endregion Fare_GetFareFamilyDescription

        #region Flight Detail
        public async Task<DetailResponse.FlightDetailResponse> FlightDetail(ResultResponse.Flightdata Selected_flight_result, SearchRequest.FlightDetailRequest request, int totaladult, int totalchild, int totalinfant, UserDetailDTO userdetail)
        {
            DetailResponse.FlightDetailResponse Basket_response = new DetailResponse.FlightDetailResponse();
            DetailResponse.Data data = new Data();
            DetailResponse.expire expire = new expire();
            List<DetailResponse.Request_passengers> Request_passengers = new List<DetailResponse.Request_passengers>();
            try
            {
                #region Request
                string password = "U9MbJZjzR^EP";
                var url = _configuration["FlightSettings:AirProductionURL"];
                Guid guid = Guid.NewGuid();
                string guidString = guid.ToString();
                byte[] nonce = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(nonce);
                }
                DateTime timestamp = DateTime.UtcNow;
                string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
                string encodedNonce = Convert.ToBase64String(nonce);
                string passSHA = "";
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                    byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                    byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                    Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                    Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                    Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                    byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                    passSHA = Convert.ToBase64String(passSHABytes);
                }
                StringBuilder sb = new();

                int seg = 1;
                int flightIndicator = 1;
                int itemNumber = 1;
                int itemNumberindex = 0;
                string carrierInformation_otherCompany = "";

                foreach (var OutboundInbounddata in Selected_flight_result.OutboundInboundlist)
                {
                    string FlightIndicator_str = flightIndicator.ToString();
                    // string RBD = OutboundInbounddata.flightlist[0].RBDLIST.Count > 1 ? (OutboundInbounddata.flightlist[0].RBDLIST.Count >= itemNumberindex ? OutboundInbounddata.flightlist[0].RBDLIST[itemNumberindex] : OutboundInbounddata.flightlist[0].RBD) : OutboundInbounddata.flightlist[0].RBD;

                    foreach (var flightlist in OutboundInbounddata.flightlist)
                    {
                        string RBD = flightlist.RBD;
                        //string RBD = "O";
                        if(request.fareindex>0&& !string.IsNullOrEmpty(request.RBD))
                        {
                            RBD = request.RBD;
                        }
                        carrierInformation_otherCompany = flightlist.MarketingAirline.code;
                        sb.Append("  <segmentGroup><segmentInformation><flightDate><departureDate>" + Convert.ToDateTime(flightlist.Departure.Datetime).ToString("ddMMyy") + "</departureDate><departureTime>" + Convert.ToDateTime(flightlist.Departure.Datetime).ToString("HHmm") + "</departureTime><arrivalDate>" + Convert.ToDateTime(flightlist.Arrival.Datetime).ToString("ddMMyy") + "</arrivalDate><arrivalTime>" + Convert.ToDateTime(flightlist.Arrival.Datetime).ToString("HHmm") + "</arrivalTime></flightDate><boardPointDetails><trueLocationId>" + flightlist.Departure.Iata + "</trueLocationId></boardPointDetails><offpointDetails><trueLocationId>" + flightlist.Arrival.Iata + "</trueLocationId></offpointDetails><companyDetails><marketingCompany>" + flightlist.MarketingAirline.code + "</marketingCompany><operatingCompany>" + flightlist.OperatingAirline.code + "</operatingCompany></companyDetails><flightIdentification><flightNumber>" + flightlist.MarketingAirline.number + "</flightNumber><bookingClass>" + RBD + "</bookingClass></flightIdentification><flightTypeDetails><flightIndicator>" + FlightIndicator_str + "</flightIndicator></flightTypeDetails><itemNumber>" + itemNumber + "</itemNumber></segmentInformation></segmentGroup> ");
                        itemNumber++;   
                    }
                    itemNumberindex++;
                    flightIndicator++;
                }

                //sb.Append("<segmentGroup>");
                //sb.Append("<segmentInformation>");
                //sb.Append("<flightDate>");
                //sb.Append("<departureDate>" + data.depDt.ToString("ddMMyy") + "</departureDate>");
                //sb.Append("<departureTime>" + data.departureTime.Replace(":", "") + "</departureTime>");
                //sb.Append("<arrivalDate>" + data.arrDt.ToString("ddMMyy") + "</arrivalDate>");
                //sb.Append("<arrivalTime>" + data.arrivalTime.Replace(":", "") + "</arrivalTime>");
                //sb.Append("</flightDate>");
                //sb.Append("<boardPointDetails>");
                //sb.Append("<trueLocationId>" + data.depCode.Split(',')[0] + "</trueLocationId>");
                //sb.Append("</boardPointDetails>");
                //sb.Append("<offpointDetails>");
                //sb.Append("<trueLocationId>" + data.arrCode.Split(',')[0] + "</trueLocationId>");
                //sb.Append("</offpointDetails>");
                //sb.Append("<companyDetails>");
                //sb.Append("<marketingCompany>" + data.marketingCompany.Split('|')[0] + "</marketingCompany>");
                //sb.Append("<operatingCompany></operatingCompany>");
                //sb.Append("</companyDetails>");
                //sb.Append("<flightIdentification>");
                //sb.Append("<flightNumber>" + data.flightNumber + "</flightNumber>");
                //sb.Append("<bookingClass>" + data.bookingClass + "</bookingClass>");
                //sb.Append("</flightIdentification>");
                //sb.Append("<flightTypeDetails>");
                //sb.Append("<flightIndicator>" + seg + "</flightIndicator>");
                //sb.Append("</flightTypeDetails>");
                //sb.Append("<itemNumber>" + seg + "</itemNumber>");
                //sb.Append("</segmentInformation>");
                //sb.Append("</segmentGroup>");

                int main_pax_indx = 1;
                int pax_indx = 1;
                int system_pax_indx = 1;
                StringBuilder dbps = new();
                if (totaladult != 0)
                {
                    dbps.Append("<passengersGroup>");
                    dbps.Append("<segmentRepetitionControl>");
                    dbps.Append("<segmentControlDetails>");
                    dbps.Append("<quantity>1</quantity>");
                    dbps.Append("<numberOfUnits>" + totaladult + "</numberOfUnits>");
                    dbps.Append("</segmentControlDetails>");
                    dbps.Append("</segmentRepetitionControl>");
                    dbps.Append("<travellersID>");
                    for (int i = 0; i < totaladult; i++)
                    {
                        dbps.Append("<travellerDetails>");
                        dbps.Append("<measurementValue>" + (i + 1) + "</measurementValue>");
                        dbps.Append("</travellerDetails>");
                    }
                    dbps.Append("</travellersID>");
                    dbps.Append("<discountPtc>");
                    dbps.Append("<valueQualifier>ADT</valueQualifier>");
                    dbps.Append("</discountPtc>");
                    dbps.Append("</passengersGroup>");

                    Request_passengers.Add(new Models.Flight.DetailResponse.Request_passengers()
                    {
                        index = main_pax_indx,
                        paxtype = (int)PaxtypeEnum.Adult,
                        quantity = totaladult,
                    });

                    main_pax_indx++;
                }
                if (totalchild > 0)
                {
                    dbps.Append("<passengersGroup>");
                    dbps.Append("<segmentRepetitionControl>");
                    dbps.Append("<segmentControlDetails>");
                    dbps.Append("<quantity>2</quantity>");
                    dbps.Append("<numberOfUnits>" + totalchild + "</numberOfUnits>");
                    dbps.Append("</segmentControlDetails>");
                    dbps.Append("</segmentRepetitionControl>");
                    dbps.Append("<travellersID>");
                    for (int i = 0; i < totalchild; i++)
                    {
                        dbps.Append("<travellerDetails>");
                        dbps.Append("<measurementValue>" + (totaladult + i + 1) + "</measurementValue>");
                        dbps.Append("</travellerDetails>");
                    }
                    dbps.Append("</travellersID>");
                    dbps.Append("<discountPtc>");
                    dbps.Append("<valueQualifier>CNN</valueQualifier>");
                    dbps.Append("</discountPtc>");
                    dbps.Append("</passengersGroup>");

                    Request_passengers.Add(new Models.Flight.DetailResponse.Request_passengers()
                    {
                        index = main_pax_indx,
                        paxtype = (int)PaxtypeEnum.Child,
                        quantity = totalchild,
                    });
                    main_pax_indx++;
                }
                if (totalinfant > 0)
                {
                    dbps.Append("<passengersGroup>");
                    dbps.Append("<segmentRepetitionControl>");
                    dbps.Append("<segmentControlDetails>");
                    dbps.Append("<quantity>3</quantity>");
                    dbps.Append("<numberOfUnits>" + totalinfant + "</numberOfUnits>");
                    dbps.Append("</segmentControlDetails>");
                    dbps.Append("</segmentRepetitionControl>");
                    dbps.Append("<travellersID>");
                    for (int i = 0; i < totalinfant; i++)
                    {
                        dbps.Append("<travellerDetails>");
                        dbps.Append("<measurementValue>" + (i + 1) + "</measurementValue>");
                        dbps.Append("</travellerDetails>");
                    }
                    dbps.Append("</travellersID>");
                    dbps.Append("<discountPtc>");
                    dbps.Append("<valueQualifier>INF</valueQualifier>");
                    dbps.Append("<fareDetails>");
                    dbps.Append("<qualifier>766</qualifier>");
                    dbps.Append("</fareDetails>");
                    dbps.Append("</discountPtc>");
                    dbps.Append("</passengersGroup>");

                    Request_passengers.Add(new Models.Flight.DetailResponse.Request_passengers()
                    {
                        index = main_pax_indx,
                        paxtype = (int)PaxtypeEnum.Infant,
                        quantity = totalinfant,
                    });

                    main_pax_indx++;
                }

                string selectedfare = "";
                //if (fareindex>0) {
                //    selectedfare = "<pricingOptionInformation><fareReference><referenceType>TST</referenceType> <uniqueReference>" + fareindex + "</uniqueReference> </fareReference></pricingOptionInformation>";
                //}
                //if (fareindex > 0)
                //{
                //    selectedfare = "<pricingOptionGroup><pricingOptionKey><pricingOptionKey>FBA</pricingOptionKey></pricingOptionKey><optionDetail><criteriaDetails><attributeType>ECOVALU</attributeType></criteriaDetails></optionDetail></pricingOptionGroup>";
                //}
                //<paxSegTstReference><referenceDetails><type>P</type><value>1</value></referenceDetails><referenceDetails><type>S</type><value>1</value></referenceDetails></paxSegTstReference>
                var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                    + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                    + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                    + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                    + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                    + "<s:Header>"
                    + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID>"
                    + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "TIPNRQ_18_1_1A</a:Action>"
                    + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                    + "<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">"
                    + "<oas:UsernameToken xmlns:oas=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\""
                    + " xmlns:oas1=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" oas1:Id=\"UsernameToken-1\">"
                    + "<oas:Username>" + _configuration["FlightSettings:AirUserName"] + "</oas:Username>"
                    + "<oas:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" + encodedNonce + "</oas:Nonce>"
                    + "<oas:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">" + passSHA + "</oas:Password>"
                    + "<oas1:Created>" + formattedTimestamp + "</oas1:Created>"
                    + "</oas:UsernameToken>"
                    + "</Security>"
                    + "<h:AMA_SecurityHostedUser xmlns:h=\"http://xml.amadeus.com/2010/06/Security_v1\">"
                    + "<h:UserID POS_Type=\"1\" PseudoCityCode=\"" + _configuration["FlightSettings:AirOfficeID"] + "\" AgentDutyCode=\"" + _configuration["FlightSettings:AirDutyCode"] + "\" RequestorType=\"U\"/>"
                    + "</h:AMA_SecurityHostedUser>"
                    + "<awsse:Session TransactionStatusCode=\"Start\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\"/>"
                    + "</s:Header>"
                    + "<s:Body>"
                    + "<Fare_InformativePricingWithoutPNR>"
                    + dbps.ToString()
                    + sb.ToString()
                    + "<pricingOptionGroup>"
                    + "<pricingOptionKey>"
                    + "<pricingOptionKey>VC</pricingOptionKey>"
                    + "</pricingOptionKey>"
                    + "<carrierInformation>"
                    + "<companyIdentification>"
                    + "<otherCompany>" + carrierInformation_otherCompany + "</otherCompany>"
                    + "</companyIdentification>"
                    + "</carrierInformation>"
                    + "</pricingOptionGroup>"
                    + "<pricingOptionGroup>"
                    + "<pricingOptionKey>"
                    + "<pricingOptionKey>RP</pricingOptionKey>"
                    + "</pricingOptionKey>"
                    + "</pricingOptionGroup>"
                    + selectedfare
                    + "<pricingOptionGroup>"
                    + "<pricingOptionKey>"
                    + "<pricingOptionKey>RLO</pricingOptionKey>"
                    + "</pricingOptionKey>"
                    + "</pricingOptionGroup>"
                    + "<pricingOptionGroup>"
                    + "<pricingOptionKey>"
                    + "<pricingOptionKey>FCO</pricingOptionKey>"
                    + "</pricingOptionKey>"
                    + "<currency>"
                    + "<firstCurrencyDetails>"
                    + "<currencyQualifier>FCO</currencyQualifier>"
                    + "<currencyIsoCode>" + APIcurrency + "</currencyIsoCode>"
                    + "</firstCurrencyDetails>"
                    + "</currency>"
                    + "</pricingOptionGroup>"
                    + "</Fare_InformativePricingWithoutPNR>"
                    + "</s:Body>"
                    + "</s:Envelope>", null, "application/xml");

                #endregion Request

                #region Response

                var requestContent = content.ReadAsStringAsync().Result;

                string soapAction = _configuration["FlightSettings:AirSoapAction"] + "TIPNRQ_18_1_1A";
                var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    if (!Directory.Exists(xmlfiles))
                        Directory.CreateDirectory(xmlfiles);
                    File.WriteAllText(Path.Combine(xmlfiles, Selected_flight_result.id + "_Fare_InformativePricingWithoutPNR_Request.xml"), requestContent, Encoding.UTF8);
                    File.WriteAllText(Path.Combine(xmlfiles, Selected_flight_result.id + "_Fare_InformativePricingWithoutPNR_Response.xml"), re ?? string.Empty, Encoding.UTF8);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_InformativePricingWithoutPNR_response.Envelope));
                StringReader rdr = new StringReader(re);
                com.ThirdPartyAPIs.Amadeus.Flight.Fare_InformativePricingWithoutPNR_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_InformativePricingWithoutPNR_response.Envelope)serializer.Deserialize(rdr);
                rdr.Close();


                System.IO.File.WriteAllText(xmlfiles + request.sc + Selected_flight_result.id + "_Fare_InformativePricingWithoutPNR_Response.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);


                if (result.Body != null && result.Body.Fare_InformativePricingWithoutPNRReply != null && result.Body.Fare_InformativePricingWithoutPNRReply.errorGroup != null)
                {
                    if (result.Body.Fare_InformativePricingWithoutPNRReply.errorGroup.errorWarningDescription.freeText[0].ToString() == "NO FARE FOR BOOKING CODE-TRY OTHER PRICING OPTIONS")
                    {

                        Selected_flight_result.faretype = Selected_flight_result.farefamilylistdata[1].ToString();
                        //goto InformativePricing;
                        return Basket_response;
                    }
                }


                if (result == null || result.Body == null || result.Body.Fare_InformativePricingWithoutPNRReply == null || result.Body.Fare_InformativePricingWithoutPNRReply.errorGroup != null || result.Body.Fare_InformativePricingWithoutPNRReply.mainGroup == null || result.Body.Fare_InformativePricingWithoutPNRReply.mainGroup.pricingGroupLevelGroup == null || result.Body.Fare_InformativePricingWithoutPNRReply.mainGroup.pricingGroupLevelGroup.Length == 0)
                {

                    string message = "";

                    //string origin_name = Search_criteria_obj.destinations[0].departureairportname;
                    //string origin_iata = Search_criteria_obj.destinations[0].departureairportcode;
                    //string destination_name = Search_criteria_obj.destinations[0].arrivalairportname;
                    //string destination_iata = Search_criteria_obj.destinations[0].arrivalairportcode;

                    //string depart_date = Search_criteria_obj.destinations[0].date;
                    //string return_date = "";

                    //if (Search_criteria_obj.destinations.Count > 1)
                    //{
                    //    return_date = Search_criteria_obj.destinations[Search_criteria_obj.destinations.Count - 1].date;
                    //}

                    //int adults = Search_criteria_obj.totaladult;
                    //int children = Search_criteria_obj.totalchild;
                    //int infants = Search_criteria_obj.totalinfant;

                    //string cabin = Search_criteria_obj.flightclass;
                    //string stops = Search_criteria_obj.isdirect.ToString();
                    //string Token = Search_criteria_obj.Token;

                    //string airline = "";
                    //if (Search_criteria_obj.airline_code != null && Search_criteria_obj.airline_code != "" && Search_criteria_obj.airline_name != null && Search_criteria_obj.airline_name != "")
                    //{
                    //    string airline_name = Search_criteria_obj.airline_name;
                    //    string airline_codes = Search_criteria_obj.airline_code;


                    //    airline = "&airline=" + airline_codes + "&airlinename=" + airline_name;
                    //}


                    if (result != null && result.Body != null && result.Body.Fare_InformativePricingWithoutPNRReply != null && result.Body.Fare_InformativePricingWithoutPNRReply.errorGroup != null && result.Body.Fare_InformativePricingWithoutPNRReply.errorGroup.errorWarningDescription != null && result.Body.Fare_InformativePricingWithoutPNRReply.errorGroup.errorWarningDescription.freeText != null)
                    {
                        message = result.Body.Fare_InformativePricingWithoutPNRReply.errorGroup.errorWarningDescription.freeText[0].ToString();
                    }
                    else
                    {
                        message = "NO FARE FOR BOOKING. TRY OTHER PRICING OPTIONS";
                    }
                    Basket_response.message = message;
                    return Basket_response;
                }
                #region Currency
                double currency_exchangerate = 1;
                string currency = userdetail.Currency;
                string currency_sign = userdetail.CurrencySign;
                try
                {

                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer.Rootobject Rootobject_curency_list = new CurrencyExchange.currencyexchange_rate_fixer.Rootobject();
                    //var client = _httpClient.CreateClient(); // optional: CreateClient("Amadeus") if you register a named client
                    com.ThirdPartyAPIs.CurrencyExchange.currencyexchange_rate_fixer currency_exchange_rate_fixer = new CurrencyExchange.currencyexchange_rate_fixer(_configuration, _env, _genericRepository);

                    //string currency = currency_exchange_rate_fixer.Currency("");
                    Rootobject_curency_list = await currency_exchange_rate_fixer.get_exchage_rate();
                    currency_exchangerate = currency_exchange_rate_fixer.currency_exchange_rate(Rootobject_curency_list, currency, APIcurrency);
                    if (currency_exchangerate == 0)
                    {
                        currency = APIcurrency;
                        currency_sign = APIcurrency_sign;
                        currency_exchangerate = 1;
                    }
                }
                catch (Exception ex)
                {
                    currency = APIcurrency;
                    currency_sign = APIcurrency_sign;
                    currency_exchangerate = 1;
                }
                #endregion

                foreach (var legs in result.Body.Fare_InformativePricingWithoutPNRReply.mainGroup.pricingGroupLevelGroup)
                {

                    if (legs.fareInfoGroup.segmentLevelGroup == null || legs.fareInfoGroup.segmentLevelGroup.Length == 0)
                    {
                        return Basket_response;
                    }

                    var Unavaiolable_segments = Array.Find(legs.fareInfoGroup.segmentLevelGroup, item => item.additionalInformation != null && item.additionalInformation.priceTicketDetails != null && Array.IndexOf(item.additionalInformation.priceTicketDetails, "ANS") > -1);

                    if (Unavaiolable_segments != null)
                    {


                        // Basket_response.message = "alert('Segment not available!');  $('body').hide(); window.location='/Flight/result?sc=" + Search_criteria_obj.Token + "'  ";
                        Basket_response.message = "Segment not available!";
                        return Basket_response;
                    }
                }

                //Task.WaitAll(tasks.ToArray());

                #region Fare Rules
                //fare_rules(result, id, sc);
                #endregion;


                // Seat_map_bind(Selected_flight_result, sc);

                #region Variable

                //string importantstaticdata = System.Configuration.ConfigurationManager.AppSettings["importantfiles"].ToString();

                //string airlinefilelistHtmls = System.IO.File.ReadAllText(importantstaticdata + "Airlinelist.json", System.Text.ASCIIEncoding.UTF8);

                //List<Models.Flight.airport_search.airlinefile> airlinefilelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Flight.airport_search.airlinefile>>(airlinefilelistHtmls);

                //string listairportjson = System.IO.File.ReadAllText(importantstaticdata + "trawexairportlist.json", System.Text.ASCIIEncoding.UTF8);

                //List<Models.Flight.airport_search.airportlist> airportfilelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Flight.airport_search.airportlist>>(listairportjson);

                List<Models.Flight.ResultResponse.OutboundInbounddata> Itinerary_data = new List<Models.Flight.ResultResponse.OutboundInbounddata>();

                List<Models.Flight.DetailResponse.tarriffinfo> tarriffinfo = new List<Models.Flight.DetailResponse.tarriffinfo>();

                List<Models.Flight.ResultResponse.pax_fareDetailsBySegment> pax_fareDetailsBySegment = new List<Models.Flight.ResultResponse.pax_fareDetailsBySegment>();

                List<Models.Flight.DetailResponse.Pax_Data> Pax_Data = new List<Models.Flight.DetailResponse.Pax_Data>();

                Models.Flight.DetailResponse.required_field required_field = new Models.Flight.DetailResponse.required_field();

                Models.Flight.DetailResponse.Price_Data Price_Data = new Models.Flight.DetailResponse.Price_Data();

                #endregion;

                #region Price

                #region Getting Airline Discount
                //Mode  true = B2B , false = B2C
                //Origin  true = Algeria , false = other
                //discount_type  true = Fixed , false = Percentage
                //markup_type true = Fixed , false = Percentage

                string airline_code = Selected_flight_result.OutboundInboundlist[0].flightlist[0].MarketingAirline.code;
                string airport_code = Selected_flight_result.OutboundInboundlist[0].flightlist[0].Departure.Iata;
                string class_code = Selected_flight_result.OutboundInboundlist[0].flightlist[0].CabinClassText;

                //Models.Admin.airportdata.airlinejsonlist Airline_discount_found = new Models.Admin.airportdata.airlinejsonlist();

                //foreach (DataRow dr in Dt_airline_discount_markup.Select("code='" + airline_code + "'"))
                //{
                //    /*Airline_discount_found.discount_type = Convert.ToBoolean(dr["discount_type"].ToString());
                //    Airline_discount_found.discount = Convert.ToDouble(dr["discount"].ToString());

                //    Airline_discount_found.markup_type = Convert.ToBoolean(dr["markup_type"].ToString());
                //    Airline_discount_found.markup = Convert.ToDouble(dr["markup"].ToString());*/


                //    Airline_discount_found.ispercentage = dr["ispercentage"].ToString() != null && dr["ispercentage"].ToString() != "" ? Convert.ToBoolean(dr["ispercentage"].ToString()) : false;
                //    Airline_discount_found.markup_percentage = dr["markup_percentage"].ToString() != null && dr["markup_percentage"].ToString() != "" ? Convert.ToDouble(dr["markup_percentage"].ToString()) : 0;

                //    Airline_discount_found.isfixed = dr["isfixed"].ToString() != null && dr["isfixed"].ToString() != "" ? Convert.ToBoolean(dr["isfixed"].ToString()) : false;
                //    Airline_discount_found.markup_fixed = dr["markup_fixed"].ToString() != null && dr["markup_fixed"].ToString() != "" ? Convert.ToDouble(dr["markup_fixed"].ToString()) : 0;

                //    break;
                //}
                #endregion;


                double api_totalprice = 0;
                double api_basefare = 0;
                double api_tax = 0;
                double net_totalprice = 0;
                double net_basefare = 0;
                double net_tax = 0;
                bool isRefundable = false;

                foreach (var travelerPricings in result.Body.Fare_InformativePricingWithoutPNRReply.mainGroup.pricingGroupLevelGroup)
                {

                    // Finding Pax type with (numberOfPax > quantity) sending in Request
                    Models.Flight.DetailResponse.Request_passengers Request_passengers_find = Request_passengers.Find(x => x.index.ToString() == travelerPricings.numberOfPax[0].quantity);

                    if (Request_passengers_find == null)
                    {
                        continue;
                    }

                    //Amadeus_wsdl.MonetaryInformationDetailsType_262581C Base_fare_obj = Array.Find(travelerPricings.fareInfoGroup.fareAmount.otherMonetaryDetails, item => item.typeQualifier == "E");

                    //var Base_fare_obj = travelerPricings.fareInfoGroup.fareAmount.monetaryDetails;// Array.Find(travelerPricings.fareInfoGroup.fareAmount.monetaryDetails, item => item.typeQualifier == "B");

                    var Base_fare_obj = Array.Find(travelerPricings.fareInfoGroup.fareAmount.otherMonetaryDetails, item => item.typeQualifier == "E");

                    var Total_fare_obj = Array.Find(travelerPricings.fareInfoGroup.fareAmount.otherMonetaryDetails, item => item.typeQualifier == "712");

                    //Base_fare_obj = Total_fare_obj;
                    if (Base_fare_obj == null)
                    {
                        Base_fare_obj = Total_fare_obj;
                    }


                    int quantity = Request_passengers_find.quantity;

                    double api_base_price_per_leg = Convert.ToDouble(Base_fare_obj.amount) * Request_passengers_find.quantity;
                    double api_total_price_per_leg = Convert.ToDouble(Total_fare_obj.amount) * Request_passengers_find.quantity;
                    double api_tax_price_per_leg = api_total_price_per_leg - api_base_price_per_leg;


                    api_totalprice = api_totalprice + api_total_price_per_leg;
                    api_basefare = api_basefare + api_base_price_per_leg;
                    api_tax = api_tax + api_tax_price_per_leg;


                    double net_base_price_per_leg = Convert.ToDouble(api_base_price_per_leg);
                    double net_total_price_per_leg = Convert.ToDouble(api_total_price_per_leg);
                    double net_tax_price_per_leg = api_tax_price_per_leg;


                    #region Appling Discount
                    double base_price_discount_amount_pax = 0;
                    double tax_price_discount_amount_pax = 0;
                    double total_price_discount_amount_pax = 0;
                    //if (Airline_discount_found.discount > 0)
                    //{
                    //    if (Airline_discount_found.discount_type)
                    //    {
                    //        base_price_discount_amount_pax = Airline_discount_found.discount;
                    //        total_price_discount_amount_pax = Airline_discount_found.discount;
                    //    }
                    //    else if (Airline_discount_found.discount_type == false)
                    //    {
                    //        base_price_discount_amount_pax = ((net_base_price_per_leg * Airline_discount_found.discount) / 100);
                    //        tax_price_discount_amount_pax = ((net_tax_price_per_leg * Airline_discount_found.discount) / 100);
                    //        total_price_discount_amount_pax = ((net_total_price_per_leg * Airline_discount_found.discount) / 100);
                    //    }

                    //    if (base_price_discount_amount_pax > net_base_price_per_leg || base_price_discount_amount_pax < 0)
                    //    {
                    //        base_price_discount_amount_pax = 0;
                    //        tax_price_discount_amount_pax = 0;
                    //        total_price_discount_amount_pax = 0;
                    //    }
                    //}

                    net_base_price_per_leg = net_base_price_per_leg - base_price_discount_amount_pax;
                    net_total_price_per_leg = net_total_price_per_leg - total_price_discount_amount_pax;
                    net_tax_price_per_leg = net_tax_price_per_leg - tax_price_discount_amount_pax;
                    #endregion;

                    #region Applying Markup
                    double base_price_markup_amount_pax = 0;
                    double tax_price_markup_amount_pax = 0;
                    double total_price_markup_amount_pax = 0;
                    /*if (Airline_discount_found.markup > 0)
                    {*/
                    /*if (Airline_discount_found.markup_type)
                    {
                        //  base_price_markup_amount_pax = Airline_discount_found.markup / total_pax;
                        // total_price_markup_amount_pax = Airline_discount_found.markup / total_pax;
                        base_price_markup_amount_pax = Airline_discount_found.markup;
                        total_price_markup_amount_pax = Airline_discount_found.markup;
                    }
                    else if (Airline_discount_found.markup_type == false)
                    {
                        base_price_markup_amount_pax = ((net_base_price_per_leg * Airline_discount_found.markup) / 100);
                        tax_price_markup_amount_pax = ((net_tax_price_per_leg * Airline_discount_found.markup) / 100);
                        total_price_markup_amount_pax = ((net_total_price_per_leg * Airline_discount_found.markup) / 100);
                    }*/

                    //if (Airline_discount_found.isfixed == true)
                    //{
                    //    //  base_price_markup_amount_pax = Airline_discount_found.markup / total_pax;
                    //    // total_price_markup_amount_pax = Airline_discount_found.markup / total_pax;
                    //    base_price_markup_amount_pax = base_price_markup_amount_pax + Airline_discount_found.markup_fixed;
                    //    total_price_markup_amount_pax = total_price_markup_amount_pax + Airline_discount_found.markup_fixed;
                    //}
                    //if (Airline_discount_found.ispercentage == true)
                    //{
                    //    base_price_markup_amount_pax = base_price_markup_amount_pax + ((net_base_price_per_leg * Airline_discount_found.markup_percentage) / 100);
                    //    tax_price_markup_amount_pax = tax_price_markup_amount_pax + ((net_tax_price_per_leg * Airline_discount_found.markup_percentage) / 100);
                    //    total_price_markup_amount_pax = total_price_markup_amount_pax + ((net_total_price_per_leg * Airline_discount_found.markup_percentage) / 100);
                    //}

                    if (base_price_markup_amount_pax > net_base_price_per_leg || base_price_markup_amount_pax < 0)
                    {
                        base_price_markup_amount_pax = 0;
                        tax_price_markup_amount_pax = 0;
                        total_price_markup_amount_pax = 0;
                    }
                    /*}*/
                    net_base_price_per_leg = net_base_price_per_leg + base_price_markup_amount_pax;
                    net_total_price_per_leg = net_total_price_per_leg + total_price_markup_amount_pax;
                    net_tax_price_per_leg = net_tax_price_per_leg + tax_price_markup_amount_pax;

                    //if (admin_fixed_allow)
                    //{
                    //    net_base_price_per_leg = net_base_price_per_leg + admin_fixed;
                    //    net_total_price_per_leg = net_total_price_per_leg + admin_fixed;
                    //}
                    //if (admin_percentage_allow)
                    //{
                    //    net_base_price_per_leg = net_base_price_per_leg + ((net_base_price_per_leg * admin_percentage) / 100);
                    //    net_total_price_per_leg = net_total_price_per_leg + ((net_total_price_per_leg * admin_percentage) / 100);
                    //    net_tax_price_per_leg = net_tax_price_per_leg + ((net_tax_price_per_leg * admin_percentage) / 100);
                    //}
                    #endregion;


                    net_totalprice = net_totalprice + net_total_price_per_leg;
                    net_basefare = net_basefare + net_base_price_per_leg;
                    net_tax = net_tax + net_tax_price_per_leg;


                    double base_price_per_leg = Convert.ToDouble((net_base_price_per_leg * currency_exchangerate).ToString("#0.00"));
                    double total_price_per_leg = Convert.ToDouble((net_total_price_per_leg * currency_exchangerate).ToString("#0.00"));
                    //double base_price_per_leg = Convert.ToDouble((net_base_price_per_leg).ToString("#0.00"));
                    //double total_price_per_leg = Convert.ToDouble((net_total_price_per_leg).ToString("#0.00"));
                    double tax_price_per_leg = total_price_per_leg - base_price_per_leg;

                    int PaxType = Request_passengers_find.paxtype;
                    string PaxType_text = "";
                    if (PaxType == (int)PaxtypeEnum.Adult)
                    {
                        PaxType_text = "Adult";
                    }
                    else if (PaxType == (int)PaxtypeEnum.Child)
                    {
                        PaxType_text = "Child";
                    }
                    else if (PaxType == (int)PaxtypeEnum.Infant)
                    {
                        PaxType_text = "Infant";
                    }

                    tarriffinfo.Add(new Models.Flight.DetailResponse.tarriffinfo()
                    {
                        api_baseprice = api_base_price_per_leg,
                        api_tax = api_tax_price_per_leg,
                        api_totalprice = api_total_price_per_leg,

                        net_baseprice = net_base_price_per_leg,
                        net_tax = net_tax_price_per_leg,
                        net_totalprice = net_total_price_per_leg,
                        baseprice = Convert.ToDouble(base_price_per_leg.ToString("#0.00")),
                        currency = currency,
                        currency_sign = currency_sign,
                        paxtype = PaxType,
                        paxtype_text = PaxType_text,
                        //paxid = travelerPricings.travelerId,
                        per_pax_total_price = Convert.ToDouble((total_price_per_leg / quantity).ToString("#0.00")),
                        quantity = quantity,
                        tax = Convert.ToDouble(tax_price_per_leg.ToString("#0.00")),
                        totalprice = Convert.ToDouble((total_price_per_leg).ToString("#0.00"))
                    });

                }

                Price_Data.tarriffinfo = tarriffinfo;


                double totalprice = Convert.ToDouble((net_totalprice * currency_exchangerate).ToString("#0.00"));
                double basefare = Convert.ToDouble((net_basefare * currency_exchangerate).ToString("#0.00"));
                double tax = Convert.ToDouble((net_tax * currency_exchangerate).ToString("#0.00"));

                Price_Data.api_seat_price = 0;
                Price_Data.net_seat_price = 0;
                Price_Data.seat_price = 0;

                Price_Data.api_base_price = api_basefare;
                Price_Data.api_currency = APIcurrency;
                Price_Data.api_tax_price = api_tax;
                Price_Data.api_total_price = api_totalprice;


                Price_Data.net_base_price = net_basefare;
                Price_Data.net_tax_price = net_tax;
                Price_Data.net_total_price = net_totalprice;

                Price_Data.base_price = basefare;
                Price_Data.currency = currency;
                Price_Data.currency_sign = currency_sign;
                Price_Data.tax_price = tax;
                Price_Data.total_price = totalprice;

                #endregion;

                #region Segment

                Itinerary_data = Selected_flight_result.OutboundInboundlist;

                foreach (var outboundInboundlist in Itinerary_data)
                {
                    foreach (var flightlist in outboundInboundlist.flightlist)
                    {
                        List<Models.Flight.ResultResponse.Checkinbaggage> CheckinBaggage = new List<Models.Flight.ResultResponse.Checkinbaggage>();

                        var departiata = flightlist.Departure.Iata;
                        var arrivaliata = flightlist.Arrival.Iata;


                        var segmentdata = Array.Find(result.Body.Fare_InformativePricingWithoutPNRReply.mainGroup.pricingGroupLevelGroup[0].fareInfoGroup.segmentLevelGroup, x => x.segmentInformation.boardPointDetails.trueLocationId == departiata && x.segmentInformation.offpointDetails.trueLocationId == arrivaliata);

                        if (segmentdata == null || segmentdata.baggageAllowance == null || segmentdata.baggageAllowance.baggageDetails == null)
                        {
                            flightlist.CheckinBaggage = CheckinBaggage;
                            continue;
                        }

                        #region RBD class

                        string RBD = "";

                        List<string> RBDLIST = new List<string>();

                        if (RBD == "")
                        {
                            RBD = segmentdata.segmentInformation.flightIdentification.bookingClass;
                        }
                        if (segmentdata.cabinGroup != null && segmentdata.cabinGroup.Length > 0)
                        {
                            foreach (var cabingroup in segmentdata.cabinGroup)
                            {
                                if (cabingroup.cabinSegment != null && cabingroup.cabinSegment.bookingClassDetails != null && cabingroup.cabinSegment.bookingClassDetails.Length > 0)
                                {
                                    foreach (var bookingcldetail in cabingroup.cabinSegment.bookingClassDetails)
                                    {
                                        if (RBDLIST.FindAll(x => x.ToString() == bookingcldetail.designator).Count == 0)
                                        {
                                            RBDLIST.Add(bookingcldetail.designator);
                                        }
                                    }
                                }
                            }
                        }
                        flightlist.RBDLIST = RBDLIST;
                        flightlist.RBD = RBD;
                        #endregion

                        #region Baggage

                        var baggunit = "pc";

                        if (segmentdata.baggageAllowance.baggageDetails != null && segmentdata.baggageAllowance.baggageDetails.quantityCode == "700")
                        {
                            baggunit = "K";
                        }

                        var baggagevalue = segmentdata.baggageAllowance.baggageDetails.freeAllowance + baggunit;

                        if (baggagevalue == "0K" || baggagevalue == "0pc" || baggagevalue == "0")
                        {
                            baggagevalue = "No Baggage";
                        }

                        CheckinBaggage.Add(new Models.Flight.ResultResponse.Checkinbaggage()
                        {
                            Type = baggunit,
                            Value = baggagevalue,
                        });

                        flightlist.CheckinBaggage = CheckinBaggage;
                        #endregion;
                    }
                }

                #endregion;

                #region Pax Data
                DateTime departuredate = Convert.ToDateTime(Itinerary_data[0].flightlist[0].Departure.Datetime);

                //int pax_index = 1;
                //foreach (var Travelerpricing in All_Request_passengers)
                //{
                //    bool passport_mandotary = false;
                //    bool residenceRequired = false;
                //    bool IssuanceCityRequired = false;
                //    bool contact_required = true;
                //    bool redressRequiredIfAny = false;


                //    string paxtype = Travelerpricing.paxtype;
                //    string paxtype_text = Travelerpricing.paxtype;
                //    TimeSpan expiry_min_days = (departuredate.AddMonths(3)) - DateTime.Now;
                //    TimeSpan expiry_max_days = (departuredate.AddYears(12)) - DateTime.Now;
                //    TimeSpan max_days = new TimeSpan();
                //    TimeSpan min_days = new TimeSpan();
                //    if (paxtype.ToLower() == "inf")
                //    {
                //        paxtype_text = "INFANT";
                //        min_days = DateTime.Now - (departuredate.AddYears(-2));
                //        max_days = DateTime.Now - DateTime.Now;
                //        paxtype = "INF";
                //    }
                //    else if (paxtype.ToLower() == "chd")
                //    {
                //        max_days = DateTime.Now - (departuredate.AddYears(-2));
                //        min_days = DateTime.Now - (departuredate.AddYears(-12));
                //        paxtype = "CHD";
                //    }
                //    else
                //    {
                //        max_days = DateTime.Now - (departuredate.AddYears(-12));
                //        min_days = DateTime.Now - (departuredate.AddYears(-200));
                //        paxtype = "ADT";
                //    }

                //    Pax_Data.Add(new Models.Flight.DetailResponse.Pax_Data()
                //    {
                //        associatedAdultId = Travelerpricing.associatedAdultId,
                //        api_traveller_id = Travelerpricing.index,
                //        index = pax_index,
                //        basic_details = new Models.Flight.DetailResponse.passenger_basic_details()
                //        {
                //            age = "",
                //            dateofbirth = "",
                //            firstname = "",
                //            gender = "",
                //            lastname = "",
                //            middlename = "",
                //            PassengerNationality = "",
                //            paxtype = paxtype,
                //            paxtype_text = "Passenger " + pax_index + " (" + paxtype_text + ")",
                //            title = "",
                //            meal_prefrence = "",
                //            seat_prefrence = "",
                //            frequent_flyer_number = "",
                //        },

                //        contact = new Models.Flight.DetailResponse.passengercontact() { email = "", title = "", firstname = "", lastname = "", phone_countrycode = "", phone_number = "" },

                //        Passport = new Models.Flight.DetailResponse.Passport_details() { country = "", expiry_date = "", expiry_maxdays = Convert.ToDouble(expiry_max_days.TotalDays), expiry_mindays = Convert.ToDouble(expiry_min_days.TotalDays), issue_date = "", number = "", city = "" },

                //        maxdays = Convert.ToDouble(max_days.TotalDays),

                //        mindays = Convert.ToDouble(min_days.TotalDays),

                //        required = new Models.Flight.DetailResponse.required_field_passenger() { passport_mandotary = passport_mandotary, PaxNameCharacterLimits = 70, redressRequiredIfAny = redressRequiredIfAny, residenceRequired = residenceRequired, contact = contact_required },

                //    });
                //    pax_index++;
                //}

                #endregion;

                //  Pax_Data = fill_passenger_details_auto(Pax_Data);

                #region Required Fields
                required_field.ContactNumber = true;
                required_field.Email = true;
                #endregion;

                string tkt_time_limit = "";
                //data.leadpax_detail = new Models.Flight.DetailResponse.leadpax_detail() { email = "", phone_countrycode = "", phone_number = "" };


                #region Expire
                expire.expire_datetime = DateTime.Now.AddMinutes(20).ToString();
                expire.seconds_time_limit = Convert.ToInt32((DateTime.Now.AddMinutes(20) - DateTime.Now).TotalSeconds);
                #endregion;

                string faresourcename = "";
                try
                {
                    if (Selected_flight_result != null && Selected_flight_result.farefamilylistdata != null && Selected_flight_result.farefamilylistdata.Count > 0)
                    {
                        faresourcename = Selected_flight_result.farefamilylistdata.Find(x => x.code == Selected_flight_result.faretype).name;
                    }
                }
                catch { }

                //data.expire = expire;
                data.sc = request.sc;
                data.id = Selected_flight_result.id;
                data.pcc = Selected_flight_result.pcc;
                data.price = Price_Data;
                data.Flight_Data = Itinerary_data;
                data.RequestPax = Request_passengers;
                //data.Pax_Data = Pax_Data;
                //data.required_field = required_field;
                data.isRefundable = Selected_flight_result.isRefundable;
                data.faresourcecode = Selected_flight_result.faretype;
                data.faresourcename = faresourcename;
                data.Fare_type = Selected_flight_result.faretype;
                data.fareFamiliesList = Selected_flight_result.farefamilylistdata;
                //data.search_criteria = Search_criteria_obj;

                data.supplier = (int)SupplierEnum.Amadeus;

                Basket_response.data = data;
                Basket_response.success = true;

                if (!string.IsNullOrWhiteSpace(xmlfiles))
                {
                    System.IO.File.WriteAllText(xmlfiles + request.sc + Selected_flight_result.id + "_flight_revalidate.json", System.Text.Json.JsonSerializer.Serialize(Basket_response), System.Text.ASCIIEncoding.UTF8);
                }

                #endregion Response
            }
            catch (Exception ex)
            {
                _errorLogRepository.AddErrorLog(ex, "AmadeusConfig->FlightDetail|~|" + request.sc + "", JsonConvert.SerializeObject(Selected_flight_result));
            }
            return Basket_response;
        }

        #endregion Flight detail

        #region Booking APIs

        #region Air_SellFromRecommendation
        public async Task<Air_SellFromRecommendation_response.System_air_sellfromrecommendation> Air_SellFromRecommendation(DetailResponse.FlightDetailResponse flightbasket)
        {
            Air_SellFromRecommendation_response.System_air_sellfromrecommendation System_air_sellfromrecommendation = new Air_SellFromRecommendation_response.System_air_sellfromrecommendation();

            #region Request
            string password = "U9MbJZjzR^EP";

            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string statuscode = "OK";
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();
            #region Request Body
            int pax_quantity = flightbasket.data.RequestPax.FindAll(x => x.paxtype != (int)PaxtypeEnum.Infant).Count;

            sb.AppendLine("   <Air_SellFromRecommendation><messageActionDetails><messageFunctionDetails><messageFunction>183</messageFunction><additionalMessageFunction>M1</additionalMessageFunction></messageFunctionDetails></messageActionDetails>  ");

            int itemNumberindex = 0;

            foreach (var OutboundInbounddata in flightbasket.data.Flight_Data)
            {
                sb.AppendLine(" <itineraryDetails><originDestinationDetails><origin>" + OutboundInbounddata.flightlist[0].Departure.Iata + "</origin><destination>" + OutboundInbounddata.flightlist[OutboundInbounddata.flightlist.Count - 1].Arrival.Iata + "</destination></originDestinationDetails><message><messageFunctionDetails><messageFunction>183</messageFunction></messageFunctionDetails></message>");

                //  string RBD = OutboundInbounddata.flightlist[0].RBDLIST.Count > 1 ? (OutboundInbounddata.flightlist[0].RBDLIST.Count >= itemNumberindex ? OutboundInbounddata.flightlist[0].RBDLIST[itemNumberindex] : OutboundInbounddata.flightlist[0].RBD) : OutboundInbounddata.flightlist[0].RBD;

                foreach (var legs in OutboundInbounddata.flightlist)
                {
                    string FlightIndicator_str = "";
                    if (legs.availabilityCnxType_slice_dice != null && legs.availabilityCnxType_slice_dice != "")
                    {
                        FlightIndicator_str = "<flightTypeDetails><flightIndicator>" + legs.availabilityCnxType_slice_dice + "</flightIndicator></flightTypeDetails>";
                    }
                    //sb.AppendLine("    <itineraryDetails><originDestinationDetails><origin>" + legs.Departure.Iata + "</origin><destination>" + legs.Arrival.Iata + "</destination></originDestinationDetails><message><messageFunctionDetails><messageFunction>183</messageFunction></messageFunctionDetails></message><segmentInformation><travelProductInformation><flightDate><departureDate>" + Convert.ToDateTime(legs.Departure.Datetime).ToString("ddMMyy") + "</departureDate></flightDate><boardPointDetails><trueLocationId>" + legs.Departure.Iata + "</trueLocationId></boardPointDetails><offpointDetails><trueLocationId>" + legs.Arrival.Iata + "</trueLocationId></offpointDetails><companyDetails><marketingCompany>" + legs.MarketingAirline.code + "</marketingCompany></companyDetails><flightIdentification><flightNumber>" + legs.MarketingAirline.number + "</flightNumber><bookingClass>" + legs.RBD + "</bookingClass></flightIdentification>" + FlightIndicator_str + "</travelProductInformation><relatedproductInformation><quantity>" + pax_quantity + "</quantity><statusCode>NN</statusCode></relatedproductInformation></segmentInformation></itineraryDetails> ");

                    string RBD = legs.RBD;

                    sb.AppendLine(" <segmentInformation><travelProductInformation><flightDate><departureDate>" + Convert.ToDateTime(legs.Departure.Datetime).ToString("ddMMyy") + "</departureDate></flightDate><boardPointDetails><trueLocationId>" + legs.Departure.Iata + "</trueLocationId></boardPointDetails><offpointDetails><trueLocationId>" + legs.Arrival.Iata + "</trueLocationId></offpointDetails><companyDetails><marketingCompany>" + legs.MarketingAirline.code + "</marketingCompany></companyDetails><flightIdentification><flightNumber>" + legs.MarketingAirline.number + "</flightNumber><bookingClass>" + RBD + "</bookingClass></flightIdentification>" + FlightIndicator_str + "</travelProductInformation><relatedproductInformation><quantity>" + pax_quantity + "</quantity><statusCode>NN</statusCode></relatedproductInformation></segmentInformation>");
                }
                sb.AppendLine("</itineraryDetails>");

                itemNumberindex++;
            }

            sb.AppendLine("  </Air_SellFromRecommendation>");
            #endregion;

            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                + "<s:Header>"
                + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID>"
                + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "ITAREQ_05_2_IA</a:Action>"
                //+ "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                //+ "<awsse:Session TransactionStatusCode=\"Start\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\"/>"
                //+ "<awsse:SessionId>" + data.sessionData.Split('|')[0]
                //+ "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(Convert.ToInt32(data.sessionData.Split('|')[1]) + 1) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                //+ data.sessionData.Split('|')[2] + "</awsse:SecurityToken> </awsse:Session>"
                + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                + "<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">"
                + "<oas:UsernameToken xmlns:oas=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\""
                + " xmlns:oas1=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" oas1:Id=\"UsernameToken-1\">"
                + "<oas:Username>" + _configuration["FlightSettings:AirUserName"] + "</oas:Username>"
                + "<oas:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" + encodedNonce + "</oas:Nonce>"
                + "<oas:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">" + passSHA + "</oas:Password>"
                + "<oas1:Created>" + formattedTimestamp + "</oas1:Created>"
                + "</oas:UsernameToken>"
                + "</Security>"
                + "<h:AMA_SecurityHostedUser xmlns:h=\"http://xml.amadeus.com/2010/06/Security_v1\">"
                + "<h:UserID POS_Type=\"1\" PseudoCityCode=\"" + _configuration["FlightSettings:AirOfficeID"] + "\" AgentDutyCode=\"" + _configuration["FlightSettings:AirDutyCode"]
                + "\" RequestorType=\"U\"/>"
                + "</h:AMA_SecurityHostedUser>"
                + "<awsse:Session TransactionStatusCode=\"Start\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\"/>"
                + "</s:Header>"
                + "<s:Body>"
                + sb.ToString()
                + "</s:Body>"
                + "</s:Envelope>", null, "application/xml");


            #endregion Request

            #region Response

            var requestContent = content.ReadAsStringAsync().Result;

            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "ITAREQ_05_2_IA";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);


            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(Path.Combine(xmlfiles, flightbasket.data.id + "_Air_SellFromRecommendation_Request.xml"), requestContent, Encoding.UTF8);
                File.WriteAllText(Path.Combine(xmlfiles, flightbasket.data.id + "_Air_SellFromRecommendation_Response.xml"), re ?? string.Empty, Encoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Air_SellFromRecommendation_response.Envelope));
            StringReader rdr = new StringReader(re);
            com.ThirdPartyAPIs.Amadeus.Flight.Air_SellFromRecommendation_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Air_SellFromRecommendation_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            //System.IO.File.WriteAllText(xmlfiles + flightbasket.data.id + "_Air_SellFromRecommendation_Response.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);

            if (result == null || result.Body == null || result.Body.Air_SellFromRecommendationReply == null || result.Body.Air_SellFromRecommendationReply.itineraryDetails == null || result.Body.Air_SellFromRecommendationReply.itineraryDetails.Length == 0)
            {
                return System_air_sellfromrecommendation;
            }


            foreach (Amadeus_wsdl.Air_SellFromRecommendationReplyItineraryDetails itineraryDetails in result.Body.Air_SellFromRecommendationReply.itineraryDetails)
            {

                if (itineraryDetails.errorItinerarylevel != null)
                {
                    System_air_sellfromrecommendation.success = false;
                    return System_air_sellfromrecommendation;
                }

                foreach (var segment in itineraryDetails.segmentInformation)
                {
                    if (segment.actionDetails == null || segment.actionDetails.statusCode == null || segment.actionDetails.statusCode[0] != "OK" || segment.actionDetails.quantity != pax_quantity.ToString())
                    {


                        System_air_sellfromrecommendation.success = false;
                        return System_air_sellfromrecommendation;
                    }
                endloop:;
                    System_air_sellfromrecommendation.success = true;
                }
            }


            #endregion Response 

            System_air_sellfromrecommendation.SecurityToken = result.Header.Session.SecurityToken;
            System_air_sellfromrecommendation.SessionId = result.Header.Session.SessionId;
            return System_air_sellfromrecommendation;
        }

        #endregion Air_SellFromRecommendation


        #region Fare_PricePNRWithBookingClass
        public async Task<Fare_PricePNRWithBookingClass_response.Envelope> Fare_PricePNRWithBookingClass(DetailResponse.FlightDetailResponse flightbasket, string SequenceNumber, Air_SellFromRecommendation_response.System_air_sellfromrecommendation IS_FLIGHT_AVAILABLE_FOR_BOOK)
        {

            #region Request
            string password = "U9MbJZjzR^EP";

            string str = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();


            #region Request Body

            //string Validating_carrier_code = dt_flight_details.Rows[0]["marketing_airline_code"].ToString();
            string Validating_carrier_code = flightbasket.data.Flight_Data[0].flightlist[0].MarketingAirline.code.ToString();

            string farefamilydata = "";

            //if (dt_main_table.Rows[0]["faretype"].ToString() != null && dt_main_table.Rows[0]["faretype"].ToString() != "")
            if (!string.IsNullOrEmpty(flightbasket.data.Fare_type))
            {
                farefamilydata += "<pricingOptionGroup><pricingOptionKey><pricingOptionKey>PFF</pricingOptionKey></pricingOptionKey><optionDetail><criteriaDetails><attributeType>FF</attributeType><attributeDescription>" + flightbasket.data.Fare_type + "</attributeDescription></criteriaDetails></optionDetail></pricingOptionGroup>";
            }

            //string fopoption = "<pricingOptionGroup><pricingOptionKey><pricingOptionKey>RN</pricingOptionKey></pricingOptionKey></pricingOptionGroup><pricingOptionGroup><pricingOptionKey><pricingOptionKey>RLA</pricingOptionKey></pricingOptionKey></pricingOptionGroup>";

            //<pricingOptionGroup><pricingOptionKey><pricingOptionKey>RLO</pricingOptionKey></pricingOptionKey></pricingOptionGroup>

            sb.AppendLine("<Fare_PricePNRWithBookingClass>" + farefamilydata + "<pricingOptionGroup><pricingOptionKey><pricingOptionKey>RP</pricingOptionKey></pricingOptionKey></pricingOptionGroup><pricingOptionGroup><pricingOptionKey><pricingOptionKey>RLO</pricingOptionKey></pricingOptionKey></pricingOptionGroup><pricingOptionGroup><pricingOptionKey><pricingOptionKey>VC</pricingOptionKey></pricingOptionKey><carrierInformation><companyIdentification><otherCompany>" + Validating_carrier_code + "</otherCompany></companyIdentification></carrierInformation></pricingOptionGroup><pricingOptionGroup><pricingOptionKey><pricingOptionKey>FCO</pricingOptionKey></pricingOptionKey><currency><firstCurrencyDetails><currencyQualifier>FCO</currencyQualifier><currencyIsoCode>" + APIcurrency + "</currencyIsoCode></firstCurrencyDetails></currency></pricingOptionGroup></Fare_PricePNRWithBookingClass>");

            //sb.AppendLine("<Fare_PricePNRWithBookingClass><pricingOptionGroup><pricingOptionKey><pricingOptionKey>RP</pricingOptionKey></pricingOptionKey></pricingOptionGroup></Fare_PricePNRWithBookingClass>");

            #endregion;

            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                     + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                     + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                     + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                     + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                     + "<s:Header>"
                     + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID> "
                     + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "TPCBRQ_23_2_1A</a:Action>"
                     + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                     + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                     + "<awsse:SessionId>" + IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId
                     + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(SequenceNumber) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                     + IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken + "</awsse:SecurityToken> </awsse:Session>"
                     + "</s:Header>"
                     + "<s:Body>"
                     + sb.ToString()
                     + "</s:Body>"
                     + "</s:Envelope>", null, "application/xml");

            #endregion Request

            #region Response

            var requestContent = content.ReadAsStringAsync().Result;
            //reqt.Headers.Add("soapAction", _configuration["FlightSettings:AirSoapAction"] + "ITAREQ_05_2_IA");
            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "TPCBRQ_23_2_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(Path.Combine(xmlfiles, flightbasket.data.id + "_Fare_PricePNRWithBookingClass_Request.xml"), requestContent, Encoding.UTF8);
                File.WriteAllText(Path.Combine(xmlfiles, flightbasket.data.id + "_Fare_PricePNRWithBookingClass_Response.xml"), re ?? string.Empty, Encoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Fare_PricePNRWithBookingClass_response.Envelope));
            StringReader rdr = new StringReader(re);
            com.ThirdPartyAPIs.Amadeus.Flight.Fare_PricePNRWithBookingClass_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Fare_PricePNRWithBookingClass_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();
            System.IO.File.WriteAllText(xmlfiles + flightbasket.data.id + "_Fare_PricePNRWithBookingClass_Response.json", System.Text.Json.JsonSerializer.Serialize(result), System.Text.ASCIIEncoding.UTF8);
            if (result != null && result.Body != null && result.Body.Fare_PricePNRWithBookingClassReply != null && result.Body.Fare_PricePNRWithBookingClassReply.Length > 0)
            {
                result.success = true;
            }

            #endregion Response

            return result;

        }

        #endregion Fare_PricePNRWithBookingClass

        #region Security SignOut
        public async Task<Security_SignOut_response.Envelope> Security_SignOut(string SequenceNumber, Air_SellFromRecommendation_response.System_air_sellfromrecommendation IS_FLIGHT_AVAILABLE_FOR_BOOK)
        {

            #region Request
            string password = "U9MbJZjzR^EP";

            string str = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();

            sb.Append("<Security_SignOut />");

            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                + "<s:Header>"
                + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID> "
                + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "VLSSOQ_04_1_1A</a:Action>"
                + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                + "<awsse:SessionId>" + IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId
                + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(SequenceNumber) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                + IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken + "</awsse:SecurityToken> </awsse:Session>"
                + "</s:Header>"
                + "<s:Body>"
                + sb.ToString()
                + "</s:Body>"
                + "</s:Envelope>", null, "application/xml");

            #endregion Request

            //var reqt = new HttpRequestMessage(HttpMethod.Post, url);
            //reqt.Headers.Add("soapAction", _configuration["FlightSettings:AirSoapAction"] + "ITAREQ_05_2_IA");
            //reqt.Content = content;
            //var requestContent = content.ReadAsStringAsync().Result;
            ////SaveAmadeusLog(requestContent, "-", "Request Fare_PriceUpsellWithoutPNR");
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //var response = await _httpClient.SendAsync(reqt);
            //var re = response.Content.ReadAsStringAsync().Result;

            #region Response


            var requestContent = content.ReadAsStringAsync().Result;

            //string soapAction = _configuration["FlightSettings:AirSoapAction"] + "ITAREQ_05_2_IA";
            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "VLSSOQ_04_1_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);


            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                //File.WriteAllText(Path.Combine(xmlfiles, flightbasket.data.id + "_Security_SignOut_Request.xml"), requestContent, Encoding.UTF8);
                //File.WriteAllText(Path.Combine(xmlfiles, flightbasket.data.id + "_Security_SignOut_Response.xml"), re ?? string.Empty, Encoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope));
            StringReader rdr = new StringReader(re);
            com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope result = (com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            return result;

            #endregion Response
        }

        #endregion Security SignOut


        private async Task<com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope> PNR_AddMultiElements_1(FlightBookingResponseDTO ds, string SequenceNumber)
        {
            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
            com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope result = new com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope();
            if (ds == null)
            {
                return result;
            }
            FlightBookingDTO maintable = ds.Booking;
            List<PaxDetailDTO> paxdetails = ds.PaxDetails;
            List<FlightDetailDTO> flightdetails = ds.FlightDetails;
            List<FlightBaggageDTO> flightbaggage = ds.Baggages;
            List<PriceBreakdownDTO> pricebreakdown = ds.PriceBreakdowns;

            if (maintable == null || flightdetails == null || paxdetails == null || flightdetails.Count == 0 || paxdetails.Count == 0)
            {
                return result;
            }

            #region Request

            string password = "U9MbJZjzR^EP";
            string str = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();

            #region Request Body
            //string Validating_carrier_code = "YY";//dt_flight_details.Rows[0]["marketing_airline_code"].ToString();

            sb.AppendLine(" <PNR_AddMultiElements><pnrActions><optionCode>0</optionCode></pnrActions> ");

            #region Passenger Information

            List<PaxDetailDTO> infantpax = ds.PaxDetails.FindAll(x => x.paxtype == (int)PaxtypeEnum.Infant);

            int pax_index = 1;
            int adt_index = 0;
            string SSR_meal_seat_request = "";
            foreach (var dr in paxdetails)
            {
                if (dr.paxtype == (int)PaxtypeEnum.Infant)
                {
                    continue;
                }
                int quantity = 1;
                string Infant_passenger_details = "";
                string infantIndicator = "";
                //if (dr["associatedAdultId"].ToString() != "" && dt_pax_details.Select("paxtype='INF' and api_traveller_id='" + dr["associatedAdultId"].ToString() + "'").Length > 0)
                //{
                //    foreach (DataRow dr_infant in dt_pax_details.Select("paxtype='INF' and api_traveller_id='" + dr["associatedAdultId"].ToString() + "'"))
                //    {
                //        quantity++;
                //        infantIndicator = "<infantIndicator>3</infantIndicator>";
                //        Infant_passenger_details = "  <passengerData><travellerInformation><traveller><surname>" + dr_infant["lastname"].ToString().Trim() + "</surname></traveller><passenger><firstName>" + (dr_infant["firstname"].ToString() + " " + dr_infant["middlename"].ToString()).Trim() + "</firstName><type>INF</type></passenger></travellerInformation><dateOfBirth><dateAndTimeDetails><date>" + Convert.ToDateTime(dr_infant["dateofbirth"].ToString().Trim()).ToString("ddMMMyy") + "</date></dateAndTimeDetails></dateOfBirth></passengerData>";
                //    }
                //}

                int ptype = dr.paxtype;
                string paxtype = "";

                if (ptype == (int)PaxtypeEnum.Child)
                {
                    paxtype = "CNN";
                }
                string date = "<date>" + Convert.ToDateTime(dr.dateofbirth).ToString("ddMMMyy") + "</date>";
                if (ptype == (int)PaxtypeEnum.Adult)
                {
                    paxtype = "";
                    date = "";

                    if (infantpax != null && infantpax.Count > 0 && adt_index < infantpax.Count)
                    {
                        var dr_infant = infantpax[adt_index];
                        quantity++;
                        infantIndicator = "<infantIndicator>3</infantIndicator>";
                        Infant_passenger_details = "  <passengerData><travellerInformation><traveller><surname>" + dr_infant.lastname + "</surname></traveller><passenger><firstName>" + dr_infant.firstname + " " + dr_infant.middlename + "</firstName><type>INF</type></passenger></travellerInformation><dateOfBirth><dateAndTimeDetails><date>" + Convert.ToDateTime(dr_infant.dateofbirth).ToString("ddMMMyy") + "</date></dateAndTimeDetails></dateOfBirth></passengerData>";
                    }
                    adt_index++;
                }
                else
                {
                    paxtype = "<type>" + paxtype + "</type>";
                }

                string title = dr.title;

                sb.AppendLine("         <travellerInfo><elementManagementPassenger><reference><qualifier>PR</qualifier><number>" + pax_index + "</number></reference><segmentName>NM</segmentName></elementManagementPassenger><passengerData><travellerInformation><traveller><surname>" + dr.lastname + "</surname><quantity>" + quantity + "</quantity></traveller><passenger><firstName>" + (dr.firstname + " " + dr.middlename + " " + title).Trim() + "</firstName>" + paxtype + "" + infantIndicator + "</passenger></travellerInformation><dateOfBirth><dateAndTimeDetails>" + date + "</dateAndTimeDetails></dateOfBirth></passengerData>" + Infant_passenger_details + "</travellerInfo> ");

                pax_index++;
            }
            #endregion;


            #region dataElementsMaster 
            sb.AppendLine(" <dataElementsMaster><marker1 /> ");

            int dataElementsMaster_index = 1;

            #region Contact Basic Details
            sb.AppendLine("  <dataElementsIndiv><elementManagementData><reference><qualifier>OT</qualifier><number>" + dataElementsMaster_index + "</number></reference><segmentName>AP</segmentName></elementManagementData><freetextData><freetextDetail><subjectQualifier>3</subjectQualifier><type>6</type></freetextDetail><longFreetext>Gds.aadviktech.com</longFreetext></freetextData></dataElementsIndiv> ");


            dataElementsMaster_index++;

            //Request_str.AppendLine("  <dataElementsIndiv><elementManagementData><reference><qualifier>OT</qualifier><number>" + dataElementsMaster_index + "</number></reference><segmentName>AP</segmentName></elementManagementData><freetextData><freetextDetail><subjectQualifier>3</subjectQualifier><type>P02</type></freetextDetail><longFreetext>" + dt_main_table.Rows[0]["email"].ToString() + "</longFreetext></freetextData></dataElementsIndiv> ");

            sb.AppendLine("<dataElementsIndiv><elementManagementData><segmentName>SSR</segmentName></elementManagementData><serviceRequest><ssr><type>CTCE</type><status>HK</status><quantity>1</quantity><companyId>YY</companyId><freetext>" + maintable.email.Replace("@", "//") + "</freetext></ssr></serviceRequest><referenceForDataElement><reference><qualifier>PR</qualifier><number>1</number></reference></referenceForDataElement></dataElementsIndiv>");

            sb.AppendLine("<dataElementsIndiv><elementManagementData><segmentName>SSR</segmentName></elementManagementData><serviceRequest><ssr><type>CTCM</type><freetext>" + maintable.phone_country_code + "" + maintable.phone_number + "</freetext></ssr></serviceRequest><referenceForDataElement><reference><qualifier>PR</qualifier><number>1</number></reference></referenceForDataElement></dataElementsIndiv>");

            //Request_str.AppendLine("<dataElementsIndiv><elementManagementData><segmentName>SSR</segmentName></elementManagementData><serviceRequest><ssr><type>CTCR-REFUSED</type><freetext>Customer refused</freetext></ssr></serviceRequest><referenceForDataElement><reference><qualifier>PR</qualifier><number>1</number></reference></referenceForDataElement></dataElementsIndiv>");
            #endregion;

            sb.AppendLine("</dataElementsMaster>");
            #endregion;

            sb.AppendLine("  </PNR_AddMultiElements>");
            #endregion;



            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                    + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                    + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                    + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                    + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                    + "<s:Header>"
                    + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID> "
                    + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "PNRADD_21_1_1A</a:Action>"
                    + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                    + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                    + "<awsse:SessionId>" + maintable.api_SessionId
                    + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(SequenceNumber) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                    + maintable.api_SecurityToken + "</awsse:SecurityToken> </awsse:Session>"
                    + "</s:Header>"
                    + "<s:Body>"
                    + sb.ToString()
                    + "</s:Body>"
                    + "</s:Envelope>", null, "application/xml");

            #endregion;

            #region Response

            var requestContent = content.ReadAsStringAsync().Result;

            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "PNRADD_21_1_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(xmlfiles + "PNR_AddMultiElements_1_request_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", requestContent, System.Text.ASCIIEncoding.UTF8);
                File.WriteAllText(xmlfiles + "PNR_AddMultiElements_1_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", re, System.Text.ASCIIEncoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope));
            StringReader rdr = new StringReader(re);
            result = (com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            #endregion;

            return result;
        }

        private async Task<com.ThirdPartyAPIs.Amadeus.Flight.Ticket_CreateTSTFromPricing_response.Envelope> Ticket_CreateTSTFromPricing(com.ThirdPartyAPIs.Amadeus.Flight.Fare_PricePNRWithBookingClass_response.Envelope Fare_PricePNRWithBookingClass_response, FlightBookingResponseDTO ds, string SequenceNumber)
        {
            com.ThirdPartyAPIs.Amadeus.Flight.Ticket_CreateTSTFromPricing_response.Envelope result = new Amadeus.Flight.Ticket_CreateTSTFromPricing_response.Envelope();
            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
            if (ds == null)
            {
                return result;
            }
            FlightBookingDTO maintable = ds.Booking;
            List<PaxDetailDTO> paxdetails = ds.PaxDetails;
            List<FlightDetailDTO> flightdetails = ds.FlightDetails;
            List<FlightBaggageDTO> flightbaggage = ds.Baggages;
            List<PriceBreakdownDTO> pricebreakdown = ds.PriceBreakdowns;

            if (maintable == null || flightdetails == null || paxdetails == null || flightdetails.Count == 0 || paxdetails.Count == 0)
            {
                return result;
            }

            #region Request
            string password = "U9MbJZjzR^EP";

            string str = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();

            #region Request Body

            sb.AppendLine(" <Ticket_CreateTSTFromPricing> ");

            int pax_index = 1;
            foreach (var Fare_PricePNRWithBookingClassReply in Fare_PricePNRWithBookingClass_response.Body.Fare_PricePNRWithBookingClassReply)
            {
                sb.AppendLine(" <psaList><itemReference><referenceType>TST</referenceType><uniqueReference>" + Fare_PricePNRWithBookingClassReply.fareReference.uniqueReference + "</uniqueReference></itemReference></psaList> ");
            }

            sb.AppendLine(" </Ticket_CreateTSTFromPricing> ");


            #endregion;

            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                   + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                   + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                   + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                   + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                   + "<s:Header>"
                   + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID> "
                   + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "TAUTCQ_04_1_1A</a:Action>"
                   + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                   + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                   + "<awsse:SessionId>" + maintable.api_SessionId
                   + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(SequenceNumber) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                   + maintable.api_SecurityToken + "</awsse:SecurityToken> </awsse:Session>"
                   + "</s:Header>"
                   + "<s:Body>"
                   + sb.ToString()
                   + "</s:Body>"
                   + "</s:Envelope>", null, "application/xml");

            #endregion;

            #region Response

            var requestContent = content.ReadAsStringAsync().Result;

            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "TAUTCQ_04_1_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(xmlfiles + "Ticket_CreateTSTFromPricing_request_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", requestContent, System.Text.ASCIIEncoding.UTF8);
                File.WriteAllText(xmlfiles + "Ticket_CreateTSTFromPricing_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", re, System.Text.ASCIIEncoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.Ticket_CreateTSTFromPricing_response.Envelope));
            StringReader rdr = new StringReader(re);
            result = (com.ThirdPartyAPIs.Amadeus.Flight.Ticket_CreateTSTFromPricing_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            File.WriteAllText(xmlfiles + "Ticket_CreateTSTFromPricing_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);

            #endregion;

            return result;
        }

        private async Task<com.ThirdPartyAPIs.Amadeus.Flight.FOP_CreateFormOfPayment_response.Envelope> FOP_CreateFormOfPayment(FlightBookingResponseDTO ds, string SequenceNumber)
        {
            com.ThirdPartyAPIs.Amadeus.Flight.FOP_CreateFormOfPayment_response.Envelope result = new FOP_CreateFormOfPayment_response.Envelope();
            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");
            if (ds == null)
            {
                return result;
            }
            FlightBookingDTO maintable = ds.Booking;
            List<PaxDetailDTO> paxdetails = ds.PaxDetails;
            List<FlightDetailDTO> flightdetails = ds.FlightDetails;
            List<FlightBaggageDTO> flightbaggage = ds.Baggages;
            List<PriceBreakdownDTO> pricebreakdown = ds.PriceBreakdowns;

            if (maintable == null || flightdetails == null || paxdetails == null || flightdetails.Count == 0 || paxdetails.Count == 0)
            {
                return result;
            }
            #region Request
            string password = "U9MbJZjzR^EP";
            string str = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                + "<s:Header>"
                + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID> "
                + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "TFOPCQ_15_4_1A</a:Action>"
                + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                + "<awsse:SessionId>" + maintable.api_SessionId
                + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(SequenceNumber) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                + maintable.api_SecurityToken + "</awsse:SecurityToken> </awsse:Session>"
                + "</s:Header>"
                + "<s:Body>"
                + "<FOP_CreateFormOfPayment>"
                + "<transactionContext>"
                + "<transactionDetails>"
                + "<code>FP</code>"
                + "</transactionDetails>"
                + "</transactionContext>"
                + "<fopGroup>"
                + "<fopReference/>"
                + "<mopDescription>"
                + "<fopSequenceNumber>"
                + "<sequenceDetails>"
                + "<number>1</number>"
                + "</sequenceDetails>"
                + "</fopSequenceNumber>"
                + "<mopDetails>"
                + "<fopPNRDetails>"
                + "<fopDetails>"
                + "<fopCode>CASH</fopCode>"
                + "</fopDetails>"
                + "</fopPNRDetails>"
                + "</mopDetails>"
                + "</mopDescription>"
                + "</fopGroup>"
                + "</FOP_CreateFormOfPayment>"
                + "</s:Body>"
                + "</s:Envelope>", null, "application/xml");
            #endregion;

            #region Response

            var requestContent = content.ReadAsStringAsync().Result;

            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "TFOPCQ_15_4_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(xmlfiles + "FOP_CreateFormOfPayment_request_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", requestContent, System.Text.ASCIIEncoding.UTF8);
                File.WriteAllText(xmlfiles + "FOP_CreateFormOfPayment_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", re, System.Text.ASCIIEncoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.FOP_CreateFormOfPayment_response.Envelope));
            StringReader rdr = new StringReader(re);
            result = (com.ThirdPartyAPIs.Amadeus.Flight.FOP_CreateFormOfPayment_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            File.WriteAllText(xmlfiles + "FOP_CreateFormOfPayment_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);

            #endregion;


            return result;
        }

        private async Task<com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope> PNR_AddMultiElements_2(FlightBookingResponseDTO ds, com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope PNR_AddMultiElements_1, string SequenceNumber)
        {
            com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope result = new Amadeus.Flight.PNR_AddMultiElements_response.Envelope();

            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");

            #region Request
            if (ds == null)
            {
                return result;
            }
            FlightBookingDTO maintable = ds.Booking;
            List<PaxDetailDTO> paxdetails = ds.PaxDetails;
            List<FlightDetailDTO> flightdetails = ds.FlightDetails;
            List<FlightBaggageDTO> flightbaggage = ds.Baggages;
            List<PriceBreakdownDTO> pricebreakdown = ds.PriceBreakdowns;

            if (maintable == null || flightdetails == null || paxdetails == null || flightdetails.Count == 0 || paxdetails.Count == 0)
            {
                return result;
            }

            string password = "U9MbJZjzR^EP";
            string str = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();

            #region Request Body

            #region Find TSA required
            bool TSA_required = false;
            foreach (var originDestinationDetails in PNR_AddMultiElements_1.Body.PNR_Reply.originDestinationDetails)
            {
                var findtsa = Array.Find(originDestinationDetails.itineraryInfo, item => item.elementsIndicators != null && item.elementsIndicators.statusInformation != null && item.elementsIndicators.statusInformation.indicator != null && item.elementsIndicators.statusInformation.indicator == "TSA");
                if (findtsa != null)
                {
                    TSA_required = true;
                    break;
                }
            }
            #endregion

            #region Passenger Information
            string SSR_passport_details_request = "";
            string SSR_meal_seat_request = "";
            foreach (var dr in paxdetails)
            {
                string firstname = (dr.firstname + " " + dr.middlename).Trim().ToLower();
                string lastname = dr.lastname.Trim().ToLower();
                string title = (dr.title).Trim().ToLower();
                string dob = Convert.ToDateTime(dr.dateofbirth.ToString().Trim()).ToString("ddMMyyyy");

                string pax_index = "";

                if (dr.paxtype != (int)PaxtypeEnum.Infant)
                {
                    firstname = (firstname + " " + title).Trim().ToLower();
                }
                foreach (var travellerInfo in PNR_AddMultiElements_1.Body.PNR_Reply.travellerInfo)
                {
                    try
                    {
                        var find_passenger = Array.Find(travellerInfo.passengerData, item => item.travellerInformation.traveller.surname.ToLower().Trim() == lastname && item.travellerInformation.passenger[0].firstName.ToLower().Trim() == firstname &&
                 item.dateOfBirth.dateAndTimeDetails.date.ToLower().Trim() == dob);

                        if (find_passenger != null)
                        {
                            pax_index = travellerInfo.elementManagementPassenger.reference.number;
                            break;
                        }
                    }
                    catch
                    {

                        var find_passenger = Array.Find(travellerInfo.passengerData, item => item.travellerInformation.traveller.surname.ToLower().Trim() == lastname && item.travellerInformation.passenger[0].firstName.ToLower().Trim() == firstname);

                        if (find_passenger != null)
                        {
                            pax_index = travellerInfo.elementManagementPassenger.reference.number;
                            break;
                        }
                    }
                }

                if (pax_index == "")
                {
                    return result;
                }

                string Passport_gender = dr.gender == (int)GenderEnum.Male ? "M" : "F";

                if (dr.paxtype == (int)PaxtypeEnum.Infant)
                {
                    if (Passport_gender.ToLower() == "m")
                    {
                        Passport_gender = "MI";
                    }
                    else
                    {
                        Passport_gender = "FI";
                    }
                }

                string Passport_details_docs = "";

                Passport_details_docs = "P-" + dr.passport_country + "-" + dr.passport_number + "-" + dr.nationality + "-" + Convert.ToDateTime(dr.dateofbirth).ToString("ddMMMyy") + "-" + Passport_gender + "-" + (dr.passport_expirydate != null ? Convert.ToDateTime(dr.passport_expirydate.ToString()).ToString("ddMMMyy") : "") + "-" + dr.lastname + "-" + dr.firstname + "";

                SSR_passport_details_request = SSR_passport_details_request + "<dataElementsIndiv><elementManagementData><segmentName>SSR</segmentName></elementManagementData><serviceRequest><ssr><type>DOCS</type><status>HK</status><quantity>1</quantity><companyId>YY</companyId><freetext>" + Passport_details_docs + "</freetext></ssr></serviceRequest><referenceForDataElement><reference><qualifier>PT</qualifier><number>" + pax_index + "</number></reference></referenceForDataElement></dataElementsIndiv>";

                if (TSA_required)
                {
                    Passport_details_docs = "----" + Convert.ToDateTime(dr.dateofbirth).ToString("ddMMMyy") + "-" + Passport_gender + "--" + dr.lastname + "-" + dr.firstname + "";

                    SSR_passport_details_request = "<dataElementsIndiv><elementManagementData><segmentName>SSR</segmentName></elementManagementData><serviceRequest><ssr><type>DOCS</type><status>HK</status><quantity>1</quantity><companyId>YY</companyId><freetext>" + Passport_details_docs + "</freetext></ssr></serviceRequest><referenceForDataElement><reference><qualifier>PT</qualifier><number>" + pax_index + "</number></reference></referenceForDataElement></dataElementsIndiv>";

                    //SSR_passport_details_request = SSR_passport_details_request + "<dataElementsIndiv><elementManagementData><segmentName>SSR</segmentName></elementManagementData><serviceRequest><ssr><type>DOCS</type><status>HK</status><quantity>1</quantity><companyId>YY</companyId><freetext>" + Passport_details_docs + "</freetext></ssr></serviceRequest><referenceForDataElement><reference><qualifier>PT</qualifier><number>" + pax_index + "</number></reference></referenceForDataElement></dataElementsIndiv>";
                }

                if (dr.paxtype == (int)PaxtypeEnum.Infant)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(dr.meal_preference))
                {
                    SSR_meal_seat_request = SSR_meal_seat_request + "<dataElementsIndiv><elementManagementData><segmentName>SSR</segmentName></elementManagementData><serviceRequest><ssr><type>" + dr.meal_preference + "</type><freetext/></ssr></serviceRequest><referenceForDataElement><reference><qualifier>PT</qualifier><number>" + pax_index + "</number></reference>" +
                         //"<reference><qualifier>ST</qualifier><number>1</number></reference>" +
                         "</referenceForDataElement></dataElementsIndiv>";
                    //  SSR_meal_seat_request = SSR_meal_seat_request + "<dataElementsIndiv><elementManagementData><segmentName>STR</segmentName></elementManagementData><seatGroup><seatRequest><seat><type>NSSA</type></seat></seatRequest></seatGroup><referenceForDataElement><reference><qualifier>PT</qualifier><number>" + pax_index + "</number></reference><reference><qualifier>ST</qualifier><number>1</number></reference></referenceForDataElement></dataElementsIndiv>";
                }

                if (!string.IsNullOrEmpty(dr.seat_preference))
                {
                    //  SSR_meal_seat_request = SSR_meal_seat_request + "<dataElementsIndiv><elementManagementData><segmentName>STR</segmentName></elementManagementData><serviceRequest><ssrb><seatType>" + dr["seat_preference"].ToString() + "</seatType></ssrb></serviceRequest><referenceForDataElement><reference><qualifier>PT</qualifier><number>" + pax_index + "</number></reference>" +
                    //   "<reference><qualifier>ST</qualifier><number>1</number></reference>" +
                    //   "</referenceForDataElement></dataElementsIndiv>";

                    SSR_meal_seat_request = SSR_meal_seat_request + "<dataElementsIndiv><elementManagementData><segmentName>STR</segmentName></elementManagementData><seatGroup><seatRequest><seat><type>NSSA</type></seat></seatRequest></seatGroup><referenceForDataElement><reference><qualifier>PT</qualifier><number>" + pax_index + "</number></reference><reference><qualifier>ST</qualifier><number>1</number></reference></referenceForDataElement></dataElementsIndiv>";
                }
            }
            #endregion;

            sb.AppendLine("  <PNR_AddMultiElements><pnrActions><optionCode>11</optionCode></pnrActions><dataElementsMaster><marker1 />" + SSR_meal_seat_request + SSR_passport_details_request + "<dataElementsIndiv><elementManagementData><reference><qualifier>OT</qualifier><number>2</number></reference><segmentName>TK</segmentName></elementManagementData><ticketElement><ticket><indicator>OK</indicator></ticket></ticketElement></dataElementsIndiv><dataElementsIndiv><elementManagementData><segmentName>RF</segmentName></elementManagementData><freetextData><freetextDetail><subjectQualifier>3</subjectQualifier><type>P22</type></freetextDetail><longFreetext>Flight Savers - Application</longFreetext></freetextData></dataElementsIndiv></dataElementsMaster></PNR_AddMultiElements>");

            #endregion;

            //string Request_string = Request_str.ToString();

            //File.WriteAllText(xml_files + "PNR_AddMultiElements_2_request_" + dt_main_table.Rows[0]["system_searchcode"].ToString() + dt_main_table.Rows[0]["system_faresourcecode"].ToString() + ".xml", Request_string, System.Text.ASCIIEncoding.UTF8);
            #endregion;
            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                   + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                   + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                   + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                   + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                   + "<s:Header>"
                   + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID> "
                   + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "PNRADD_21_1_1A</a:Action>"
                   + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                   + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                   + "<awsse:SessionId>" + maintable.api_SessionId
                   + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(SequenceNumber) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                   + maintable.api_SecurityToken + "</awsse:SecurityToken> </awsse:Session>"
                   + "</s:Header>"
                   + "<s:Body>"
                   + sb.ToString()
                   + "</s:Body>"
                   + "</s:Envelope>", null, "application/xml");


            #region Response

            var requestContent = content.ReadAsStringAsync().Result;

            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "PNRADD_21_1_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(xmlfiles + "PNR_AddMultiElements_2_request_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", requestContent, System.Text.ASCIIEncoding.UTF8);
                File.WriteAllText(xmlfiles + "PNR_AddMultiElements_2_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", re, System.Text.ASCIIEncoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope));
            StringReader rdr = new StringReader(re);
            result = (com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            File.WriteAllText(xmlfiles + "PNR_AddMultiElements_2_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);

            #endregion;


            return result;
        }


        private async Task<com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope> PNR_AddMultiElements_3(FlightBookingResponseDTO ds, com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope PNR_AddMultiElements_1, string SequenceNumber)
        {
            com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope result = new Amadeus.Flight.PNR_AddMultiElements_response.Envelope();
            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");

            if (ds == null)
            {
                return result;
            }
            FlightBookingDTO maintable = ds.Booking;
            List<PaxDetailDTO> paxdetails = ds.PaxDetails;
            List<FlightDetailDTO> flightdetails = ds.FlightDetails;
            List<FlightBaggageDTO> flightbaggage = ds.Baggages;
            List<PriceBreakdownDTO> pricebreakdown = ds.PriceBreakdowns;

            if (maintable == null || flightdetails == null || paxdetails == null || flightdetails.Count == 0 || paxdetails.Count == 0)
            {
                return result;
            }

            #region Request

            string password = "U9MbJZjzR^EP";
            string str = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();

            #region Request Body
            sb.AppendLine("   <PNR_AddMultiElements><pnrActions><optionCode>21</optionCode></pnrActions>");


            //Request_str.AppendLine("<dataElementsMaster><marker1 />");
            //"<dataElementsIndiv><elementManagementData><reference><qualifier>OT</qualifier><number>2</number></reference><segmentName>TK</segmentName></elementManagementData><ticketElement><ticket><indicator>OK</indicator></ticket></ticketElement></dataElementsIndiv>"

            //Request_str.AppendLine("<dataElementsIndiv><elementManagementData><segmentName>RF</segmentName></elementManagementData><freetextData><freetextDetail><subjectQualifier>3</subjectQualifier></freetextDetail><longFreetext>Flight Savers - Application</longFreetext></freetextData></dataElementsIndiv></dataElementsMaster>");

            sb.AppendLine("</PNR_AddMultiElements> ");

            #endregion;

            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                   + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                   + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                   + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                   + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                   + "<s:Header>"
                   + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID> "
                   + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "PNRADD_21_1_1A</a:Action>"
                   + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                   + "<awsse:Session TransactionStatusCode=\"InSeries\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\">"
                   + "<awsse:SessionId>" + maintable.api_SessionId
                   + "</awsse:SessionId> <awsse:SequenceNumber>" + Convert.ToInt32(SequenceNumber) + "</awsse:SequenceNumber> <awsse:SecurityToken>"
                   + maintable.api_SecurityToken + "</awsse:SecurityToken> </awsse:Session>"
                   + "</s:Header>"
                   + "<s:Body>"
                   + sb.ToString()
                   + "</s:Body>"
                   + "</s:Envelope>", null, "application/xml");

            //var reqt = new HttpRequestMessage(HttpMethod.Post, url);
            //reqt.Headers.Add("soapAction", _configuration["FlightSettings:AirSoapAction"] + "PNRADD_21_1_1A");
            //reqt.Content = content;
            //var requestContent = content.ReadAsStringAsync().Result;
            ////SaveAmadeusLog(requestContent, "-", "Request Fare_PriceUpsellWithoutPNR");
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //var response = await _httpClient.SendAsync(reqt);
            //var re = response.Content.ReadAsStringAsync().Result;

            #endregion;

            #region Response


            var requestContent = content.ReadAsStringAsync().Result;

            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "PNRADD_21_1_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);

            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(xmlfiles + "PNR_AddMultiElements_3_request_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", requestContent, System.Text.ASCIIEncoding.UTF8);
                File.WriteAllText(xmlfiles + "PNR_AddMultiElements_3_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".xml", re, System.Text.ASCIIEncoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope));
            StringReader rdr = new StringReader(re);
            result = (com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            File.WriteAllText(xmlfiles + "PNR_AddMultiElements_3_response_" + maintable.system_searchcode + maintable.system_faresourcecode + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);

            #endregion;

            return result;
        }


        internal async Task<com.ThirdPartyAPIs.Amadeus.Flight.PNR_Retrieve_response.Envelope> PNR_Retrieve(string PNR, string pcc)
        {
            com.ThirdPartyAPIs.Amadeus.Flight.PNR_Retrieve_response.Envelope result = new Amadeus.Flight.PNR_Retrieve_response.Envelope();
            var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");

            #region Request
            string password = "U9MbJZjzR^EP";
            string str = "", ticketno = "";
            var url = _configuration["FlightSettings:AirProductionURL"];
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            byte[] nonce = new byte[16];
            string status = "";
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonce);
            }
            DateTime timestamp = DateTime.UtcNow;
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            string encodedNonce = Convert.ToBase64String(nonce);
            string passSHA = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha1.ComputeHash(passwordBytes);

                byte[] timestampBytes = Encoding.ASCII.GetBytes(formattedTimestamp);
                byte[] combinedBytes = new byte[nonce.Length + timestampBytes.Length + passwordHash.Length];

                Buffer.BlockCopy(nonce, 0, combinedBytes, 0, nonce.Length);
                Buffer.BlockCopy(timestampBytes, 0, combinedBytes, nonce.Length, timestampBytes.Length);
                Buffer.BlockCopy(passwordHash, 0, combinedBytes, nonce.Length + timestampBytes.Length, passwordHash.Length);

                byte[] passSHABytes = sha1.ComputeHash(combinedBytes);
                passSHA = Convert.ToBase64String(passSHABytes);
            }
            StringBuilder sb = new();

            #region Request Body
            sb.AppendLine("<PNR_Retrieve><retrievalFacts><retrieve><type>2</type></retrieve><reservationOrProfileIdentifier><reservation><controlNumber>" + PNR + "</controlNumber></reservation></reservationOrProfileIdentifier></retrievalFacts></PNR_Retrieve>");
            #endregion;

            #endregion;



            #region Response
            //string Soap_action = "PNRRET_21_1_1A"; // for test

            var content = new StringContent("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\""
                    + " xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\""
                    + " xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\""
                    + " xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\""
                    + " xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\">"
                    + "<s:Header>"
                    + "<a:MessageID xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + guidString + "</a:MessageID>"
                    + "<a:Action xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirSoapAction"] + "PNRRET_14_2_1A</a:Action>"
                    + "<a:To xmlns:a=\"http://www.w3.org/2005/08/addressing\">" + _configuration["FlightSettings:AirProductionURL"] + "</a:To>"
                    + "<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">"
                    + "<oas:UsernameToken xmlns:oas=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\""
                    + " xmlns:oas1=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" oas1:Id=\"UsernameToken-1\">"
                    + "<oas:Username>" + _configuration["FlightSettings:AirUserName"] + "</oas:Username>"
                    + "<oas:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" + encodedNonce + "</oas:Nonce>"
                    + "<oas:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">" + passSHA + "</oas:Password>"
                    + "<oas1:Created>" + formattedTimestamp + "</oas1:Created>"
                    + "</oas:UsernameToken>"
                    + "</Security>"
                    + "<h:AMA_SecurityHostedUser xmlns:h=\"http://xml.amadeus.com/2010/06/Security_v1\">"
                    + "<h:UserID POS_Type=\"1\" PseudoCityCode=\"" + pcc + "\" AgentDutyCode=\"" + _configuration["FlightSettings:AirDutyCode"] + "\" RequestorType=\"U\"/>"
                    + "</h:AMA_SecurityHostedUser>"
                    + "<awsse:Session TransactionStatusCode=\"Start\" xmlns:awsse=\"http://xml.amadeus.com/2010/06/Session_v3\"/>"
                    + "</s:Header>"
                    + "<s:Body>"
                    + sb.ToString()
                    + "</s:Body>"
                    + "</s:Envelope>", null, "application/xml");

            //var reqt = new HttpRequestMessage(HttpMethod.Post, url);
            //reqt.Headers.Add("soapAction", _configuration["FlightSettings:AirSoapAction"] + "PNRRET_14_2_1A");
            //reqt.Content = content;
            //var requestContent = content.ReadAsStringAsync().Result;
            ////SaveAmadeusLog(requestContent, "-", "Request PNR_Retrieve");
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //var response = await _httpClient.SendAsync(reqt);
            //var re = response.Content.ReadAsStringAsync().Result;

            var requestContent = content.ReadAsStringAsync().Result;

            string soapAction = _configuration["FlightSettings:AirSoapAction"] + "PNRRET_14_2_1A";
            var re = await WebRequestUtilityAmadeus.InvokePostRequestAmadeus(url, soapAction, content);


            if (!string.IsNullOrWhiteSpace(xmlfiles))
            {
                if (!Directory.Exists(xmlfiles))
                    Directory.CreateDirectory(xmlfiles);
                File.WriteAllText(xmlfiles + "PNR_Retrieve_request_" + PNR + ".xml", requestContent, System.Text.ASCIIEncoding.UTF8);
                File.WriteAllText(xmlfiles + "PNR_Retrieve_response_" + PNR + ".xml", re, System.Text.ASCIIEncoding.UTF8);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(com.ThirdPartyAPIs.Amadeus.Flight.PNR_Retrieve_response.Envelope));
            StringReader rdr = new StringReader(re);
            result = (com.ThirdPartyAPIs.Amadeus.Flight.PNR_Retrieve_response.Envelope)serializer.Deserialize(rdr);
            rdr.Close();

            #endregion;


            return result;
        }


        #endregion Booking APIs


        #region Main Booking page Direct 

        public async Task<CommonResponse> BookingPageDirect(string systemrefrence = "")
        {
            CommonResponse Response = new CommonResponse();
            try
            {
                var xmlfiles = Path.Combine(_env.ContentRootPath, "XmlFiles/");

                FlightBookingDTO maintable = new FlightBookingDTO();
                List<PaxDetailDTO> paxdetails = new List<PaxDetailDTO>();
                List<FlightDetailDTO> flightdetails = new List<FlightDetailDTO>();
                List<FlightBaggageDTO> flightbaggage = new List<FlightBaggageDTO>();
                List<PriceBreakdownDTO> pricebreakdown = new List<PriceBreakdownDTO>();

                //var result = _genericRepository.LoadMultipleResultSets<List<UsersDTO>, List<UsersDTO>>("sp_GenericBulkInsert", user);

                //var result = await _genericRepository.LoadMultipleResultSets<FlightBookingDTO, FlightDetailDTO, PaxDetailDTO,PriceBreakdownDTO,FlightBaggageDTO, Object >("sp_temp_flight_booking", new { refr = systemrefrence });

                var result = await _genericRepository.LoadMultipleResultSets<FlightBookingDTO, FlightDetailDTO, PaxDetailDTO, PriceBreakdownDTO, FlightBaggageDTO, object>("sp_temp_flight_booking", new { refr = systemrefrence });

                //var result = await _genericRepository.LoadMultipleResultSets<object,object,object,object,object, object>( "sp_temp_flight_booking", new { refr = systemrefrence } );

                // Usage

                if (result == null)
                {
                    Response.Status = HttpStatusCode.NotFound;
                    Response.Data = "Flight not available anymore!";
                    return Response;
                }

                if (result != null)
                {
                    // Single booking (usually one row)
                    maintable = result.List1?.FirstOrDefault();
                    // Multiple rows
                    flightdetails = result.List2 ?? new List<FlightDetailDTO>();
                    paxdetails = result.List3 ?? new List<PaxDetailDTO>();
                    pricebreakdown = result.List4 ?? new List<PriceBreakdownDTO>();
                    flightbaggage = result.List5 ?? new List<FlightBaggageDTO>();
                }
                if (maintable == null || flightdetails == null || paxdetails == null || flightdetails.Count == 0 || paxdetails.Count == 0)
                {
                    Response.Status = HttpStatusCode.NotFound;
                    Response.Data = "Flight not available anymore!";
                    return Response;
                }

                string sc = maintable.system_searchcode;
                string id = maintable.system_faresourcecode;

                if (File.Exists(xmlfiles + sc + id + "_flight_revalidate.json") == false)
                {
                    Response.Status = HttpStatusCode.NotFound;
                    Response.Data = "No Data Found.";
                    return Response;
                }

                bool Lccairlines = maintable.is_lcc_airline;

                string flightbasket_text = System.IO.File.ReadAllText(xmlfiles + sc + id + "_flight_revalidate.json", System.Text.ASCIIEncoding.UTF8);

                DetailResponse.FlightDetailResponse flightbasket = Newtonsoft.Json.JsonConvert.DeserializeObject<DetailResponse.FlightDetailResponse>(flightbasket_text);

                string paymentid = maintable.system_payment_reference;
                string email = maintable.email;
                int paymenttype = maintable.paymenttype;

                string leadpaxname = "";
                try
                {
                    var leadpax = paxdetails.Find(x => x.isleadpax == true && x.paxtype == (int)PaxtypeEnum.Adult);
                    if (leadpax == null || leadpax == null || leadpax == null)
                    {
                        Response.Status = HttpStatusCode.NotFound;
                        Response.Data = "Flight not available anymore!";
                        return Response;
                    }
                    leadpaxname = leadpax.title + " " + leadpax.firstname + " " + leadpax.lastname;
                }
                catch { }

                #region Air_SellFromRecommendation
                Air_SellFromRecommendation_response.System_air_sellfromrecommendation IS_FLIGHT_AVAILABLE_FOR_BOOK = new Air_SellFromRecommendation_response.System_air_sellfromrecommendation();
                IS_FLIGHT_AVAILABLE_FOR_BOOK = await Air_SellFromRecommendation(flightbasket);

                if (IS_FLIGHT_AVAILABLE_FOR_BOOK.success == false)
                {
                    Response.Status = HttpStatusCode.NotFound;
                    Response.Data = "Flight not available anymore!";
                    try
                    {
                        if (paymenttype != null)
                        {
                            // objmail.BookingError(email, leadpaxname, paymentid);
                        }
                    }
                    catch { }
                    return Response;
                }

                var parameter = new
                {
                    system_reference = systemrefrence,
                    api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken,
                    api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId,
                    status = (int)TempBookingStatusEnum.Wait,
                };

                var bookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBooking", parameter);
                maintable.api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken;
                maintable.api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId;

                #endregion;

                string marketing_airline_code = flightdetails[0].marketing_airline_code;

                #region Booking Process

                FlightBookingResponseDTO ds = new FlightBookingResponseDTO();
                ds.Booking = maintable;
                ds.FlightDetails = flightdetails;
                ds.PaxDetails = paxdetails;
                ds.PriceBreakdowns = pricebreakdown;
                ds.Baggages = flightbaggage;


                string BOOKING_PNR = "";

                #region PNR_AddMultiElements_1
                string SequenceNumber = "2";

                com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope PNR_AddMultiElements_1_obj = await PNR_AddMultiElements_1(ds, SequenceNumber);

                if (PNR_AddMultiElements_1_obj == null || PNR_AddMultiElements_1_obj.Body == null || PNR_AddMultiElements_1_obj.Body.PNR_Reply == null || PNR_AddMultiElements_1_obj.Body.PNR_Reply.generalErrorInfo != null)
                {
                    SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();

                    com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                    Response.Status = HttpStatusCode.InternalServerError;
                    Response.Data = "An error Occured please contact website administration!";

                    //try
                    //{
                    //    try
                    //    {
                    //        if (paymenttype != null)
                    //        {
                    //           // objmail.BookingError(email, leadpaxname, paymentid);
                    //        }
                    //    }
                    //    catch { }
                    //}
                    //catch { }

                    return Response;
                }
                #endregion;

                #region Fare_PricePNRWithBookingClass
                // Checking Price of the pnr
                SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();

                com.ThirdPartyAPIs.Amadeus.Flight.Fare_PricePNRWithBookingClass_response.Envelope Fare_PricePNRWithBookingClass_obj = await Fare_PricePNRWithBookingClass(flightbasket, SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                if (Fare_PricePNRWithBookingClass_obj == null || Fare_PricePNRWithBookingClass_obj.Body == null || Fare_PricePNRWithBookingClass_obj.Body.Fare_PricePNRWithBookingClassReply == null || Fare_PricePNRWithBookingClass_obj.Body.Fare_PricePNRWithBookingClassReply.Length == 0)
                {
                    SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                    com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                    var parameterError = new
                    {
                        system_reference = systemrefrence,
                        api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken,
                        api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId,
                        status = (int)TempBookingStatusEnum.Error,
                    };

                    var Errorbookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBooking", parameterError);

                    Response.Status = HttpStatusCode.InternalServerError;
                    Response.Data = "An error Occured please contact website administration!";
                    return Response;
                }


                // Price Change  Cancel the PNR

                double totalprice = 0;

                foreach (var pricepnr in Fare_PricePNRWithBookingClass_obj.Body.Fare_PricePNRWithBookingClassReply)
                {

                    var priceobj = Array.Find(pricepnr.fareDataInformation.fareDataSupInformation, x => x.fareDataQualifier == "712");

                    totalprice += priceobj != null && priceobj.fareAmount != null ? Convert.ToDouble(priceobj.fareAmount) * pricepnr.paxSegReference.Length : 0;
                }


                if (totalprice != Convert.ToDouble(maintable.api_totalprice))
                {
                    SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                    com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                    var parametererror = new
                    {
                        system_reference = systemrefrence,
                        api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken,
                        api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId,
                        status = (int)TempBookingStatusEnum.Wait,
                    };

                    var tempbookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBooking", parametererror);

                    Response.Status = HttpStatusCode.OK;
                    Response.Data = "Price change please contact website administration!";

                    return Response;
                }

                #endregion;

                #region Ticket_CreateTSTFromPricing
                SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                com.ThirdPartyAPIs.Amadeus.Flight.Ticket_CreateTSTFromPricing_response.Envelope Ticket_CreateTSTFromPricing_obj = await Ticket_CreateTSTFromPricing(Fare_PricePNRWithBookingClass_obj, ds, SequenceNumber);

                if (Ticket_CreateTSTFromPricing_obj == null || Ticket_CreateTSTFromPricing_obj.Body == null || Ticket_CreateTSTFromPricing_obj.Body.Ticket_CreateTSTFromPricingReply == null || Ticket_CreateTSTFromPricing_obj.Body.Ticket_CreateTSTFromPricingReply.Length == 0)
                {
                    SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                    com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                    var parameterError = new
                    {
                        system_reference = systemrefrence,
                        api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken,
                        api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId,
                        status = (int)TempBookingStatusEnum.Error,
                    };

                    var Errorbookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBooking", parameterError);

                    Response.Status = HttpStatusCode.InternalServerError;
                    Response.Data = "An error Occured please contact website administration!";

                    return Response;
                }

                #endregion;

                #region FOP_CreateFormOfPayment
                if (Lccairlines == false)
                {
                    SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                    com.ThirdPartyAPIs.Amadeus.Flight.FOP_CreateFormOfPayment_response.Envelope FOP_CreateFormOfPayment_obj = await FOP_CreateFormOfPayment(ds, SequenceNumber);

                    if (Ticket_CreateTSTFromPricing_obj == null || Ticket_CreateTSTFromPricing_obj.Body == null || Ticket_CreateTSTFromPricing_obj.Body.Ticket_CreateTSTFromPricingReply == null || Ticket_CreateTSTFromPricing_obj.Body.Ticket_CreateTSTFromPricingReply.Length == 0)
                    {
                        SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                        com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                        var parameterError = new
                        {
                            system_reference = systemrefrence,
                            api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken,
                            api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId,
                            status = (int)TempBookingStatusEnum.Error,
                        };

                        var Errorbookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBooking", parameterError);

                        Response.Status = HttpStatusCode.InternalServerError;
                        Response.Data = "An error Occured please contact website administration!";
                        return Response;
                    }
                }

                #endregion;

                #region PNR_AddMultiElements_2
                SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope PNR_AddMultiElements_2_obj = new com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope();

                try
                {
                    PNR_AddMultiElements_2_obj = await PNR_AddMultiElements_2(ds, PNR_AddMultiElements_1_obj, SequenceNumber);
                    if (PNR_AddMultiElements_2_obj == null || PNR_AddMultiElements_2_obj.Body == null || PNR_AddMultiElements_2_obj.Body.PNR_Reply == null || PNR_AddMultiElements_2_obj.Body.PNR_Reply.generalErrorInfo != null)
                    {
                        SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                        com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                        var parameterError = new
                        {
                            system_reference = systemrefrence,
                            api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken,
                            api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId,
                            status = (int)TempBookingStatusEnum.Error,
                        };

                        var Errorbookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBooking", parameterError);

                        Response.Status = HttpStatusCode.InternalServerError;
                        Response.Data = "An error Occured please contact website administration!";
                        return Response;
                    }
                }
                catch (Exception ex)
                {
                    Response.Status = HttpStatusCode.InternalServerError;
                    Response.Data = ex.Message.ToString();
                    return Response;
                }

                BOOKING_PNR = PNR_AddMultiElements_2_obj.Body.PNR_Reply.pnrHeader[0].reservationInfo[0].controlNumber;
                var parameterpnr = new
                {
                    id = maintable.id,
                    status = (int)TempBookingStatusEnum.PNRCreated,
                    PNR = BOOKING_PNR,
                };

                var pnrbookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBookingPNR", parameterpnr);

                #endregion;

                #region Spirit Airline
                if (Lccairlines)
                {
                    bool Booked_successfully = false;
                    for (int i = 1; i <= 3; i++)
                    {

                        SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                        com.ThirdPartyAPIs.Amadeus.Flight.PNR_AddMultiElements_response.Envelope PNR_AddMultiElements_3_obj = await PNR_AddMultiElements_3(ds, PNR_AddMultiElements_2_obj, SequenceNumber);

                        if (PNR_AddMultiElements_3_obj == null || PNR_AddMultiElements_3_obj.Body == null || PNR_AddMultiElements_3_obj.Body.PNR_Reply == null)
                        {
                            SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                            com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_objnew = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                            var parameterError = new
                            {
                                system_reference = systemrefrence,
                                api_SecurityToken = IS_FLIGHT_AVAILABLE_FOR_BOOK.SecurityToken,
                                api_SessionId = IS_FLIGHT_AVAILABLE_FOR_BOOK.SessionId,
                                status = (int)TempBookingStatusEnum.Error,
                            };

                            var Errorbookingid = await _genericRepository.Save<int, object>("sp_UpdateTempFlightBooking", parameterError);

                            Response.Status = HttpStatusCode.InternalServerError;
                            Response.Data = "An error Occured please contact website administration!";

                            return Response;
                        }

                        //goto createticket;

                    }

                    //if (Booked_successfully)
                    //{
                    //    SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                    //    com.ThirdPartyAPIs.Models.Amadeus.Flight.PNR_AddMultiElements_response.Envelope PNR_AddMultiElements_4_obj = PNR_AddMultiElements_4(ds, PNR_AddMultiElements_2_obj, SequenceNumber);
                    //}

                    //createticket:;
                }
                #endregion;

                #region Security_SignOut
                SequenceNumber = (Convert.ToInt16(SequenceNumber) + 1).ToString();
                com.ThirdPartyAPIs.Amadeus.Flight.Security_SignOut_response.Envelope Security_SignOut_obj = await Security_SignOut(SequenceNumber, IS_FLIGHT_AVAILABLE_FOR_BOOK);

                #endregion;

                #region PNR Retirive
                com.ThirdPartyAPIs.Amadeus.Flight.PNR_Retrieve_response.Envelope PNR_Retrieve_response_obj = await PNR_Retrieve(PNR_AddMultiElements_2_obj.Body.PNR_Reply.pnrHeader[0].reservationInfo[0].controlNumber.ToString(), flightbasket.data.pcc);
                #endregion;

                #endregion Booking Process

                #region Insert Main Table

                #region main table 

                int bookingstatus = (int)BookingStatusEnum.Queue;

                var parameters = new
                {
                    id = 0,

                    departure_code = maintable.departure_code,
                    departure_city = maintable.departure_city,
                    departure_name = maintable.departure_name,
                    departure_datetime = maintable.departure_datetime,

                    arrival_code = maintable.arrival_code,
                    arrival_city = maintable.arrival_city,
                    arrival_name = maintable.arrival_name,
                    arrival_datetime = maintable.arrival_datetime,

                    system_payment_reference = maintable.system_payment_reference,
                    triptype = maintable.triptype,
                    faretype = maintable.faretype,
                    searchcode = maintable.searchcode,
                    userid = maintable.userid,

                    api_currency = maintable.api_currency,
                    api_baseprice = maintable.api_baseprice,
                    api_totalprice = maintable.api_totalprice,
                    api_taxprice = maintable.api_taxprice,

                    currency = maintable.currency,
                    baseprice = maintable.baseprice,
                    totalprice = maintable.totalprice,
                    taxprice = maintable.taxprice,
                    extraserviceprice = maintable.extraserviceprice,

                    total_adult = maintable.total_adult,
                    total_child = maintable.total_child,
                    total_infant = maintable.total_infant,

                    api_faresourcecode = maintable.api_faresourcecode,
                    system_faresourcecode = maintable.system_faresourcecode,
                    system_searchcode = maintable.system_searchcode,
                    system_reference = maintable.system_reference,

                    phone_country_code = maintable.phone_country_code,
                    phone_number = maintable.phone_number,
                    email = maintable.email,

                    status = bookingstatus,
                    HoldAllowed = maintable.HoldAllowed,
                    IsRefundable = maintable.IsRefundable,
                    createddate = maintable.createddate,

                    paymenttype = maintable.paymenttype,
                    api_extraserviceprice = maintable.api_extraserviceprice,

                    api_tkt_time_limit = maintable.api_tkt_time_limit,
                    api_SecurityToken = maintable.api_SecurityToken,
                    api_SessionId = maintable.api_SessionId,

                    is_lcc_airline = maintable.is_lcc_airline,
                    paymentstatus = maintable.paymentstatus,
                    searchchanel = maintable.searchchanel,

                    ipaddress = maintable.ipaddress,
                    pcc = maintable.pcc,
                    supplier = maintable.supplier,
                    PNR = BOOKING_PNR
                };

                var mainbookingid = await _genericRepository.Save<int, object>("sp_InsertFlightBooking", parameters);

                if (mainbookingid == null || mainbookingid == 0)
                {
                    Response.Status = HttpStatusCode.InternalServerError;
                    Response.Data = "An error Occured please contact website administration!";
                    return Response;
                }

                #endregion

                #region Insert in Sub tables
                string paxdetailqry = "";
                string flightdetailqry = "";
                string flight_baggage_qry = "";
                string price_breakdown = "";
                // Set the Bookingid for each item using ForEach
                paxdetails.ForEach(paxDetail => paxDetail.bookingid = mainbookingid.ToString());

                paxdetailqry = JsonConvert.SerializeObject(paxdetails);
                if (paxdetails != null && paxdetails.Count > 0 && !string.IsNullOrEmpty(paxdetailqry))
                {
                    var userparameter = new
                    {
                        TableName = "Flight_pax_detail",
                        JsonData = paxdetailqry
                    };
                    var paxsave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", userparameter);
                }

                flightdetails.ForEach(x => x.bookingid = mainbookingid.ToString());
                flightdetailqry = JsonConvert.SerializeObject(flightdetails);
                if (flightdetails != null && flightdetails.Count > 0 && !string.IsNullOrEmpty(flightdetailqry))
                {
                    var detailparameter = new
                    {
                        TableName = "Flight_detail",
                        JsonData = flightdetailqry
                    };
                    var detailsave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", detailparameter);
                }

                flightbaggage.ForEach(x => x.bookingid = mainbookingid.ToString());
                flight_baggage_qry = JsonConvert.SerializeObject(flightbaggage);
                if (flightbaggage != null && flightbaggage.Count > 0 && !string.IsNullOrEmpty(flight_baggage_qry))
                {
                    var baggageparameter = new
                    {
                        TableName = "Flight_baggage",
                        JsonData = flight_baggage_qry
                    };
                    var baggagesave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", baggageparameter);
                }

                pricebreakdown.ForEach(x => x.bookingid = mainbookingid.ToString());
                price_breakdown = JsonConvert.SerializeObject(pricebreakdown);
                if (pricebreakdown != null && pricebreakdown.Count > 0 && !string.IsNullOrEmpty(price_breakdown))
                {
                    var priceparameter = new
                    {
                        TableName = "Flight_price_breakdown",
                        JsonData = price_breakdown
                    };
                    var pricesave = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", priceparameter);
                }
                #endregion Insert in Sub tables

                #endregion

                var returnresponse = new
                {
                    BookingId = mainbookingid,
                    PNR = BOOKING_PNR,
                    BookingStatus = bookingstatus
                };

                Response.Status = HttpStatusCode.OK;
                Response.Data = returnresponse;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Status = HttpStatusCode.InternalServerError;
                Response.Data = "Flight not available anymore!";
                _errorLogRepository.AddErrorLog(ex, "AmadeusConfig->BookingPageDirect", systemrefrence);
                return Response;
            }

        }


        #endregion Main Booking page Direct


        #region Test Bulk data save

        public CommonResponse testingsavedata_old()
        {
            CommonResponse Response = new CommonResponse();
            try
            {
                List<UsersDTO> user = new List<UsersDTO>();
                user.Add(new UsersDTO()
                {
                    UserName = "testuser",
                    Email = "temstemail",
                    CreatedBy = 1
                });
                user.Add(new UsersDTO()
                {
                    UserName = "testuser1",
                    Email = "temstemail",
                    CreatedBy = 1
                });
                user.Add(new UsersDTO()
                {
                    UserName = "testuser2",
                    Email = "temstemail",
                    CreatedBy = 1
                });
                user.Add(new UsersDTO()
                {
                    UserName = "testuser3",
                    Email = "temstemail3",
                    CreatedBy = 1
                });

                //var result = _genericRepository.SaveBulk<List<UsersDTO>, List<UsersDTO>>("sp_save_users", user).GetAwaiter().GetResult();
                var result = _genericRepository.Save<List<UsersDTO>, List<UsersDTO>>("sp_GenericBulkInsert", user);

            }
            catch (Exception ex)
            {

            }
            return Response;
        }

        public async Task<CommonResponse> testingsavedata()
        {
            CommonResponse response = new CommonResponse();

            try
            {
                string temp_bookingid = "jagdishjaat_replace_id";

                List<UsersDTO> users = new List<UsersDTO>
        {
            new UsersDTO { UserName = "testuser",  Email = temp_bookingid,  CreatedBy = 1},
            new UsersDTO { UserName = "testuser1", Email = temp_bookingid,  CreatedBy = 1},
            new UsersDTO { UserName = "testuser2", Email = temp_bookingid,  CreatedBy = 1},
            new UsersDTO { UserName = "testuser3", Email = temp_bookingid, CreatedBy = 1 }
        };
                var bookingid = "1001";

                string useersjson = JsonConvert.SerializeObject(users).Replace(temp_bookingid, bookingid);

                var parameters = new
                {
                    TableName = "Users",
                    JsonData = useersjson
                };

                var parameter = new
                {
                    UserName = "Users",
                    Email = "test",
                    CreatedBy = 1
                };

                int result = await _genericRepository.Save<int, object>("sp_GenericBulkInsert", parameters);
                //int result2 = await _genericRepository.Save<int, object>("sp_insert_user", parameter);

                //response.Status = result > 0;
                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = ex.Message;
            }

            return response;
        }

        #endregion Test Bulk data save

    }
}