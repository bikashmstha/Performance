// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace Stress.Framework
{
    public class StressTestStatisticsMessage : TestCaseMessage, ITestStatisticsMessage
    {
        public StressTestStatisticsMessage(ITestCase testCase, string key, object value)
            : base(testCase)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }

        public object Value { get; private set; }
    }
}