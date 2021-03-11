using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var resultFileNameWithoutExtension = $"{v}_{videoCode}_AV";

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
    }
}
