using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CutytWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async void OnGet()
        {
            //var httpClient = new HttpClient();

            //var res = await httpClient.GetStringAsync("https://api0.datasea.org/exec?cli=yt-dlp&arguments=--print%20%22%()j%22%20https://fb.watch/i0YUJz_i2-/");

            //YtDlpJsonViewModel model = JsonConvert.DeserializeObject<YtDlpJsonViewModel>(res);
        }
    }
}