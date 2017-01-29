// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Test.Perf.WebFx.Apps.HelloWorld
{
    public class Startup
    {
        private static readonly byte[] _helloWorldPayload = Encoding.UTF8.GetBytes("Hello, World!");
        private static IWebHost _host;

        public void Configure(IApplicationBuilder app)
        {
            app.Use(next => async context =>
            {
                try
                {
                    var qs = context.Request.QueryString.ToString();
                    if ("?exit".Equals(qs))
                    {
                        var applicationLifetime = (IApplicationLifetime)_host.Services.GetService(typeof(IApplicationLifetime));
                        applicationLifetime.StopApplication();
                    }
                    await next(context);
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine(e);
                    throw;
                }
            });

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
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            while(true)
            {
                _host = new WebHostBuilder()
                    .UseKestrel()
                    .UseConfiguration(config)
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

                _host.Run();
            }
        }
    }
}

