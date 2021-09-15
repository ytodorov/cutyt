using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
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

        private string cutYtBaseAddress = "https://localhost:44347/";

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
                cutYtBaseAddress = "https://www.cutyt.com/";
            }

            httpClient = httpClientFactory.CreateClient();

            // Very important to be big interval. Big files won't be downloaded else.
            httpClient.Timeout = TimeSpan.FromHours(2);

            AppConstants.YtWorkingDir = Path.Combine(hostEnvironment.WebRootPath, "downloads");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Route("/downloads")]
        public IActionResult Downloads()

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


            //var youTubeUrlFullDescriptionReply = await rebusBus.SendRequest<YouTubeUrlFullDescriptionReply>(new GetYouTubeUrlFullDescriptionJob() { Id = v }, timeout: AppConstants.RebusTimeout);

            YouTubeUrlFullDescription youTubeUrlFullDescription = await YoutubeDlHelper.GetYouTubeUrlFullDescription(v);
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
                Formats = infos
            };

            return PartialView(allVM);
        }

        public async Task<IActionResult> GetDownloadLink(string v, string vimeoId, string selectedOption, string ytUrl, double start, double end, bool? shouldTrim)
        {
            //selectedOption = selectedOption?.Replace(" ", "+");
            ytUrl = Helpers.GetFullUrlFromYouTube(ytUrl, httpClientFactory.CreateClient());
            Uri uri = new Uri(ytUrl);
            var parsedQSTest = HttpUtility.ParseQueryString(uri.Query);
            v = parsedQSTest["v"];
            if (string.IsNullOrEmpty(v))
            {
                v = ytUrl.Replace("https://youtu.be/", string.Empty);
            }

            if (string.IsNullOrEmpty(selectedOption))
            {
                selectedOption = "best";
            }

            string url = string.Empty;
            if (v != "-1")
            {
                url = $"https://www.youtube.com/watch?v={v}";
            }
            if (vimeoId != "-1" && vimeoId != null)
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

            //var linkviewModel = await rebusBus.SendRequest<YoutubeDownloadLinkReply>(new GetYoutubeDownloadLinkJob()
            //{
            //    SelectedOption = selectedOption,
            //    Url = url,
            //    OutputFileName = outputFileName,
            //    V = v,
            //    ShouldTrim = shouldTrim.GetValueOrDefault(),
            //    Start = start,
            //    End = end,

            //}
            //, timeout: AppConstants.RebusTimeout);

            var job = new GetYoutubeDownloadLinkJob()
            {
                SelectedOption = selectedOption,
                Url = url,
                OutputFileName = outputFileName,
                V = v,
                ShouldTrim = shouldTrim.GetValueOrDefault(),
                Start = start,
                End = end,

            };

            var linkviewModel = await YoutubeDlHelper.GetYoutubeDownloadLinkReply(job, telemetryClient, $"{cutYtBaseAddress}Downloads/");

            return PartialView(linkviewModel);
        }

        [Route("/getfiles")]
        public async Task<IActionResult> GetFiles([DataSourceRequest] DataSourceRequest request)
        {
            var url = $"{AppConstants.YtWorkingDir}\\DownloadedFilesInfo\\downloadedFiles.json";
            if (System.IO.File.Exists(url))
            {
                var json = await System.IO.File.ReadAllTextAsync(url); //await httpClient.GetStringAsync(url);
                var existingReplies = JsonSerializer.Deserialize<List<YoutubeDownloadLinkReply>>(json);
                existingReplies = existingReplies.OrderByDescending(s => s.DownloadedOn).ToList();
                var dsResult = existingReplies.ToDataSourceResult(request);
                return Json(dsResult);
            }
            return Json(new List<YoutubeDownloadLinkReply>().ToDataSourceResult(request));
        }

        [Route("/watch")]
        public IActionResult Watch(string v)
        {
            var view = View("Index", $"https://www.youtube.com/watch?v={v}");
            return view;
        }

        [Route("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [Route("/test")]
        public async Task<IActionResult> Test([FromQuery] string url = "https://www.youtube.com/watch?v=ImK7w8lmDhY")
        {
            var path = Path.Combine(hostEnvironment.WebRootPath, "Downloads");
            ProcessResult res = await ProcessAsyncHelper.ExecuteShellCommand($@"{path}\youtube-dl.exe", $"{url}");
            return Json(res.StadardOutput);
        }

        public IActionResult GetEnv()
        {
            return Json(hostEnvironment.EnvironmentName);
        }
    }
}
