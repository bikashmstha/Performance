// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelloWorldMvc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvcWithDefaultRoute();
        }

        public static void Main(string[] args)
        {
#if MCJ
            var context = System.Runtime.Loader.AssemblyLoadContext.Default;
            context.SetProfileOptimizationRoot(".");
            context.StartProfileOptimization("startup.prof");
#endif
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .AddCommandLine(args)
                .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://+:5000")
                .UseConfiguration(config)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            if(GCSettings.IsServerGC)
            {
               Console.WriteLine("Server GC");
            }
            else
            {
                Console.WriteLine("Workstation GC");
            }

            host.Run();
        }
    }
}

