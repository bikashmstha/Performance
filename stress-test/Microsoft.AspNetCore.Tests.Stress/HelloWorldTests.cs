// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Stress.Framework;
using Xunit;

namespace Microsoft.AspNetCore.Tests.Stress
{
    [Collection("May share ports")]
    public class HelloWorldTests : StressTestBase
    {
        public static async Task HelloWorld_Warmup(HttpClient client)
        {
            var response = await client.GetAsync("/");
            response.EnsureSuccessStatusCode();
        }

        [Stress("BasicKestrel", WarmupMethodName = nameof(HelloWorld_Warmup))]
        public Task Middleware_HelloWorld()
        {
            return IterateAsync(async client =>
            {
                var response = await client.GetAsync("/");
                response.EnsureSuccessStatusCode();
            });
        }

        [Stress("HelloWorldMvc", WarmupMethodName = nameof(HelloWorld_Warmup))]
        public Task Mvc_HelloWorld()
        {
            return IterateAsync(async client =>
            {
                var response = await client.GetAsync("/");
                response.EnsureSuccessStatusCode();
            });
        }
    }
}
