﻿using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Cutyt.Core.Rebus.Replies;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using WebApplication1.Models;
using static ProcessAsyncHelper;

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

        //public IActionResult GetAllFiles()
        //{
        //    var files = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").OrderByDescending(s => new FileInfo(s).CreationTime).ToList();

        //    files = files.Where(f => !f.EndsWith(".dll") && !f.EndsWith(".exe") && !f.EndsWith(".part") && !f.EndsWith(".ytdl") && !f.Contains("-frag", StringComparison.CurrentCultureIgnoreCase)).ToList();

        //    List<YoutubeDownloadLinkReply> list = new List<YoutubeDownloadLinkReply>();

        //    foreach (var file in files)
        //    {
        //        var name = Path.GetFileName(file);
        //        YoutubeDownloadLinkReply linkViewModel = new YoutubeDownloadLinkReply()
        //        {
        //            Name = name,
        //            Url = $"{serverAddressOfServices}{name}"
        //        };

        //        list.Add(linkViewModel);
        //    }


        //    return Json(list);
        //}

        //public IActionResult DetailProducts_Read([DataSourceRequest] DataSourceRequest request)
        //{
        //    var files = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").OrderByDescending(s => new FileInfo(s).CreationTime).ToList();

        //    files = files.Where(f => !f.EndsWith(".dll") && !f.EndsWith(".exe") && !f.EndsWith(".part") && !f.EndsWith(".ytdl") && !f.Contains("-frag", StringComparison.CurrentCultureIgnoreCase)).ToList();

        //    List<YoutubeDownloadLinkReply> list = new List<YoutubeDownloadLinkReply>();

        //    foreach (var file in files)
        //    {
        //        var name = Path.GetFileName(file);
        //        YoutubeDownloadLinkReply linkViewModel = new YoutubeDownloadLinkReply()
        //        {
        //            Name = name,
        //            Url = $"{serverAddressOfServices}{name}"
        //        };

        //        list.Add(linkViewModel);
        //    }

        //    return Json(list.ToDataSourceResult(request));
        //}

        //public IActionResult Exec(string program = "youtube-dl.exe", string args = "https://www.youtube.com/watch?v=rzfmZC3kg3M",
        //    string ytUrl = "", string v = "", string selectedOption = "", string start = "", string end = "", bool shouldTrim = false)
        //{
        //    // ???? ytUrl = https://consent.youtube.com/ml?continue=https://www.youtube.com/watch?v=_xtloJqfIrs&feature=youtu.be
        //    try
        //    {
        //        if (v?.Length > 20)
        //        {
        //            throw new Exception($"{v} is not valid for v in youtube URL");
        //        }
        //        selectedOption = HttpUtility.UrlDecode(selectedOption);
        //        string filePathResult = string.Empty;

        //        YoutubeDlHelper.FreeSpaceOnHardDiskIfNeeded();

        //        if (selectedOption.Contains("--audio-format"))
        //        {
        //            selectedOption = selectedOption.Split(" ").Last();
        //            filePathResult = YoutubeDlHelper.DownloadCustomAudio(v, selectedOption, telemetryClient);
        //        }
        //        else
        //        {
        //            var selectedVideoOption = selectedOption.Split(" ").FirstOrDefault();
        //            filePathResult = YoutubeDlHelper.MergeAudioAndVideoToMp4(v, selectedVideoOption, telemetryClient);
        //        }

        //        if (shouldTrim)
        //        {
        //            filePathResult = YoutubeDlHelper.CutFile(filePathResult, start, end, telemetryClient);
        //        }

        //        //AddWatermark(filePathResult); cannot be played in browser. dunno why

        //        var selectedOptionWithoutPlus = selectedOption.Replace("+", string.Empty);
        //        //var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";

        //        string physicalFileName = Path.GetFileName(filePathResult);

        //        var fileNameFromArgs = GetFileNameFromArgs(ytUrl);
        //        var fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileNameFromArgs);
        //        var fileNameWithoutDashV = fileNameWithoutExtensions.Replace($"-{v}", string.Empty);

        //        YoutubeDownloadLinkReply testLVM = new YoutubeDownloadLinkReply()
        //        {
        //            Name = fileNameFromArgs,
        //            Url = $"{serverAddressOfServices}{physicalFileName}",
        //            FileName = fileNameFromArgs,
        //            DisplayName = fileNameWithoutDashV,
        //            V = v,
        //            Start = start,  
        //            End = end,
        //        };

        //        return Json(testLVM);

        //    }
        //    catch (Exception ex)
        //    {
        //        telemetryClient.TrackException(ex);
        //        var json = new JsonResult(ex.Message + ex.StackTrace);
        //        return json;
        //    }
        //}

        //private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    var data = e.Data;
        //}

        ////private void ConvertFromMkvToMp4(string source, string destination)
        ////{
        ////    // ffmpeg -ss 00:01:00 -i input.mp4 -to 00:02:00 -c copy output.mp4

        ////    string filename = string.Empty;

        ////    var programFullPath = $@"{AppConstants.YtWorkingDir}\ffmpeg.exe";

        ////    Process p = new Process();
        ////    p.StartInfo.FileName = programFullPath;
        ////    p.StartInfo.Arguments = $" -i {source} -c copy {destination}";// $" -ss 00:01:00 -i input.mp4 -to 00:02:00 -c copy output.mp4";
        ////    p.StartInfo.RedirectStandardOutput = true;
        ////    p.StartInfo.RedirectStandardError = true;
        ////    p.StartInfo.UseShellExecute = false;

        ////    p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

        ////    p.StartInfo.CreateNoWindow = true;
        ////    p.StartInfo.ErrorDialog = false;

        ////    p.Start();


        ////    string result = p.StandardOutput.ReadToEnd().Trim();
        ////    string error = p.StandardError.ReadToEnd().Trim();

        ////    p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

        ////}

        ////private void AddWatermark(string fileName)
        ////{
        ////    var programFullPath = $@"{AppConstants.YtWorkingDir}\ffmpeg.exe";

        ////    string output = Path.Combine("E:\\Files", $"output{DateTime.Now.Ticks}{System.IO.Path.GetExtension(fileName)}");

        ////    Process p = new Process();
        ////    p.StartInfo.FileName = programFullPath;
        ////    p.StartInfo.Arguments = $"-i {fileName} -i logo.png -filter_complex \"overlay=main_w-overlay_w-5:main_h-overlay_h-5\" -codec:a copy {output}";// $" -ss 00:01:00 -i input.mp4 -to 00:02:00 -c copy output.mp4";
        ////    p.StartInfo.RedirectStandardOutput = true;
        ////    p.StartInfo.RedirectStandardError = true;
        ////    p.StartInfo.UseShellExecute = false;

        ////    p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

        ////    p.StartInfo.CreateNoWindow = true;
        ////    p.StartInfo.ErrorDialog = false;

        ////    p.Start();

        ////    string result = p.StandardOutput.ReadToEnd().Trim();
        ////    string error = p.StandardError.ReadToEnd().Trim();

        ////    p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

        ////    System.IO.File.Delete(fileName);

        ////    System.IO.File.Move(output, fileName, true);
        ////}

        //private string GetFileNameFromArgs(string ytUrl)
        //{
        //    string filename = string.Empty;
        //    // youtube-dl -f bestvideo+bestaudio "https://www.youtube.com/watch?v=LXb3EKWsInQ&t=156s" -k

        //    ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"--get-filename {ytUrl} --encoding UTF8").Result;

        //    //var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";

        //    //Process p = new Process();
        //    //p.StartInfo.FileName = programFullPath;
        //    //p.StartInfo.Arguments = $"--get-filename {ytUrl} --encoding UTF8"; // Very important to use proper encoding
        //    //p.StartInfo.RedirectStandardOutput = true;
        //    //p.StartInfo.RedirectStandardError = true;
        //    //p.StartInfo.UseShellExecute = false;

        //    //p.StartInfo.WorkingDirectory = $@"{AppConstants.YtWorkingDir}";

        //    //p.StartInfo.CreateNoWindow = true;
        //    //p.StartInfo.ErrorDialog = false;

        //    ////System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        //    //p.StartInfo.StandardOutputEncoding = Encoding.UTF8; // Very important to use proper encoding
        //    //p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        //    //p.Start();

        //    //string result = p.StandardOutput.ReadToEnd().Trim();

        //    //string error = p.StandardError.ReadToEnd();

        //    //p.WaitForExit(ProcessConstants.WaitForExitTotalMilliseconds);

        //    return res.StadardOutput;

        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
