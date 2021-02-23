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

        public HomeController(IHostEnvironment hostEnvironment, TelemetryClient telemetryClient)
        {
            this.hostEnvironment = hostEnvironment;
            this.telemetryClient = telemetryClient;
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
            p.WaitForExit((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

            string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            var rows = result.Split($"\n");

            List<YouTubeInfoViewModel> infos = new List<YouTubeInfoViewModel>();

            var startIndex = rows.ToList().IndexOf("format code  extension  resolution note");
            for (int i = startIndex + 1; i < rows.Length - 1; i++)
            {
                YouTubeInfoViewModel info = new YouTubeInfoViewModel();


                var currentRow = rows[i];

                info.TextWithoutCode = currentRow.Substring(4).Trim();

                var rowparts = currentRow.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                info.FormatCode = rowparts[0];
                info.Extension = rowparts[1];

                int infoLength = currentRow.LastIndexOf(",") - currentRow.IndexOf(",");

                int resolutionLength = currentRow.IndexOf(",") - 24;

                info.Resolution = currentRow.Substring(24, resolutionLength).Trim(',').Trim();

                info.Note = currentRow.Substring(currentRow.IndexOf(",") + 1, infoLength).Trim(',').Trim();

                info.Size = currentRow.Substring(currentRow.LastIndexOf(",") + 1).Trim();

                infos.Add(info);
            }

            return Json(infos);
        }

        public IActionResult Exec(string program = "youtube-dl.exe", string args = "https://www.youtube.com/watch?v=rzfmZC3kg3M")
        {
            HttpContext.Response.ContentType = "text/plain; charset=utf-8";
            if (!program.EndsWith(".exe"))
            {
                program += ".exe";
            }

            try
            {
                var programFullPath = @"E:\Files\youtube-dl.exe";

                var ticks = DateTime.Now.Ticks.ToString();

                var newProgramFullPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "files", ticks, program);
                var newDirectory = Path.GetDirectoryName(newProgramFullPath);
                Directory.CreateDirectory(newDirectory);

                //System.IO.File.Copy(programFullPath, newProgramFullPath);

                //var allMiscFiles = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "YoutubeMisc"));

                //foreach (var fileToCopy in allMiscFiles)
                //{
                //    newProgramFullPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "files", ticks, System.IO.Path.GetFileName(fileToCopy));
                //    System.IO.File.Copy(fileToCopy, newProgramFullPath, true);
                //}

                var fileNameFromArgs = GetFileNameFromArgs(args);
                var fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileNameFromArgs);
                var allFiles = Directory.GetFiles(@"E:\Files");

                var existingFiles = allFiles
                    .Where(f => f.Contains($"{fileNameWithoutExtensions}", StringComparison.InvariantCultureIgnoreCase) 
                    && !f.EndsWith(".part", StringComparison.InvariantCultureIgnoreCase)
                    && !f.EndsWith(".ytdl", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                var existingFile = existingFiles.OrderByDescending(s => new FileInfo(s).Length).FirstOrDefault();
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

                    p.OutputDataReceived += P_OutputDataReceived;

                    p.Start();
                    p.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);

                    string result = p.StandardOutput.ReadToEnd();

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
                    string error = p.StandardError.ReadToEnd();
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

                        //var startIndexOfV = args.IndexOf("?v=");
                        //var lastIndexOfQuote = args.LastIndexOf("\"");

                        //var v = args.Substring(startIndexOfV + 3, lastIndexOfQuote - 3 - startIndexOfV);

                        newFiles = newFiles.Where(f => f.Contains(fileNameWithoutExtensions, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        var newFile = newFiles.OrderByDescending(s => new FileInfo(s).LastAccessTimeUtc).FirstOrDefault();

                        string name = Path.GetFileName(newFile);

                        LinkViewModel linkViewModel = new LinkViewModel()
                        {
                            Name = name,
                            Url = $"http://cutyt.westeurope.cloudapp.azure.com/{finalFileName}"
                        };
                        return Json(linkViewModel);
                    }

                    LinkViewModel errorViewModel = new LinkViewModel()
                    {
                        Name = "error",
                        Url = error
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

        private string GetFileNameFromArgs(string args)
        {
            string filename = string.Empty;
            // youtube-dl -f bestvideo+bestaudio "https://www.youtube.com/watch?v=LXb3EKWsInQ&t=156s" -k

            var programFullPath = @"E:\Files\youtube-dl.exe";

            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = $"{args} --get-filename";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();
            p.WaitForExit((int)TimeSpan.FromHours(1).TotalMilliseconds);

            string result = p.StandardOutput.ReadToEnd().Trim();

            return result;


        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
