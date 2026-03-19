using Amadeus_wsdl;
using Azure.Core;
using com.Services.ServiceInterface;
using com.Services.Services;
using Com.AuthProvider;
using Com.Common.DTO;
using Com.Zoope.Common.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;

namespace com.FlightBokingAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IAdminService _adminServices;
        //[Authorize(Roles = "SuperAdmin,Admin")]
        
        public AdminController(IAdminService adminService, IErrorLogRepository errorLogRepository)
        {
            _adminServices = adminService;
            _errorLogRepository = errorLogRepository;
            
        }

        #region Manage Airport
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("GetAirportList")]
        public async Task<ActionResult> GetAirportList([FromBody] PaginationAndSortingDTO airportListFilterDTO, IErrorLogRepository errorLogRepository)
        {
            try
            {
                if (airportListFilterDTO != null)
                {
                    CommonResponse Response = await _adminServices.GetAirportList(airportListFilterDTO);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->GetAirportList", JsonConvert.SerializeObject(airportListFilterDTO));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }
        
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("AddUpdateAirport")]
        public async Task<ActionResult> AddUpdateAirport([FromBody] AirportDTO request)
        {
            try
            {
                if (request != null)
                {
                    int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    CommonResponse Response = await _adminServices.AddUpdateAirport(request);
                    return Ok(Response);
                }
                else
                    return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->AddUpdateGiftCard", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }
        
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("DeleteAirportById/{Id}")]
        public async Task<ActionResult> DeleteAirportById([FromBody] long Id)
        {
            try
            {
                CommonResponse Response = await _adminServices.DeleteAirportById(Id);
                return Ok(Response);
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->DeleteAirportById", "Id=" + Id);
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }

        #endregion Manage Airport


        #region Manage Airline

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("GetAirlineList")]
        public async Task<ActionResult> GetAirlineList([FromBody] PaginationAndSortingDTO AirlineListFilterDTO, IErrorLogRepository errorLogRepository)
        {
            try
            {
                if (AirlineListFilterDTO != null)
                {
                    CommonResponse Response = await _adminServices.GetAirlineList(AirlineListFilterDTO);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->GetAirlineList", JsonConvert.SerializeObject(AirlineListFilterDTO));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }
        //[Authorize(Roles = "SuperAdmin,Admin")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("AddUpdateAirline")]
        public async Task<ActionResult> AddUpdateAirline([FromBody] AirlineDTO request)
        {
            try
            {
                if (request != null)
                {
                    int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    CommonResponse Response = await _adminServices.AddUpdateAirline(request);
                    return Ok(Response);
                }
                else
                    return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->AddUpdateGiftCard", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }
        
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("DeleteAirlineById/{Id}")]
        public async Task<ActionResult> DeleteAirlineById(long Id)
        {
            try
            {
                CommonResponse Response = await _adminServices.DeleteAirlineById(Id);
                return Ok(Response);
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->DeleteAirlineById", "Id=" + Id);
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }

        #endregion Manage Airline


        #region User 
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("AddUser")]
        public async Task<ActionResult> AddUser([FromBody] AddUserDTO request)
        {
            //var formCollection = await Request.ReadFormAsync();
            try
            {
                if (request != null)
                {
                    //int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    CommonResponse Response = await _adminServices.AddUser(request);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->AddUser", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("EditUser")]
        public async Task<ActionResult> EditUser([FromBody] AddUserDTO request)
        {
            //var formCollection = await Request.ReadFormAsync();
            try
            {
                if (request != null)
                {
                    //int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    CommonResponse Response = await _adminServices.EditUser(request);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->AddUser", JsonConvert.SerializeObject(request));
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("GetUserDetailById")]
        public async Task<ActionResult> GetUserDetailById(int UserId)
        {
            try
            {
                if (UserId != null && UserId > 0)
                {
                    //int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    CommonResponse Response = await _adminServices.GetUserDetailById(UserId);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "User->GetUserDetailById", null);
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("GetUserList")]
        public async Task<ActionResult> GetUserList(UserListFilterDTO reqModel)
        {
            try
            {
                if (reqModel != null)
                {
                    CommonResponse Response = await _adminServices.UserList(reqModel);
                    return Ok(Response);
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "User->GetUserList", null);
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
            }
        }

        #endregion


    }
}
