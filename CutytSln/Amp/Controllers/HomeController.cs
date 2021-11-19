using Amp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Amp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [OutputCache(Duration = 1200)]
        public IActionResult Index()
        {
            var url = this.HttpContext.Request.Path.ToString();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/watch")]
        public IActionResult Watch(string v)
        {
            var url = this.HttpContext.Request.Path.ToString();
            ViewResult view = View("Index", $"https://www.youtube.com/watch?v={v}");
            return view;
        }

        [Route("url")]
        public IActionResult Url(string url, string search)
        {
            var result = Json(new { url = $"https://stackoverflow.com/questions/50027232/how-to-redirect-a-page-amp/50043339" });
            return result;
        }

        [Route("time")]
        public IActionResult Time()
        {
            var result = Json(new { time = $"{DateTime.Now}" });
            return result;
        }

        [Route("times")]
        public IActionResult Times(string name)
        {
            
            var now = new { time = $"{DateTime.Now}" };
            var yesterday = new { time = $"{DateTime.Now.AddDays(-1)}" };

            List<object> times = new List<object>()
            { now, yesterday};

            JsonResult result;
            if (name != null)
            {
                result = Json(new { items = times });
            }
            else
            {
                result = Json(new { items = new List<object>() });
            }
            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}