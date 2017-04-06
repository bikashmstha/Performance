// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace MvcBenchmarks.InMemory
{
    public class TestAppFixture<TStartup> : IDisposable
         where TStartup : class
    {
        private readonly TestServer _server;

        public TestAppFixture()
        {
            var builder = new WebHostBuilder();
            builder.UseStartup<TStartup>();
            builder.UseProjectOf<TStartup>();

            _server = new TestServer(builder);
            Client = _server.CreateClient();
        }

        public HttpClient Client { get; protected set; }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }
    }
}
