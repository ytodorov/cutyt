using Cutyt.Core;
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
        public HomeController(IHostEnvironment hostEnvironment, TelemetryClient telemetryClient)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
