using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Tasks;
using static ProcessAsyncHelper;

namespace CutytRebusServer
{
    class Program
    {        
        const string ConnectionString = "server=.\\SQLDEVELOPER2019; database=rebussamples; trusted_connection=true";

        static void Main()
        {
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
            string serverAddressOfServices = "http://localhost:14954/";
            //var computerNameDanchoHome = "YTODOROV-NB";

            if (!Environment.MachineName.Equals("YTODOROV-NB", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddressOfServices = "http://cut.westeurope.cloudapp.azure.com/";
            }

            TelemetryClient telemetryClient = new TelemetryClient(new TelemetryConfiguration() { InstrumentationKey = "3b83045b-16b7-4d93-8cfc-7983fb8e5500" });
            using var adapter = new BuiltinHandlerActivator();

            adapter.Handle<GetYouTubeUrlFullDescriptionJob>(async (bus, job) =>
            {
                var json = string.Empty;
                try
                {
                    var cachedFileDirectory = Path.Combine(AppConstants.YtWorkingDir, "Meta");
                    if (!Directory.Exists(cachedFileDirectory))
                    {
                        Directory.CreateDirectory(cachedFileDirectory);
                    }

                    var cachedFiles = Directory.GetFiles(cachedFileDirectory);
                    var cachedFile = cachedFiles.FirstOrDefault(f => f.EndsWith($"{job.Id}.json"));

                    if (!string.IsNullOrEmpty(cachedFile))
                    {
                        json = File.ReadAllText(cachedFile);
                    }
                    else
                    {
                        var res = await ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"-j \"{job.Id}\"");

                        json = res.StadardOutput;

                        File.WriteAllText(Path.Combine(cachedFileDirectory, $"{job.Id}.json"), json);
                    }

                    var options = new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        PropertyNameCaseInsensitive = true
                    };

                    var youTubeUrlFullDescription = JsonSerializer.Deserialize<YouTubeUrlFullDescription>(json, options);

                    YouTubeUrlFullDescriptionReply youTubeUrlFullDescriptionReply = new YouTubeUrlFullDescriptionReply()
                    {
                        YouTubeUrlFullDescription = youTubeUrlFullDescription
                    };

                    await bus.Reply(youTubeUrlFullDescriptionReply);
                }
                catch (Exception ex)
                {

                    ExceptionTelemetry exceptionTelemetry = new ExceptionTelemetry(ex);
                    exceptionTelemetry.Exception = ex;
                    var jobAsJson = JsonSerializer.Serialize(job, new JsonSerializerOptions() { WriteIndented = true });
                    exceptionTelemetry.Properties.Add("job", jobAsJson);
                    exceptionTelemetry.Properties.Add("json", json);

                    telemetryClient.TrackException(exceptionTelemetry);
                }
            });

            adapter.Handle<GetDownloadedFilesJob>(async (bus, job) =>
            {
                try
                {
                    DownloadedFilesReply downloadedFilesReply = new DownloadedFilesReply();

                    downloadedFilesReply.UrlToDownloadJsonMetaInfo = $"{serverAddressOfServices}/DownloadedFilesInfo/downloadedFiles.json";
                    //var filesMetaInfos = YoutubeDlHelper.GetDownloadedFilesMetaInfo();

                    //downloadedFilesReply.Files.AddRange(filesMetaInfos);

                    await bus.Reply(downloadedFilesReply);
                }
                catch (Exception ex)
                {
                    ExceptionTelemetry exceptionTelemetry = new ExceptionTelemetry(ex);
                    exceptionTelemetry.Exception = ex;
                    var jobAsJson = JsonSerializer.Serialize(job, new JsonSerializerOptions() { WriteIndented = true });
                    exceptionTelemetry.Properties.Add("job", jobAsJson);

                    telemetryClient.TrackException(exceptionTelemetry);
                }
            });

