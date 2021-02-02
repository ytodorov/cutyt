using Cutyt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Cutyt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IHttpClientFactory httpClientFactory;


        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Generate(string from, string to)
        {
            var httpClient = httpClientFactory.CreateClient();

            var youTubeV = "https://www.youtube.com/watch?v=oK_rLUl2_8w";

            var link = await httpClient.GetStringAsync($"http://cutyt.westeurope.cloudapp.azure.com/home/exec?args={youTubeV}");

            var resultLink = link.Replace(@"C:\inetpub\wwwroot\wwwroot\", "http://cutyt.westeurope.cloudapp.azure.com/").Replace("\\", "/");
            var name = resultLink.Substring(resultLink.LastIndexOf("/") + 1);
            var result = new { resultLink, name };
            return Json(result);
        }

        [HttpPost]
        public IActionResult GetYouTubeV(string ytUrl)
        {
            var parts = ytUrl?.Split(new string[] { "/watch?" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count == 2)
            {
                var qs = parts[1];
                var parsedQS = HttpUtility.ParseQueryString(qs);
                var v = parsedQS["v"];
                var result = new { v };
                return Json(result);
            }
            return Json(string.Empty);
        }
        //getyoutubev

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
