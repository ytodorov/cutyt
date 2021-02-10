using Cutyt.Core.Classes;
using Microsoft.ApplicationInsights;
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

        public IActionResult Exec(string program = "youtube-dl.exe", string args = "https://www.youtube.com/watch?v=rzfmZC3kg3M")
        {
            HttpContext.Response.ContentType = "text/plain; charset=utf-8";
            if (!program.EndsWith(".exe"))
            {
                program += ".exe";
            }

            try
            {
                var allFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories);




                var programFullPath = allFiles.FirstOrDefault(f => Path.GetFileName(f).ToLower().Equals(program.ToLower()));

                var newProgramFullPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "files", DateTime.Now.Ticks.ToString(), program);
                var newDirectory = Path.GetDirectoryName(newProgramFullPath);
                Directory.CreateDirectory(newDirectory);

                System.IO.File.Copy(programFullPath, newProgramFullPath);

                if (!string.IsNullOrEmpty(newProgramFullPath))
                {

                    Process p = new Process();
                    p.StartInfo.FileName = newProgramFullPath;
                    p.StartInfo.Arguments = args;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.UseShellExecute = false;

                    p.StartInfo.WorkingDirectory = newDirectory;

                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.ErrorDialog = false;

                    p.Start();
                    p.WaitForExit((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

                    string result = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    if (string.IsNullOrEmpty(error))
                    {
                        var newFiles = Directory.GetFiles(newDirectory);
                        var newFile = newFiles.FirstOrDefault(f => !f.EndsWith(".exe"));

                        string name = Path.GetFileName(newFile);

                        // Copy File To Azure Storage
                        // System.IO.File.Copy(newFile, Path.Combine(@"\\stcutyt.file.core.windows.net", name), true);

                        Helpers.UploadFileInAzureFileShare(newFile, hostEnvironment);

                        var fileToDelete = newFiles.FirstOrDefault(f => f.EndsWith(".exe"));
                        System.IO.File.Delete(fileToDelete);

                        string sas = "?sv=2019-12-12&ss=f&srt=sco&sp=rl&se=2051-02-09T05:56:05Z&st=2020-02-08T21:56:05Z&spr=https&sig=c5Z%2FrDJsaABP5NzNR56OI7RlVPCdfbJgBsCTxX3PiGw%3D";
                        string url = $"https://stcutyt.file.core.windows.net/cutyt/{name}{sas}";
                        LinkViewModel linkViewModel = new LinkViewModel()
                        {
                            Name = name,
                            Url = url
                        };                        
                        return Json(linkViewModel);
                    }
                    return Content(result + "ГРЕШКИ" + error, "text/plain");
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
