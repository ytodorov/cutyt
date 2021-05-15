using Cutyt.Core;
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

        private string cutYtBaseAddress = "https://localhost:44309/";

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
                cutYtBaseAddress = "https://www.cutyt.com/";

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

        [Route("watch")]
        public IActionResult WatchYoutube(string v)
        {
            var queryString = Request.QueryString;
            IndexViewModel indexViewModel = new IndexViewModel();
            var queryStringNV = HttpUtility.ParseQueryString(queryString.ToString());
            indexViewModel.V = queryStringNV["v"];
            var view = RedirectPermanent($"{cutYtBaseAddress}youtube?v={v}");

            return view;
        }

        [Route("/youtube")]
        public IActionResult Youtube(string v)
        {
            IndexViewModel indexViewModel = new IndexViewModel();
            indexViewModel.V = v;
            var view = View("Index", indexViewModel);
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
        public async Task<JsonResult> Generate(string v, string vimeoId, string selectedOption)
        {
            if (string.IsNullOrEmpty(selectedOption))
            {
                selectedOption = "best";
            }

            string url = string.Empty;
            if (v != "-1")
            {

                url = $"https://www.youtube.com/watch?v={v}";
            }
            if (vimeoId != "-1")
            {
                url = $"https://vimeo.com/{vimeoId}";
            }

            // PROBLEMS with bestvideo%2Bbestaudio - youtube-dl is stuck and does not quit on time. This was due from wrong exe files.
            // best - use single file -> mp4
            var selectedOptionWithoutPlus = selectedOption.Replace("+", string.Empty);

            var outputFileName = $"{v}{selectedOptionWithoutPlus}";
            if (selectedOptionWithoutPlus.Contains("--audio-format"))
            {
                outputFileName = $"{v}{selectedOption.Split(" ").Last()}";
            }
            var encodedUrl = $"{serverAddressOfServices}home/exec?args=-f {HttpUtility.UrlEncode(selectedOption)}" +
                $" --no-part \"{url}\" --output \"{outputFileName}.%(ext)s\" -k -v&ytUrl={url}&V={v}&selectedOption={HttpUtility.UrlEncode(selectedOption)}";
            var json = await httpClient.GetStringAsync(encodedUrl); // %2B = +

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            var linkviewModel = JsonSerializer.Deserialize<LinkViewModel>(json, jsonSerializerOptions);

            return Json(linkviewModel);
        }

        [HttpPost]
        public IActionResult GetYouTubeV(string ytUrl)
        {
            var parsedQSTest = HttpUtility.ParseQueryString(ytUrl);

            ytUrl = Helpers.GetFullUrlFromYouTube(ytUrl, httpClientFactory.CreateClient());
            YouTubeInfoResult youTubeInfoResult = new YouTubeInfoResult();
            SetVideoIdPart(ytUrl, youTubeInfoResult);
            var parts = ytUrl?.Split(new string[] { "/watch?" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //string url = $"https://www.youtube.com/watch?v={v}";

            var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            List<YouTubeInfoViewModel> infos = httpClient.GetFromJsonAsync<List<YouTubeInfoViewModel>>($"{serverAddressOfServices}home/getyoutubeinfo?url={ytUrl}").Result;

            foreach (var info in infos)
            {
                info.TextWithoutCode = info.TextWithoutCode.Replace(", video only", string.Empty);
            }

            infos = infos.GroupBy(c => c.VideoResolutionP)
                .Select(s => s.LastOrDefault())
                .Where(s => s.VideoResolutionP != null)
                .ToList();

            youTubeInfoResult.Infos = infos;


            var result = new { youTubeInfoResult };
            return Json(result);
        }
        //getyoutubev

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string id)
        {
            if (id == "404")
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;               
            }
            else if (id == "500")
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            
            return View("Index");
        }

        private void SetVideoIdPart(string url, YouTubeInfoResult youTubeInfoResult)
        {
            youTubeInfoResult.V = "-1";
            youTubeInfoResult.VimeoId = "-1";
            var parts = url?.Split(new string[] { "/watch?" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts?.Count == 2)
            {
                var qs = parts[1];
                var parsedQS = HttpUtility.ParseQueryString(qs);
                var v = parsedQS["v"];
                youTubeInfoResult.V = v;
            }
            else if (url.Contains("https://vimeo.com/", StringComparison.InvariantCultureIgnoreCase))
            {
                var vimeoId = url.Replace("https://vimeo.com/", string.Empty);
                youTubeInfoResult.VimeoId = vimeoId;
            }
        }

        
    }
}
