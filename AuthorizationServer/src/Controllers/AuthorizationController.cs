using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using FP.OAuth.AuthorizationServer.Configuration;
using FP.OAuth.AuthorizationServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace FP.OAuth.AuthorizationServer.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppConfig _appConfig;

        public AuthorizationController(
            IOptions<IdentityOptions> identityOptions,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _appConfig = configuration.Get<AppConfig>();
        }

        [HttpPost("~/auth/token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
        {
         
            if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                var ticket = await CreateTicketAsync(request, user);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            if (request.IsRefreshTokenGrantType())
            {
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerDefaults.AuthenticationScheme);
                var user = await _userManager.GetUserAsync(info.Principal);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The refresh token is no longer valid."
                    });
                }

                if (!await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                var ticket = await CreateTicketAsync(request, user, info.Properties);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            if (request.IsAuthorizationCodeGrantType())
            {
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerDefaults.AuthenticationScheme);
                var user = await _userManager.GetUserAsync(info.Principal);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The refresh token is no longer valid."
                    });
                }

                if (!await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }
                var ticket = await CreateTicketAsync(request, user, info.Properties);
                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }


            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user,
            AuthenticationProperties properties = null)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var ticket = new AuthenticationTicket(principal, properties, OpenIdConnectServerDefaults.AuthenticationScheme);

            if (request.IsPasswordGrantType())
            {
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles,
                }.Intersect(request.GetScopes()));
            }

            ticket.SetResources(_appConfig.Jwt.Audiences);

            foreach (Claim claim in ticket.Principal.Claims.Where(x=>x.Type != _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType))
            {
                List<string> destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                if (claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile) ||
                    claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email) ||
                    claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            ticket.SetAccessTokenLifetime(TimeSpan.FromHours(5));
           
            return ticket;
        }
    }
}
