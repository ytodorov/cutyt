using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Cutyt.Core.Storage;
using CutytKendoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Web;

namespace BlazorServerApp.Data
{
    public class WatchService
    {
        string baseUrl = "http://localhost:5036"; // http://localhost:5036 // https://execprogram.azurewebsites.net

        public async Task<YouTubeAllInfoViewModel> GetYouTubeAllInfoViewModel(string url)
        {
            // get the single url
            List<string> splits = url.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            if (splits.Count > 1)
            {
                url = splits[0];
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                //ContentResult res = Content($"'{url}' must be valid URL!");
                //res.StatusCode = 500;
                //return res;
            }
            else
            {
                if (!result.ToString().Contains("youtube", StringComparison.CurrentCultureIgnoreCase) &&
                    !result.ToString().Contains("youtu.be", StringComparison.CurrentCultureIgnoreCase))
                {
                    //ContentResult res = Content($"'{url}' must be valid YouTube url!");
                    //res.StatusCode = 500;
                    //return res;
                }
            }

            using var httpClient = new HttpClient();
            string fullUrl = Helpers.GetFullUrlFromYouTube(url, httpClient);

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

            allVM.Url = fullUrl;
            allVM.V = v;

            return allVM;
        }

        public async Task<YoutubeDownloadedFileInfo> GetDownloadLink(PostDataDownloadLinkViewModel postDataDownloadLinkViewModel)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromHours(1);

            string fullUrl = $"https://www.youtube.com/watch?v={postDataDownloadLinkViewModel.V}";

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            string jsonDescription = await httpClient.GetStringAsync(
                $"{baseUrl}/run?command=youtube-dl.exe&args=-j \"{fullUrl}\"");

            YouTubeUrlFullDescription youTubeUrlFullDescription = JsonSerializer.Deserialize<YouTubeUrlFullDescription>(jsonDescription, options);

            if (youTubeUrlFullDescription.Duration == postDataDownloadLinkViewModel.End && postDataDownloadLinkViewModel.Start == 0)
            {
                postDataDownloadLinkViewModel.ShouldTrim = false;
            }

            var uniqueKey = postDataDownloadLinkViewModel.UniqueKey;

  
            //selectedOption = selectedOption?.Replace(" ", "+");
            string ytUrl = Helpers.GetFullUrlFromYouTube(postDataDownloadLinkViewModel.YtUrl, httpClient);
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
                Ip = postDataDownloadLinkViewModel.Ip,
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

            return linkviewModel;
        }
    }
}