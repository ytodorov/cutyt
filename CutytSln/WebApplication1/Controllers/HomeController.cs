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
                //var allFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories);




                //var programFullPath = allFiles.FirstOrDefault(f => Path.GetFileName(f).ToLower().Equals(program.ToLower()));


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
                    p.WaitForExit((int)TimeSpan.FromHours(1).TotalMilliseconds);

                    string result = p.StandardOutput.ReadToEnd();

                    string finalFileName = string.Empty;

                    var resultRows = result.Split($"\n", StringSplitOptions.RemoveEmptyEntries);

                    var lastResultRow = resultRows.Last(s => !string.IsNullOrWhiteSpace(s));

                    if (lastResultRow.Contains(" has already been downloaded and merged", StringComparison.InvariantCultureIgnoreCase))
                    {
                        finalFileName = lastResultRow.Replace(" has already been downloaded and merged", string.Empty).Replace("[download] ", string.Empty).Trim();
                    }
                    else if (lastResultRow.Contains("Merging formats into"))
                    {
                        finalFileName = lastResultRow.Replace("[ffmpeg] Merging formats into ", string.Empty).Trim();

                    }

                    string error = p.StandardError.ReadToEnd();
                    error = error.Replace("ERROR:", string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        EventTelemetry et = new EventTelemetry()
                        {
                            Name = "result",
                        };
                        et.Properties.Add("text", result);

                        telemetryClient.TrackEvent(et);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        EventTelemetry et = new EventTelemetry()
                        {
                            Name = "error",
                        };
                        et.Properties.Add("text", error);

                        telemetryClient.TrackEvent(et);
                    }

                    string serverUrl = "http://localhost:14954";

                    if (!hostEnvironment.IsDevelopment())
                    {
                        serverUrl = "http://cutyt.westeurope.cloudapp.azure.com";
                    }

                    if (error?.Contains("error", StringComparison.InvariantCultureIgnoreCase) != true)
                    {
                        var newFiles = Directory.GetFiles(@"E:\Files");

                        var startIndexOfV = args.IndexOf("?v=");
                        var lastIndexOfQuote = args.LastIndexOf("\"");

                        var v = args.Substring(startIndexOfV + 3, lastIndexOfQuote - 3 - startIndexOfV);


                        //var v = args.Replace("https://www.youtube.com/watch?v=", string.Empty);
                        newFiles = newFiles.Where(f => f.Contains(finalFileName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        var newFile = newFiles.OrderByDescending(s => new FileInfo(s).LastAccessTimeUtc).FirstOrDefault();

                        string name = Path.GetFileName(newFile);

                        // Copy File To Azure Storage
                        // System.IO.File.Copy(newFile, Path.Combine(@"\\stcutyt.file.core.windows.net", name), true);

                        //Helpers.UploadFileInAzureFileShare(newFile, hostEnvironment);

                        //var fileToDelete = newFiles.FirstOrDefault(f => f.EndsWith(".exe"));
                        //System.IO.File.Delete(fileToDelete);

                        //string sas = "?sv=2019-12-12&ss=f&srt=sco&sp=rl&se=2051-02-09T05:56:05Z&st=2020-02-08T21:56:05Z&spr=https&sig=c5Z%2FrDJsaABP5NzNR56OI7RlVPCdfbJgBsCTxX3PiGw%3D";
                        //string url = $"https://stcutyt.file.core.windows.net/cutyt/{name}{sas}";


                        //var fileInWwwFiles = Path.Combine(newDirectory, name);
                        //System.IO.File.Copy(newFile, fileInWwwFiles);

                        //string url = fileInWwwFiles.Replace(hostEnvironment.ContentRootPath, serverUrl).Replace("\\", "/").Replace("wwwroot/", string.Empty);
                        LinkViewModel linkViewModel = new LinkViewModel()
                        {
                            Name = name,
                            Url = $"http://cutyt.westeurope.cloudapp.azure.com/{finalFileName}"
                        };                        
                        return Json(linkViewModel);
                    }
                                      
                    //var errorFilePath = Path.Combine(hostEnvironment.ContentRootPath, "error.txt");
                    //System.IO.File.WriteAllText(errorFilePath, error);

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
