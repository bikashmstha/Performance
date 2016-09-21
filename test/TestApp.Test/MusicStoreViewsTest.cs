// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Benchmarks.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MusicStoreViews;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class MusicStoreViewsTest : IDisposable
    {
        private TestServer _server;
        private HttpClient _client;

        public MusicStoreViewsTest()
        {
            var builder = new WebHostBuilder();
            builder.UseStartup<Startup>();
            builder.UseProjectOf<Startup>();

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Benchmark(Iterations = 1, WarmupIterations = 0)]
        public async Task AddressAndPayment_ViewPerformsAsExpected()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/Home/AddressAndPayment");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("Create")]
        [InlineData("Register")]
        public async Task MusicStoreViews_ViewsAreSuccessful(string actionName)
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/Home/{actionName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}
