// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;

namespace Stress.Framework
{
    public abstract class StressTestCaseBase : XunitTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        protected StressTestCaseBase()
        {
        }

        public StressTestCaseBase(
            string testApplicationName,
            ServerType serverType,
            IMethodInfo warmupMethod,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod)
            : base(diagnosticMessageSink, TestMethodDisplay.Method, testMethod)
        {
            // Override display name to avoid getting info about TestMethodArguments in the name; Fact has no arguments.
            var name = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute))
                .First()
                .GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

            TestApplicationName = testApplicationName;
            TestMethodName = name;
            WarmupMethod = warmupMethod;
            DisplayName = name;
            ServerType = serverType;
        }

        public string TestApplicationName { get; private set; }

        public abstract IStressMetricCollector MetricCollector { get; }

        public string TestMethodName { get; protected set; }

        public IMethodInfo WarmupMethod { get; protected set; }

        public ServerType ServerType { get; protected set; }

        public override void Deserialize(IXunitSerializationInfo info)
        {
            base.Deserialize(info);

            ServerType = (ServerType)Enum.Parse(typeof(ServerType), info.GetValue<string>(nameof(ServerType)));
            TestApplicationName = info.GetValue<string>(nameof(TestApplicationName));
            TestMethodName = info.GetValue<string>(nameof(TestMethodName));

            var warmupMethodName = info.GetValue<string>(nameof(WarmupMethod));
            WarmupMethod = TestMethod.TestClass.Class.GetMethod(warmupMethodName, includePrivateMethod: false);
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            base.Serialize(info);

            info.AddValue(nameof(ServerType), ServerType.ToString());
            info.AddValue(nameof(TestApplicationName), TestApplicationName);
            info.AddValue(nameof(TestMethodName), TestMethodName);
            info.AddValue(nameof(WarmupMethod), WarmupMethod.Name);
        }

        protected override string GetUniqueID()
        {
            return $"{TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name}{TestMethod.TestClass.Class.Name}{TestMethod.Method.Name}";
        }
    }
}
