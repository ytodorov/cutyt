using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Cutyt.Core;
using Cutyt.Core.Classes;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System.Linq;

namespace CutYtFunc
{
    public class Function3
    {
        IHostEnvironment host;
        IOptions<ExecutionContextOptions> executionContext;
        public Function3(IHostEnvironment host, IOptions<ExecutionContextOptions> executionContext)
        {
            this.host = host;
            this.executionContext = executionContext;
        }

        [FunctionName("Function3")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                string name = req.Query["name"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                name = name ?? data?.name;

                string responseMessage = string.IsNullOrEmpty(name)
                    ? "Yordan This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, {name}. Yordan This HTTP triggered function executed successfully.";



                if (string.IsNullOrEmpty(name))
                {
                    name = "g98DRdOPD0g";
                }

                var path1 = host.ContentRootPath;
                var path2 = Environment.CurrentDirectory;
                var path3 = executionContext.Value.AppDirectory;

                //var res = await ProcessAsyncHelperNoLog.ExecuteShellCommand(
                //            $@"{path3}\youtube-dl.exe",
                //            $" --write-info-json --skip-download https://www.youtube.com/watch?v={name}");

                //var file = Directory.GetFiles(path3, "*.*", SearchOption.AllDirectories).FirstOrDefault(f => f.Contains(name, StringComparison.InvariantCultureIgnoreCase));
                //if (file != null)
                //{
                //    var r = File.ReadAllText(file);
                //    return new OkObjectResult(r);
                //}

                //return new OkObjectResult(Directory.GetFiles(path3));

                var res = ProcessSyncHelper.ExecuteShellCommand(
                            $@"{path3}\youtube-dl.exe",
                            $" --get-title https://www.youtube.com/watch?v={name}");

                return new OkObjectResult(res.StadardOutput);
            }
            catch (Exception ex)
            {
                return new OkObjectResult(ex.Message + ex.InnerException?.Message + ex.InnerException?.InnerException?.Message);
            }

            
        }
    }
}
