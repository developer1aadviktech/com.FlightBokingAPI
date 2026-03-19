using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.DTO
{
    public class UserLoginResultDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public string Currency { get; set; }
        public DateTime TokenExpireTime { get; set; }
    }
}
