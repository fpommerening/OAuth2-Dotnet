using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FP.OAuth.LoginWithGitHub.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace FP.OAuth.LoginWithGitHub.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly Guid _localId;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            var cfg = _configuration.Get<AppConfig>();
            _localId = !string.IsNullOrEmpty(cfg.LocalId) ? Guid.Parse(cfg.LocalId) : Guid.Empty;
        }

        [HttpGet("~/login")]
        public IActionResult Login()
        {
            var cfg = _configuration.Get<AppConfig>();
            var state = $"{_localId:N}#{Guid.NewGuid():N}";

            var queryParam = new Dictionary<string, string>
            {
                ["client_id"] = cfg.ClientId,
                ["state"] = state,
                ["scope"] = "user"
            };

            var url = QueryHelpers.AddQueryString("https://github.com/login/oauth/authorize", queryParam);
            return Redirect(url);
        }

        [HttpGet("~/api/oauth/access_token")]
        public async Task<IActionResult> AccessToken()
        {
            string state = Request.Query["state"];
            if (!string.IsNullOrEmpty(state) && state.Length == 65)
            {
                var proxyId = Guid.Parse(state.Substring(0, 32));
                if (proxyId == _localId)
                {
                    var accessToken = await GetAccessToken();
                    var userInfo = await GetGitHubUser(accessToken);
                    return View("../user/index", userInfo );
                }
            }
            return StatusCode((int) HttpStatusCode.BadRequest);
        }

        private async Task<AccessToken> GetAccessToken()
        {
            var cfg = _configuration.Get<AppConfig>();

            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://github.com/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "login/oauth/access_token");
                var keyValues = new List<KeyValuePair<string, string>>();

                keyValues.Add(new KeyValuePair<string, string>("grant_type", "code"));
                keyValues.Add(new KeyValuePair<string, string>("client_id", cfg.ClientId));
                keyValues.Add(new KeyValuePair<string, string>("client_secret", cfg.ClientSecret));
                keyValues.Add(new KeyValuePair<string, string>("code", Request.Query["code"]));


                tokenRequest.Content = new FormUrlEncodedContent(keyValues);
                var tokenResponse = await client.SendAsync(tokenRequest);
                var contentAsString = await tokenResponse.Content.ReadAsStringAsync();

                return Newtonsoft.Json.JsonConvert.DeserializeObject<Business.AccessToken>(contentAsString);
            }
        }

        private async Task<Business.GitHubUser> GetGitHubUser(Business.AccessToken accessToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(accessToken.token_type, accessToken.access_token);
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                var userRequest = new HttpRequestMessage(HttpMethod.Get, "user");

                var userResponse = await client.SendAsync(userRequest);
                var userContentAsString = await userResponse.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Business.GitHubUser>(userContentAsString);
            }
        }

    }
}