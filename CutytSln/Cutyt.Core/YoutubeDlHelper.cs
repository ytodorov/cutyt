using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core
{
    public static class YoutubeDlHelper
    {
        public static string DownloadCustomAudio(string v, string audioFormat)
        {
            var resultFileNameWithoutExtension = $"{v}_{audioFormat}";

            var fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }


            var programFullPath = @"E:\Files\youtube-dl.exe";
            var args = $"-f bestaudio -x --audio-format {audioFormat} {v} --output \"{resultFileNameWithoutExtension}.%(ext)s\"";
            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();

            string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string DownloadBestAudio(string v)
        {
            var resultFileNameWithoutExtension = $"{v}_bestaudio";

            var fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }


            var programFullPath = @"E:\Files\youtube-dl.exe";
            var args = $"-f bestaudio {v} --output \"{resultFileNameWithoutExtension}.%(ext)s\"";
            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();

            string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string DownloadVideo(string v, string code)
        {
            var resultFileNameWithoutExtension = $"{v}_{code}";

            var fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }


            var programFullPath = @"E:\Files\youtube-dl.exe";
            var args = $"-f {code} {v} --output \"{resultFileNameWithoutExtension}.%(ext)s\"";
            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();

            string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string MergeAudioAndVideoToMp4(string v, string videoCode)
        {
            var resultFileNameWithoutExtension = $"{v}_{videoCode.Replace("+", "_")}_AV";

            var fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            if (!string.IsNullOrEmpty(fullFilePath))
            {
                return fullFilePath;
            }

            var audioPath = DownloadBestAudio(v);

            if (string.IsNullOrEmpty(videoCode))
            {
                return audioPath;
            }

            var videoPath = DownloadVideo(v, videoCode);

            var programFullPath = @"E:\Files\ffmpeg.exe";
            var args = $"-i {videoPath} -i {audioPath} -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 {resultFileNameWithoutExtension}.mp4";
            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();

            // For some VERY STRANGE reason ffmpeg will block undefinetely on this line
            //string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            fullFilePath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(resultFileNameWithoutExtension));

            return fullFilePath;
        }

        public static string CutFile(string fileName, string start, string end)
        {
            double.TryParse(start, NumberStyles.Any, CultureInfo.InvariantCulture, out double startTime);
            double.TryParse(end, NumberStyles.Any, CultureInfo.InvariantCulture, out double endTime);

            double duration = Math.Round(endTime - startTime, 1);

            var startTimeSpan = TimeSpan.FromSeconds(startTime);
            var durationTimeSpan = TimeSpan.FromSeconds(duration);


            var startParam = $"{startTimeSpan.Hours.ToString("00")}:{startTimeSpan.Minutes.ToString("00")}:{startTimeSpan.Seconds.ToString("00")}.{startTimeSpan.Milliseconds.ToString("0")}";
            var durationParam = $"{durationTimeSpan.Hours.ToString("00")}:{durationTimeSpan.Minutes.ToString("00")}:{durationTimeSpan.Seconds.ToString("00")}.{durationTimeSpan.Milliseconds.ToString("0")}";

            var inputFile =  Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(fileName));
            var inputFileWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
            var ext = Path.GetExtension(inputFile);
            string outputFile = $"{inputFileWithoutExtension}_{start}_{end}{ext}";

            var outputFileFullPath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(outputFile));

            if (!string.IsNullOrEmpty(outputFileFullPath))
            {
                return outputFileFullPath;
            }
            var programFullPath = @"E:\Files\ffmpeg.exe";
            //var args = $"-ss 00:01:00 -i {inputFile} -to 00:00:02 -c copy {outputFile}";
            var args = $"-ss {startParam} -i {inputFile} -to {durationParam} -c copy {outputFile}";

            Process p = new Process();
            p.StartInfo.FileName = programFullPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.WorkingDirectory = @"E:\Files";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;

            p.Start();

            // For some VERY STRANGE reason ffmpeg will block undefinetely on this line
            //string result = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            outputFileFullPath = Directory.GetFiles(@"E:\Files").FirstOrDefault(f => f.Contains(outputFile));

            return outputFileFullPath;
        }
    }
}
