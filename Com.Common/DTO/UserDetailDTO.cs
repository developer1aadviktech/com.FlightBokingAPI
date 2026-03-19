using Com.Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.DTO
{
    public class AddUserDTO
    {

        //public int Id { get; set; }
        public int UserId { get; set; }
        //public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string FullName { get; set; }
        public string AgencyName { get; set; }
        public string AgencyCode { get; set; }
        public string Country { get; set; }
        public string PCCcode { get; set; }
        public string Currency { get; set; }
        public string CurrencySign { get; set; }
        public int MarkupType { get; set; }
        public decimal MarkupValue { get; set; }
        //public string Password { get; set; }
        
    }
    public class UserDetailDTO
    {

        //public int Id { get; set; }
        public int rno { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public int MarkupType { get; set; }
        public decimal MarkupValue { get; set; } = 0;
        public string FullName { get; set; }
        public string AgencyName { get; set; }
        public string AgencyCode { get; set; }
        public string Country { get; set; }
        public string PCCcode { get; set; }
        public string Currency { get; set; }
        public string CurrencySign { get; set; }
        //public string Password { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UserStatus { get; set; } // Assuming "UserStatus" is a string or enum

    }
    public class UserListDTO
    {

        //public int Id { get; set; }
        public int rno { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string FullName { get; set; }
        public string AgencyName { get; set; }
        public string AgencyCode { get; set; }
        //public string Password { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastActivity { get; set; }
        public int UserStatus { get; set; } // Assuming "UserStatus" is a string or enum

    }
}
