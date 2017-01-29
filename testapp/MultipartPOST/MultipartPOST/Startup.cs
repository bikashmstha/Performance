// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MultipartPost
{
    public class Startup
    {
        private const long OneByte = 1;
        private const long OneKilobyte = OneByte * 1024;
        private const long OneMegabyte = OneKilobyte * 1024;
        private const long OneGigabyte = OneMegabyte * 1024;
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = OneGigabyte; //let's enable uploads of up to 1 GB per part
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestLineSize = 1 * (int)OneKilobyte;
                options.Limits.MaxRequestBufferSize = 4 * OneKilobyte; //let's customize the max buffer size per request
            });
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(next => async (context) =>
            {
                try
                {
                    await next(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void Main(string[] args)
        {
            Console.WriteLine($"Running in { IntPtr.Size * 8 } bits");

            var config = new ConfigurationBuilder()
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

