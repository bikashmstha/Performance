// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Benchmarks.Framework;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Stress.Framework
{
    public class StressTestCaseDiscoverer : FactDiscoverer
    {
        private static readonly string StressTestBaseName = new ReflectionTypeInfo(typeof(StressTestBase)).Name;
        private readonly IMessageSink _diagnosticMessageSink;

        public StressTestCaseDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override IXunitTestCase CreateTestCase(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            if (!IsStressTestBase(testMethod.TestClass.Class))
            {
                return new ExecutionErrorTestCase(
                    _diagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    testMethod,
                    $"Could not resolve stress test because its parent class did not inherit from {testMethod.TestClass.Class.Name}.");
            }

            var skipReason = testMethod.EvaluateSkipConditions();
            if (skipReason != null)
            {
                return new SkippedTestCase(
                    skipReason,
                    _diagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    testMethod);
            }

            var testApplicationName = factAttribute.GetNamedArgument<string>(nameof(StressAttribute.TestApplicationName));
            var iterations = StressConfig.Instance.RunIterations ?
                factAttribute.GetNamedArgument<long>(nameof(StressAttribute.Iterations)) :
                1;
            var threads = factAttribute.GetNamedArgument<int>(nameof(StressAttribute.Clients));
            var serverType = factAttribute.GetNamedArgument<ServerType>(nameof(StressAttribute.Server));
            var warmupMethod = ResolveWarmupMethod(testMethod, factAttribute);

            return new StressTestCase(
                testApplicationName,
                iterations,
                threads,
                serverType,
                warmupMethod,
                _diagnosticMessageSink,
                testMethod);
        }

        private static IMethodInfo ResolveWarmupMethod(ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var warmupMethodName = factAttribute.GetNamedArgument<string>(nameof(StressAttribute.WarmupMethodName));
            var warmupMethod = testMethod.TestClass.Class.GetMethod(warmupMethodName, includePrivateMethod: false);

            return warmupMethod;
        }

        private static bool IsStressTestBase(ITypeInfo typeInfo)
        {
            if (string.Equals(typeInfo.Name, StressTestBaseName, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (var intrfc in typeInfo.Interfaces)
            {
                if (IsStressTestBase(intrfc))
                {
                    return true;
                }
            }

            if ((typeInfo.BaseType as ReflectionTypeInfo).Type != null)
            {
                return IsStressTestBase(typeInfo.BaseType);
            }

            return false;
        }
    }
}
