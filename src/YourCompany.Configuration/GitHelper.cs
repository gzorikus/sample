using System;
using System.Diagnostics;

namespace YourCompany.Configuration
{
    internal static class GitHelper
    {
        internal static string GetCurrentBranch(string commandPath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = commandPath
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            string branchName = process.ExitCode != 0 || string.IsNullOrEmpty(output) ? null : output;
            return branchName ?? throw new ApplicationException("branchName == null");
        }
    }
}