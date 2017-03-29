// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using NullMessageSink = Xunit.Sdk.NullMessageSink;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;

namespace Benchmarks.Framework
{
    public abstract class BenchmarkTestCaseBase : XunitTestCase
    {
        private static readonly string IMetricCollectorTypeInfoName =
            new ReflectionTypeInfo(typeof(IMetricCollector)).Name;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        protected BenchmarkTestCaseBase()
        {
            // No way for us to get access to the message sink on the execution de-serialization path.
            // Fortunately, have reported errors during discovery.
            DiagnosticMessageSink = new NullMessageSink();
        }

        public BenchmarkTestCaseBase(
            string variation,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(diagnosticMessageSink, TestMethodDisplay.Method, testMethod, null)
        {
            // Override display name to avoid getting info about TestMethodArguments in the
            // name (this is covered by the concept of Variation for benchmarks)
            var name = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute))
                .First()
                .GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

            TestMethodName = name;
            DisplayName = $"{name} [Variation: {variation}]";

            DiagnosticMessageSink = diagnosticMessageSink;
            Variation = variation;

            var potentialMetricCollector = testMethod.Method.GetParameters().FirstOrDefault();

            if (potentialMetricCollector != null && IsMetricCollector(potentialMetricCollector.ParameterType))
            {
                var methodArguments = new List<object> { MetricCollector };
                if (testMethodArguments != null)
                {
                    methodArguments.AddRange(testMethodArguments);
                }

                TestMethodArguments = methodArguments.ToArray();
            }
            else
            {
                TestMethodArguments = testMethodArguments;
            }
        }

        protected IMessageSink DiagnosticMessageSink { get; }

        public abstract IMetricCollector MetricCollector { get; }

        public string TestMethodName { get; protected set; }

        public string Variation { get; protected set; }

        public override void Deserialize(IXunitSerializationInfo info)
        {
            base.Deserialize(info);

            TestMethodName = info.GetValue<string>(nameof(TestMethodName));
            Variation = info.GetValue<string>(nameof(Variation));
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            base.Serialize(info);

            info.AddValue(nameof(TestMethodName), TestMethodName);
            info.AddValue(nameof(Variation), Variation);
        }

        protected override string GetUniqueID()
        {
            return $"{TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name}{TestMethod.TestClass.Class.Name}{TestMethod.Method.Name}{Variation}";
        }

        private static bool IsMetricCollector(ITypeInfo typeInfo)
        {
            if (string.Equals(typeInfo.Name, IMetricCollectorTypeInfoName, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (var intrfc in typeInfo.Interfaces)
            {
                if (IsMetricCollector(intrfc))
                {
                    return true;
                }
            }

            if ((typeInfo.BaseType as ReflectionTypeInfo)?.Type != null)
            {
                return IsMetricCollector(typeInfo.BaseType);
            }

            return false;
        }
    }
}
