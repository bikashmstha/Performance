// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Benchmarks.Framework;
using Benchmarks.Utility.Helpers;
using Benchmarks.Utility.Logging;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Tests.Performance
{
    public class WebPerformance : IBenchmarkTest, IClassFixture<SampleManager>
    {
        private readonly SampleManager _sampleManager;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);
        private readonly int _retry = 10;

        public WebPerformance(SampleManager sampleManager)
        {
            _sampleManager = sampleManager;
        }

        public IMetricCollector Collector { get; set; } = new NullMetricCollector();

        [Benchmark(Iterations = 10, WarmupIterations = 0)]
        [BenchmarkVariation("BasicKestrel_DevelopmentScenario", "BasicKestrel")]
        [BenchmarkVariation("StarterMvc_DevelopmentScenario", "StarterMvc")]
        public void Development_Startup(string sampleName)
        {
            var framework = Microsoft.Extensions.Internal.RuntimeEnvironment.RuntimeType;
            var testName = $"{sampleName}.{framework}.{nameof(Development_Startup)}";
            var logger = LogUtility.LoggerFactory.CreateLogger(testName);

            var testProject = _sampleManager.GetRestoredSample(sampleName);
            Assert.True(testProject != null, $"Fail to set up test project.");
            logger.LogInformation($"Test project is set up at {testProject}");

            var testAppStartInfo = DotnetHelper.GetDefaultInstance().BuildStartInfo(testProject, "run");

            RunStartup(5000, logger, testAppStartInfo);
        }

        [Benchmark(Iterations = 10, WarmupIterations = 0)]
        [BenchmarkVariation("BasicKestrel_DotNet_ProductionScenario", "BasicKestrel")]
        public void Production_DotNet_Startup(string sampleName)
        {
            var framework = Microsoft.Extensions.Internal.RuntimeEnvironment.RuntimeType;
            var appliationFramework = GetFrameworkName(framework);
            var testName = $"{sampleName}.{framework}.{nameof(Production_DotNet_Startup)}";
            var logger = LogUtility.LoggerFactory.CreateLogger(testName);

            var testProject = _sampleManager.GetDotNetPublishedSample(sampleName, appliationFramework);
            Assert.True(testProject != null, $"Fail to set up test project.");
            logger.LogInformation($"Test project is set up at {testProject}");

            var startInfo = GetStartInfo(testProject, sampleName);
            RunStartup(5000, logger, startInfo);
        }

        [Benchmark(Iterations = 10, WarmupIterations = 0)]
        [BenchmarkVariation("BasicKestrel_DotNet_ProductionScenario", "BasicKestrel")]
        public void GracefulExit(string sampleName)
        {
            var framework = Microsoft.Extensions.Internal.RuntimeEnvironment.RuntimeType;
            var appliationFramework = GetFrameworkName(framework);
            var testName = $"{sampleName}.{framework}.{nameof(GracefulExit)}";
            var logger = LogUtility.LoggerFactory.CreateLogger(testName);

            var testProject = _sampleManager.GetDotNetPublishedSample(sampleName, appliationFramework);
            Assert.True(testProject != null, $"Fail to set up test project.");
            logger.LogInformation($"Test project is set up at {testProject}");

            var startInfo = GetStartInfo(testProject, sampleName);
            var process = Process.Start(startInfo);
            HttpResponseMessage response = null;

            try
            {
                Task<HttpResponseMessage> webtask = null;
                var url = "http://localhost:5000/";
                var responseRetrived = false;
                using (var client = new HttpClient())
                {
                    for (int i = 0; i < _retry; ++i)
                    {
                        try
                        {
                            webtask = client.GetAsync(url);
                            if (webtask.Wait(_timeout))
                            {
                                responseRetrived = true;
                                break;
                            }
                            else
                            {
                                logger.LogError("Http client timeout.");
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                if (responseRetrived)
                {
                    response = webtask.Result;
                    response.EnsureSuccessStatusCode();
                }
            }
            finally
            {
                if(process != null && !process.HasExited)
                {
                    process.KillTree();
                }
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        [Benchmark(Iterations = 10, WarmupIterations = 0)]
        [BenchmarkVariation("BasicKestrel_DevelopmentScenario", "BasicKestrel")]
        public void Development_Update_Startup(string sampleName)
        {
            var framework = Microsoft.Extensions.Internal.RuntimeEnvironment.RuntimeType;
            var testName = $"{sampleName}.{framework}.{nameof(Development_Startup)}";
            var logger = LogUtility.LoggerFactory.CreateLogger(testName);

            var testProject = _sampleManager.GetRestoredSample(sampleName);
            Assert.True(testProject != null, $"Fail to set up test project.");
            logger.LogInformation($"Test project is set up at {testProject}");

            var testAppStartInfo = DotnetHelper.GetDefaultInstance().BuildStartInfo(testProject, "run");
            var process = Process.Start(testAppStartInfo);
            Thread.Sleep(5000);
            process.KillTree();
            logger.LogInformation("Run server before updating");

            // update source code
            var lines = File.ReadLines(Path.Combine(testProject, "Startup.cs")).ToArray();
            for (var i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Trim().StartsWith("private const string FixedResponse = "))
                {
                    lines[i] = $"private const string FixedResponse = \"{Guid.NewGuid()}\";";
                }
            }

            var retry = 0;
            while (retry < 3)
            {
                try
                {
                    File.WriteAllLines(Path.Combine(testProject, "Startup.cs"), lines);
                    break;
                }
                catch (IOException)
                {
                    ++retry;
                }
            }
            Assert.True(retry <= 3, "Failed to write the source code for 3 times.");
            logger.LogInformation("Update source code");

            RunStartup(5000, logger, testAppStartInfo);
        }

        private ProcessStartInfo GetStartInfo(string testProject, string sampleName)
        {
            string processPath = Path.Combine(testProject, $"{sampleName}.exe");
            string processArguments = string.Empty;
            if (!File.Exists(processPath))
            {
                processPath = DotnetHelper.GetDefaultInstance().GetDotnetExecutable();
                processArguments = "\"" + Path.Combine(testProject, $"{sampleName}.dll") + "\"";
            }
            return new ProcessStartInfo(processPath)
            {
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                Arguments = processArguments
            };
        }

        private static string GetFrameworkName(string runtimeType)
        {
            if (string.Equals(runtimeType, "clr", StringComparison.OrdinalIgnoreCase))
            {
                return "net451";
            }
            if (string.Equals(runtimeType, "coreclr", StringComparison.OrdinalIgnoreCase))
            {
                return "netcoreapp1.1";
            }
            Assert.False(true, $"Unknown framework {runtimeType}");
            return null;
        }

        private void RunStartup(int port, ILogger logger, ProcessStartInfo testAppStartInfo)
        {
            Task<HttpResponseMessage> webtask = null;
            Process process = null;
            HttpResponseMessage response = null;
            var responseRetrived = false;
            var url = $"http://localhost:{port}/";

            using (var client = new HttpClient())
            {
                using (Collector.StartCollection())
                {
                    process = Process.Start(testAppStartInfo);
                    for (int i = 0; i < _retry; ++i)
                    {
                        try
                        {
                            webtask = client.GetAsync(url);
                            if (webtask.Wait(_timeout))
                            {
                                responseRetrived = true;
                                break;
                            }
                            else
                            {
                                logger.LogError("Http client timeout.");
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            if (process != null && !process.HasExited)
            {
                var processId = process.Id;
                process.KillTree();
                logger.LogDebug($"Kill process {processId}");
            }

            if (responseRetrived)
            {
                response = webtask.Result;
                logger.LogInformation($"Response {response.StatusCode}");
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                finally
                {
                    response.Dispose();
                }
            }
        }
    }
}
