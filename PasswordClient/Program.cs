using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FP.OAuth.PasswordClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            var request = new HttpRequestMessage(HttpMethod.Post, "auth/token");

            var keyValues = new List<KeyValuePair<string, string>>();

            keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));
            keyValues.Add(new KeyValuePair<string, string>("scope", "offline_access profile email roles"));
            keyValues.Add(new KeyValuePair<string, string>("resource", "http://localhost:5000"));
            keyValues.Add(new KeyValuePair<string, string>("username", "user-8041fef86d"));
            keyValues.Add(new KeyValuePair<string, string>("password", "PW-8ba350344459df8#"));

            request.Content = new FormUrlEncodedContent(keyValues);
            var response = client.SendAsync(request).GetAwaiter().GetResult();
            var contentAsString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        }
    }
}
