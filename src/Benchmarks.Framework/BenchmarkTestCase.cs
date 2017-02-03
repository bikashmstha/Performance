// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Benchmarks.Framework
{
    public class BenchmarkTestCase : BenchmarkTestCaseBase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public BenchmarkTestCase()
        {
        }

        public BenchmarkTestCase(
            int iterations,
            int warmupIterations,
            string framework,
            string variation,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(variation, diagnosticMessageSink, testMethod, testMethodArguments)
        {
            Iterations = iterations;
            WarmupIterations = warmupIterations;

            if (!string.IsNullOrEmpty(framework))
            {
                Framework = framework;
            }
        }

        public override IMetricCollector MetricCollector { get; } = new MetricCollector();

        public int Iterations { get; protected set; }

        public int WarmupIterations { get; protected set; }

        public string Framework { get; private set; }

        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new BenchmarkTestCaseRunner(
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

            Framework = info.GetValue<string>(nameof(Framework));
            Iterations = info.GetValue<int>(nameof(Iterations));
            WarmupIterations = info.GetValue<int>(nameof(WarmupIterations));
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            base.Serialize(info);

            info.AddValue(nameof(Framework), Framework);
            info.AddValue(nameof(Iterations), Iterations);
            info.AddValue(nameof(WarmupIterations), WarmupIterations);
        }
    }
}
