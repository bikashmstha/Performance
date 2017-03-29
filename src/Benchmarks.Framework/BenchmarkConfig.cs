// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace Benchmarks.Framework
{
    public class BenchmarkConfig
    {
        private static readonly Lazy<BenchmarkConfig> _instance = new Lazy<BenchmarkConfig>(() =>
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                    .AddJsonFile("config.json")
                    .AddEnvironmentVariables()
                    .Build();

                var resultDatabasesSection = config.GetSection("Benchmarks:ResultDatabases");

                return new BenchmarkConfig
                {
                    RunIterations = bool.Parse(config["Benchmarks:RunIterations"]),
                    ResultDatabases = resultDatabasesSection.GetChildren().Select(s => s.Value).ToList(),
                    BenchmarkDatabaseInstance = config["Benchmarks:BenchmarkDatabaseInstance"],
                    ProductReportingVersion = config["Benchmarks:ProductReportingVersion"],
                    CustomData = config["Benchmarks:CustomData"],
                    DeployerLogging = bool.Parse(config["Benchmarks:DeployerLogging"] ?? "true"),
                };
            });

        private BenchmarkConfig()
        {
        }

        public static BenchmarkConfig Instance =>_instance.Value;

        public bool RunIterations { get; private set; }

        public IEnumerable<string> ResultDatabases { get; private set; }

        public string BenchmarkDatabaseInstance { get; private set; }

        public string ProductReportingVersion { get; private set; }

        public string CustomData { get; private set; }

        public bool DeployerLogging { get; private set; }
    }
}
