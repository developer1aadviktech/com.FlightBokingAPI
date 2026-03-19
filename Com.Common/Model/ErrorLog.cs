using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.Model
{
    public class ErrorLog
    {
        [Key]
        public int ErrorId { get; set; }
        public string Module { get; set; }
        public string Input { get; set; }
        public string Error { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
