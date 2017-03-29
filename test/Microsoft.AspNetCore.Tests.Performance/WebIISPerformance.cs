// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Benchmarks.Framework;
using Benchmarks.Utility.Helpers;
using Benchmarks.Utility.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Testing.xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.AspNetCore.Tests.Performance
{
    /// <summary>
    /// This test relies on Microsoft.AspNetCore and IIS Express.
    /// </summary>
    public class WebIISPerformance : IBenchmarkTest, IClassFixture<IISTestManager>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IISTestManager _testManager;

        public WebIISPerformance(IISTestManager testManager)
        {
            _loggerFactory = LogUtility.LoggerFactory;
            _testManager = testManager;

            var deployerLogger = BenchmarkConfig.Instance.DeployerLogging ? _loggerFactory : NullLoggerFactory.Instance;
            _testManager.Initialize(deployerLogger);
        }

        public IMetricCollector Collector { get; set; } = new NullMetricCollector();

        [OSSkipCondition(OperatingSystems.Linux)]
        [OSSkipCondition(OperatingSystems.MacOSX)]
        [Benchmark(Iterations = 1, WarmupIterations = 0, Skip = "Requires optional IIS Express enabled on test machine")]
        [BenchmarkVariation("StarterMvc_IISScenario", "StarterMvc", RuntimeFlavor.Clr, Framework = "DNX.Clr")]
        [BenchmarkVariation("StarterMvc_IISScenario", "StarterMvc", RuntimeFlavor.CoreClr, Framework = "DNX.CoreClr")]
        public void Startup(string sampleName, RuntimeFlavor runtimeFlavor)
        {
            var testName = $"{sampleName}.{runtimeFlavor}.{nameof(Startup)}";
            var logger = _loggerFactory.CreateLogger(testName);
            var siteUrl = _testManager.GetSite(sampleName, runtimeFlavor, restart: true);
            Assert.NotNull(siteUrl);

            var client = new HttpClient();

            using (Collector.StartCollection())
            {
                var responseTask = client.GetAsync(siteUrl);
                Assert.True(responseTask.Wait(TimeSpan.FromMinutes(10)));
                logger.LogInformation($"Response {responseTask.Result.StatusCode}");
                responseTask.Result.EnsureSuccessStatusCode();
            }
        }
    }
}
