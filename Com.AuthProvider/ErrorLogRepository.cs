using Com.Common.Model;
using Com.Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.AuthProvider
{
    public class ErrorLogRepository:IErrorLogRepository
    {

        private readonly AppDBContext _db;
        public ErrorLogRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task AddErrorLog(Exception ex, string module, string input)
        {
            try
            {
                ErrorLog errorLog = new ErrorLog();
                StringBuilder error = new StringBuilder();

                do
                {
                    error.Append(ex.Message + " | " + ex.StackTrace);
                    error.Append(Environment.NewLine);

                    ex = ex.InnerException;
                }
                while (ex != null);

                errorLog.Module = module;
                errorLog.Input = input;
                errorLog.Error = error.ToString();
                errorLog.CreatedOn = DateUtility.GetNowTime();

                _db.ErrorLog.Add(errorLog);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorLog errorLog = new ErrorLog();
                StringBuilder error = new StringBuilder();
                error.Append(e.Message);

                errorLog.Module = "ErrorLogRepositoryCommon -> AddErrorLog";
                errorLog.Input = "Exception occured in try of AddErrorLog";
                errorLog.Error = e.Message;
                errorLog.CreatedOn = DateUtility.GetNowTime();

                _db.Add(errorLog);
                _db.SaveChanges();
            }
        }

        public async Task AddErrorLog(string error, string module, string input)
        {
            ErrorLog errorLog = new ErrorLog();

            errorLog.Module = module;
            errorLog.Input = input;
            errorLog.Error = error;
            errorLog.CreatedOn = DateUtility.GetNowTime();

            _db.ErrorLog.Add(errorLog);
            _db.SaveChanges();

        }
    }
}
