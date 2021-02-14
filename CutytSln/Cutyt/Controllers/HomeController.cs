﻿using Cutyt.Core.Classes;
using Cutyt.Core.Enums;
using Cutyt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Cutyt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IHttpClientFactory httpClientFactory;

        private IHostEnvironment hostEnvironment;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Generate(string v, DownloadOptions downloadOptions)
        {
            var httpClient = httpClientFactory.CreateClient();

            var youTubeV = $"https://www.youtube.com/watch?v={v}";

            string serverAddressOfServices = "http://localhost:14954/";
            string ytServerAddress = "https://localhost:44309/";

            if (!hostEnvironment.EnvironmentName.Equals("Development", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddressOfServices = "http://cutyt.westeurope.cloudapp.azure.com/";
                ytServerAddress = "https://www.cutyt.com/";

            }

            var additionalOptions = string.Empty;

            if (downloadOptions != DownloadOptions.VideoAndAudio)
            {
                additionalOptions = $" -x --audio-format {downloadOptions.ToString().ToLowerInvariant()} ";
            }

            var json = await httpClient.GetStringAsync($"{serverAddressOfServices}home/exec?args={additionalOptions}{youTubeV}");


            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            var linkviewModel = JsonSerializer.Deserialize<LinkViewModel>(json, jsonSerializerOptions);

            //var resultLink = link.Replace(@"C:\inetpub\wwwroot\wwwroot\", "http://cutyt.westeurope.cloudapp.azure.com/").Replace("\\", "/");
            //var name = resultLink.Substring(resultLink.LastIndexOf("/") + 1);

            var fileNameFromUrl = linkviewModel.Url.Substring(linkviewModel.Url.LastIndexOf("/") + 1);

            using (var client = new WebClient())
            {

                var localFile = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot", "files", fileNameFromUrl);

                client.DownloadFile(linkviewModel.Url, localFile);

            }

            LinkViewModel result = new LinkViewModel()
            {
                Name = fileNameFromUrl,
                Url = $"{ytServerAddress}files/{fileNameFromUrl}"
            };


            return Json(result);
        }

        [HttpPost]
        public IActionResult GetYouTubeV(string ytUrl)
        {
            ytUrl = GetFullUrlFromYouTube(ytUrl);
            var parts = ytUrl?.Split(new string[] { "/watch?" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts?.Count == 2)
            {
                var qs = parts[1];
                var parsedQS = HttpUtility.ParseQueryString(qs);
                var v = parsedQS["v"];
                var result = new { v };
                return Json(result);
            }
            return Json(string.Empty);
        }
        //getyoutubev

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string GetFullUrlFromYouTube(string url)
        {
            if (url?.ToLowerInvariant()?.Contains("youtu.be", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                var client = httpClientFactory.CreateClient();
                var response = client.GetAsync(url).Result;
                var fullUrl = response?.RequestMessage?.RequestUri?.ToString();
                return fullUrl;
            }
            else
            {
                return url;
            }
        }
    }
}
