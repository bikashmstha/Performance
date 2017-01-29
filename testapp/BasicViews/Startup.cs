// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace BasicViews
{
    public class Startup
    {
        public Startup(IHostingEnvironment hosting)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["Data:DefaultConnection:ConnectionStringBasicViews"];
            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<BasicViewsContext>(c => c.UseSqlServer(connectionString));

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            CreateDatabase(app.ApplicationServices);

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

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        private void CreateDatabase(IServiceProvider services)
        {
            using (var serviceScope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = services.GetRequiredService<BasicViewsContext>();
                dbContext.Database.EnsureDeleted();
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
                dbContext.Database.EnsureCreated();
            }
        }

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
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

