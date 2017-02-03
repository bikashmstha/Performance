// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;

namespace Benchmarks.Utility.Helpers
{
    public class DotnetHelper
    {
        private static readonly DotnetHelper _default = new DotnetHelper();
        private readonly string _dotnetAppName = "dotnet";

        public static DotnetHelper GetDefaultInstance() => _default;

        private DotnetHelper() { }

        public ProcessStartInfo BuildStartInfo(
            string appbasePath,
            string argument)
        {
            var dotnetPath = GetDotnetExecutable();
            var psi = new ProcessStartInfo(dotnetPath, argument)
            {
                WorkingDirectory = appbasePath,
                UseShellExecute = _dotnetAppName.Equals(dotnetPath)
            };

            return psi;
        }

        public bool Restore(string workingDir, bool quiet = false, bool useShellExecute = false)
        {
            var dotnet = GetDotnetExecutable();
            var psi = new ProcessStartInfo(dotnet)
            {
                Arguments = "restore" + (quiet ? " --verbosity minimal" : string.Empty),
                WorkingDirectory = workingDir,
                UseShellExecute = useShellExecute
            };

            var proc = Process.Start(psi);

            var exited = proc.WaitForExit(300 * 1000);

            return exited && proc.ExitCode == 0;
        }

        public bool Publish(string workingDir, string outputDir, string framework, bool useShellExecute = false)
        {
            var psi = new ProcessStartInfo(GetDotnetExecutable())
            {
                Arguments = $"publish --output \"{outputDir}\"",
                WorkingDirectory = workingDir,
                UseShellExecute = useShellExecute
            };

            if (!string.IsNullOrEmpty(framework))
            {
                psi.Arguments = $"{psi.Arguments} --framework {framework}";
            }

            var proc = Process.Start(psi);
            var exited = proc.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);

            return exited && proc.ExitCode == 0;
        }

        public bool Publish(string workingDir, string outputDir)
        {
            return Publish(workingDir, outputDir, framework: null);
        }

        public string GetDotnetPath()
        {
            string path = null;
            var envDotnetHomeVariable = Environment.GetEnvironmentVariable("DOTNET_INSTALL_DIR");
            if (envDotnetHomeVariable != null && Directory.Exists(path = Environment.ExpandEnvironmentVariables(envDotnetHomeVariable)))
            {
                return path;
            }
            var envHome = Environment.GetEnvironmentVariable("HOME");
            if (envHome != null && Directory.Exists(path = Path.Combine(envHome, ".dotnet")))
            {
                return path;
            }
            var envLocalAppData = Environment.GetEnvironmentVariable("LocalAppData");
            if (envLocalAppData != null && Directory.Exists(path = Path.Combine(envLocalAppData, "Microsoft", "dotnet")))
            {
                return path;
            }
            return null;
        }

        public string GetDotnetExecutable()
        {
            var dotnetPath = GetDotnetPath();
            if (dotnetPath != null)
            {
                return Path.Combine(dotnetPath, _dotnetAppName);
            }
            return _dotnetAppName;
        }
    }
}
