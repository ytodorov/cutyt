using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostEnvironment hostEnvironment;

        private TelemetryClient telemetryClient;

        private string serverAddressOfServices = "http://localhost:14954/";

        public HomeController(IHostEnvironment hostEnvironment, TelemetryClient telemetryClient)
        {
            this.hostEnvironment = hostEnvironment;
            this.telemetryClient = telemetryClient;

            if (!hostEnvironment.EnvironmentName.Equals("Development", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddressOfServices = "http://cutyt.westeurope.cloudapp.azure.com/";

            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Kmp()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult GetYoutubeDuration(string url = "https://www.youtube.com/watch?v=vLM-v7LeiEg")
        {
            var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";
            var args = $"{url} --get-duration";
            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();
            
            string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

            if (!string.IsNullOrEmpty(error))
            {
                telemetryClient.TrackException(new Exception(error));
            }

            return Json(result);
        }
            public IActionResult GetYoutubeInfo(string url = "https://www.youtube.com/watch?v=vLM-v7LeiEg")
        {
            var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";
            var args = $"-F {url}";
            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;


            EventTelemetry eventTelemetry = new EventTelemetry("Start GetYoutubeInfo");
            eventTelemetry.Properties.Add("url", url);
            telemetryClient.TrackEvent(eventTelemetry);
            p.Start();
            
            string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

            if (!string.IsNullOrEmpty(error))
            {
                telemetryClient.TrackException(new Exception(error));
            }

            var rows = result.Split($"\n");

            List<YouTubeInfoViewModel> infos = new List<YouTubeInfoViewModel>();

            var startIndex = rows.ToList().IndexOf(rows.FirstOrDefault(f=> f.StartsWith("format")));
            for (int i = startIndex + 1; i < rows.Length - 1; i++)
            {
                YouTubeInfoViewModel info = new YouTubeInfoViewModel();


                var currentRow = rows[i];

                

                var rowparts = currentRow.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                info.FormatCode = rowparts[0];
                info.Extension = rowparts[1];

                info.TextWithoutCode = currentRow.Replace(rowparts[0], string.Empty).Trim();

                int infoLength = currentRow.LastIndexOf(",") - currentRow.IndexOf(",");

                if (currentRow.Contains("audio only", StringComparison.InvariantCultureIgnoreCase))
                {
                    info.Resolution = $"{rowparts[2]} {rowparts[3]}";
                }
                else
                {
                    info.Resolution = rowparts[2];
                }
                int resolutionLength = currentRow.IndexOf(",") - 24;

                //info.Resolution = currentRow.Substring(24, resolutionLength).Trim(',').Trim();

                var resolutionParts = info.Resolution?.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                if (!resolutionParts[0].Contains("audio", StringComparison.InvariantCultureIgnoreCase))
                {
                    info.ResolutionWidthByHeight = resolutionParts[0];
                }

                info.Note = currentRow.Substring(currentRow.IndexOf(",") + 1, infoLength).Trim(',').Trim();

                info.Size = currentRow.Substring(currentRow.LastIndexOf(",") + 1).Trim();

                infos.Add(info);
            }

            foreach (var info in infos)
            {
                if (info.TextWithoutCode.Contains("video only"))
                {
                    info.DownloadSwitchAudioAndVideo = $"{info.FormatCode}+bestaudio";
                }
                else
                {
                    info.DownloadSwitchAudioAndVideo = info.FormatCode;
                }
                //else if (!info.TextWithoutCode.Contains("audio only"))
                //{
                //    info.DownloadSwitchAudioAndVideo = info.FormatCode;
                //}
                //else
                //{
                    
                //}
            }

            return Json(infos);
        }

        public IActionResult GetAllFiles()
        {
            var files = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").OrderByDescending(s => new FileInfo(s).CreationTime).ToList();

            files = files.Where(f => !f.EndsWith(".dll") && !f.EndsWith(".exe") && !f.EndsWith(".part") && !f.EndsWith(".ytdl") && !f.Contains("-frag", StringComparison.CurrentCultureIgnoreCase)).ToList();

            List<LinkViewModel> list = new List<LinkViewModel>();

            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                LinkViewModel linkViewModel = new LinkViewModel()
                {
                    Name = name,
                    Url = $"{serverAddressOfServices}{name}"
                };

                list.Add(linkViewModel);
            }


            return Json(list);
        }

        public IActionResult DetailProducts_Read([DataSourceRequest] DataSourceRequest request)
        {
            var files = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").OrderByDescending(s => new FileInfo(s).CreationTime).ToList();

            files = files.Where(f => !f.EndsWith(".dll") && !f.EndsWith(".exe") && !f.EndsWith(".part") && !f.EndsWith(".ytdl") && !f.Contains("-frag", StringComparison.CurrentCultureIgnoreCase)).ToList();

            List<LinkViewModel> list = new List<LinkViewModel>();

            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                LinkViewModel linkViewModel = new LinkViewModel()
                {
                    Name = name,
                    Url = $"{serverAddressOfServices}{name}"
                };

                list.Add(linkViewModel);
            }

            return Json(list.ToDataSourceResult(request));
        }

        public IActionResult Exec(string program = "youtube-dl.exe", string args = "https://www.youtube.com/watch?v=rzfmZC3kg3M", string ytUrl = "", string v = "", string selectedOption = "", string start = "", string end = "")
        {
            try
            {
                selectedOption = HttpUtility.UrlDecode(selectedOption);
                string filePathResult = string.Empty;
                if (selectedOption.Contains("--audio-format"))
                {
                    selectedOption = selectedOption.Split(" ").Last();
                    filePathResult = YoutubeDlHelper.DownloadCustomAudio(v, selectedOption, telemetryClient);
                }
                else
                {
                    var selectedVideoOption = selectedOption.Split(" ").FirstOrDefault();
                    filePathResult = YoutubeDlHelper.MergeAudioAndVideoToMp4(v, selectedVideoOption, telemetryClient);
                }

                filePathResult = YoutubeDlHelper.CutFile(filePathResult, start, end, telemetryClient);

                //AddWatermark(filePathResult); cannot be played in browser. dunno why

                var selectedOptionWithoutPlus = selectedOption.Replace("+", string.Empty);
                //var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";

                string physicalFileName = Path.GetFileName(filePathResult);

                var fileNameFromArgs = GetFileNameFromArgs(ytUrl);
                var fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileNameFromArgs);
                var fileNameWithoutDashV = fileNameWithoutExtensions.Replace($"-{v}", string.Empty);

                LinkViewModel testLVM = new LinkViewModel()
                {
                    Name = fileNameFromArgs,
                    Url = $"{serverAddressOfServices}{physicalFileName}",
                    FileName = fileNameFromArgs,
                    DisplayName = fileNameWithoutDashV,
                };

                return Json(testLVM);

            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                var json = new JsonResult(ex.Message + ex.StackTrace);
                return json;
            }
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;
        }

        //private void ConvertFromMkvToMp4(string source, string destination)
        //{
        //    // ffmpeg -ss 00:01:00 -i input.mp4 -to 00:02:00 -c copy output.mp4

        //    string filename = string.Empty;

        //    var programFullPath = $@"{AppConstants.YtWorkingDir}\ffmpeg.exe";

        //    Process p = new Process();
        //    p.StartInfo.FileName = programFullPath;
        //    p.StartInfo.Arguments = $" -i {source} -c copy {destination}";// $" -ss 00:01:00 -i input.mp4 -to 00:02:00 -c copy output.mp4";
        //    p.StartInfo.RedirectStandardOutput = true;
        //    p.StartInfo.RedirectStandardError = true;
        //    p.StartInfo.UseShellExecute = false;

        //    p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

        //    p.StartInfo.CreateNoWindow = true;
        //    p.StartInfo.ErrorDialog = false;

        //    p.Start();


        //    string result = p.StandardOutput.ReadToEnd().Trim();
        //    string error = p.StandardError.ReadToEnd().Trim();

        //    p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

        //}

        //private void AddWatermark(string fileName)
        //{
        //    var programFullPath = $@"{AppConstants.YtWorkingDir}\ffmpeg.exe";

        //    string output = Path.Combine("E:\\Files", $"output{DateTime.Now.Ticks}{System.IO.Path.GetExtension(fileName)}");

        //    Process p = new Process();
        //    p.StartInfo.FileName = programFullPath;
        //    p.StartInfo.Arguments = $"-i {fileName} -i logo.png -filter_complex \"overlay=main_w-overlay_w-5:main_h-overlay_h-5\" -codec:a copy {output}";// $" -ss 00:01:00 -i input.mp4 -to 00:02:00 -c copy output.mp4";
        //    p.StartInfo.RedirectStandardOutput = true;
        //    p.StartInfo.RedirectStandardError = true;
        //    p.StartInfo.UseShellExecute = false;

        //    p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

        //    p.StartInfo.CreateNoWindow = true;
        //    p.StartInfo.ErrorDialog = false;

        //    p.Start();

        //    string result = p.StandardOutput.ReadToEnd().Trim();
        //    string error = p.StandardError.ReadToEnd().Trim();

        //    p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

        //    System.IO.File.Delete(fileName);

        //    System.IO.File.Move(output, fileName, true);
        //}

        private string GetFileNameFromArgs(string ytUrl)
        {
            string filename = string.Empty;
            // youtube-dl -f bestvideo+bestaudio "https://www.youtube.com/watch?v=LXb3EKWsInQ&t=156s" -k

            var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";

            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = $"--get-filename {ytUrl}";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();
            
            string result = p.StandardOutput.ReadToEnd().Trim();
            string error = p.StandardError.ReadToEnd();

            p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

            return result;

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
