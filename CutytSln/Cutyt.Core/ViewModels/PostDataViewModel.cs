
namespace CutytKendoWeb.Models;
public class PostDataDownloadLinkViewModel
{
    public string V { get; set; }

    public string VimeoId { get; set; }

    public string SelectedOption { get; set; }

    public string YtUrl { get; set; }

    public double Start { get; set; }

    public double End { get; set; }

    public bool? ShouldTrim { get; set; }

    public string Title { get; set; }

    public string SignalrId { get; set; }
}

// [FromBody] string v, [FromBody] string vimeoId, [FromBody] string selectedOption, [FromBody] string ytUrl,
// [FromBody] double start, [FromBody] double end, [FromBody] bool? shouldTrim, [FromBody] string title