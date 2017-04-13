// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Benchmarks.Framework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microbenchmarks.Tests
{
    public class HostingTests : BenchmarkTestBase
    {
        [Benchmark]
        public void MainToConfigureOverhead()
        {
            var args = new[] { "--captureStartupErrors", "true" };

            using (Collector.StartCollection())
            {
                var config = new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                    .Build();

                var builder = new WebHostBuilder()
                    .UseConfiguration(config)
                    .UseStartup(typeof(TestStartup))
                    .ConfigureServices(ConfigureTestServices);

                var host = builder.Build();
                host.Start();
                host.Dispose();
            }
        }

        private void ConfigureTestServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IServer), new TestServer());
            services.AddSingleton(Collector);
        }

        private class TestStartup
        {
            public void Configure(IApplicationBuilder app, IMetricCollector collector)
            {
                collector.StopCollection();
            }
        }

        private class TestServer : IServer
        {
            public IFeatureCollection Features { get; }

            public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
            {
                // No-op, we don't want to actually start the server.
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                // No-op, nothing to stop.
                return Task.CompletedTask;
            }

            public void Dispose()
            {
            }
        }
    }
}
