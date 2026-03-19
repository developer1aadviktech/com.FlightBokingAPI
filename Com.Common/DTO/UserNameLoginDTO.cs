using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.DTO
{
    public class UserNameLoginDTO
    {
        [Required]
        public string UserName { get; set; }
    }
    public class UserLoginDTO : UserNameLoginDTO
    {
        [Required]
        public string Password { get; set; }
    }
    public class UserNameOTPLoginDTO : UserNameLoginDTO
    {
        [Required]
        public string OTP { get; set; }
    }
}
