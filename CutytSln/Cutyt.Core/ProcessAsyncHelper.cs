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

public static class ProcessAsyncHelper
{
    public static async Task<ProcessResult> ExecuteShellCommand(
        string command,
        string arguments,
        TelemetryClient telemetryClient)
    {
        int timeout = int.MaxValue;

        var result = new ProcessResult()
        {
            ExitCode = -1,
        };

        using (var process = new Process())
        {
            // If you run bash-script on Linux it is possible that ExitCode can be 255.
            // To fix it you can try to add '#!/bin/bash' header to the script.

            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(command);
            process.StartInfo.FileName = command;

            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;


            var outputBuilder = new StringBuilder();
            var outputCloseEvent = new TaskCompletionSource<bool>();

            process.OutputDataReceived += (s, e) =>
            {
                // The output stream has been closed i.e. the process has terminated
                if (e.Data == null)
                {
                    outputCloseEvent.SetResult(true);
                }
                else
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            var errorBuilder = new StringBuilder();
            var errorCloseEvent = new TaskCompletionSource<bool>();

            process.ErrorDataReceived += (s, e) =>
            {
                // The error stream has been closed i.e. the process has terminated
                if (e.Data == null)
                {
                    errorCloseEvent.SetResult(true);
                }
                else
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            bool isStarted;

            try
            {
                isStarted = process.Start();

                //if (!process.HasExited)
                //{
                //    //Limit the CPU usage to 45%
                //    var jobHandle = LimitCpuUsage.CreateJobObject(null, null);
                //    AssignProcessToJobObject(jobHandle, process.Handle);
                //    var cpuLimits = new LimitCpuUsage.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION();
                //    cpuLimits.ControlFlags = (UInt32)(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_ENABLE | CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP);
                //    cpuLimits.CpuRate = 5 * 100; // Limit CPu usage to 10%
                //    var pointerToJobCpuLimits = Marshal.AllocHGlobal(Marshal.SizeOf(cpuLimits));
                //    Marshal.StructureToPtr(cpuLimits, pointerToJobCpuLimits, false);
                //    if (!SetInformationJobObject(jobHandle, JOBOBJECTINFOCLASS.JobObjectCpuRateControlInformation, pointerToJobCpuLimits, (uint)Marshal.SizeOf(cpuLimits)))
                //    {
                //        Console.WriteLine("Error !");
                //    }

                //    process.PriorityClass = ProcessPriorityClass.BelowNormal;
                //}
            }
            catch (Exception error)
            {
                // Usually it occurs when an executable file is not found or is not executable

                result.Completed = true;
                result.ExitCode = -1;
                result.StadardOutput = error.Message;

                isStarted = false;
            }

            if (isStarted)
            {
                // Reads the output stream first and then waits because deadlocks are possible
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Creates task to wait for process exit using timeout
                var waitForExit = WaitForExitAsync(process, timeout);

                // Create task to wait for process exit and closing all output streams
                var processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);

                // Waits process completion and then checks it was not completed by timeout
                if (await Task.WhenAny(Task.Delay(timeout), processTask) == processTask && waitForExit.Result)
                {
                    result.Completed = true;
                    result.ExitCode = process.ExitCode;

                    // Adds process output if it was completed with error
                    //if (process.ExitCode != 0)
                    {
                        result.StadardOutput = $"{outputBuilder}";
                        result.StandardError = $"{errorBuilder}";
                    }
                }
                else
                {
                    try
                    {
                        // Kill hung process
                        process.Kill();
                    }
                    catch
                    {
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(result.StandardError))
        {
            //if (!command.Contains("ffmpeg", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    telemetryClient.TrackException(new Exception(result.StandardError));
            //}
        }

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