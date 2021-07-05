using Cutyt.Core.Infrastructure;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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

            // The following line enables Application Insights telemetry collection.
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

            // Add framework services.
            services
                .AddControllersWithViews()
                // Maintain property names during serialization. See:
                // https://github.com/aspnet/Announcements/issues/194
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            // Add Kendo UI services to the services container
            services.AddKendo();

            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseDeveloperExceptionPage();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            //app.UseStatusCodePagesWithRedirects("/error?id={0}");

            app.Use(async (ctx, next) =>
            {
                await next();

                if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    string originalPath = ctx.Request.Path.Value;
                    ctx.Items["originalPath"] = originalPath;
                    ctx.Request.Path = "/";
                    await next();
                }
            });

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting?view=aspnetcore-2.2
            var mn = Environment.MachineName;
            if (!env.EnvironmentName.Equals("Development", StringComparison.InvariantCultureIgnoreCase) || !mn.Equals("DESKTOP-B3U6MF0", StringComparison.InvariantCultureIgnoreCase))
            {
                //Very problematic. !!!could lead to error: This site can't be reached
                app.UseRewriter(new RewriteOptions()
                //.AddRedirect("(.*)/$", "$1", (int)HttpStatusCode.MovedPermanently) // Strip trailing slash
                //.AddRedirect("(.*[^/])$", "$1/", (int)HttpStatusCode.MovedPermanently) // Enforce trailing slash - Problems with static files - not found with ending slash
                //.AddRedirect("^$", "/", (int)HttpStatusCode.MovedPermanently) // // Enforce trailing slash - Only for root domain

                //.AddRedirect("https://cutyt.com", "https://www.cutyt.com/", (int)HttpStatusCode.MovedPermanently)
                .AddRedirectToHttps((int)HttpStatusCode.MovedPermanently)
                .AddRedirectToWww((int)HttpStatusCode.MovedPermanently) //Very problematic. !!!could lead to error: This site can't be reached
                .Add(new RedirectLowerCaseRule())
                .AddRedirect("https://www.cutyt.com", "https://www.cutyt.com/", (int)HttpStatusCode.MovedPermanently)
                //.AddRedirect("https://www.cutyt.com/1", "https://www.cutyt.com/2", (int)HttpStatusCode.MovedPermanently)
                );
            }

            app.UseResponseCompression();

            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".mkv"] = "video/x-matroska";
            // Remove MP4 videos.

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            //app.UseHttpsRedirection();
            //app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}