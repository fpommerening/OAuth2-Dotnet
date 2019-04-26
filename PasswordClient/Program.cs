using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace FP.OAuth.PasswordClient
{
    class Program
    {
        static void Main(string[] args)
        {

            AccessToken accessToken = null;
            using (var tokenclient = new HttpClient())
            {
                tokenclient.BaseAddress = new Uri("https://oauth-registry.demo-apps.de");
                var request = new HttpRequestMessage(HttpMethod.Post, "auth/token");

                var keyValues = new List<KeyValuePair<string, string>>();

                keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));
                keyValues.Add(new KeyValuePair<string, string>("scope", "offline_access profile email roles"));
                keyValues.Add(new KeyValuePair<string, string>("resource", "https://oauth-resource.demo-apps.de"));
                keyValues.Add(new KeyValuePair<string, string>("username", "user-81bd13063d"));
                keyValues.Add(new KeyValuePair<string, string>("password", "PW-3f8e5514c599bd2#"));

                request.Content = new FormUrlEncodedContent(keyValues);
                var response = tokenclient.SendAsync(request).GetAwaiter().GetResult();
                var contentAsString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                accessToken = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessToken>(contentAsString);
            }

            using (var valueclient = new HttpClient())
            {
                valueclient.BaseAddress = new Uri( "https://oauth-resource.demo-apps.de");
                valueclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.access_token);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new {value = 42});
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = valueclient.PutAsync($"api/value/", data).GetAwaiter().GetResult();
                var contentAsString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                
            }

        }
    }
}
