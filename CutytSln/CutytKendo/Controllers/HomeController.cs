using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CutytKendo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IHttpClientFactory httpClientFactory;

        private readonly IMemoryCache cache;

        private IHostEnvironment hostEnvironment;

        private string serverAddressOfServices = "http://localhost:14954/";

        private string cutYtBaseAddress = "https://localhost:44309/";

        HttpClient httpClient = null;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.hostEnvironment = hostEnvironment;
            this.cache = cache;

            if (!hostEnvironment.EnvironmentName.Equals("Development", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddressOfServices = "http://cutyt.westeurope.cloudapp.azure.com/";
                cutYtBaseAddress = "https://www.cutyt.com/";

            }

            httpClient = httpClientFactory.CreateClient();

            httpClient.Timeout = TimeSpan.FromHours(1);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult GetUrlDetails(string url)
        {
            ViewData["Message"] = "Your contact page.";

            return PartialView();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
