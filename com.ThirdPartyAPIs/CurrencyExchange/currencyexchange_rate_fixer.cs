using Azure.Core;
using Com.Common.Utility;
using Com.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace com.ThirdPartyAPIs.CurrencyExchange
{
    public class currencyexchange_rate_fixer
    {
        private static string apikey = "66640eee0c9dfe5ce75cbbaa6f72260a";
        private string baseurl = "http://data.fixer.io/api/latest?access_key=" + apikey;

        private readonly IConfiguration _configuration;
        private readonly IGenericRepository _genericRepository;
        //private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _env;

        public currencyexchange_rate_fixer(IConfiguration configuration, IWebHostEnvironment env, IGenericRepository genericRepository)
        {
            _genericRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            //_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<Rootobject> get_exchage_rate()
        {
            var importantfiles = Path.Combine(_env.ContentRootPath, "ImportantFiles/");
            
            Rootobject Rootobject = new Rootobject();
            if (File.Exists(importantfiles + "fixercurrencyrate.json"))
            {
                string filetext = File.ReadAllText(importantfiles + "fixercurrencyrate.json");
                Rootobject = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(filetext);
                
                //return Rootobject;
                try
                {
                    if (Convert.ToDateTime(Rootobject.datetime_system).Date == DateTime.Now.Date)
                    {
                        return Rootobject;
                    }
                }
                catch { return Rootobject; }
            }
            //var reqt = new HttpRequestMessage(HttpMethod.Post, baseurl);
            //var response = await _httpClient.SendAsync(reqt);
            //var re = response.Content.ReadAsStringAsync().Result;
            var re = await WebRequestUtility.InvokePostRequest(baseurl, "");

            Rootobject = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(re);
            Rootobject.datetime_system = DateTime.Now.ToString();
            if (!string.IsNullOrWhiteSpace(importantfiles))
            {
                if (!Directory.Exists(importantfiles))
                    Directory.CreateDirectory(importantfiles);
                File.WriteAllText(Path.Combine(importantfiles + "fixercurrencyrate.json"), re ?? string.Empty, Encoding.UTF8);
            }

            return Rootobject;
        }


        public double currency_exchange_rate(Rootobject Rootobject, string from, string to)
        {
            double exchange_rate = 0;

            double first_currency_rate_to_base = 1;

            try
            {
                if (Rootobject != null && Rootobject.@base != null)
                {
                    if (Rootobject.@base.ToLower().Trim() != to.ToLower().Trim())
                    {
                        KeyValuePair<string, string> obj_first_currency_rate_to_base = Rootobject.rates.FirstOrDefault(t => t.Key.ToLower().Trim() == to.ToLower().Trim());
                        if (obj_first_currency_rate_to_base.Key != null && obj_first_currency_rate_to_base.Value != null)
                        {
                            first_currency_rate_to_base = Convert.ToDouble((1 / Convert.ToDouble(obj_first_currency_rate_to_base.Value)).ToString("#0.0000000000000000"));
                        }
                    }

                    KeyValuePair<string, string> obj_currency = Rootobject.rates.FirstOrDefault(t => t.Key.ToLower().Trim() == from.ToLower().Trim());
                    if (obj_currency.Key != null && obj_currency.Value != null)
                    {
                        exchange_rate = Convert.ToDouble(obj_currency.Value);
                    }
                }

            }
            catch
            {
                exchange_rate = 0;
            }


            return Convert.ToDouble((exchange_rate * first_currency_rate_to_base).ToString("#0.000000000000000000"));
        }

      
        public class Rootobject
        {
            public bool success { get; set; }
            public int timestamp { get; set; }
            public string @base { get; set; }
            public string datetime_system { get; set; }
            public string date { get; set; }
            public Dictionary<string, string> rates { get; set; }
        }


    }
}
