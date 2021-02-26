using Cutyt.Core.Classes;
using Cutyt.Core.Enums;
using Cutyt.Core.ViewModels;
using Cutyt.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

        private readonly IMemoryCache cache;

        private IHostEnvironment hostEnvironment;

        private string serverAddressOfServices = "http://localhost:14954/";
        //private string ytServerAddress = "https://localhost:44309/";

        HttpClient httpClient = null;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.hostEnvironment = hostEnvironment;
            this.cache = cache;

            if (!hostEnvironment.EnvironmentName.Equals("Development", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddressOfServices = "http://cutyt.westeurope.cloudapp.azure.com/";
                //ytServerAddress = "https://www.cutyt.com/";
                //ytServerAddress = "http://cutyt.westeurope.cloudapp.azure.com/";

            }

            httpClient = httpClientFactory.CreateClient();

            httpClient.Timeout = TimeSpan.FromHours(1);
        }

        public IActionResult Index()
        {
            var queryString = Request.QueryString;
            IndexViewModel indexViewModel = new IndexViewModel();
            var queryStringNV = HttpUtility.ParseQueryString(queryString.ToString());
            indexViewModel.V = queryStringNV["v"];
            var view = View(indexViewModel);
            return view;
        }

        public async Task<JsonResult> GetAllFiles()
        {
            var json = await httpClient.GetStringAsync($"{serverAddressOfServices}home/getallfiles");

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            var items = JsonSerializer.Deserialize<List<LinkViewModel>>(json, jsonSerializerOptions);

            var result = new { items };

            return Json(result);
        }

            [HttpPost]
        public async Task<JsonResult> Generate(string v, string formatCode)
        {
            var youTubeUrl = $"https://www.youtube.com/watch?v={v}";

            // PROBLEMS with bestvideo%2Bbestaudio - youtube-dl is stuck and does not quit on time
            // best - use single file -> mp4
            var json = await httpClient.GetStringAsync($"{serverAddressOfServices}home/exec?args=-f best --no-part \"{youTubeUrl}\" -k -v&ytUrl={youTubeUrl}"); // %2B = +

             JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            var linkviewModel = JsonSerializer.Deserialize<LinkViewModel>(json, jsonSerializerOptions);

            var fileNameFromUrl = linkviewModel.Url.Substring(linkviewModel.Url.LastIndexOf("/") + 1);

            LinkViewModel result = new LinkViewModel()
            {
                Name = fileNameFromUrl,
                Url = $"{serverAddressOfServices}{fileNameFromUrl}"
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

                string url = $"https://www.youtube.com/watch?v={v}";

                var httpClient = httpClientFactory.CreateClient();
                var infos = httpClient.GetFromJsonAsync<List<YouTubeInfoViewModel>>($"{serverAddressOfServices}home/getyoutubeinfo?url={url}").Result;
                YouTubeInfoResult youTubeInfoResult = new YouTubeInfoResult();
                youTubeInfoResult.V = v;

                // show only best for now
                infos = infos.Where(s => s.TextWithoutCode.Contains("(best)")).ToList();
                youTubeInfoResult.Infos = infos;
                var result = new { youTubeInfoResult };
                return Json(result);
            }
            return Json(string.Empty);
        }
        //getyoutubev

        public IActionResult Privacy()
        {
            return View();
        }

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
