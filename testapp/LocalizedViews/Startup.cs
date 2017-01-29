// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalizedViews
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddViewLocalization(options => options.ResourcesPath = "Resources");
        }

        public void Configure(IApplicationBuilder app)
        {
            // Default to en-CA locale and make sure nothing overrides that choice.
            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-CA"),
            };
            options.RequestCultureProviders.Clear();
            app.UseRequestLocalization(options);

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
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

