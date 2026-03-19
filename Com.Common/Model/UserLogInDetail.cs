using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.Model
{
    public class UserLogInDetail
    {

        public int UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string IPAddress { get; set; }
        public bool Active { get; set; }
        public bool IsFromMobile { get; set; }
        public string Token { get; set; }
    }
}
