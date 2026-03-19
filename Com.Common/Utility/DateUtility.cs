
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.Utility
{
    public static class DateUtility
    {

        public static string GetDateTimeString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string GetDateString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string GetMarsDate()
        {
            return GetNowTime().ToString("dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string GetMarsTime()
        {
            return GetNowTime().ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Date must be in yyyy-MM-dd format
        public static DateTime GetDateFromString(string date)
        {
            return new DateTime(Convert.ToInt32(date.Split('-')[0]), Convert.ToInt32(date.Split('-')[1]), Convert.ToInt32(date.Split('-')[2]));
        }

        // Date must be in dd-MM-yyyy format
        public static DateTime GetDateFromStringKYC(string date)
        {
            try
            {
                return DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
            //return new DateTime(Convert.ToInt32(date.Split('-')[2]), Convert.ToInt32(date.Split('-')[1]), Convert.ToInt32(date.Split('-')[0]));
        }

        public static DateTime GetDateTimeFromString(string date)
        {
            return DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static DateTime GetNowTime()
        {
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            return indianTime;
        }


        public static string GetNowTimeStamp()
        {
            return GetNowTime().ToString("yyyyMMddHHmmssffff", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string GetYMDTimeStamp()
        {
            return GetNowTime().ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string GetFlightTime(DateTime date)
        {
            return date.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string GetFlightDateTime(DateTime date)
        {
            return date.ToString("dddd MMMM d, yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string GetFlightDate(DateTime date)
        {
            return date.ToString("dddd MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
        //2025-02-13T21:59:00+05:30
        public static string GetISODate()
        {
            return GetNowTime().ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static DateTime GetDateFromISOString(string date)
        {
            return DateTime.ParseExact(date, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static DateTime GetBankDateTime(this string date)
        {
            return DateTime.ParseExact(date, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }
        public static string GetBankDate(this DateTime date)
        {
            return date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        }

        public static DateTime GetBankDate(this string date)
        {
            return DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        }

        public static string GetJulianDate()
        {
            DateTime date = GetNowTime();
            string hour = date.Hour > 9 ? date.Hour.ToString() : $"0{date.Hour}";
            string minute = date.Minute > 9 ? date.Minute.ToString() : $"0{date.Minute}";
            string dt = $"{(date.Year % 10) * 1000 + date.DayOfYear}{hour}{minute}";
            return dt;
        }
        public static DateTime GetGiftCardUploadExpiryDate(string date)
        {
            return DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        //25 char Time
        public static string GetISOwithTimeZoneOffset()
        {
            return GetNowTime().ToString("yyyy-MM-dd'T'HH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
