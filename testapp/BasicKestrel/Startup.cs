// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Test.Perf.WebFx.Apps.HelloWorld
{
    public class Startup
    {
        private static readonly byte[] _helloWorldPayload = Encoding.UTF8.GetBytes("Hello, World!");

        public void Configure(IApplicationBuilder app)
        {
            app.Run(context =>
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/plain";
                // HACK: Setting the Content-Length header manually avoids the cost of serializing the int to a string.
                //       This is instead of: httpContext.Response.ContentLength = _helloWorldPayload.Length;
                context.Response.Headers["Content-Length"] = "13";
                return context.Response.Body.WriteAsync(_helloWorldPayload, 0, _helloWorldPayload.Length);
            });
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
                .UseConfiguration(config)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

