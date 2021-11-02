using Cutyt.Core.Constants;
using Cutyt.Core.Extensions;
using Cutyt.Core.Hubs;
using Cutyt.Core.Infrastructure;
using Cutyt.Core.Rebus.Jobs;
using CutytKendoWeb;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebEssentials.AspNetCore.OutputCaching;

namespace CutytKendo
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
            services.AddSingleton<ITelemetryInitializer, RequestBodyInitializer>();
            services.AddApplicationInsightsTelemetry();

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Clear();
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();

                var mimeTypes = ResponseCompressionDefaults.MimeTypes;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "document", "text/html", "image/x-icon" });
            });


            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });



            services
                .AddControllersWithViews()
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddOutputCaching(options =>
            {
                // Disabling cache by setting duration to 1 - adds with IsMobile usage // TO DO
                options.Profiles["default"] = new OutputCacheProfile
                {
                    Duration = TimeSpan.FromHours(2).TotalSeconds,
                    VaryByParam = "c"
                };

                options.Profiles["short"] = new OutputCacheProfile
                {
                    Duration = TimeSpan.FromMinutes(10).TotalSeconds,
                    UseAbsoluteExpiration = true,
                };
            });

            services.AddKendo();

            services.AddHttpClient();

            services.AddSignalR()
                    .AddAzureSignalR("Endpoint=https://cutyt.service.signalr.net;AccessKey=CqW6IpODOQ1vwEncPHN67KhUIr08xvLLv1Y4HNoj7ek=;Version=1.0;");

           

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TelemetryClient applicationInsightsClient)
        {
            applicationInsightsClient.TrackEvent("Application Started");

            // Both must be here.
            app.UseExceptionHandler("/error"); // 500
            app.UseStatusCodePagesWithReExecute("/error", "?code={0}"); // 400

            app.Use(async (ctx, next) =>
            {
                if ((int)ctx.Response.StatusCode < 400)
                {
                    string body = await ctx.Request.GetRawBodyAsync();
                    ctx.Items.Add("_custom_http_body", body);
                }
                await next();
            });

            app.Use(async (ctx, next) =>
            {
                Stopwatch stopWatch = Stopwatch.StartNew();

                ctx.Response.OnStarting(
                    async () =>
                    {
                        ctx.Response.Headers["X-Response-Time"] = stopWatch.Elapsed.TotalMilliseconds.ToString();
                        await Task.FromResult(0);
                    });

                await next();
            });

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting?view=aspnetcore-2.2
            var mn = Environment.MachineName;
            if (!env.EnvironmentName.Equals("Development", StringComparison.InvariantCultureIgnoreCase) || !mn.Equals("yTodorov-nb", StringComparison.InvariantCultureIgnoreCase))
            {
                //Very problematic. !!!could lead to error: This site can't be reached
                app.UseRewriter(new RewriteOptions()
                .AddRedirect("(.*)/$", "$1", (int)HttpStatusCode.MovedPermanently) // Strip trailing slash
                //.AddRedirect("(.*[^/])$", "$1/", (int)HttpStatusCode.MovedPermanently) // Enforce trailing slash - Problems with static files - not found with ending slash
                //.AddRedirect("^$", "/", (int)HttpStatusCode.MovedPermanently) // // Enforce trailing slash - Only for root domain

                //.AddRedirect("https://cutyt.com", "https://www.cutyt.com/", (int)HttpStatusCode.MovedPermanently)
                .AddRedirectToHttps((int)HttpStatusCode.MovedPermanently)
                .AddRedirectToWww((int)HttpStatusCode.MovedPermanently) //Very problematic. !!!could lead to error: This site can't be reached
                .Add(new RedirectLowerCaseRule())
                //.AddRedirect("https://www.cutyt.com", "https://www.cutyt.com/", (int)HttpStatusCode.MovedPermanently)
                //.AddRedirect("https://www.cutyt.com/1", "https://www.cutyt.com/2", (int)HttpStatusCode.MovedPermanently)
                );
            }

            app.UseResponseCompression();

            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".mkv"] = "video/x-matroska";

            provider.Mappings[".opus"] = "audio/ogg";
            // Remove MP4 videos.

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            //app.UseHttpsRedirection();
            //app.UseStaticFiles();

            app.UseCookiePolicy();


            app.UseRouting();

            app.UseAuthorization();

            app.UseOutputCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<ChatHub>("/chat");
            });

        }

        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (DisallowsSameSiteNone(userAgent))
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

        public static bool DisallowsSameSiteNone(string userAgent)
        {
            // Check if a null or empty string has been passed in, since this
            // will cause further interrogation of the useragent to fail.
            if (String.IsNullOrWhiteSpace(userAgent))
                return false;

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking
            // stack.
            if (userAgent.Contains("CPU iPhone OS 12") ||
                userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
            // This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }

    }
}