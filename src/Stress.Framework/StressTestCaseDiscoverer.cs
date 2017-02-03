// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Stress.Framework
{
    public class StressTestCaseDiscoverer : IXunitTestCaseDiscoverer
    {
        private static readonly string StressTestBaseName = new ReflectionTypeInfo(typeof(StressTestBase)).Name;
        private readonly IMessageSink _diagnosticMessageSink;

        public StressTestCaseDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public virtual IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            if (!IsStressTestBase(testMethod.TestClass.Class))
            {
                _diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage(
                        $"Could not resolve stress test because its parent class did not inherit from {testMethod.TestClass.Class.Name}."));
                return Enumerable.Empty<IXunitTestCase>();
            }

            var variations = testMethod.Method
                .GetCustomAttributes(typeof(StressVariationAttribute))
                .Select(a => new
                {
                    Name = a.GetNamedArgument<string>(nameof(StressVariationAttribute.VariationName)),
                    TestMethodArguments = a.GetNamedArgument<object[]>(nameof(StressVariationAttribute.Data))
                })
                .ToList();

            if (!variations.Any())
            {
                variations.Add(new
                {
                    Name = "Default",
                    TestMethodArguments = new object[0]
                });
            }
            var servers = factAttribute.GetNamedArgument<ServerType[]>(nameof(StressAttribute.Servers));

            var tests = new List<IXunitTestCase>();
            foreach (var serverType in servers)
            {
                foreach (var variation in variations)
                {
                    var warmupMethod = ResolveWarmupMethod(testMethod, factAttribute);
                    var iterations = StressConfig.Instance.RunIterations ? factAttribute.GetNamedArgument<long>(nameof(StressAttribute.Iterations)) : 1;
                    var threads = factAttribute.GetNamedArgument<int>(nameof(StressAttribute.Clients));

                    var variationName = variation.Name;
                    if (servers.Length > 1)
                    {
                        variationName = variationName + "_" + serverType;
                    }

                    tests.Add(new StressTestCase(
                        factAttribute.GetNamedArgument<string>(nameof(StressAttribute.TestApplicationName)),
                        iterations,
                        threads,
                        variationName,
                        serverType,
                        warmupMethod,
                        _diagnosticMessageSink,
                        testMethod,
                        variation.TestMethodArguments));
                }

            }

            return tests;
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
