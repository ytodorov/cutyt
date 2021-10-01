﻿using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Cutyt.Core.Storage;
using CutytKendoWeb.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using static ProcessAsyncHelper;

namespace CutytKendo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IHttpClientFactory httpClientFactory;

        private readonly IMemoryCache cache;

        private IWebHostEnvironment hostEnvironment;

        private string cutYtBaseAddress = "https://stcutyt.blob.core.windows.net/media/"; //"https://localhost:44347/";

        HttpClient httpClient = null;

        TelemetryClient telemetryClient;

        public HomeController(
            ILogger<HomeController> logger,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment hostEnvironment,
            IMemoryCache cache,
            TelemetryClient telemetryClient)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.hostEnvironment = hostEnvironment;
            this.cache = cache;
            this.telemetryClient = telemetryClient;

            var mn = Environment.MachineName;
            if (!mn.Equals("DESKTOP-B3U6MF0", StringComparison.InvariantCultureIgnoreCase) &&
                !mn.Equals("yTodorov-nb", StringComparison.InvariantCultureIgnoreCase))
            {
                cutYtBaseAddress = "https://stcutyt.blob.core.windows.net/media/"; //"https://www.cutyt.com/";
            }

            httpClient = httpClientFactory.CreateClient();

            // Very important to be big interval. Big files won't be downloaded else.
            httpClient.Timeout = TimeSpan.FromHours(2);

            AppConstants.YtWorkingDir = Path.Combine(hostEnvironment.WebRootPath, "downloads");
        }

        [OutputCache(Profile = "default")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [OutputCache(Profile = "default")]
        [Route("/alldownloads")]
        public IActionResult AllDownloads()
        {
            return View();
        }

        [OutputCache(Profile = "default")]
        [Route("/mydownloads")]
        public IActionResult MyDownloads()
        {
            return View();
        }

        [Route("/downloads/{id}")]
        public IActionResult Downloads(string id)
        {
            return View();
        }

        [Route("/contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public async Task<IActionResult> GetUrlDetails(PostDataViewModel postDataViewModel)
        {
            string url = postDataViewModel.Url;

            // get the single url
            var splits = url.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            if (splits.Count > 1)
            {
                url = splits[0];
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                var res = Content($"'{url}' must be valid URL!");
                res.StatusCode = 500;
                return res;
            }
            else
            {
                if (!result.ToString().Contains("youtube", StringComparison.CurrentCultureIgnoreCase) &&
                    !result.ToString().Contains("youtu.be", StringComparison.CurrentCultureIgnoreCase))
                {
                    var res = Content($"'{url}' must be valid YouTube url!");
                    res.StatusCode = 500;
                    return res;
                }
            }


            var fullUrl = Helpers.GetFullUrlFromYouTube(url, httpClientFactory.CreateClient());

            Uri uri = new Uri(fullUrl);
            var parsedQSTest = HttpUtility.ParseQueryString(uri.Query);
            var v = parsedQSTest["v"];

            fullUrl = $"https://www.youtube.com/watch?v={v}";

            //YouTubeUrlFullDescription youTubeUrlFullDescription = await YoutubeDlHelper.GetYouTubeUrlFullDescription(v, telemetryClient);

            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            var jsonStream = await httpClient.GetStreamAsync(
                $"https://execprogram.azurewebsites.net/run?command=youtube-dl.exe&args=-j \"{fullUrl}\"");

            var youTubeUrlFullDescription = JsonSerializer.Deserialize<YouTubeUrlFullDescription>(jsonStream, options);

            var durationInSeconds = youTubeUrlFullDescription.Duration;
            var infos = youTubeUrlFullDescription.Formats;

            foreach (var info in infos)
            {
                if (info.Width != null)
                {
                    info.DownloadSwitchAudioAndVideo = $"{info.Format_Id}+bestaudio";
                }
                else
                {
                    info.DownloadSwitchAudioAndVideo = info.Format_Id;
                }
            }

            // remove files bigger then 1GB
            infos = infos.GroupBy(c => c.Format_Note)
                .Select(s => s.LastOrDefault())
                .Where(s => s.Width != null)
                .Where(s => s.FileSize != null && s.FileSize < 1024 * 1024 * 1024) // 1 GB
                .ToList();

            YouTubeAllInfoViewModel allVM = new YouTubeAllInfoViewModel()
            {
                DurationInSeconds = durationInSeconds.GetValueOrDefault(),
                Formats = infos,
                Title = youTubeUrlFullDescription.Title
            };

            return PartialView(allVM);
        }

        [HttpPost]
        public async Task<IActionResult> GetDownloadLink(PostDataDownloadLinkViewModel postDataDownloadLinkViewModel)
        {
            //selectedOption = selectedOption?.Replace(" ", "+");
            var ytUrl = Helpers.GetFullUrlFromYouTube(postDataDownloadLinkViewModel.YtUrl, httpClientFactory.CreateClient());
            Uri uri = new Uri(ytUrl);
            var parsedQSTest = HttpUtility.ParseQueryString(uri.Query);
            postDataDownloadLinkViewModel.V = parsedQSTest["v"];
            if (string.IsNullOrEmpty(postDataDownloadLinkViewModel.V))
            {
                postDataDownloadLinkViewModel.V = ytUrl.Replace("https://youtu.be/", string.Empty);
            }

            if (string.IsNullOrEmpty(postDataDownloadLinkViewModel.SelectedOption))
            {
                postDataDownloadLinkViewModel.SelectedOption = "best";
            }

            string url = string.Empty;
            if (postDataDownloadLinkViewModel.V != "-1")
            {
                url = $"https://www.youtube.com/watch?v={postDataDownloadLinkViewModel.V}";
            }
            if (postDataDownloadLinkViewModel.VimeoId != "-1" && postDataDownloadLinkViewModel.VimeoId != null)
            {
                url = $"https://vimeo.com/{postDataDownloadLinkViewModel.VimeoId}";
            }

            // best - use single file -> mp4
            var selectedOptionWithoutPlus = postDataDownloadLinkViewModel.SelectedOption.Replace("+", string.Empty);

            var outputFileName = $"{postDataDownloadLinkViewModel.V}{selectedOptionWithoutPlus}";

            var job = new DownloadLinkRequestViewModel()
            {
                SelectedOption = postDataDownloadLinkViewModel.SelectedOption,
                Url = url,
                OutputFileName = outputFileName,
                V = postDataDownloadLinkViewModel.V,
                ShouldTrim = postDataDownloadLinkViewModel.ShouldTrim.GetValueOrDefault(),
                Start = postDataDownloadLinkViewModel.Start,
                End = postDataDownloadLinkViewModel.End,
                Ip = HttpContext.Connection.RemoteIpAddress.ToString(),
                Title = postDataDownloadLinkViewModel.Title

            };

            if (selectedOptionWithoutPlus.Contains("--audio-format"))
            {
                outputFileName = $"{postDataDownloadLinkViewModel.V}{postDataDownloadLinkViewModel.SelectedOption.Split(" ").Last()}";
                job.AudioFormat = postDataDownloadLinkViewModel.SelectedOption.Split(" ").Last();
            }

            job.OutputFileName = outputFileName;

            var json = JsonSerializer.Serialize(job);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var urlToGet = "https://execprogram.azurewebsites.net/getbloburl"; //"http://localhost:5036/getbloburl"; //"https://execprogram.azurewebsites.net/getbloburl";

            var response = await httpClient.PostAsync(urlToGet, data);

            var linkviewModel = await response.Content.ReadFromJsonAsync<YoutubeDownloadedFileInfo>();
            //var linkviewModel = await YoutubeDlHelper.GetDownloadLinkReply(job, telemetryClient, $"{cutYtBaseAddress}");

            return PartialView(linkviewModel);
        }

        [OutputCache(Profile = "short")]
        [Route("/getfiles")]
        public async Task<IActionResult> GetFiles([DataSourceRequest] DataSourceRequest request)
        {
            var blobs = await BlobStorageHelper.ListBlobs(telemetryClient);

            return Json(blobs.ToDataSourceResult(request));
        }

        [Route("/getmyfiles")]
        public async Task<IActionResult> GetMyFiles([DataSourceRequest] DataSourceRequest request)
        {
            var blobs = await BlobStorageHelper.ListBlobs(telemetryClient);
            blobs = blobs.Where(r => r.Ip == HttpContext.Connection.RemoteIpAddress.ToString()).ToList();

            return Json(blobs.ToDataSourceResult(request));
        }

        [Route("/watch")]
        public IActionResult Watch(string v)
        {
            var view = View("Index", $"https://www.youtube.com/watch?v={v}");
            return view;
        }

        [OutputCache(Profile = "default")]
        [Route("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/error")]
        public IActionResult Error()
        {
            if (HttpContext.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
            {
                return View("error500");
            }
            else if (HttpContext.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                return View("error400");
            }
            else
            {
                return View("error");
            }
        }

        public IActionResult GetEnv()
        {
            return Json(hostEnvironment.EnvironmentName);
        }

        [Route("/test")]
        public async Task<IActionResult> Test()
        {

            var json = JsonSerializer.Serialize(new DownloadLinkRequestViewModel());
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = "http://localhost:5036/getbloburl";
            var response = await httpClient.PostAsync(url, data);

            var obj = await response.Content.ReadFromJsonAsync<DownloadLinkRequestViewModel>();

            return Json(obj);
        }

        [Route("/local")]
        public IActionResult Local()
        {
            string local = @"D:\local";

            DirectoryInfo directoryInfo = new DirectoryInfo(local);
            if (directoryInfo.Exists)
            {
                var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Select(s => new { s.FullName, s.Length, s.CreationTimeUtc });
                return Json(files);
            }

            return Json("");
        }
    }
}
