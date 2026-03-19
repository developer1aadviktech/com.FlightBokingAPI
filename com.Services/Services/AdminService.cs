using Azure;
using Azure.Core;
using com.Services.ServiceInterface;
using com.ThirdPartyAPIs.Models.Flight;
using Com.AuthProvider;
using Com.Common;
using Com.Common.DTO;
using Com.Common.Utility;
using Com.Repository;
using Com.Zoope.Common.Resource;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;
using static Com.Common.Utility.AllEnums;

namespace com.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IIdentityAuthProvider _identityAuthProvider;
        public AdminService(IConfiguration configuration, IGenericRepository genericRepository, IErrorLogRepository errorLogRepository, IWebHostEnvironment env, IIdentityAuthProvider identityAuthProvider)//IHttpClientFactory httpClientFactory,
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            //_configuration = configuration;
            //_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _errorLogRepository = errorLogRepository;
            _env = env;
            _identityAuthProvider = identityAuthProvider;
        }

        #region Manage Airport 

        public async Task<CommonResponse> GetAirportList(PaginationAndSortingDTO request)
        {
            CommonResponse response = new CommonResponse();
            request.PageNo = request.PageNo == null || request.PageNo == 0 ? 1 : request.PageNo;
            request.PageSize = request.PageSize == null || request.PageSize == 0 ? 10 : request.PageSize;

            var parameters = new
            {
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                SearchString = request.SearchString,
            };

            List<AirportListDTO> result = (await _genericRepository.LoadData<AirportListDTO, Object>("GetAirportList", parameters)).ToList();

            ListResponseDTO CommonListRespDTO = new ListResponseDTO();
            CommonListRespDTO.metaData = new Metadata();
            CommonListRespDTO.metaData.totalItems = result.Where(x => x.rno == -1).FirstOrDefault().Id; //giver total records
            CommonListRespDTO.metaData.itemsPerPage = request.PageSize;
            int totalpages = Convert.ToInt32(result[0].Id) / request.PageSize;
            CommonListRespDTO.metaData.totalPages = totalpages == 0 ? 1 : totalpages;
            CommonListRespDTO.items = result.Where(x => x.rno != -1).ToList();

            if (result == null)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = "An error occurred!";
                return response;
            }
            response.Status = HttpStatusCode.OK;
            response.Data = CommonListRespDTO;
            return response;
        }

        public async Task<CommonResponse> AddUpdateAirport(AirportDTO request)
        {
            CommonResponse response = new CommonResponse();
            #region Validation

            if (request == null || string.IsNullOrEmpty(request.AirportCode))
            {
                response.Data = "Please provide the airport code.";
                response.Status = HttpStatusCode.BadRequest;
                return response;
            }

            if (request == null || string.IsNullOrEmpty(request.AirportName))
            {
                response.Data = "Please provide the airport name.";
                response.Status = HttpStatusCode.BadRequest;
                return response;
            }

            if (request == null || string.IsNullOrEmpty(request.City))
            {
                response.Data = "Please provide the airport city.";
                response.Status = HttpStatusCode.BadRequest;
                return response;
            }

            if (request == null || string.IsNullOrEmpty(request.Country))
            {
                response.Data = "Please provide the airport country.";
                response.Status = HttpStatusCode.BadRequest;
                return response;
            }
            #endregion

            var parameters = new
            {
                Id = request.Id,
                AirportCode = request.AirportCode,
                AirportName = request.AirportName,
                City = request.City,
                Country = request.Country,
            };
            CommonSPResponseDTO CommonSPResponseDTO = await _genericRepository.Save<CommonSPResponseDTO, object>("AddUpdateAirportlist", parameters);

            if (CommonSPResponseDTO == null)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = Resource.Error500;
                return response;
            }

            if (CommonSPResponseDTO.Status == "ERROR")
            {
                response.Status = HttpStatusCode.AlreadyReported;
                response.Data = CommonSPResponseDTO.Message;
                return response;
            }
            else
            {
                response.Status = HttpStatusCode.OK;
                response.Data = CommonSPResponseDTO.Message;
            }
                Task.Run(() => UpdateAirportJson());

            response.Status = HttpStatusCode.OK;
            return response;
        }

        public async Task<CommonResponse> DeleteAirportById(long Id)
        {
            CommonResponse response = new CommonResponse();

            var parameters = new
            {
                Id = Id,
            };
            var id = await _genericRepository.Save<int, object>("DeleteAirportListById", parameters);

            if (id == null)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = Resource.Error500;
                return response;
            }

            Task.Run(() => UpdateAirportJson());

            response.Status = HttpStatusCode.OK;
            response.Data = "Airport deleted successfully.";
            return response;
        }

        public async Task UpdateAirportJson()
        {
            List<AirportListDTO> result = (await _genericRepository.LoadData<AirportListDTO, Object>("GetAllAirportList", null)).ToList();

            var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");

            System.IO.File.WriteAllText(importantfiles + "Airportlist.json", JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);

        }

        #endregion Manage Airport

        #region Manage Airline 

        public async Task<CommonResponse> GetAirlineList(PaginationAndSortingDTO request)
        {
            CommonResponse response = new CommonResponse();

            request.PageNo = request.PageNo == null || request.PageNo == 0 ? 1 : request.PageNo;
            request.PageSize = request.PageSize == null || request.PageSize == 0 ? 10 : request.PageSize;

            var parameters = new
            {
                PageNo = request.PageNo,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                SearchString = request.SearchString,
            };

            List<AirlineListDTO> result = (await _genericRepository.LoadData<AirlineListDTO, Object>("GetAirlineList", parameters)).ToList();

            ListResponseDTO CommonListRespDTO = new ListResponseDTO();
            CommonListRespDTO.metaData = new Metadata();
            CommonListRespDTO.metaData.totalItems = result.Where(x => x.rno == -1).FirstOrDefault().Id; //giver total records
            CommonListRespDTO.metaData.itemsPerPage = request.PageSize;
            int totalpages = Convert.ToInt32(result[0].Id) / request.PageSize;
            CommonListRespDTO.metaData.totalPages = totalpages == 0 ? 1 : totalpages;
            CommonListRespDTO.items = result.Where(x => x.rno != -1).ToList();

            if (result == null)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = "An error occurred!";
                return response;
            }
            response.Status = HttpStatusCode.OK;
            response.Data = CommonListRespDTO;
            return response;
        }

        public async Task<CommonResponse> AddUpdateAirline(AirlineDTO request)
        {
            CommonResponse response = new CommonResponse();

            var parameters = new
            {
                Id = request.Id,
                AirlineCode = request.AirlineCode,
                AirlineName = request.AirlineName,
            };
            CommonSPResponseDTO CommonSPResponse = await _genericRepository.Save<CommonSPResponseDTO, object>("AddUpdateAirline", parameters);

            if (CommonSPResponse == null)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = Resource.Error500;
                return response;
            }

            if (CommonSPResponse.Status == "ERROR")
            {
                response.Status = HttpStatusCode.AlreadyReported;
                response.Data = CommonSPResponse.Message;
                return response;
            }
            else
            {
                response.Status = HttpStatusCode.OK;
                response.Data = CommonSPResponse.Message;
            }
            Task.Run(() => UpdateAirlineJson());

            response.Status = HttpStatusCode.OK;
            return response;
        }

        public async Task<CommonResponse> DeleteAirlineById(long Id)
        {
            CommonResponse response = new CommonResponse();

            var parameters = new
            {
                Id = Id,
            };
            var id = await _genericRepository.Save<int, object>("DeleteAirlineById", parameters);

            if (id == null)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = Resource.Error500;
                return response;
            }

            Task.Run(() => UpdateAirlineJson());

            response.Status = HttpStatusCode.OK;
            response.Data = "Airline deleted successfully.";
            return response;
        }

        public async Task UpdateAirlineJson()
        {
            List<AirlineListDTO> result = (await _genericRepository.LoadData<AirlineListDTO, Object>("GetAllAirlineList", null)).ToList();

            var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");

            System.IO.File.WriteAllText(importantfiles + "Airlinelist.json", JsonConvert.SerializeObject(result), System.Text.ASCIIEncoding.UTF8);

        }

        #endregion Manage Airline

        #region Users 

        public async Task<CommonResponse> AddUser(AddUserDTO request)
        {
            CommonResponse response = new CommonResponse();

            try
            {
                #region Validation

                if (request == null || request.UserId != 0)
                {
                    response.Data = "Bad Request";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrEmpty(request.Email))
                {
                    response.Data = "Please Enter Email.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    response.Data = "Please Enter Email.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    response.Data = "Please Enter Phone Number.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    response.Data = "Please Enter Full Name.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.AgencyName))
                {
                    response.Data = "Please Enter Agency Name.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.AgencyCode))
                {
                    response.Data = "Please Enter Agency Code.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.Country))
                {
                    response.Data = "Please Enter Country.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.PCCcode))
                {
                    response.Data = "Please Enter PCC Code.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.Currency))
                {
                    response.Data = "Please Enter Currency.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.CurrencySign))
                {
                    response.Data = "Please Enter Currency Sign.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                // Markup Validation

                //if (request.IsFixedMarkup && request.FixedMarkup <= 0)
                //{
                //    response.Data = "Please Enter Valid Fixed Markup.";
                //    response.Status = HttpStatusCode.BadRequest;
                //    return response;
                //}

                //if (request.IsPercentageMarkup)
                //{
                //    if (request.PercentageMarkup <= 0 || request.PercentageMarkup > 100)
                //    {
                //        response.Data = "Percentage Markup must be between 0 and 100.";
                //        response.Status = HttpStatusCode.BadRequest;
                //        return response;
                //    }
                //}

                //// Optional: prevent both markups at same time
                //if (request.IsFixedMarkup && request.IsPercentageMarkup)
                //{
                //    response.Data = "Please select either Fixed Markup or Percentage Markup.";
                //    response.Status = HttpStatusCode.BadRequest;
                //    return response;
                //}

                #endregion

                IdentityUser<int> result = await _identityAuthProvider.FindByNameAsync(request.Email);
                if (result != null)
                {
                    response.Data = "Email already exists. Please try with different Email.";
                    response.Status = HttpStatusCode.OK;
                    return response;
                }
                else
                {
                    // Generate a random password
                    //string password = GenerateRandomPassword();
                    //string password = new string('*', 12);
                    string password = GeneratePassword(12);

                    var user = new IdentityUser<int> { UserName = request.Email, Email = request.Email, EmailConfirmed = true, PhoneNumber = request.PhoneNumber, PhoneNumberConfirmed = true, LockoutEnabled = false, TwoFactorEnabled = false, AccessFailedCount = 0, PasswordHash = password };
                    var resultCreateuser = await _identityAuthProvider.CreateAsync(user, UserRoleEnum.User);
                    if (resultCreateuser.Succeeded)
                    {
                        UserLoginResultDTO resultUser = new UserLoginResultDTO();
                        IdentityUser<int> loginUser = new IdentityUser<int>() { UserName = request.Email, PasswordHash = password };
                        var resultSignIn = await _identityAuthProvider.PasswordSignInAsync(loginUser);

                        IdentityUser<int> identityUser = await _identityAuthProvider.FindByNameAsync(request.Email);
                        var parameters = new
                        {

                            // Save UserDetails data 
                            IsEdit = false,
                            UserId = identityUser.Id,
                            FullName = request.FullName,
                            AgencyName = request.AgencyName,
                            AgencyCode = request.AgencyCode,
                            Country = request.Country,
                            PCCcode = request.PCCcode,
                            Currency = request.Currency,
                            CurrencySign = request.CurrencySign,
                            MarkupType = request.MarkupType,
                            MarkupValue = request.MarkupValue,                            
                            Password = password,
                            UserStatus = (int)UserStatusEnum.Active,
                        };

                        var Userdetial = await _genericRepository.Save<int, object>("AddUpdateUserDetails", parameters);

                        if (Userdetial != null)
                        {
                            response.Status = HttpStatusCode.OK;
                            response.Data = "User add successfully!";
                            return response;
                        }
                        else
                        {
                            response.Status = HttpStatusCode.FailedDependency;
                            response.Data = "User details were not saved. Please try again.";
                            return response;
                        }
                    }
                    else
                    {
                        string pwdmessage = "";
                        if (resultCreateuser != null && resultCreateuser.Errors != null)
                        {
                            pwdmessage = resultCreateuser.Errors?.FirstOrDefault().Description;
                        }
                        response.Status = HttpStatusCode.FailedDependency;
                        response.Data = "Password Creation Failed." + pwdmessage;
                        return response;
                    }

                }
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "Admin->AddUser", JsonConvert.SerializeObject(request));
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = e.Message.ToString();
                //response.Data = Resource.Error500;
                return response;
            }

        }

        public static string GeneratePassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            StringBuilder password = new StringBuilder();

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    int index = randomBytes[i] % validChars.Length;
                    password.Append(validChars[index]);
                }
            }

            return password.ToString();
        }


        public async Task<CommonResponse> EditUser(AddUserDTO request)
        {
            CommonResponse response = new CommonResponse();

            try
            {

                #region Validation

                if (request == null || request.UserId < 0)
                {
                    response.Data = "Bad Request";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrEmpty(request.Email))
                {
                    response.Data = "Please Enter Email.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    response.Data = "Please Enter Email.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    response.Data = "Please Enter Phone Number.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    response.Data = "Please Enter Full Name.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.AgencyName))
                {
                    response.Data = "Please Enter Agency Name.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.AgencyCode))
                {
                    response.Data = "Please Enter Agency Code.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.Country))
                {
                    response.Data = "Please Enter Country.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.PCCcode))
                {
                    response.Data = "Please Enter PCC Code.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.Currency))
                {
                    response.Data = "Please Enter Currency.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.CurrencySign))
                {
                    response.Data = "Please Enter Currency Sign.";
                    response.Status = HttpStatusCode.BadRequest;
                    return response;
                }

                // Markup Validation

                //if (request.IsFixedMarkup && request.FixedMarkup <= 0)
                //{
                //    response.Data = "Please Enter Valid Fixed Markup.";
                //    response.Status = HttpStatusCode.BadRequest;
                //    return response;
                //}

                //if (request.IsPercentageMarkup)
                //{
                //    if (request.PercentageMarkup <= 0 || request.PercentageMarkup > 100)
                //    {
                //        response.Data = "Percentage Markup must be between 0 and 100.";
                //        response.Status = HttpStatusCode.BadRequest;
                //        return response;
                //    }
                //}

                //// Optional: prevent both markups at same time
                //if (request.IsFixedMarkup && request.IsPercentageMarkup)
                //{
                //    response.Data = "Please select either Fixed Markup or Percentage Markup.";
                //    response.Status = HttpStatusCode.BadRequest;
                //    return response;
                //}

                #endregion



                IdentityUser<int> userresp = await _identityAuthProvider.FindByIdAsync(request.UserId.ToString());
                if (userresp != null)
                {
                    userresp.PhoneNumber = request.PhoneNumber;
                    userresp.PhoneNumberConfirmed = true;

                    await _identityAuthProvider.UpdateAsync(userresp);

                    var parameters = new
                    {
                        UserId = request.UserId
                    };

                    UserDetailDTO ruserdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", parameters);
                    if (ruserdetail != null && ruserdetail.UserId != null)
                    {
                        var edtparameters = new
                        {
                            // Save UserDetails data 
                            IsEdit = true,
                            UserId = ruserdetail.UserId,
                            FullName = request.FullName,
                            AgencyName = request.AgencyName,
                            AgencyCode = request.AgencyCode,
                            Country = request.Country,
                            PCCcode = request.PCCcode,
                            Currency = request.Currency,
                            CurrencySign = request.CurrencySign,
                            MarkupType = request.MarkupType,
                            MarkupValue = request.MarkupValue,
                            Password = "",
                            UserStatus = (int)UserStatusEnum.Active,
                        };

                        var Userdetial = await _genericRepository.Save<int, object>("AddUpdateUserDetails", edtparameters);

                        if (Userdetial != null)
                        {
                            response.Status = HttpStatusCode.OK;
                            response.Data = "User add successfully!";
                            return response;
                        }
                        else
                        {
                            response.Status = HttpStatusCode.FailedDependency;
                            response.Data = "User details were not saved. Please try again.";
                            return response;
                        }
                    }
                    response.Status = HttpStatusCode.NotFound;
                    response.Data = "User Detail not found.";
                    //response.Data = Resource.Error500;
                    return response;

                }
                else
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.Data = "User not found.";
                    //response.Data = Resource.Error500;
                    return response;
                }
            }
            catch (Exception e)
            {
                _errorLogRepository.AddErrorLog(e, "AdminService->EditUser", JsonConvert.SerializeObject(request));
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = e.Message.ToString();
                //response.Data = Resource.Error500;
                return response;
            }

        }

        public async Task<CommonResponse> GetUserDetailById(int userid)
        {
            CommonResponse Response = new CommonResponse();
            IdentityUser<int> result = await _identityAuthProvider.FindByIdAsync(userid.ToString());
            if (result == null)
            {
                Response.Data="Invalid User.";
                Response.Status=HttpStatusCode.NotFound;
                return Response;
            }
            var parameters = new
            {
                UserId = userid
            };

            UserDetailDTO ruserdetail = await _genericRepository.LoadSingleData<UserDetailDTO, object>("GetUserById", parameters);
            if (ruserdetail != null && ruserdetail.UserId != null)
            {
                Response.Data=ruserdetail;
                Response.Status = HttpStatusCode.OK;
                return Response;
            }
            else
            {
                Response.Status=HttpStatusCode.NotFound;
                Response.Data="No Record found.";
                return Response;
            }
        }

        public async Task<CommonResponse> UserList(UserListFilterDTO request)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                if (request == null || request.PageNo == null || request.PageNo <= 0 || request.PageSize == null || request.PageSize <= 0)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.Data = "Invalid Request.";
                    return response;
                }

                var parameters = new
                {
                    PageNo = request.PageNo,
                    PageSize = request.PageSize,
                    UserStatus = request.UserStatus,
                    StartDate = request.StartDate,
                    Enddate = request.EndDate,
                    SortBy=request.SortBy,
                    SearchString = request.SearchString
                };

                List<UserListDTO> result = (await _genericRepository.LoadData<UserListDTO, Object>("GetUserList", parameters)).ToList();
                //var result = (await _genericRepository.LoadData<object, Object>("GetUserList", parameters)).ToList();

                //List<CustomerDetailsResponseDTO>   = JsonConvert.DeserializeObject<List<CustomerDetailsResponseDTO>>(responseGetCustomerList);

                ListResponseDTO CommonListRespDTO = new ListResponseDTO();
                CommonListRespDTO.metaData = new Metadata();
                CommonListRespDTO.metaData.totalItems = result.Where(x => x.rno == -1).FirstOrDefault().UserStatus; //giver total records
                CommonListRespDTO.metaData.itemsPerPage = request.PageSize;
                int totalpages = Convert.ToInt32(result[0].UserStatus) / request.PageSize;
                //int totalpages = Convert.ToInt32(result.Count) / request.PageSize;
                CommonListRespDTO.metaData.totalPages = totalpages == 0 ? 1 : totalpages;
                CommonListRespDTO.items = result.Where(x => x.rno != -1).ToList();
                //customerListRespDTO.items = result.ToList();

                if (result == null)
                {
                    response.Status = HttpStatusCode.InternalServerError;
                    response.Data = "An error occurred!";
                    return response;
                }
                response.Status = HttpStatusCode.OK;
                response.Data = CommonListRespDTO;
                return response;
            }
            catch(Exception ex)
            {
                _errorLogRepository.AddErrorLog(ex, "User->GetUserList", null);
                response.Status = HttpStatusCode.InternalServerError;
                response.Data = Resource.Error500;
                return response;
            }
        }

        #endregion

    }
}
