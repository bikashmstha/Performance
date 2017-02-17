// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#if !NET452
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
#endif
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Stress.Framework
{
    public class StressTestCaseRunner : XunitTestCaseRunner
    {
        private static readonly string _machineName = GetMachineName();
        private static readonly string _framework = GetFramework();
        private readonly IMessageSink _diagnosticMessageSink;

        public StressTestCaseRunner(
            StressTestCase testCase,
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

        public new StressTestCase TestCase { get; }

        protected override async Task<RunSummary> RunTestAsync()
        {
            var runSummary = new StressRunSummary
            {
                TestClassFullName = TestCase.TestMethod.TestClass.Class.Name,
                TestClass = TestCase.TestMethod.TestClass.Class.Name.Split('.').Last(),
                TestMethod = TestCase.TestMethodName,
                Variation = TestCase.Variation,
                RunStarted = DateTime.UtcNow,
                MachineName = _machineName,
                Framework = _framework,
                Architecture = IntPtr.Size > 4 ? "x64" : "x86",
                Iterations = TestCase.Iterations,
            };

            // Warmup
            var server = new StressTestServer(
                TestCase.ServerType,
                TestCase.TestApplicationName,
                TestCase.TestMethodName,
                port: 5000,
                metricCollector: TestCase.MetricCollector);

            using (server)
            {
                var startResult = await server.StartAsync();

                if (!startResult.SuccessfullyStarted)
                {
                    _diagnosticMessageSink.OnMessage(
                        new DiagnosticMessage("Failed to start application server."));
                    return new RunSummary() { Total = 1, Failed = 1 };
                }
                else
                {
                    using (startResult.ServerHandle)
                    {
                        await (Task)TestCase.WarmupMethod?.ToRuntimeMethod().Invoke(null, new[] { server.ClientFactory() });

                        TestCase.MetricCollector.Reset();
                        var runner = CreateRunner(server, TestCase);
                        runSummary.Aggregate(await runner.RunAsync());
                    }
                }

            }

            if (runSummary.Failed != 0)
            {
                _diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage($"No valid results for {TestCase.DisplayName}. {runSummary.Failed} of {TestCase.Iterations} iterations failed."));
            }
            else
            {
                runSummary.PopulateMetrics(TestCase.MetricCollector);
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage(runSummary.ToString()));

                runSummary.PublishOutput(TestCase, MessageBus);
            }

            return runSummary;
        }

        private XunitTestRunner CreateRunner(StressTestServer server, StressTestCase testCase)
        {
            var name = DisplayName;

            return new StressTestRunner(
                server,
                new XunitTest(TestCase, name),
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

        private class StressTestRunner : XunitTestRunner
        {
            private readonly StressTestServer _server;

            public StressTestRunner(
                StressTestServer server,
                ITest test,
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
                _server = server;
            }

            protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator) =>
                new StressTestInvoker(
                    _server,
                    Test,
                    MessageBus,
                    TestClass,
                    ConstructorArguments,
                    TestMethod,
                    TestMethodArguments,
                    BeforeAfterAttributes,
                    aggregator,
                    CancellationTokenSource).RunAsync();
        }

        private class StressTestInvoker : XunitTestInvoker
        {
            private readonly StressTestServer _server;

            public StressTestInvoker(
                StressTestServer server,
                ITest test,
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
                _server = server;
            }

            protected override object CreateTestClass()
            {
                var testClass = base.CreateTestClass();
                var StressTestBase = testClass as StressTestBase;

                if (StressTestBase != null)
                {
                    var stressTestCase = (TestCase as StressTestCase);
                    StressTestBase.Iterations = stressTestCase.Iterations;
                    StressTestBase.Clients = stressTestCase.Clients;
                    StressTestBase.Collector = stressTestCase.MetricCollector;
                    StressTestBase.ClientFactory = _server.ClientFactory;
                }

                return testClass;
            }

            protected override Task BeforeTestMethodInvokedAsync()
            {
                var stressTestCase = this.TestCase as StressTestCase;
                if (stressTestCase != null)
                {
                    stressTestCase.MetricReporter.Start(MessageBus, stressTestCase);
                }
                return base.BeforeTestMethodInvokedAsync();
            }

            protected override Task AfterTestMethodInvokedAsync()
            {
                var testCase = this.TestCase as StressTestCase;
                if (testCase != null)
                {
                    testCase.MetricReporter.Stop();
                }
                return base.AfterTestMethodInvokedAsync();
            }
        }
    }
}
