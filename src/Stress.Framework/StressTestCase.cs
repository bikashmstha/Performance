// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Stress.Framework
{
    public class StressTestCase : StressTestCaseBase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public StressTestCase()
        {
        }

        public StressTestCase(
            string testApplicationName,
            long iterations,
            int clients,
            string variation,
            ServerType serverType,
            IMethodInfo warmupMethod,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(
            testApplicationName,
            variation,
            serverType,
            warmupMethod,
            diagnosticMessageSink,
            testMethod,
            testMethodArguments)
        {
            Clients = clients;
            Iterations = iterations;
        }

        public override IStressMetricCollector MetricCollector { get; } = new StressMetricCollector();

        public virtual IStressMetricReporter MetricReporter { get; } = new TimedMetricReporter();

        public long Iterations { get; protected set; }

        public int Clients { get; protected set; }

        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new StressTestCaseRunner(
                this,
                DisplayName,
                SkipReason,
                constructorArguments,
                TestMethodArguments,
                messageBus,
                aggregator,
                cancellationTokenSource,
                DiagnosticMessageSink).RunAsync();
        }

        public override void Deserialize(IXunitSerializationInfo info)
        {
            base.Deserialize(info);

            Clients = info.GetValue<int>(nameof(Clients));
            Iterations = info.GetValue<long>(nameof(Iterations));
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            base.Serialize(info);

            info.AddValue(nameof(Clients), Clients);
            info.AddValue(nameof(Iterations), Iterations);
        }
    }
}
