using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPS.Areas.Admin;
using OPS.Business;
using OPS.Entity;
using OPS.Entity.Schemas;
using System;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Microsoft.Extensions.Logging;
using OPS.Middlewares;
using OPS.Utility;

namespace OPS
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
            GlobalSettings.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Db context
            services.AddDbContext<OpsContext>(options => options.UseMySql(Configuration.GetConnectionString("OpsTest"),
                mySqlDbContextOptionsBuilder
                    =>
                {
                    mySqlDbContextOptionsBuilder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null!);
                }));

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });

            // Add Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<OpsContext>()
                .AddDefaultTokenProviders();

            // Password setting
            services.Configure<IdentityOptions>(options =>
            {
            //    // Password settings.
            //    options.Password.RequireDigit = true;
            //    options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequiredLength = 4;

            //    // Lockout settings.
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //    options.Lockout.MaxFailedAccessAttempts = 5;
            //    options.Lockout.AllowedForNewUsers = true;

            });
            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(720);
                options.LoginPath = "/account/login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
                options.EventsType = typeof(AdminAuthenticationEvents);
            });
            services.AddScoped<AdminAuthenticationEvents>();

            services.AddFlashMessage();

            services.AddControllersWithViews();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //services.AddSingleton<ILoggerRepo, LoggerRepo>();//==>Logger
            services.AddRouting(options => options.LowercaseUrls = true);

            var builder = services.AddRazorPages();
#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif
            //if (Env.IsDevelopment())
            //{
            //    builder.AddRazorRuntimeCompilation();
            //}
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            loggerFactory.AddLog4Net();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<InvalidUrlMiddleware>();//--> Catch invalid Url
            app.UseMiddleware<RequestResponseLoggingMiddleware>(); //--> logger
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "Admin",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Survey}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
