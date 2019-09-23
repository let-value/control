using System;
using System.Diagnostics;

namespace PowerControl
{
    public static class Command
    {
        public static string Exec(string command)
        {
            var process = new Process {StartInfo = GetStartInfo(command)};

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }

        private static ProcessStartInfo GetStartInfo(string command)
        {
            if (OperatingSystem.IsWindows())
                return new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

            if (OperatingSystem.IsLinux())
                return new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
            
            throw new PlatformNotSupportedException();
        }
    }
}
