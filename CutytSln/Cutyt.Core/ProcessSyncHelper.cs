using Cutyt.Core.Kernels;
using Microsoft.ApplicationInsights;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Cutyt.Core.Kernels.LimitCpuUsage;

public static class ProcessSyncHelper
{
    public static ProcessResult ExecuteShellCommand(
        string command,
        string arguments
        )
    {
        var result = new ProcessResult()
        {
            ExitCode = -1,
        };

        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = command;
        p.StartInfo.Arguments = arguments;
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        result.StadardOutput = output;

        return result;
    }
    private static Task<bool> WaitForExitAsync(Process process, int timeout)
    {
        return Task.Run(() => process.WaitForExit(timeout));
    }


    public struct ProcessResult
    {
        public bool Completed;

        public int? ExitCode;

        public string StadardOutput;
        public string StandardError { get; set; }
    }
}