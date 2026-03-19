using com.Services.ServiceInterface;
using Com.AuthProvider;
using Com.Common.DTO;
using Com.Common.Model;
using Com.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using static Com.Common.Utility.AllEnums;

namespace com.FlightBokingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IIdentityAuthProvider _identityAuthProvider;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IGenericRepository _genericRepository;
        private readonly IAccountService _accountService;
        public AccountController(IIdentityAuthProvider identityAuthProvider, IErrorLogRepository errorLogRepository,IGenericRepository genericRepository, IAccountService accountService)
        {
            _identityAuthProvider = identityAuthProvider;
            _errorLogRepository = errorLogRepository;
            _genericRepository = genericRepository;
            _accountService = accountService;
        }

        [HttpPost("LoginUser")]
        public async Task<ActionResult> LoginUser(UserLoginDTO model)
        {
            try
            {
                if (model != null)
                {
                    //if (ModelState.IsValid) // no use of ModelState here, as api controller will return bad request automatically without entering in this method 
                    //{
                    UserLoginResultDTO resultUser = new UserLoginResultDTO();
                    IdentityUser<int> user = new IdentityUser<int>() { UserName = model.UserName, PasswordHash = model.Password };
                    var result = await _identityAuthProvider.PasswordSignInAsync(user);

                    if (result.Succeeded)
                    {
                        IdentityUser<int> identityUser = await _identityAuthProvider.FindByNameAsync(model.UserName);
                        var roles = await _identityAuthProvider.GetUserRole(identityUser);
                        if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
                        {
                            resultUser = new UserLoginResultDTO() { Id = identityUser.Id.ToString(), UserName = identityUser.UserName, PhoneNumber = identityUser.PhoneNumber };
                            (resultUser.Token, resultUser.TokenExpireTime) = await _identityAuthProvider.GetJwtToken(model.UserName);

                            var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                            // Check if the request is from a mobile device
                            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                            bool isFromMobile = !string.IsNullOrEmpty(userAgent) &&
                                                (userAgent.Contains("Mobi") || userAgent.Contains("Android") || userAgent.Contains("iPhone") || userAgent.Contains("iPad"));

                            UserLogInDetail userLogInDetail = new UserLogInDetail();
                            userLogInDetail.UserId = identityUser.Id;
                            userLogInDetail.Token = resultUser.Token;
                            //userLogInDetail.CreatedOn = DateUtility.GetNowTime();
                            //userLogInDetail.UpdatedOn = DateUtility.GetNowTime();
                            userLogInDetail.IPAddress = clientIpAddress;
                            userLogInDetail.Active = true;
                            userLogInDetail.IsFromMobile = isFromMobile;
                            _accountService.AddUserLogInDetail(userLogInDetail);
                            return Ok(new CommonResponse { Status = HttpStatusCode.OK, Data = resultUser });
                        }
                        else if (roles.Contains("User"))
                        {
                            resultUser = new UserLoginResultDTO() { Id = identityUser.Id.ToString(), UserName = identityUser.UserName, PhoneNumber = identityUser.PhoneNumber };
                            (resultUser.Token, resultUser.TokenExpireTime) = await _identityAuthProvider.GetJwtToken(model.UserName);
                            var parameters = new
                            {
                                UserId = identityUser.Id
                            };

                            UserDetailDTO ruserdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", parameters);

                            resultUser.Currency = ruserdetail.Currency;

                            var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                            // Check if the request is from a mobile device
                            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                            bool isFromMobile = !string.IsNullOrEmpty(userAgent) &&
                                                (userAgent.Contains("Mobi") || userAgent.Contains("Android") || userAgent.Contains("iPhone") || userAgent.Contains("iPad"));

                            UserLogInDetail userLogInDetail = new UserLogInDetail();
                            userLogInDetail.UserId = identityUser.Id;
                            userLogInDetail.Token = resultUser.Token;
                            //userLogInDetail.CreatedOn = DateUtility.GetNowTime();
                            //userLogInDetail.UpdatedOn = DateUtility.GetNowTime();
                            userLogInDetail.IPAddress = clientIpAddress;
                            userLogInDetail.Active = true;
                            userLogInDetail.IsFromMobile = isFromMobile;
                            _accountService.AddUserLogInDetail(userLogInDetail);
                            return Ok(new CommonResponse { Status = HttpStatusCode.OK, Data = resultUser });
                        }
                        else
                        {
                            return Ok(new CommonResponse { Status = HttpStatusCode.NotFound, Data = "Invalid user role." });
                        }
                    }
                    return Ok(new CommonResponse { Status = HttpStatusCode.NotFound, Data = "Invalid user credentials" });

                    //}
                    //else
                    //{
                    //    string errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    //    return Ok(new CommonResponseDTO { Status = HttpStatusCode.BadRequest, Data = errors });
                    //}
                    //return await Task.FromResult(new ResponseObject(false, string.Join(",", ModelState.Values.SelectMany(x => x.Errors)), null));
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.BadRequest, Data = "Invalid input" });
            }
            catch (Exception ex)
            {
                _errorLogRepository.AddErrorLog(ex, "Account->LoginUser", JsonConvert.SerializeObject(model));
                //return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = Resource.Error500 });
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Internal Server Error found!" });
            }

        }


        [HttpPost("Logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var result = await _identityAuthProvider.SignOutAsync();
                if (result)
                {
                    return Ok(new CommonResponse { Status = HttpStatusCode.OK, Data = "Logout success" });
                }
                return Ok(new CommonResponse { Status = HttpStatusCode.NotFound, Data = "User not found" });
            }
            catch (Exception ex)
            {
                _errorLogRepository.AddErrorLog(ex, "Account->Logout", null);
                return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Internal Server Error found!" });
            }
        }


        //[HttpPost("CreateAdminUser")]
        //public async Task<ActionResult> CreateAdminUser()
        //{
        //    try
        //    {
        //        string password = "Admin@123";

        //        var user = new IdentityUser<int> { UserName = "8302486066", PhoneNumber = "8302486066", PhoneNumberConfirmed = true, LockoutEnabled = false, TwoFactorEnabled = false, AccessFailedCount = 0, PasswordHash = password };
        //        var resultCreateuser = await _identityAuthProvider.CreateAsync(user, UserRoleEnum.Admin);

        //        //if (resultCreateuser)
        //        //{
        //        //    return Ok(new CommonResponse { Status = HttpStatusCode.OK, Data = "Logout success" });
        //        //}
        //        return Ok(new CommonResponse { Status = HttpStatusCode.NotFound, Data = "User created" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _errorLogRepository.AddErrorLog(ex, "Account->Logout", null);
        //        return Ok(new CommonResponse { Status = HttpStatusCode.InternalServerError, Data = "An Internal Server Error found!" });
        //    }
        //}


    }
}
