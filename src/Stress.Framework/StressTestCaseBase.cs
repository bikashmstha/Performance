// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using NullMessageSink = Xunit.Sdk.NullMessageSink;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;

namespace Stress.Framework
{
    public abstract class StressTestCaseBase : XunitTestCase
    {
        private static readonly string IMetricCollectorTypeInfoName =
            new ReflectionTypeInfo(typeof(IStressMetricCollector)).Name;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        protected StressTestCaseBase()
        {
            // No way for us to get access to the message sink on the execution de-serialization path.
            // Fortunately, have reported errors during discovery.
            DiagnosticMessageSink = new NullMessageSink();
        }

        public StressTestCaseBase(
            string testApplicationName,
            string variation,
            ServerType serverType,
            IMethodInfo warmupMethod,
            IMessageSink diagnosticMessageSink,
            ITestMethod testMethod,
            object[] testMethodArguments)
            : base(diagnosticMessageSink, TestMethodDisplay.Method, testMethod, null)
        {
            // Override display name to avoid getting info about TestMethodArguments in the
            // name (this is covered by the concept of Variation for Stress)
            var name = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute))
                .First()
                .GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

            TestApplicationName = testApplicationName;
            TestMethodName = name;
            WarmupMethod = warmupMethod;
            DisplayName = $"{name} [Variation: {variation}]";

            DiagnosticMessageSink = diagnosticMessageSink;
            Variation = variation;
            ServerType = serverType;

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

        public string TestApplicationName { get; private set; }

        protected IMessageSink DiagnosticMessageSink { get; private set; }

        public abstract IStressMetricCollector MetricCollector { get; }

        public string TestMethodName { get; protected set; }

        public string Variation { get; protected set; }

        public IMethodInfo WarmupMethod { get; protected set; }

        public ServerType ServerType { get; protected set; }

        public override void Deserialize(IXunitSerializationInfo info)
        {
            base.Deserialize(info);

            ServerType = (ServerType)Enum.Parse(typeof(ServerType), info.GetValue<string>(nameof(ServerType)));
            TestApplicationName = info.GetValue<string>(nameof(TestApplicationName));
            TestMethodName = info.GetValue<string>(nameof(TestMethodName));
            Variation = info.GetValue<string>(nameof(Variation));

            var warmupMethodName = info.GetValue<string>(nameof(WarmupMethod));
            WarmupMethod = TestMethod.TestClass.Class.GetMethod(warmupMethodName, includePrivateMethod: false);
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            base.Serialize(info);

            info.AddValue(nameof(ServerType), ServerType.ToString());
            info.AddValue(nameof(TestApplicationName), TestApplicationName);
            info.AddValue(nameof(TestMethodName), TestMethodName);
            info.AddValue(nameof(Variation), Variation);
            info.AddValue(nameof(WarmupMethod), WarmupMethod.Name);
        }

        protected override string GetSkipReason(IAttributeInfo factAttribute) => EvaluateSkipConditions(TestMethod) ?? base.GetSkipReason(factAttribute);

        private string EvaluateSkipConditions(ITestMethod testMethod)
        {
            var conditionAttributes = testMethod.Method
                .GetCustomAttributes(typeof(ITestCondition))
                .OfType<ReflectionAttributeInfo>()
                .Select(attributeInfo => attributeInfo.Attribute)
                .ToList();

            conditionAttributes.AddRange(testMethod.TestClass.Class
                .GetCustomAttributes(typeof(ITestCondition))
                .OfType<ReflectionAttributeInfo>()
                .Select(attributeInfo => attributeInfo.Attribute));

            var reasons = conditionAttributes.Cast<ITestCondition>()
                .Where(condition => !condition.IsMet)
                .Select(condition => condition.SkipReason)
                .ToList();

            return reasons.Count > 0 ? string.Join(Environment.NewLine, reasons) : null;
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

            if ((typeInfo.BaseType as ReflectionTypeInfo).Type != null)
            {
                return IsMetricCollector(typeInfo.BaseType);
            }

            return false;
        }
    }
}