            adapter.Handle<GetYoutubeDownloadLinkJob>(async (bus, job) =>
            {
                YoutubeDownloadLinkReply reply = new YoutubeDownloadLinkReply();

                try
                {
                    if (job.V?.Length > 20)
                    {
                        throw new Exception($"{job.V} is not valid for v in youtube URL");
                    }

                    var selectedOption = job.SelectedOption;

                    string filePathResult = string.Empty;

                    YoutubeDlHelper.FreeSpaceOnHardDiskIfNeeded();

                    if (selectedOption.Contains("--audio-format"))
                    {
                        selectedOption = selectedOption.Split(" ").Last();
                        filePathResult = YoutubeDlHelper.DownloadCustomAudio(job.V, selectedOption, telemetryClient);
                    }
                    else
                    {
                        var selectedVideoOption = selectedOption.Split(" ").FirstOrDefault();
                        filePathResult = YoutubeDlHelper.MergeAudioAndVideoToMp4(job.V, selectedVideoOption, telemetryClient);
                    }

                    if (job.ShouldTrim.GetValueOrDefault())
                    {
                        filePathResult = YoutubeDlHelper.CutFile(filePathResult, job.Start.ToString(), job.End.ToString(), telemetryClient);
                    }

                    //AddWatermark(filePathResult); cannot be played in browser. dunno why

                    var selectedOptionWithoutPlus = selectedOption.Replace("+", string.Empty);
                    //var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";

                    string physicalFileName = Path.GetFileName(filePathResult);

                    //var fileNameFromArgs = GetFileNameFromArgs(ytUrl);

                    ProcessResult res = await ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"--get-filename {job.Url} --encoding UTF8");
                    var fileNameFromArgs = res.StadardOutput;

                    var fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileNameFromArgs);
                    var fileNameWithoutDashV = fileNameWithoutExtensions.Replace($" [{job.V}]", string.Empty, StringComparison.InvariantCultureIgnoreCase);

                    var size = new FileInfo(filePathResult).Length;
                    reply = new YoutubeDownloadLinkReply()
                    {
                        Id = job.V,
                        Name = fileNameFromArgs,
                        Url = $"{serverAddressOfServices}{physicalFileName}",
                        FileName = fileNameFromArgs,
                        DisplayName = fileNameWithoutDashV,
                        V = job.V,
                        Start = job.Start.ToString(),
                        End = job.End.ToString(),
                        FileOnDiskNameWithoutExtension = Path.GetFileNameWithoutExtension(physicalFileName),
                        FileOnDiskExtension = Path.GetExtension(physicalFileName),
                        FileOnDiskNameWithExtension = Path.GetFileName(physicalFileName),   
                        DownloadedOn = DateTime.UtcNow,
                        FileOnDiskSize = size,

                    };

                    YoutubeDlHelper.SaveDownloadedFilesMetaInfo(reply);

                    await bus.Reply(reply);
                }
                catch (Exception ex)
                {
                    ExceptionTelemetry exceptionTelemetry = new ExceptionTelemetry(ex);
                    exceptionTelemetry.Exception = ex;
                    var jobAsJson = JsonSerializer.Serialize(job, new JsonSerializerOptions() { WriteIndented = true });
                    exceptionTelemetry.Properties.Add("job", jobAsJson);

                    telemetryClient.TrackException(exceptionTelemetry);
                }
            });

            Configure.With(adapter)
            .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))

            .Transport(t =>
            t.UseAzureServiceBus(AppConstants.ServiceBusConnectionString, "consumer.input").SetMessagePayloadSizeLimit(25600000)
            .SetMessagePeekLockDuration(TimeSpan.FromMinutes(5)).AutomaticallyRenewPeekLock())
            //https://github.com/rebus-org/Rebus/wiki/Workers-and-parallelism#defaults
             .Options(o =>
             {
                 o.SetNumberOfWorkers(15);
                 o.SetMaxParallelism(15);
             })
            .Start();

            Console.WriteLine("Press X button to quit");

            while (true)
            {
                Console.ReadLine();
            }

        }

        private static async void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            await Console.Out.WriteLineAsync(e.Data);
        }
    }
}
