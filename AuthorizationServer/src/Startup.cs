﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AspNet.Security.OpenIdConnect.Primitives;
using FP.OAuth.AuthorizationServer.Configuration;
using FP.OAuth.AuthorizationServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenIddict.Abstractions;

namespace FP.OAuth.AuthorizationServer
{
    public class Startup
    {

        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppConfig>(_configuration);
            var appConfig = _configuration.Get<AppConfig>();

            services.AddCors();
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                })
                .AddSessionStateTempDataProvider();

            services.AddSession();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
                (
                    appConfig.Auth.ConnectionString,
                    appConfig.Auth.IdentityDatabase
                );

            services.AddSingleton(new MongoClient(appConfig.Auth.ConnectionString).GetDatabase(appConfig.Auth.TokenDatabase));

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;


            });

            services.AddOpenIddict()

                .AddCore(options =>
                {
                    options.UseMongoDb();

                })

                .AddServer(options =>
                {
                    options.RegisterScopes(OpenIdConnectConstants.Scopes.Email,
                        OpenIdConnectConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles
                    );

                    options.UseMvc();

                    options.EnableTokenEndpoint("/auth/token");
                    options.EnableAuthorizationEndpoint("/login");


                    options.AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowAuthorizationCodeFlow();

                    options.AcceptAnonymousClients();
                    options.UseJsonWebTokens();

                    options.DisableHttpsRequirement();

                    options.AddSigningKey(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.Jwt.SigningKey)));

                });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddAuthentication(o => { o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(options =>
                {
                    options.Authority = appConfig.Jwt.Authority;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = OpenIdConnectConstants.Claims.Subject,
                        RoleClaimType = OpenIdConnectConstants.Claims.Role,
                        ValidateIssuer = true,
                        ValidAudiences = appConfig.Jwt.Audiences,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.Jwt.SigningKey))
                    };
                });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = 
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.RequireHeaderSymmetry = false;
            });

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseAuthentication();
            app.UseSession();

            
            app.UseMvcWithDefaultRoute();
        }
    }
}
