using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.AuthProvider
{
    public interface IErrorLogRepository
    {
        Task AddErrorLog(Exception ex, string module, string input);
        Task AddErrorLog(string error, string module, string input);
    }
}
