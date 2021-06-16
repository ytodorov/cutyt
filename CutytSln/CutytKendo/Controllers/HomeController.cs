using Cutyt.Core;
using Cutyt.Core.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace CutytKendo.Controllers
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

            var mn = Environment.MachineName;
            if (!hostEnvironment.EnvironmentName.Equals("Development", StringComparison.InvariantCultureIgnoreCase) || !mn.Equals("DESKTOP-B3U6MF0", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddressOfServices = "http://cutyt.westeurope.cloudapp.azure.com/";
                cutYtBaseAddress = "https://www.cutyt.com/";

            }

            httpClient = httpClientFactory.CreateClient();

            httpClient.Timeout = TimeSpan.FromMinutes(5);
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

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public async Task<IActionResult> GetUrlDetails(string url)
        {

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri resul))
            {
                var res = Content($"'{url}' must be valid URL!");
                res.StatusCode = 500;
                return res;
            }
            else
            {
                if (!resul.ToString().Contains("youtube", StringComparison.CurrentCultureIgnoreCase) &&
                    !resul.ToString().Contains("youtu.be", StringComparison.CurrentCultureIgnoreCase))
                {
                    var res = Content($"'{url}' must be valid YouTube url!");
                    res.StatusCode = 500;
                    return res;
                }
            }

            var fullUrl = Helpers.GetFullUrlFromYouTube(url, httpClientFactory.CreateClient());
            List<YouTubeInfoViewModel> infos = await httpClient.GetFromJsonAsync<List<YouTubeInfoViewModel>>($"{serverAddressOfServices}home/getyoutubeinfo?url={fullUrl}");
            string duration = await httpClient.GetFromJsonAsync<string>($"{serverAddressOfServices}home/GetYoutubeDuration?url={fullUrl}");


            var durationInSeconds = YoutubeDlHelper.GetTotalSecondsFromString(duration);

            
            foreach (var info in infos)
            {
                info.TextWithoutCode = info.TextWithoutCode.Replace(", video only", string.Empty);
            }

            infos = infos.GroupBy(c => c.VideoResolutionP)
                .Select(s => s.LastOrDefault())
                .Where(s => s.VideoResolutionP != null)
                .ToList();

            YouTubeAllInfoViewModel allVM = new YouTubeAllInfoViewModel()
            { 
                DurationInSeconds = durationInSeconds,
                Infos = infos
            };


            return PartialView(allVM);
        }

        public async Task<IActionResult> GetDownloadLink(string v, string vimeoId, string selectedOption, string ytUrl, string start, string end)
        {
            //selectedOption = selectedOption?.Replace(" ", "+");
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

            var encodedUrl = $"{serverAddressOfServices}home/exec?args=-f {HttpUtility.UrlEncode(selectedOption)}" +
                $" --no-part \"{url}\" --output \"{outputFileName}.%(ext)s\" -k -v&ytUrl={url}&V={v}&selectedOption={HttpUtility.UrlEncode(selectedOption)}" +
                $"&start={start}&end={end}";
            var json = await httpClient.GetStringAsync(encodedUrl); // %2B = +

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            LinkViewModel linkviewModel = JsonSerializer.Deserialize<LinkViewModel>(json, jsonSerializerOptions);

            return PartialView(linkviewModel);
        }

        [Route("/watch")]
        public IActionResult Watch(string v)
        {
            var view = View("Index", $"https://www.youtube.com/watch?v={v}");
            return view;
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult GetEnv()
        {
            return Json(hostEnvironment.EnvironmentName);
        }
    }
}
