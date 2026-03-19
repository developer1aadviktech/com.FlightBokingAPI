using com.Services.ServiceInterface;
using Com.AuthProvider;
using Com.Common.DTO;
using Com.Common.Model;
using Com.Common.Utility;
using Com.Repository;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static com.ThirdPartyAPIs.Models.Flight.DetailResponse;

namespace com.Services.Services
{
    public class AccountService: IAccountService
    {
        private readonly IIdentityAuthProvider _identityAuthProvider;
        //private readonly ISMSLogService _smsLogService;
        //private readonly IMapper _mapper;
        private readonly IGenericRepository _genericRepository;
        private readonly IWebHostEnvironment _environment;
        public AccountService(IIdentityAuthProvider identityAuthProvider, IWebHostEnvironment environment, IGenericRepository genericRepository)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _identityAuthProvider = identityAuthProvider;
            //_smsLogService = smsLogService;
            //_mapper = mapper;
            _environment = environment;
        }
        public async Task<CommonResponse> AddUserLogInDetail(UserLogInDetail userLogInDetail)
        {
            CommonResponse dict = new CommonResponse();

            //WebRequestUtility.InvokePostRequest(ApplicationGlobalUtility.RepositoryBaseURL + RepositoryEndPoints.AddUserLogInDetails, JsonConvert.SerializeObject(userLogInDetail));

            var parameter = new
            {
                Id = 0,
                UserId = userLogInDetail.UserId,
                IPAddress = userLogInDetail.IPAddress,
                Active = userLogInDetail.Active,
                IsFromMobile = userLogInDetail.IsFromMobile,
                Token = userLogInDetail.Token
            };
            int result = await _genericRepository.Save<int, object>("AddUserLogInDetails", parameter);
            dict.Status=HttpStatusCode.OK;
            dict.Data="User log successfully added";
            return dict;

        }
    }
}
