using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FP.OAuth.AuthorizationServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.MongoDb.Models;

namespace FP.OAuth.AuthorizationServer.Controllers
{
    [Route("api/[controller]")]
    public class ApplicationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;

        public ApplicationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            OpenIddictApplicationManager<OpenIddictApplication> applicationManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationManager = applicationManager;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Add()
        {
            

            var model = new ApplicationModel();
            do
            {
                model.ClientId = Guid.NewGuid().ToString("N").Substring(0, 10);

            } while (await _applicationManager.FindByClientIdAsync(model.ClientId) != null);

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = model.ClientId,
                ClientSecret = model.ClientSecret,
                DisplayName = $"app-{model.ClientId}",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles
                }
            };

            var app = await _applicationManager.CreateAsync(descriptor);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = $"{model.ClientId}@demo-apps.local",
                EmailConfirmed = true,
                Applications = new List<ObjectId>{app.Id}
            };
            await _userManager.CreateAsync(user, model.UserPasssword);
            return View("index", model);
        }
    }
}
