using Com.Common.DTO;
using Com.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Services.ServiceInterface
{
    public interface IAccountService
    {
        Task<CommonResponse> AddUserLogInDetail(UserLogInDetail userLogInDetail);
    }
}
