using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using FP.OAuth.AuthorizationServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.MongoDb.Models;

namespace FP.OAuth.AuthorizationServer.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;
        private readonly IOptions<IdentityOptions> _identityOptions;

        public LoginController(IOptions<IdentityOptions> identityOptions,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            OpenIddictApplicationManager<OpenIddictApplication> applicationManager)
        {
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _applicationManager = applicationManager;
        }

        [HttpGet()]
        public IActionResult Index()
        {
            TempData["state"] = this.Request.Query["state"].ToString();
            var model = new LoginModel
            {
                response_type = Request.Query["response_type"].ToString(),
                client_id = Request.Query["client_id"].ToString(),
                redirect_uri = Request.Query["redirect_uri"].ToString(),
                state = Request.Query["state"].ToString(),
                scope = Request.Query["scope"].ToString(),
            };
            return View(model);
        }

        [HttpPost()]
        public async Task<IActionResult> Index(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.loginData.Username);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.loginData.Password, true);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var app = await _applicationManager.FindByClientIdAsync(model.client_id);
            if(user.Applications.All(x => x != app.Id))
            {
                return Unauthorized();
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);
            ticket.SetTokenUsage(OpenIdConnectConstants.TokenUsages.AuthorizationCode);

            var scope = model.scope.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (scope.Contains(OpenIdConnectConstants.Scopes.Profile))
            {
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(scope));
            }

            foreach (var claim in ticket.Principal.Claims.Where(x=>x.Type != _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType))
            {
                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                if (claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile) ||
                    claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }


            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }
    }
}
