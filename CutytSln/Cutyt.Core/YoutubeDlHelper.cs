using Cutyt.Core.Classes;
using Cutyt.Core.Constants;
using Cutyt.Core.Rebus.Jobs;
using Cutyt.Core.Rebus.Replies;
using Cutyt.Core.Storage;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static ProcessAsyncHelper;

namespace Cutyt.Core
{
    public static class YoutubeDlHelper
    {

        public static void FreeSpaceOnHardDiskIfNeeded(TelemetryClient telemetryClient)
        {
            //try
            //{
            //    string dir = "D:\\local\\DynamicCache\\wwwroot\\wwwroot\\downloads";

            //    var filesToDel = new List<string>();
            //    if (Directory.Exists(dir))
            //    {
            //        filesToDel = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).ToList();
            //        foreach (var file in filesToDel)
            //        {
            //            if (!file.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                System.IO.File.Delete(file);
            //            }
            //        }
            //    }

            //    dir = "D:\\local\\DynamicCache\\wwwroot\\wwwroot\\downloads\\Meta";

            //    if (Directory.Exists(dir))
            //    {
            //        filesToDel = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).ToList();
            //        foreach (var file in filesToDel)
            //        {
            //            if (!file.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                System.IO.File.Delete(file);
            //            }
            //        }
            //    }

            //    dir = "D:\\local\\VirtualDirectory0\\site\\wwwroot\\wwwroot\\downloads";

            //    if (Directory.Exists(dir))
            //    {
            //        filesToDel = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).ToList();
            //        foreach (var file in filesToDel)
            //        {
            //            if (!file.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                System.IO.File.Delete(file);
            //            }
            //        }
            //    }

            //    dir = "D:\\local\\VirtualDirectory0\\site\\wwwroot\\wwwroot\\downloads\\Meta";

            //    if (Directory.Exists(dir))
            //    {
            //        filesToDel = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).ToList();
            //        foreach (var file in filesToDel)
            //        {
            //            if (!file.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                System.IO.File.Delete(file);
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    telemetryClient.TrackException(ex);
            //}

            CleanDirectoryFromOldFiles(AppConstants.YtWorkingDir, telemetryClient);
            CleanDirectoryFromOldFiles("D:\\local\\VirtualDirectory0\\site\\wwwroot\\wwwroot\\downloads", telemetryClient);
            CleanDirectoryFromOldFiles("D:\\local\\DynamicCache\\wwwroot\\wwwroot\\downloads", telemetryClient);

            //DirectoryInfo directoryInfo = new DirectoryInfo(AppConstants.YtWorkingDir);
            //if (directoryInfo.Exists)
            //{
            //    var files = directoryInfo.GetFiles().ToList();

            //    files = files
            //        .OrderByDescending(s => s.CreationTimeUtc)
            //        .Where(f => !f.Name.EndsWith(".exe") && !f.Name.EndsWith(".json"))
            //        .ToList();

            //    var totalSizeInBytes = files.Sum(f => f.Length);

            //    var totalSizeInGigabytes = (double)totalSizeInBytes / 1024 / 1024 / 1024;

            //    if (totalSizeInGigabytes > 80)
            //    {
            //        var filesToDelete = files.Skip(files.Count / 3 * 2).ToList();

            //        // delete the last 33%  of the files

            //        foreach (var fileToDelete in filesToDelete)
            //        {
            //            File.Delete(fileToDelete.FullName);
            //        }
            //    }

            //}
        }

