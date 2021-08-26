using Cutyt.Core;
using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading.Tasks;
using static ProcessAsyncHelper;

namespace CutytRebusServer
{
    class Program
    {
        const string ConnectionString = "server=.\\SQLDEVELOPER2019; database=rebussamples; trusted_connection=true";

        static void Main()
        {
            string serverAddressOfServices = "http://localhost:14954/";
            //var computerNameDanchoHome = "YTODOROV-NB";

            if (!Environment.MachineName.Equals("YTODOROV-NB", StringComparison.InvariantCultureIgnoreCase))
            {
                serverAddressOfServices = "http://cutyt.westeurope.cloudapp.azure.com/";
            }

            TelemetryClient telemetryClient = new TelemetryClient(new TelemetryConfiguration() { InstrumentationKey = "3b83045b-16b7-4d93-8cfc-7983fb8e5500" });
            using var adapter = new BuiltinHandlerActivator();

            adapter.Handle<GetDownloadedFilesJob>(async (bus, job) =>
            {
                DownloadedFilesReply downloadedFilesReply = new DownloadedFilesReply();

                var filesOnAzureShare = Directory.GetFiles("Z:");

                foreach (var fullPhysicalFileName in filesOnAzureShare)
                {
                    var displayName = Path.GetFileName(fullPhysicalFileName);
                    downloadedFilesReply.Files.Add(new DownloadFileViewModel()
                    {
                        DisplayName = displayName,
                        Url = $"https://stcutyt.file.core.windows.net/cutyt/{displayName}?sv=2018-03-28&si=cutyt-ReadList&sr=s&sig=NRNKCky0S%2Bg4zi%2B6708kg2XN9U4JzqnvEL3w7znaDc4%3D",
                    });

                }

                await bus.Reply(downloadedFilesReply);
            });

            adapter.Handle<GetYoutubeDurationJob>(async (bus, job) =>
            {
                var res = await ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"-v -e \"{job.Url}\" --get-duration");

                var rows = res.StadardOutput.Split('\r');
                YoutubeDurationReply reply = new YoutubeDurationReply()
                {
                    Title = rows[0],
                    Duration = rows[1]
                };

                await bus.Reply(reply);
            });

            adapter.Handle<GetYoutubeInfosJob>(async (bus, job) =>
            {
                ProcessResult res = await ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"-F \"{job.Url}\"");

                var rows = res.StadardOutput.Split($"\n");

                List<YouTubeInfoViewModel> infos = new List<YouTubeInfoViewModel>();

                var startIndex = rows.ToList().IndexOf(rows.FirstOrDefault(f => f.StartsWith("format")));
                for (int i = startIndex + 1; i < rows.Length - 1; i++)
                {
                    YouTubeInfoViewModel info = new YouTubeInfoViewModel();

                    var currentRow = rows[i];

                    var rowparts = currentRow.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    info.FormatCode = rowparts[0];
                    info.Extension = rowparts[1];

                    info.TextWithoutCode = currentRow.Replace(rowparts[0], string.Empty).Trim();

                    int infoLength = currentRow.LastIndexOf(",") - currentRow.IndexOf(",");

                    if (currentRow.Contains("audio only", StringComparison.InvariantCultureIgnoreCase))
                    {
                        info.Resolution = $"{rowparts[2]} {rowparts[3]}";
                    }
                    else
                    {
                        info.Resolution = rowparts[2];
                    }
                    int resolutionLength = currentRow.IndexOf(",") - 24;

                    var resolutionParts = info.Resolution?.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    if (!resolutionParts[0].Contains("audio", StringComparison.InvariantCultureIgnoreCase))
                    {
                        info.ResolutionWidthByHeight = resolutionParts[0];
                    }

                    info.Note = currentRow.Substring(currentRow.IndexOf(",") + 1, infoLength).Trim(',').Trim();

                    info.Size = currentRow.Substring(currentRow.LastIndexOf(",") + 1).Trim();

                    infos.Add(info);
                }

                foreach (var info in infos)
                {
                    if (info.TextWithoutCode.Contains("video only"))
                    {
                        info.DownloadSwitchAudioAndVideo = $"{info.FormatCode}+bestaudio";
                    }
                    else
                    {
                        info.DownloadSwitchAudioAndVideo = info.FormatCode;
                    }
                }

                YoutubeInfosReply reply = new YoutubeInfosReply()
                {
                    Infos = infos
                };

                await bus.Reply(reply);
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
                    var fileNameWithoutDashV = fileNameWithoutExtensions.Replace($"-{job.V}", string.Empty);

                    reply = new YoutubeDownloadLinkReply()
                    {
                        Name = fileNameFromArgs,
                        Url = $"{serverAddressOfServices}{physicalFileName}",
                        FileName = fileNameFromArgs,
                        DisplayName = fileNameWithoutDashV,
                        V = job.V,
                        Start = job.Start.ToString(),
                        End = job.End.ToString(),
                    };

                }
                catch (Exception ex)
                {
                    telemetryClient.TrackException(ex);
                }

                await bus.Reply(reply);
            });

            Configure.With(adapter)
            .Logging(l => l.ColoredConsole(minLevel: LogLevel.Debug))
            
            .Transport(t => 
            t.UseAzureServiceBus("Endpoint=sb://testyo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wBrn2N6/rFWuv7UriFizagh0yWvyRI/cL5Q7HclN8PE=", "consumer.input")
            .SetMessagePeekLockDuration(TimeSpan.FromMinutes(5)).AutomaticallyRenewPeekLock())
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
