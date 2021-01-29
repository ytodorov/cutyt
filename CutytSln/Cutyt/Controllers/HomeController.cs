using Cutyt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Cutyt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Generate(string from, string to)
        {
            var result = new { name = "Name (from server)" };
            if (int.TryParse(to, out int toInt))
            {
                if (toInt > 1000)
                {
                    Thread.Sleep(toInt);
                }
            }
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
