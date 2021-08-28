using Cutyt.Core.Constants;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProcessAsyncHelper;

namespace Cutyt.Core
{
    public static class YoutubeDlHelper
    {

        public static void FreeSpaceOnHardDiskIfNeeded()
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

                if (totalSizeInGigabytes > 80)
                {
                    var filesToDelete = files.Skip(files.Count / 3 * 2).ToList();

                    // delete the last 33%  of the files

                    foreach (var fileToDelete in filesToDelete)
                    {
                        File.Delete(fileToDelete.FullName);
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
            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"-f bestaudio -x --audio-format {audioFormat} {v} --output \"{dir}/{resultFileNameWithoutExtension}.%(ext)s\"").Result;
                        
            if (!string.IsNullOrEmpty(res.StandardError))
            {
                telemetryClient.TrackException(new Exception(res.StandardError));
            }

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

            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"-f bestaudio {v} --output \"{AppConstants.YtWorkingDir}\\{resultFileNameWithoutExtension}.%(ext)s\"").Result;


            if (!string.IsNullOrEmpty(res.StandardError))
            {
                telemetryClient.TrackException(new Exception(res.StandardError));
            }

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


            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\youtube-dl.exe", $"-f {code} {v} --output \"{AppConstants.YtWorkingDir}\\{resultFileNameWithoutExtension}.%(ext)s\"").Result;

            if (!string.IsNullOrEmpty(res.StandardError))
            {
                telemetryClient.TrackException(new Exception(res.StandardError));
            }
            fullFilePath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string MergeAudioAndVideoToMp4(string v, string videoCode, TelemetryClient telemetryClient)
        {
            var resultFileNameWithoutExtension = $"{v}_{videoCode.Replace("+", "_")}_AV";

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

            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\ffmpeg.exe",
                $"-i {videoPath} -i {audioPath} -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 {AppConstants.YtWorkingDir}\\{resultFileNameWithoutExtension}.mp4 -y").Result;

           
            if (!string.IsNullOrEmpty(res.StandardError))
            {
                telemetryClient.TrackException(new Exception(res.StandardError));
            }

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

            var inputFile =  Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(fileName));
            var inputFileWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
            var ext = Path.GetExtension(inputFile);
            string outputFile = $"{inputFileWithoutExtension}_{start}_{end}{ext}";

            var outputFileFullPath = Directory.GetFiles($@"{AppConstants.YtWorkingDir}").FirstOrDefault(f => f.Contains(outputFile));

            if (!string.IsNullOrEmpty(outputFileFullPath))
            {
                return outputFileFullPath;
            }

            ProcessResult res = ProcessAsyncHelper.ExecuteShellCommand($@"{Environment.CurrentDirectory}\ffmpeg.exe", $"-ss {startParam} -i {inputFile} -to {durationParam} -c copy {AppConstants.YtWorkingDir}\\{outputFile} -y").Result;

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
    }
}
