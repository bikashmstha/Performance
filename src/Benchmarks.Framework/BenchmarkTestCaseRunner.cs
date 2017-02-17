// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Benchmarks.Framework.BenchmarkPersistence;
#if !NET452
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
#endif
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Benchmarks.Framework
{
    public class BenchmarkTestCaseRunner : XunitTestCaseRunner
    {
        private static readonly string _machineName = GetMachineName();
        private static readonly string _framework = GetFramework();
        private readonly IMessageSink _diagnosticMessageSink;

        public BenchmarkTestCaseRunner(
            BenchmarkTestCase testCase,
            string displayName,
            string skipReason,
            object[] constructorArguments,
            object[] testMethodArguments,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            IMessageSink diagnosticMessageSink)
            : base(
                testCase,
                displayName,
                skipReason,
                constructorArguments,
                testMethodArguments,
                messageBus,
                aggregator,
                cancellationTokenSource)
        {
            TestCase = testCase;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public new BenchmarkTestCase TestCase { get; }

        protected override async Task<RunSummary> RunTestAsync()
        {
            var runSummary = new BenchmarkRunSummary
            {
                TestClassFullName = TestCase.TestMethod.TestClass.Class.Name,
                TestClass = TestCase.TestMethod.TestClass.Class.Name.Split('.').Last(),
                TestMethod = TestCase.TestMethodName,
                Variation = TestCase.Variation,
                ProductReportingVersion = BenchmarkConfig.Instance.ProductReportingVersion,
                RunStarted = DateTime.UtcNow,
                MachineName = _machineName,
                Framework = TestCase.Framework ?? _framework,
                Architecture = IntPtr.Size > 4 ? "x64" : "x86",
                WarmupIterations = TestCase.WarmupIterations,
                Iterations = TestCase.Iterations,
                CustomData = BenchmarkConfig.Instance.CustomData
            };

            for (var i = 0; i < TestCase.WarmupIterations; i++)
            {
                var runner = CreateRunner(i + 1, TestCase, warmup: true);
                runSummary.Aggregate(await runner.RunAsync());
            }

            for (var i = 0; i < TestCase.Iterations; i++)
            {
                TestCase.MetricCollector.Reset();
                var runner = CreateRunner(i + 1, TestCase, warmup: false);
                var iterationSummary = new BenchmarkIterationSummary();
                iterationSummary.Aggregate(await runner.RunAsync(), TestCase.MetricCollector);
                runSummary.Aggregate(iterationSummary);
            }

            if (runSummary.Failed != 0)
            {
                _diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage($"No valid results for {TestCase.DisplayName}. {runSummary.Failed} of {TestCase.Iterations + TestCase.WarmupIterations} iterations failed."));
            }
            else
            {
                runSummary.PopulateMetrics();
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage(runSummary.ToString()));

                try
                {
                    BenchmarkResultProcessor.SaveSummary(runSummary);
                }
                catch (Exception ex)
                {
                    _diagnosticMessageSink.OnMessage(
                        new DiagnosticMessage($"Failed to save results {Environment.NewLine} {ex}"));
                    throw;
                }
            }

            return runSummary;
        }

        private XunitTestRunner CreateRunner(int iteration, BenchmarkTestCase testCase, bool warmup)
        {
            var iterations = warmup ? testCase.WarmupIterations : testCase.Iterations;
            var name = $"{DisplayName} [Stage: {(warmup ? "Warmup" : "Collection")}] [Iteration: {iteration}/{iterations}]";

            return new BenchmarkTestRunner(
                new XunitTest(TestCase, name),
                testCase.MetricCollector,
                MessageBus,
                TestClass,
                ConstructorArguments,
                TestMethod,
                TestMethodArguments,
                SkipReason,
                BeforeAfterAttributes,
                Aggregator,
                CancellationTokenSource);
        }

        private static string GetFramework()
        {
            return "DNX." + Microsoft.Extensions.Internal.RuntimeEnvironment.RuntimeType;
        }

        private static string GetMachineName()
        {
#if NET452
            return Environment.MachineName;
#else
            var config = new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddEnvironmentVariables()
                .Build();

            return config["computerName"];
#endif
        }

        private class BenchmarkTestRunner : XunitTestRunner
        {
            private readonly IMetricCollector _metricCollector;

            public BenchmarkTestRunner(
                ITest test,
                IMetricCollector metricCollector,
                IMessageBus messageBus,
                Type testClass,
                object[] constructorArguments,
                MethodInfo testMethod,
                object[] testMethodArguments,
                string skipReason,
                IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                ExceptionAggregator aggregator,
                CancellationTokenSource cancellationTokenSource)
                : base(
                      test,
                      messageBus,
                      testClass,
                      constructorArguments,
                      testMethod,
                      testMethodArguments,
                      skipReason,
                      beforeAfterAttributes,
                      aggregator,
                      cancellationTokenSource)
            {
                _metricCollector = metricCollector;
            }

            protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator) =>
                new BenchmarkTestInvoker(
                    Test,
                    _metricCollector,
                    MessageBus,
                    TestClass,
                    ConstructorArguments,
                    TestMethod,
                    TestMethodArguments,
                    BeforeAfterAttributes,
                    aggregator,
                    CancellationTokenSource).RunAsync();
        }

        private class BenchmarkTestInvoker : XunitTestInvoker
        {
            private readonly IMetricCollector _metricCollector;

            public BenchmarkTestInvoker(
                ITest test,
                IMetricCollector metricCollector,
                IMessageBus messageBus,
                Type testClass,
                object[] constructorArguments,
                MethodInfo testMethod,
                object[] testMethodArguments,
                IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                ExceptionAggregator aggregator,
                CancellationTokenSource cancellationTokenSource)
                : base(
                     test,
                     messageBus,
                     testClass,
                     constructorArguments,
                     testMethod,
                     testMethodArguments,
                     beforeAfterAttributes,
                     aggregator,
                     cancellationTokenSource)
            {
                _metricCollector = metricCollector;
            }

            protected override object CreateTestClass()
            {
                var testClass = base.CreateTestClass();
                var benchmarkTestBase = testClass as IBenchmarkTest;

                if (benchmarkTestBase != null)
                {
                    benchmarkTestBase.Collector = _metricCollector;
                }

                return testClass;
            }
        }
    }
}
