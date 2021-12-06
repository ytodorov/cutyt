using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Cutyt.Core.Extensions;
using Cutyt.Core.Hubs;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Cutyt.Core.Redis;
using Cutyt.Core.Storage;
using CutytKendoWeb.Models;
using IpInfo;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;

namespace CutytKendo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;

        private IWebHostEnvironment hostEnvironment;

        HttpClient httpClient = null;

        TelemetryClient telemetryClient;

        IHubContext<ChatHub> chatHub;

        string baseUrl = "https://execprogram.azurewebsites.net"; // http://localhost:5036 // https://execprogram.azurewebsites.net

        public HomeController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment hostEnvironment,
            TelemetryClient telemetryClient,
            IHubContext<ChatHub> chatHub)
        {
            this.httpClientFactory = httpClientFactory;
            this.hostEnvironment = hostEnvironment;
            this.telemetryClient = telemetryClient;
            this.chatHub = chatHub;

            httpClient = httpClientFactory.CreateClient();

            // Very important to be big interval. Big files won't be downloaded else.
            httpClient.Timeout = TimeSpan.FromHours(2);

            AppConstants.YtWorkingDir = Path.Combine(hostEnvironment.WebRootPath, "downloads");

            if (Environment.MachineName.Equals("YTODOROV-NB", StringComparison.InvariantCultureIgnoreCase))
            {
                baseUrl = "http://localhost:5036";
            }
        }


        [OutputCache(Profile = "default")]
        public IActionResult Index()
        {
            return View();
        }

        public async Task About()
        {
            await chatHub.Clients.All.SendAsync("echo", "test", "test");

        }

        [Authorize]
        [Route("/login")]
        public IActionResult LogIn()
        {
            return View();
        }

        [Route("/signin-facebook")]
        public IActionResult SigninFacebook()
        {
            // OK, here we are after login
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

        [OutputCache(Profile = "default")]
        [Route("/trending")]
        public async Task<IActionResult> Trending(string c)
        {
            List<YoutubeTrendingViewModel> youtubeTrendings = new List<YoutubeTrendingViewModel>();

            List<string> regionCodes = new List<string>() { "IN", "US", "ID", "BR", "PH", "GB", "FR", "DE", "MA", "CA" };
            if (regionCodes.Any(s => s.Equals(c?.Trim(), StringComparison.InvariantCultureIgnoreCase)) == true)
            {
                regionCodes = new List<string>() { c.Trim().ToUpperInvariant() };
            }

            foreach (string regionCode in regionCodes)
            {
                string res = await httpClient.GetStringAsync(
                    $"https://www.googleapis.com/youtube/v3/videos?part=contentDetails&maxResults=9&chart=mostPopular&regionCode={regionCode}&key=AIzaSyAi72YHlA7Smr215lCmxgWjijA21Imchdk");

                JsonNode root = JsonNode.Parse(res);
                JsonNode items = root["items"];

                List<VideoDataViewModel> videoDatas = JsonSerializer.Deserialize<List<VideoDataViewModel>>
                    (items, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                foreach (VideoDataViewModel data in videoDatas)
                {
                    string fullUrl = $"https://www.youtube.com/watch?v={data.id}";

                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        PropertyNameCaseInsensitive = true
                    };

                    string json = await httpClient.GetStringAsync(
                        $"{baseUrl}/run?command=youtube-dl.exe&args=-j \"{fullUrl}\"");

                    if (!string.IsNullOrEmpty(json))
                    {
                        YouTubeUrlFullDescription youTubeUrlFullDescription = JsonSerializer.Deserialize<YouTubeUrlFullDescription>(json, options);

                        youtubeTrendings.Add(new YoutubeTrendingViewModel
                        {
                            Id = data.id,
                            V = data.id,
                            Likes = youTubeUrlFullDescription.Like_Count.GetValueOrDefault(),
                            Dislikes = youTubeUrlFullDescription.Dislike_Count.GetValueOrDefault(),
                            Views = youTubeUrlFullDescription.View_Count.GetValueOrDefault(),
                            AverageRating = Math.Round(youTubeUrlFullDescription.Average_Rating.GetValueOrDefault(), 2),
                            Name = youTubeUrlFullDescription.Title,
                            Url = fullUrl,
                            RegionCode = regionCode,
                            ImageUrl = youTubeUrlFullDescription?.Thumbnails?.Where(s => s.TestUrl != true)?.FirstOrDefault()?.Url
                        });
                    }

                }
            }


            return View(youtubeTrendings);
        }

        [Route("/downloads/{id}")]
        public IActionResult Downloads(string id)
        {
            return View();
        }

        [OutputCache(Profile = "default")]
        [Route("/mediaplayer")]
        public IActionResult MediaPlayer()
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
            List<string> splits = url.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            if (splits.Count > 1)
            {
                url = splits[0];
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                ContentResult res = Content($"'{url}' must be valid URL!");
                res.StatusCode = 500;
                return res;
            }
            else
            {
                if (!result.ToString().Contains("youtube", StringComparison.CurrentCultureIgnoreCase) &&
                    !result.ToString().Contains("youtu.be", StringComparison.CurrentCultureIgnoreCase))
                {
                    ContentResult res = Content($"'{url}' must be valid YouTube url!");
                    res.StatusCode = 500;
                    return res;
                }
            }


            string fullUrl = Helpers.GetFullUrlFromYouTube(url, httpClientFactory.CreateClient());

            Uri uri = new Uri(fullUrl);
            System.Collections.Specialized.NameValueCollection parsedQSTest = HttpUtility.ParseQueryString(uri.Query);
            string v = parsedQSTest["v"];

            fullUrl = $"https://www.youtube.com/watch?v={v}";

            //YouTubeUrlFullDescription youTubeUrlFullDescription = await YoutubeDlHelper.GetYouTubeUrlFullDescription(v, telemetryClient);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            string jsonStream = await httpClient.GetStringAsync(
                $"{baseUrl}/run?command=youtube-dl.exe&args=-j \"{fullUrl}\"");

            YouTubeUrlFullDescription youTubeUrlFullDescription = JsonSerializer.Deserialize<YouTubeUrlFullDescription>(jsonStream, options);

            long? durationInSeconds = youTubeUrlFullDescription.Duration;
            List<YouTubeFormat> infos = youTubeUrlFullDescription.Formats;

            foreach (YouTubeFormat info in infos)
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
            string ytUrl = Helpers.GetFullUrlFromYouTube(postDataDownloadLinkViewModel.YtUrl, httpClientFactory.CreateClient());
            Uri uri = new Uri(ytUrl);
            System.Collections.Specialized.NameValueCollection parsedQSTest = HttpUtility.ParseQueryString(uri.Query);
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
            string selectedOptionWithoutPlus = postDataDownloadLinkViewModel.SelectedOption.Replace("+", string.Empty);

            string outputFileName = $"{postDataDownloadLinkViewModel.V}{selectedOptionWithoutPlus}";

            DownloadLinkRequestViewModel job = new DownloadLinkRequestViewModel()
            {
                SelectedOption = postDataDownloadLinkViewModel.SelectedOption,
                Url = url,
                OutputFileName = outputFileName,
                V = postDataDownloadLinkViewModel.V,
                ShouldTrim = postDataDownloadLinkViewModel.ShouldTrim.GetValueOrDefault(),
                Start = postDataDownloadLinkViewModel.Start,
                End = postDataDownloadLinkViewModel.End,
                Ip = HttpContext.Connection.RemoteIpAddress.ToString(),
                Title = postDataDownloadLinkViewModel.Title,
                SignalrId = postDataDownloadLinkViewModel.SignalrId,
            };

            if (selectedOptionWithoutPlus.Contains("--audio-format"))
            {
                outputFileName = $"{postDataDownloadLinkViewModel.V}{postDataDownloadLinkViewModel.SelectedOption.Split(" ").Last()}";
                job.AudioFormat = postDataDownloadLinkViewModel.SelectedOption.Split(" ").Last();
            }

            job.OutputFileName = outputFileName;

            string json = JsonSerializer.Serialize(job);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

            string urlToGet = $"{baseUrl}/getbloburl";

            HttpResponseMessage response = await httpClient.PostAsync(urlToGet, data);
            string str = await response.Content.ReadAsStringAsync();
            YoutubeDownloadedFileInfo linkviewModel = await response.Content.ReadFromJsonAsync<YoutubeDownloadedFileInfo>();
            //var linkviewModel = await YoutubeDlHelper.GetDownloadLinkReply(job, telemetryClient, $"{cutYtBaseAddress}");

            return PartialView(linkviewModel);
        }

        [OutputCache(Profile = "short")]
        [Route("/getfiles")]
        public async Task<IActionResult> GetFiles([DataSourceRequest] DataSourceRequest request)
        {
            string query = $"\"DownloadedOnTicks\" > '{DateTime.UtcNow.Date.Ticks}'";

            List<YoutubeDownloadedFileInfo> blobs = await BlobStorageHelper.ListYoutubeDownloadedFileInfoBlobs("media", telemetryClient, query);

            return Json(blobs.ToDataSourceResult(request));
        }

        [Route("/getmyfiles")]
        public async Task<IActionResult> GetMyFiles([DataSourceRequest] DataSourceRequest request)
        {
            string query = $"\"Ip\" = '{HttpContext.Connection.RemoteIpAddress.ToString().Base64StringEncode()}'";
            List<YoutubeDownloadedFileInfo> blobs = await BlobStorageHelper.ListYoutubeDownloadedFileInfoBlobs("media", telemetryClient, query);
            blobs = blobs.Where(r => r.Ip == HttpContext.Connection.RemoteIpAddress.ToString()).ToList();

            return Json(blobs.ToDataSourceResult(request));
        }

        [Route("/watch")]
        public IActionResult Watch(string v)
        {
            ViewResult view = View("Index", $"https://www.youtube.com/watch?v={v}");
            return view;
        }

        [OutputCache(Profile = "default")]
        [Route("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [OutputCache(Profile = "default")]
        [Route("/terms")]
        public IActionResult Terms()
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

            string json = JsonSerializer.Serialize(new DownloadLinkRequestViewModel());
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

            string url = $"{baseUrl}/getbloburl";
            HttpResponseMessage response = await httpClient.PostAsync(url, data);

            DownloadLinkRequestViewModel obj = await response.Content.ReadFromJsonAsync<DownloadLinkRequestViewModel>();

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

        [Route("/getip")]
        public async Task<IActionResult> GetIp()
        {
            try
            {
                string stringIp = HttpContext.Connection.RemoteIpAddress.ToString();

                RedisValue ipDetailsAsJson = RedisConnection.Connection.GetDatabase().StringGet(stringIp);

                if (ipDetailsAsJson.IsNullOrEmpty)
                {
                    IpInfoApi api = new IpInfoApi("6a852f28bb9103", httpClient);

                    FullResponse response = await api.GetInformationByIpAsync(stringIp);

                    string responseAsJson = JsonSerializer.Serialize(response);

                    RedisConnection.Connection.GetDatabase().StringSet(stringIp, responseAsJson);

                    return Json(response);
                }
                else
                {
                    FullResponse response = JsonSerializer.Deserialize<FullResponse>(ipDetailsAsJson);
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                return Json(new FullResponse() { Ip = ex.Message });
            }
        }
    }
}
