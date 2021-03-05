using Cutyt.Core.Classes;
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

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult GetYoutubeInfo(string url = "https://www.youtube.com/watch?v=vLM-v7LeiEg")
        {
            var programFullPath = @"E:\Files\youtube-dl.exe";
            var args = $"-F {url}";
            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();
            //p.WaitForExit((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

            string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

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
                if (resolutionParts?.Length == 3)
                {
                    var resolutionInP = resolutionParts[1]?.Trim();

                    if (!resolutionInP.Contains("audio", StringComparison.InvariantCultureIgnoreCase))
                    {
                        info.VideoResolutionP = resolutionInP;
                    }
                }

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
            var files = Directory.GetFiles(@"E:\Files").OrderByDescending(s => new FileInfo(s).CreationTime).ToList();

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

        public IActionResult Exec(string program = "youtube-dl.exe", string args = "https://www.youtube.com/watch?v=rzfmZC3kg3M", string ytUrl = "", string V = "", string selectedOption = "")
        {
            HttpContext.Response.ContentType = "text/plain; charset=utf-8";
            if (!program.EndsWith(".exe"))
            {
                program += ".exe";
            }

            try
            {
                var selectedOptionWithoutPlus = selectedOption.Replace("+", string.Empty);
                var programFullPath = @"E:\Files\youtube-dl.exe";

                var ticks = DateTime.Now.Ticks.ToString();

                var newProgramFullPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "files", ticks, program);
                var newDirectory = Path.GetDirectoryName(newProgramFullPath);
                Directory.CreateDirectory(newDirectory);

                var fileNameFromArgs = GetFileNameFromArgs(ytUrl, selectedOption);
                var fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileNameFromArgs);
                var allFiles = Directory.GetFiles(@"E:\Files");

                var existingFiles = allFiles
                    .Where(f => f.Contains($"{V}", StringComparison.InvariantCultureIgnoreCase) && f.Contains($"{selectedOptionWithoutPlus}", StringComparison.InvariantCultureIgnoreCase)
                    && !f.EndsWith(".part", StringComparison.InvariantCultureIgnoreCase)
                    && !f.EndsWith(".ytdl", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                var existingFile = existingFiles.OrderBy(s =>s.Length).FirstOrDefault();
                if (!string.IsNullOrEmpty(existingFile))
                {
                    var existingFileName = Path.GetFileName(existingFile);
                    LinkViewModel linkViewModel = new LinkViewModel()
                    {
                        Name = existingFileName,
                        Url = $"http://cutyt.westeurope.cloudapp.azure.com/{existingFileName}"
                    };
                    return Json(linkViewModel);
                }


                if (!string.IsNullOrEmpty(programFullPath))
                {

                    Process p = new Process();
                    p.StartInfo.FileName = programFullPath;
                    p.StartInfo.Arguments = args;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.UseShellExecute = false;

                    p.StartInfo.WorkingDirectory = @"E:\Files";

                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.ErrorDialog = false;

                    p.Start();
                    //p.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds);

                    string result = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();

                    string finalFileName = string.Empty;

                    var resultRows = result.Split($"\n", StringSplitOptions.RemoveEmptyEntries);

                    var lastResultRow = resultRows.Last(s => !string.IsNullOrWhiteSpace(s));

                    if (lastResultRow.Contains(" has already been downloaded and merged", StringComparison.InvariantCultureIgnoreCase))
                    {
                        finalFileName = lastResultRow.Replace(" has already been downloaded and merged", string.Empty).Replace("[download] ", string.Empty).Trim('"').Trim();
                    }
                    else if (lastResultRow.Contains("Merging formats into"))
                    {
                        finalFileName = lastResultRow.Replace("[ffmpeg] Merging formats into ", string.Empty).Trim('"').Trim();

                    }


                    EventTelemetry et = new EventTelemetry()
                    {
                        Name = "result",
                    };
                    et.Properties.Add("text", result);

                    telemetryClient.TrackEvent(et);
                   
                    if (error.Contains("error", StringComparison.InvariantCultureIgnoreCase))
                    {
                        EventTelemetry etError = new EventTelemetry()
                        {
                            Name = "error",
                        };
                        etError.Properties.Add("text", error);

                        telemetryClient.TrackEvent(etError);
                    }

                    if (error?.Contains("error", StringComparison.InvariantCultureIgnoreCase) != true)
                    {
                        var newFiles = Directory.GetFiles(@"E:\Files");

                        newFiles = newFiles.Where(f => f.Contains($"{V}{selectedOptionWithoutPlus}", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        var newFile = newFiles.OrderBy(s => s.Length).FirstOrDefault();

                        string name = Path.GetFileName(newFile);

                        LinkViewModel linkViewModel = new LinkViewModel()
                        {
                            Name = name,
                            Url = $"{serverAddressOfServices}{name}"
                        };
                        return Json(linkViewModel);
                    }

                    System.IO.File.WriteAllText(@"E:\Files\error.txt", error);

                    LinkViewModel errorViewModel = new LinkViewModel()
                    {
                        Name = "error",
                        Url = $"{serverAddressOfServices}error.txt"
                    };

                    return Json(errorViewModel);
                }
                return new JsonResult("No such program " + program);
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

        private string GetFileNameFromArgs(string ytUrl, string selectedOptions)
        {
            string filename = string.Empty;
            // youtube-dl -f bestvideo+bestaudio "https://www.youtube.com/watch?v=LXb3EKWsInQ&t=156s" -k

            var programFullPath = @"E:\Files\youtube-dl.exe";

            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = $"--get-filename {ytUrl}";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();
            //p.WaitForExit((int)TimeSpan.FromHours(1).TotalMilliseconds);

            string result = p.StandardOutput.ReadToEnd().Trim();

            //var extension = Path.GetExtension(result);
            //var fileNameWithoutExtension = Path.GetFileName(result);

            //string fileNameWithSelectedOption = $"{fileNameWithoutExtension}.{selectedOptions}{extension}";
            //return fileNameWithSelectedOption;
            return result;

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
