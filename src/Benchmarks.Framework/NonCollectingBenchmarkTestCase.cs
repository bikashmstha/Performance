// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Xunit.Abstractions;

namespace Benchmarks.Framework
{
    public class NonCollectingBenchmarkTestCase : BenchmarkTestCaseBase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public NonCollectingBenchmarkTestCase()
        {
        }

        public NonCollectingBenchmarkTestCase(
            string variation,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(variation, diagnosticMessageSink, testMethod, testMethodArguments)
        {
        }

        public override IMetricCollector MetricCollector { get; } = new NullMetricCollector();
    }
}
