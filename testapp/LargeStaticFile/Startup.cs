// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace LargeStaticFile
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            var assemblyName = assembly.GetName().Name;
            var options = new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(assembly, assemblyName + ".resources"),
            };

            app.UseStaticFiles(options);
        }

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var application = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://+:5000")
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .Build();

            application.Run();
        }
    }
}

