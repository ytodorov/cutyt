using Cutyt.Core.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR()
                    //.AddAzureSignalR("Endpoint=https://cutyt.service.signalr.net;AccessKey=CqW6IpODOQ1vwEncPHN67KhUIr08xvLLv1Y4HNoj7ek=;Version=1.0;");
                    .AddAzureSignalR("Endpoint=https://signalr-cutyt.service.signalr.net;AccessKey=n2LjXMMzX3UbtKt98VauwKOoG4H/Tkz7QP9Qls9LA4M=;Version=1.0;");

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
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapHub<ChatHub>("/chat");
});

app.MapGet("/", async (HttpContext context, TelemetryClient telemetryClient) =>
{
    return await Task.FromResult("OK");
});

app.MapGet("/run", async (HttpContext context, TelemetryClient telemetryClient, IHubContext<ChatHub> chatHub) =>
        {
            string args = context.Request.Query["args"];
            string command = context.Request.Query["command"];

            var blobFileNameEncoded = $"{args}{command}".Hash();

            Dictionary<string, string> metadata = new Dictionary<string, string>();

            metadata["blobFileNameEncoded"] = blobFileNameEncoded;

            var query = $"\"blobFileNameEncoded\" = '{blobFileNameEncoded}'";

            var cachedData = await BlobStorageHelper.GetFirstBlobContent("runresults", query);

            if (cachedData != null)
            {
                return cachedData;
            }

            var currDir = Environment.CurrentDirectory;

            var res = await ProcessAsyncHelperNoLog.ExecuteShellCommand($@"{currDir}\{command}", $"{args}", null, null);
            if (res.StandardError?.Contains("error:", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                telemetryClient.TrackException(new Exception(res.StandardError));
                return string.Empty;
            }

            var path = Path.GetTempFileName();
            File.WriteAllText(path, res.StadardOutput);

            await BlobStorageHelper.UploadBlob(path, blobFileNameEncoded, "runresults", metadata, telemetryClient);

            File.Delete(path);

            return res.StadardOutput;
        }
        );

app.MapPost("/getbloburl", (Func<HttpContext, TelemetryClient, IHubContext<ChatHub>, Task <YoutubeDownloadedFileInfo>>)(async (HttpContext context, TelemetryClient telemetryClient, IHubContext<ChatHub> chatHub) =>
{
    string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var job = JsonConvert.DeserializeObject<DownloadLinkRequestViewModel>(requestBody);

    var ticksNow = DateTime.Now.Ticks.ToString();

    var currDir = Environment.CurrentDirectory;

    string start = string.Empty;
    string end = string.Empty;

    string audioAndVideoOption = string.Empty;
    string audioFormatOption = string.Empty;

    var uniqueTicks = DateTime.Now.Ticks.ToString();

    string signalrId = job.SignalrId;

    string? fileNameToUploadInBLobWithoutExtension = $"{job.V}_{job.SelectedOption}_{job.Start}_{job.End}_{uniqueTicks}"
    .Replace(" ", "_")
    .Replace("+", "_");

    if (string.IsNullOrEmpty(job.AudioFormat))
    {
        job.SelectedOption = job.SelectedOption.Replace(" ", "+");

        audioAndVideoOption = job.SelectedOption;
    }
    else
    {
        audioFormatOption = $"-x --audio-format {job.AudioFormat}";

        job.SelectedOption = "bestaudio";

        fileNameToUploadInBLobWithoutExtension = $"{job.V}_{job.SelectedOption}_{job.AudioFormat}_{job.Start}_{job.End}_{uniqueTicks}"
        .Replace(" ", "_")
        .Replace("+", "_");
    }

    var output = $"{AppConstants.YtWorkingDir}\\{fileNameToUploadInBLobWithoutExtension}.%(ext)s";


    //output = output.Replace("+", "_");
    if (job.ShouldTrim.GetValueOrDefault())
    {
        start = TimeSpan.FromSeconds(job.Start).ToString("c"); //HH:mm:ss:ff"
        end = TimeSpan.FromSeconds(job.End).ToString("c");


        var args = $"--external-downloader ffmpeg --external-downloader-args \"-ss {start} -to {end}\" -f \"{job.SelectedOption}\" {audioFormatOption} \"https://www.youtube.com/watch?v={job.V}\" --merge-output-format mp4 -k --no-part --output \"{output}\"";

        var resFromShell = await ProcessAsyncHelperNoLog.ExecuteShellCommand(
                $@"{currDir}\youtube-dl.exe",
                args,
                chatHub,
                signalrId);

        if (!string.IsNullOrEmpty(resFromShell.StandardError))
        {
            if (resFromShell.StandardError?.Contains("error:", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                telemetryClient.TrackException(new Exception(resFromShell.StandardError));
            }
        }
    }
    else
    {
        var resFromShell2 = await ProcessAsyncHelperNoLog.ExecuteShellCommand(
                $@"{currDir}\youtube-dl.exe",
                $"-f \"{job.SelectedOption}\" {audioFormatOption} \"https://www.youtube.com/watch?v={job.V}\" --merge-output-format mp4 -k --no-part --output {output}",
                chatHub,
                signalrId);

        if (!string.IsNullOrEmpty(resFromShell2.StandardError))
        {
            if (resFromShell2.StandardError?.Contains("error:", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                telemetryClient.TrackException(new Exception(resFromShell2.StandardError));
            }
        }
    }


    DirectoryInfo di = new DirectoryInfo(AppConstants.YtWorkingDir);

    var allRelatedFiles = di.GetFiles()
        .OrderByDescending(f => f.CreationTime.Ticks).Where(f => f.FullName.Contains(fileNameToUploadInBLobWithoutExtension, StringComparison.InvariantCultureIgnoreCase))
        .ToList();

    var fi = allRelatedFiles.FirstOrDefault(); // the biggest file is the correct one. The latest one .OrderByDescending(f => f.Length)

    var fullFilePath = fi.FullName; //Directory.GetFiles(AppConstants.YtWorkingDir).FirstOrDefault(f => f.Contains(fileNameToUploadInBLobWithoutExtension, StringComparison.InvariantCultureIgnoreCase));

    var size = new FileInfo(fullFilePath).Length;

    string fileOnDiskNameWithExtension = Path.GetFileName(fullFilePath);

    var reply = new YoutubeDownloadedFileInfo()
    {
        Id = job.V,
        Name = job.Title,
        Url = $"https://cuteus.blob.core.windows.net/media/{fileOnDiskNameWithExtension}",
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

    metadata[nameof(YoutubeDownloadedFileInfo.DownloadedOn)] = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).Base64StringEncode();

    metadata[nameof(YoutubeDownloadedFileInfo.DownloadedOnTicks)] = DateTime.UtcNow.Ticks.ToString();

    await BlobStorageHelper.UploadBlob(fullFilePath, fileOnDiskNameWithExtension, "media", metadata, telemetryClient);

    foreach (var fileToDelete in allRelatedFiles)
    {
        if (File.Exists(fileToDelete.FullName))
        {
            fileToDelete.Delete();
        }
    }

    return reply;
}
    ));

await app.RunAsync();