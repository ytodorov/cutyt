using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CutytWeb.Pages
{
    public class MyDownloadsModel : PageModel
    {
        public List<string> Files { get; set; } = new List<string>();

        public void OnGet()
        {
            var httpClient = HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();

            var folderToSearchForFiles = $"/app/wwwroot/output/yt-dlp/{Request.HttpContext.Connection.RemoteIpAddress}";
            var res = httpClient.GetStringAsync($"https://datasea-container-app.calmsand-c9baad98.eastus.azurecontainerapps.io/exec?cli=ls&arguments={folderToSearchForFiles}").Result;


            if (!string.IsNullOrEmpty(res) && !res.Contains("directory", StringComparison.InvariantCultureIgnoreCase))
            {
                Files = res.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }



        }
    }
}