        private static void CleanDirectoryFromOldFiles(string dirPath, TelemetryClient telemetryClient)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AppConstants.YtWorkingDir);
            if (directoryInfo.Exists)
            {
                var files = directoryInfo.GetFiles().ToList();

                files = files
                    .OrderByDescending(s => s.CreationTimeUtc)
                    .Where(f => !f.Name.EndsWith(".exe") && !f.Name.EndsWith(".json"))
                    .ToList();

                var totalSizeInBytes = files.Sum(f => f.Length);

                var totalSizeInGigabytes = (double)totalSizeInBytes / 1024 / 1024 / 1024;

                if (totalSizeInGigabytes > 10)
                {
                    var filesToDelete = files.Skip(files.Count / 3 * 2).ToList();

                    // delete the last 33%  of the files

                    foreach (var fileToDelete in filesToDelete)
                    {
                        try
                        {
                            File.Delete(fileToDelete.FullName);
                        }
                        catch (Exception ex)
                        {
                            telemetryClient.TrackException(ex);
                        }
                    }
                }

            }
        }

        public static string DownloadCustomAudio(string v, string audioFormat, TelemetryClient telemetryClient)
        {
            var resultFileNameWithoutExtension = $"{v}_{audioFormat}";

            var fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }

            var dir = AppConstants.YtWorkingDir.Replace("\\", "/");
            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand(
                $@"{AppConstants.YtWorkingDir}\youtube-dl.exe",
                $"-f bestaudio -x --audio-format {audioFormat} https://www.youtube.com/watch?v={v} --output \"{dir}/{resultFileNameWithoutExtension}.%(ext)s\"",
                telemetryClient: telemetryClient).Result;

            fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string DownloadBestAudio(string v, TelemetryClient telemetryClient)
        {
            var resultFileNameWithoutExtension = $"{v}_bestaudio";

            var fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }

            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand(
                $@"{AppConstants.YtWorkingDir}\youtube-dl.exe",
                $"-f bestaudio https://www.youtube.com/watch?v={v} --output \"{AppConstants.YtWorkingDir}\\{resultFileNameWithoutExtension}.%(ext)s\"",
                telemetryClient).Result;

            fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string DownloadVideo(string v, string code, TelemetryClient telemetryClient)
        {
            var resultFileNameWithoutExtension = $"{v}_{code}";

            var fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }


            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand(
                $@"{AppConstants.YtWorkingDir}\youtube-dl.exe",
                $"-f {code} https://www.youtube.com/watch?v={v} --output \"{AppConstants.YtWorkingDir}\\{resultFileNameWithoutExtension}.%(ext)s\"",
                telemetryClient
                ).Result;

            fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string MergeAudioAndVideoToMp4(string v, string videoCode, TelemetryClient telemetryClient)
        {
            var resultFileNameWithoutExtension = $"{v}_{videoCode.Replace("+", "_")}_AV";

            if (!Directory.Exists(AppConstants.YtWorkingDir))
            {
                Directory.CreateDirectory(AppConstants.YtWorkingDir);
            }

            var fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }

            var audioPath = DownloadBestAudio(v, telemetryClient);

            if (string.IsNullOrEmpty(videoCode))
            {
                return audioPath;
            }

            var videoPath = DownloadVideo(v, videoCode, telemetryClient);

            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand(
                $@"{AppConstants.YtWorkingDir}\ffmpeg.exe",
                $"-i {videoPath} -i {audioPath} -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 {AppConstants.YtWorkingDir}\\{resultFileNameWithoutExtension}.mp4 -y -threads 1",
                telemetryClient).Result;

            //ffmpeg - does not log meaningfull values.
            //if (!string.IsNullOrEmpty(res.StandardError))
            //{
            //    telemetryClient.TrackException(new Exception(res.StandardError));
            //}

            fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string CutFile(string fileName, string start, string end, TelemetryClient telemetryClient)
        {
            double.TryParse(start, NumberStyles.Any, CultureInfo.InvariantCulture, out double startTime);
            double.TryParse(end, NumberStyles.Any, CultureInfo.InvariantCulture, out double endTime);

            double duration = Math.Round(endTime - startTime, 1);

            var startTimeSpan = TimeSpan.FromSeconds(startTime);
            var durationTimeSpan = TimeSpan.FromSeconds(duration);


            var startParam = $"{startTimeSpan.Hours.ToString("00")}:{startTimeSpan.Minutes.ToString("00")}:{startTimeSpan.Seconds.ToString("00")}.{startTimeSpan.Milliseconds.ToString("0")}";
            var durationParam = $"{durationTimeSpan.Hours.ToString("00")}:{durationTimeSpan.Minutes.ToString("00")}:{durationTimeSpan.Seconds.ToString("00")}.{durationTimeSpan.Milliseconds.ToString("0")}";

            var inputFile = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(fileName));
            var inputFileWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
            var ext = Path.GetExtension(inputFile);
            string outputFile = $"{inputFileWithoutExtension}_{start}_{end}{ext}";

            var outputFileFullPath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(outputFile));

            if (!string.IsNullOrEmpty(outputFileFullPath))
            {
                return outputFileFullPath;
            }

            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand(
                $@"{AppConstants.YtWorkingDir}\ffmpeg.exe",
                $"-ss {startParam} -i {inputFile} -to {durationParam} -c copy {AppConstants.YtWorkingDir}\\{outputFile} -y -threads 1",
                telemetryClient).Result;

            outputFileFullPath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(outputFile));

            return outputFileFullPath;
        }

        public static int GetTotalSecondsFromString(string duration = "10:00:05")
        {
            var parts = duration.Split(":", StringSplitOptions.RemoveEmptyEntries);

            int result = 0;

            if (parts.Length == 1)
            {
                result = int.Parse(parts[0]);
            }
            else if (parts.Length == 2)
            {
                result = int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
            }
            else if (parts.Length == 3)
            {
                result = int.Parse(parts[0]) * 3600 + int.Parse(parts[1]) * 60 + int.Parse(parts[2]);
            }
            return result;
        }

        public static void SaveDownloadedFilesMetaInfo(YoutubeDownloadedFileInfo reply)
        {
            var cachedFileDirectory = Path.Combine(AppConstants.YtWorkingDir, "DownloadedFilesInfo");
            if (!Directory.Exists(cachedFileDirectory))
            {
                Directory.CreateDirectory(cachedFileDirectory);
            }

            string filePath = Path.Combine(cachedFileDirectory, "downloadedFiles.json");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }

            var json = File.ReadAllText(filePath);
            var existingReplies = JsonSerializer.Deserialize<List<YoutubeDownloadedFileInfo>>(json);

            existingReplies.Add(reply);

            var existingFiles = Directory.GetFiles(AppConstants.YtWorkingDir);

            var copyOfExistingReplies = new List<YoutubeDownloadedFileInfo>();
            copyOfExistingReplies.AddRange(existingReplies);

            foreach (var rep in copyOfExistingReplies)
            {
                if (!existingFiles.Any(f => f.Contains(rep.V, StringComparison.InvariantCultureIgnoreCase)))
                {
                    existingReplies.Remove(rep);
                }
            }

            var newJson = JsonSerializer.Serialize(existingReplies, new JsonSerializerOptions() { WriteIndented = true });

            File.WriteAllText(filePath, newJson);
        }

        //public static List<YoutubeDownloadLinkReply> GetDownloadedFilesMetaInfo()
        //{
        //    var cachedFileDirectory = Path.Combine(AppConstants.YtWorkingDir, "DownloadedFilesInfo");
        //    if (!Directory.Exists(cachedFileDirectory))
        //    {
        //        Directory.CreateDirectory(cachedFileDirectory);
        //    }

        //    string filePath = Path.Combine(cachedFileDirectory, "downloadedFiles.json");

        //    if (!File.Exists(filePath))
        //    {
        //        File.WriteAllText(filePath, "{}");
        //    }

        //    var json = File.ReadAllText(filePath);
        //    var existingReplies = JsonSerializer.Deserialize<List<YoutubeDownloadLinkReply>>(json);

        //    return existingReplies;
        //}

        public static async Task<YouTubeUrlFullDescription> GetYouTubeUrlFullDescription(string Id, TelemetryClient telemetryClient)
        {
            var json = string.Empty;
            var cachedFileDirectory = Path.Combine(AppConstants.YtWorkingDir, "Meta");
            if (!Directory.Exists(cachedFileDirectory))
            {
                Directory.CreateDirectory(cachedFileDirectory);
            }

            var cachedFiles = Directory.GetFiles(cachedFileDirectory);
            var cachedFile = cachedFiles.FirstOrDefault(f => f.EndsWith($"{Id}.json"));

            if (!string.IsNullOrEmpty(cachedFile))
            {
                json = File.ReadAllText(cachedFile);
                if (string.IsNullOrWhiteSpace(json))
                {
                    var res = await ProcessAsyncHelper.ExecuteShellCommand(
                        $@"{AppConstants.YtWorkingDir}\youtube-dl.exe",
                        $"-j https://www.youtube.com/watch?v={Id}",
                        telemetryClient);

                    json = res.StadardOutput;

                    File.WriteAllText(Path.Combine(cachedFileDirectory, $"{Id}.json"), json);
                }
            }
            else
            {
                var res = await ProcessAsyncHelper.ExecuteShellCommand(
                    $@"{AppConstants.YtWorkingDir}\youtube-dl.exe",
                    $"-j https://www.youtube.com/watch?v={Id}",
                    telemetryClient);

                json = res.StadardOutput;

                File.WriteAllText(Path.Combine(cachedFileDirectory, $"{Id}.json"), json);
            }

            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            var youTubeUrlFullDescription = JsonSerializer.Deserialize<YouTubeUrlFullDescription>(json, options);

            return youTubeUrlFullDescription;
        }

        public static async Task<YoutubeDownloadedFileInfo> GetYoutubeDownloadLinkReply(GetYoutubeDownloadLinkJob job, TelemetryClient telemetryClient, string hostBaseUrl)
        {
            YoutubeDownloadedFileInfo reply = new YoutubeDownloadedFileInfo();

            if (job.V?.Length > 20)
            {
                throw new Exception($"{job.V} is not valid for v in youtube URL");
            }

            var selectedOption = job.SelectedOption;

            string fullFilePath = string.Empty;

            YoutubeDlHelper.FreeSpaceOnHardDiskIfNeeded(telemetryClient);

            if (selectedOption.Contains("--audio-format"))
            {
                selectedOption = selectedOption.Split(" ").Last();
                fullFilePath = YoutubeDlHelper.DownloadCustomAudio(job.V, selectedOption, telemetryClient);
            }
            else
            {
                var selectedVideoOption = selectedOption.Split(" ").FirstOrDefault();
                fullFilePath = YoutubeDlHelper.MergeAudioAndVideoToMp4(job.V, selectedVideoOption, telemetryClient);
            }

            if (job.ShouldTrim.GetValueOrDefault())
            {
                fullFilePath = YoutubeDlHelper.CutFile(fullFilePath, job.Start.ToString(), job.End.ToString(), telemetryClient);
            }

            //AddWatermark(filePathResult); cannot be played in browser. dunno why

            var selectedOptionWithoutPlus = selectedOption.Replace("+", string.Empty);
            //var programFullPath = $@"{AppConstants.YtWorkingDir}\youtube-dl.exe";

            string physicalFileName = Path.GetFileName(fullFilePath);

            //var fileNameFromArgs = GetFileNameFromArgs(ytUrl);

            //var metaFileToGetTitle = Directory.GetFiles(AppConstants.YtWorkingDir).FirstOrDefault(f => f.Contains(job.V, StringComparison.CurrentCultureIgnoreCase));
            var fileNameWithoutDashV = string.Empty;

            var fullDescription = await GetYouTubeUrlFullDescription(job.V, telemetryClient);

            fileNameWithoutDashV = fullDescription.Title;
            //if (metaFileToGetTitle != null)
            //{
            //    var fileContent = File.ReadAllText(metaFileToGetTitle);
            //    asd
            //}

            ProcessResult res = await ProcessAsyncHelper.ExecuteShellCommand(
                $@"{AppConstants.YtWorkingDir}\youtube-dl.exe",
                $"--get-filename {job.Url} --encoding UTF8",
                telemetryClient);

            var fileNameFromArgs = res.StadardOutput;

            var fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileNameFromArgs);
            fileNameWithoutDashV = fileNameWithoutExtensions.Replace($" [{job.V}]", string.Empty, StringComparison.InvariantCultureIgnoreCase);

            var size = new FileInfo(fullFilePath).Length;

           
            reply = new YoutubeDownloadedFileInfo()
            {
                Id = job.V,
                Name = fileNameFromArgs,
                Url = $"{hostBaseUrl}{physicalFileName}",
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
                Ip = job.Ip
            };

            Dictionary<string, string> metadata = new Dictionary<string, string>();

            metadata[nameof(YoutubeDownloadedFileInfo.Start)] = reply.Start;
            metadata[nameof(YoutubeDownloadedFileInfo.End)] = reply.End;
            metadata[nameof(YoutubeDownloadedFileInfo.FileOnDiskSize)] = reply.FileOnDiskSize.ToString();

            metadata[nameof(YoutubeDownloadedFileInfo.DisplayName)] = reply.DisplayName;

            metadata[nameof(YoutubeDownloadedFileInfo.Url)] = reply.Url;

            metadata[nameof(YoutubeDownloadedFileInfo.Ip)] = reply.Ip;

            metadata[nameof(YoutubeDownloadedFileInfo.Id)] = reply.Id;


            metadata[nameof(YoutubeDownloadedFileInfo.FileOnDiskExtension)] = reply.FileOnDiskExtension;

            await BlobStorageHelper.UploadBlob(fullFilePath, physicalFileName, metadata, telemetryClient);

            File.Delete(fullFilePath);

            //for (int i = 0; i < 10; i++)
            //{
            //    try
            //    {
            //        YoutubeDlHelper.SaveDownloadedFilesMetaInfo(reply);
            //        break;
            //    }
            //    catch(IOException ex)
            //    {
            //        if (i == 9)
            //        {
            //            telemetryClient.TrackException(ex);
            //        }
            //        Thread.Sleep(1000);
            //    }
            //}


            return reply;

        }
    }
}
