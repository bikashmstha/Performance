// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace Stress.Framework
{
    public class StressConfig
    {
        private static readonly Lazy<StressConfig> _instance = new Lazy<StressConfig>(() =>
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                    .AddJsonFile("config.json")
                    .AddEnvironmentVariables()
                    .Build();

                return new StressConfig
                {
                    RunIterations = bool.Parse(config["Stress:RunIterations"] ?? "false"),
                    Iterations = long.Parse(config["Stress:Iterations"]),
                    MetricReportInterval = int.Parse(config["Stress:MetricReportInterval"]),
                    FailDebugger = bool.Parse(config["Stress:FailDebugger"] ?? "false"),
                    DeployerLogging = bool.Parse(config["Stress:DeployerLogging"] ?? "true"),
                    StatisticOutputFolder = config["Stress:StatisticOutputFolder"],
                    Clients = int.Parse(config["Stress:Clients"] ?? "1"),
                };
            });

        private StressConfig()
        {
        }

        public static StressConfig Instance => _instance.Value;

        public bool RunIterations { get; private set; }

        public long Iterations { get; private set; }

        public int MetricReportInterval { get; private set; }

        public bool FailDebugger { get; private set; }

        public bool DeployerLogging { get; private set; }

        public string StatisticOutputFolder { get; private set; }

        public int Clients { get; private set; }
    }
}
