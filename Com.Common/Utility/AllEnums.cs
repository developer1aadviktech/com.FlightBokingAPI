using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.Utility
{
    public class AllEnums
    {
        public enum SupplierEnum
        {
            Amadeus = 1,
            Saber = 2,
        }
        public enum PaxtypeEnum
        {
            Adult = 1,
            Child = 2,
            Infant = 3,
        }
        public enum JourneyTypeEnum
        {
            OneWay = 1,
            RoundTrip = 2,
            MultiCity = 3,
        }
        public enum GenderEnum
        {
            Male = 1, //M
            Female = 2 //F
        }
        public enum RefundTypeEnum
        {
            Refundable = 1,
            NonRefundable = 2
        }

        public enum TempBookingStatusEnum
        {
            Pending = 1, //(do not call booking page direct yet)
            Wait = 2, //(booking page direct call)
            Error = 3, //(response come but error occurred)
            PNRCreated = 4, //(response come but error occurred)
            Complete = 5 //(response come but error occurred)
        }
        public enum BookingStatusEnum
        {

            Queue = 1,
            Ticketed = 2,
            Cancelled = 3
        }
        public enum MrkupTypeEnum
        {
            Fixed = 1,
            Percentage = 2,
        }
        public enum PaymentTypeEnum
        {
            Wallet = 1,
        }
        public enum PaymentStatusEnum
        {
            Pending = 0,
        }
        public enum SearchChanelEnum
        {
            Direct = 0,
        }
        public enum UserRoleEnum
        {
            [Description("Super Admin")]
            SuperAdmin = 1,
            [Description("Admin")]
            Admin = 2,
            [Description("User")]
            User = 3,            
            [Description("Reseller")]
            Reseller = 4,

        }

        public enum UserStatusEnum
        {
            [Description("Active")]
            Active = 1,
            [Description("Inactive")]
            Inactive = 2,
            [Description("Suspended")]
            Suspended = 3,

        }

    }
}
