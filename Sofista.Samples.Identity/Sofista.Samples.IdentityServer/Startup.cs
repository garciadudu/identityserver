using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sofista.Samples.IdentityServer.Infrastructure.Data;
using Sofista.Samples.IdentityServer.Models;

namespace Sofista.Samples.IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            string migrationsAssembly = typeof(Startup).Assembly.FullName;

            string connectionString = Configuration.GetConnectionString("IdentityServer");

            services.AddDbContextPool<SofistaDbContext>(options =>
                options.UseSqlServer(connectionString,
                config => config.MigrationsAssembly(migrationsAssembly)));


            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;

                options.User.RequireUniqueEmail = true;
                options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;

                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            }).AddDefaultTokenProviders()
            .AddEntityFrameworkStores<SofistaDbContext>();


            var idsvrConfig = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddJwtBearerClientAuthentication()
            .AddConfigurationStore(options =>
            {
                options.DefaultSchema = "auth";
                options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, config => config.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.DefaultSchema = "auth";
                options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, config => config.MigrationsAssembly(migrationsAssembly));
            });

            idsvrConfig.AddDeveloperSigningCredential();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Seed(app);

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static void Seed(IApplicationBuilder app)
        {
            var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();

            context.Clients.Add(new IdentityServer4.Models.Client
            {
                ClientId = "SofistaWebDev",
                ClientName = "SofistaWebDev",
                AllowedGrantTypes = GrantTypes.Implicit,
                RedirectUris = { "https://localhost:44931" },
                PostLogoutRedirectUris = { "https://localhost:44931" },
                RequireConsent = false,
                AllowedScopes = { "openid", "email", "profile", "sofistaapi" },
            }.ToEntity()
            );

            context.Clients.Add(new IdentityServer4.Models.Client
            {
                ClientId = "SofistaBackOffice",
                ClientSecrets = { new IdentityServer4.Models.Secret("SofistaPortalSecretBackOffice".ToSha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "https://localhost:44322/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:44322/" },
                RequireConsent = false,
                AllowedScopes = { "openid", "email", "profile", "sofistaapi" }
            }.ToEntity()
            );


            context.ApiResources.Add(new ApiResource("sofista", "Sofista").ToEntity());

            context.IdentityResources.Add(new IdentityResources.OpenId().ToEntity());
            context.IdentityResources.Add(new IdentityResources.Email().ToEntity());
            context.IdentityResources.Add(new IdentityResources.Address().ToEntity());
            context.IdentityResources.Add(new IdentityResources.Phone().ToEntity());
            context.IdentityResources.Add(new IdentityResources.Profile().ToEntity());

            context.SaveChanges();

            var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
            userManager.CreateAsync(new ApplicationUser
            {
                Email = "mitsao@hotmail.com",
                UserName = "mitsao@hotmail.com",
                Name = "Eduardo"
            }, "garcia").Wait();
        }
    }
}
