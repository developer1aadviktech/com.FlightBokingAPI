using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Com.Common.Utility
{
    public static class WebRequestUtility
    {

        public static string invokeGetRequest(string requestUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpWebRequest request1 = WebRequest.Create(requestUrl) as HttpWebRequest;
            request1.Method = "GET";
            HttpWebResponse httpWebResponse = (HttpWebResponse)request1.GetResponse();
            StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseString = reader.ReadToEnd();
            return responseString;
        }

        public static async Task<string> InvokePostRequest(string requestUrl, string requestBody)
        {
            using (var client = new HttpClient())
            {
                // Set the Authorization header if a bearer token is provided
                //if (!string.IsNullOrEmpty(bearerToken))
                //{
                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                //}

                // Set the request content
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Make the POST request and get the response
                HttpResponseMessage response = await client.PostAsync(requestUrl, content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and return the response body
                return await response.Content.ReadAsStringAsync();
            }
        }

        /*public static string invokePostRequest(string requestUrl, string requestBody, string bearerToken = null)
        {

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {bearerToken}");
            }
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Timeout = (2 * 60 * 1000);
            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
            requestWriter.Write(requestBody);
            requestWriter.Close();
            HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            return responseReader.ReadToEnd();
        }*/

    }
}
