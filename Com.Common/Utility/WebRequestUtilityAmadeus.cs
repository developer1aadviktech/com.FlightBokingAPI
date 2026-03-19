using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.Utility
{
    public static class WebRequestUtilityAmadeus
    {
        //private static readonly HttpClient _httpClient = new HttpClient();

        //public static async Task<string> InvokeAmadeusSoapAsync(string url, string soapAction, string content)
        public static async Task<string> InvokeAmadeusSoap(string url, string soapAction, StringContent content)
        {
            using (var client = new HttpClient())
            {

                // HTTP Request
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("soapAction", soapAction);
                request.Content = content;
                //request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/xml");
                //var response = await _httpClient.SendAsync(request);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
        public static async Task<string> InvokePostRequestAmadeus(string url, string soapAction, StringContent content)
        {
            using (var client = new HttpClient())
            {
                // HTTP Request
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("soapAction", soapAction);
                request.Content = content;
                //request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/xml");
                //var response = await _httpClient.SendAsync(request);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
        public static async Task<string> InvokePostRequestAmadeus_old(string url, string soapAction, string content)
        {
            using (var client = new HttpClient())
            {

                var reqcontent = new StringContent(content);
                // HTTP Request
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("soapAction", soapAction);
                request.Content = reqcontent;
                //request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/xml");
                //var response = await _httpClient.SendAsync(request);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
