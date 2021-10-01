using Cutyt.Core;
using Cutyt.Core.Constants;
using Cutyt.Core.Extensions;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Cutyt.Core.Storage;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var pathToDownloadedFiles = "D:\\home\\site";
if (Directory.Exists(pathToDownloadedFiles))
{
    AppConstants.YtWorkingDir = pathToDownloadedFiles;
}
else
{
    AppConstants.YtWorkingDir = Environment.CurrentDirectory;
}


app.MapGet("/run", async (HttpContext context, TelemetryClient telemetryClient) =>
        {
            try
            {
                string args = context.Request.Query["args"];
                string command = context.Request.Query["command"];

                var currDir = Environment.CurrentDirectory;

                var res = await ProcessAsyncHelperNoLog.ExecuteShellCommand(
                            $@"{currDir}\{command}",
                            $"{args}");

                return res.StadardOutput + res.StandardError;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        );

app.MapGet("/", async (HttpContext context, TelemetryClient telemetryClient) =>
{
    return await Task.FromResult("OK");
});

app.MapPost("/getbloburl", (Func<HttpContext, TelemetryClient, Task<YoutubeDownloadedFileInfo>>)(async (HttpContext context, TelemetryClient telemetryClient) =>
{
    try
    {
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        var job = JsonConvert.DeserializeObject<DownloadLinkRequestViewModel>(requestBody);

        var ticksNow = DateTime.Now.Ticks.ToString();

        var currDir = Environment.CurrentDirectory;

        string start = string.Empty;
        string end = string.Empty;

        string audioAndVideoOption = string.Empty;
        string audioFormatOption = string.Empty;

        var fileNameToUploadInBLobWithoutExtension = $"{job.V}_{job.SelectedOption}_{job.Start}_{job.End}".Replace(" ", "_");

        if (string.IsNullOrEmpty(job.AudioFormat))
        {
            job.SelectedOption = job.SelectedOption.Replace(" ", "+");

            audioAndVideoOption = job.SelectedOption;
        }
        else
        {
            audioFormatOption = $"-x --audio-format {job.AudioFormat}";

            job.SelectedOption = "bestaudio";

            fileNameToUploadInBLobWithoutExtension = $"{job.V}_{job.SelectedOption}_{job.AudioFormat}_{job.Start}_{job.End}".Replace(" ", "_");
        }

        var output = $"{AppConstants.YtWorkingDir}\\{fileNameToUploadInBLobWithoutExtension}.%(ext)s";

        if (job.ShouldTrim.GetValueOrDefault())
        {
            start = TimeSpan.FromSeconds(job.Start).ToString("c"); //HH:mm:ss:ff"
            end = TimeSpan.FromSeconds(job.End).ToString("c");


            var args = $"--external-downloader ffmpeg --external-downloader-args \"-ss {start} -to {end}\" -f \"{job.SelectedOption}\" {audioFormatOption} \"https://www.youtube.com/watch?v={job.V}\" --output \"{output}\"";

            var resFromShell = await ProcessAsyncHelperNoLog.ExecuteShellCommand(
                    $@"{currDir}\youtube-dl.exe",
                    args);
        }
        else
        {
            var resFromShell2 = await ProcessAsyncHelperNoLog.ExecuteShellCommand(
                    $@"{currDir}\youtube-dl.exe",
                    $"-f \"{job.SelectedOption}\" {audioFormatOption} \"https://www.youtube.com/watch?v={job.V}\" --output {output}");
        }
        DirectoryInfo di = new DirectoryInfo(AppConstants.YtWorkingDir);

        var fi = di.GetFiles().OrderByDescending(f => f.CreationTimeUtc).FirstOrDefault(f => f.FullName.Contains(fileNameToUploadInBLobWithoutExtension, StringComparison.InvariantCultureIgnoreCase));

        var fullFilePath = fi.FullName; //Directory.GetFiles(AppConstants.YtWorkingDir).FirstOrDefault(f => f.Contains(fileNameToUploadInBLobWithoutExtension, StringComparison.InvariantCultureIgnoreCase));

        var size = new FileInfo(fullFilePath).Length;

        string fileOnDiskNameWithExtension = Path.GetFileName(fullFilePath);

        var reply = new YoutubeDownloadedFileInfo()
        {
            Id = job.V,
            Name = job.Title,
            Url = $"https://stcutyt.blob.core.windows.net/media/{fileOnDiskNameWithExtension}",
            FileName = job.Title,
            DisplayName = job.Title,
            V = job.V,
            Start = job.Start.ToString(),
            End = job.End.ToString(),
            FileOnDiskNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath),
            FileOnDiskExtension = Path.GetExtension(fullFilePath),
            FileOnDiskNameWithExtension = fileOnDiskNameWithExtension,
            DownloadedOn = DateTime.UtcNow,
            FileOnDiskSize = size,
            Ip = job.Ip
        };

        Dictionary<string, string> metadata = new Dictionary<string, string>();

        metadata[nameof(YoutubeDownloadedFileInfo.Start)] = reply.Start.Base64StringEncode();
        metadata[nameof(YoutubeDownloadedFileInfo.End)] = reply.End.Base64StringEncode();
        metadata[nameof(YoutubeDownloadedFileInfo.FileOnDiskSize)] = reply.FileOnDiskSize.ToString().Base64StringEncode();

        metadata[nameof(YoutubeDownloadedFileInfo.DisplayName)] = reply.DisplayName.Base64StringEncode();

        metadata[nameof(YoutubeDownloadedFileInfo.Url)] = reply.Url.Base64StringEncode();

        metadata[nameof(YoutubeDownloadedFileInfo.Ip)] = reply.Ip.Base64StringEncode();

        metadata[nameof(YoutubeDownloadedFileInfo.Id)] = reply.Id.Base64StringEncode();

        metadata[nameof(YoutubeDownloadedFileInfo.FileOnDiskExtension)] = reply.FileOnDiskExtension.Base64StringEncode();

        await BlobStorageHelper.UploadBlob(fullFilePath, fileOnDiskNameWithExtension, metadata, telemetryClient);

        File.Delete(fullFilePath);

        return reply;
    }
    catch (Exception ex)
    {
        return null;
    }
}
    ));

await app.RunAsync();