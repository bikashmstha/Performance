// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Benchmarks.Utility.Helpers
{
    public class DotnetHelper
    {
        private static readonly DotnetHelper _default = new DotnetHelper();
        private static readonly string _dotnetAppName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            "dotnet.exe" :
            "dotnet";

        public static DotnetHelper GetDefaultInstance() => _default;

        private DotnetHelper()
        {
        }

        public ProcessStartInfo BuildStartInfo(string appbasePath, string arguments)
        {
            var dotnetPath = GetDotnetExecutable();
            var psi = new ProcessStartInfo(dotnetPath, arguments)
            {
                WorkingDirectory = appbasePath,
            };

            return psi;
        }

        public bool Restore(string workingDir, bool quiet = false)
        {
            var dotnet = GetDotnetExecutable();
            var psi = new ProcessStartInfo(dotnet)
            {
                Arguments = "restore" + (quiet ? " --verbosity minimal" : string.Empty),
                WorkingDirectory = workingDir,
            };

            var proc = Process.Start(psi);

            var exited = proc.WaitForExit(300 * 1000);

            return exited && proc.ExitCode == 0;
        }

        public bool Publish(string workingDir, string outputDir, string framework)
        {
            if (string.IsNullOrEmpty(framework))
            {
                // Provide required argument where multiple targets are supported. Use same framework as the running
                // test because all test sites support .NET Core App and .NET Framework. Test itself won't run under
                // .NET Framework unless on Windows.
                framework = Runtimes.GetFrameworkName("CoreCLR");
            }

            var psi = new ProcessStartInfo(GetDotnetExecutable())
            {
                Arguments = $"publish --output \"{outputDir}\" --framework {framework}",
                WorkingDirectory = workingDir,
            };

            var proc = Process.Start(psi);
            var exited = proc.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);

            return exited && proc.ExitCode == 0;
        }

        public bool Publish(string workingDir, string outputDir)
        {
            return Publish(workingDir, outputDir, framework: null);
        }

        public string SearchForDotNetInWellKnownPlaces()
        {
            var path = Environment.GetEnvironmentVariable("DOTNET_HOME");
            if (path != null && File.Exists(Path.Combine(path, _dotnetAppName)))
            {
                return path;
            }

            var envHome = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
            if (envHome != null)
            {
                path = Path.Combine(envHome, ".dotnet");
                if (File.Exists(Path.Combine(path, _dotnetAppName)))
                {
                    return path;
                }
            }

            var envLocalAppData = Environment.GetEnvironmentVariable("LocalAppData");
            if (envLocalAppData != null)
            {
                path = Path.Combine(envLocalAppData, "Microsoft", "dotnet");
                if (File.Exists(Path.Combine(path, _dotnetAppName)))
                {
                    return path;
                }
            }

            return null;
        }

        public string GetDotnetExecutable()
        {
            var mainModule = Process.GetCurrentProcess().MainModule.FileName;
            if (mainModule != null
                && Path.GetFileNameWithoutExtension(mainModule).ToLowerInvariant() == "dotnet")
            {
                return mainModule;
            }

            var dotnetPath = SearchForDotNetInWellKnownPlaces();
            if (dotnetPath != null)
            {
                return Path.Combine(dotnetPath, _dotnetAppName);
            }

            return _dotnetAppName;
        }
    }
}
