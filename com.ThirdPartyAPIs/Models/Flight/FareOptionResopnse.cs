using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.ThirdPartyAPIs.Models.Flight
{
    public class FareOptionResopnse
    {
        public class FareUpsellResponse
        {
            public DateTime? upselldate { get; set; }
            public string? amount { get; set; }
            public string? currency { get; set; }
            public string? currency_sign { get; set; }
            public string? baggageDetail { get; set; }
            public string? cancellationFee { get; set; }
            public string? dateChange { get; set; }
            public string? uniqueRef { get; set; }
            public string? RBD { get; set; }
            public int fareindex { get; set; }
            public string? farebasis { get; set; }
            public string? primarycode { get; set; }
            public string? ticketdesignator { get; set; }
            public string? farequalifier { get; set; }
            public string? typequalifier { get; set; }
            public string? carrier { get; set; }
            public string? carriercode { get; set; }
            //public string? year { get; set; }
            //public string? month { get; set; }
            //public string? day { get; set; }
            //public string? hour { get; set; }
            //public string? minute { get; set; }
            //public string? second { get; set; }
            public string? lastTktDate { get; set; }
            public string? origincode { get; set; }
            public string? destinationcode { get; set; }
            public string? origin { get; set; }
            public string? destination { get; set; }
            public string? sessionData { get; set; }
            public string miniRule { get; set; }
        }
    }
}
