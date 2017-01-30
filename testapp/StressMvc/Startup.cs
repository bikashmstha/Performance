// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarterMvc.Models;
using StarterMvc.Services;

namespace StarterMvc
{
    public class Startup
    {
        static readonly string _httpsCertFile = "stressmvc.pfx";
        static readonly string _httpsCertPwd = "stressmvc";
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

            }

            app.UseStaticFiles();

            app.UseIdentity();

            if (bool.Parse(Configuration["WebSocketOptions:ReplaceFeature"]))
            {
                app.Use((context, next) =>
                {
                    context.Features.Set<IHttpWebSocketFeature>(null);
                    return next();
                });
            }
            app.UseWebSockets();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "api",
                    template: "{controller}/{id?}");
            });

            TextContentRelayController.UseSingletonClient = bool.Parse(Configuration["TextContentRelayControllerOptions:UseSingletonClient"]);
        }

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("hosting.json", optional: true)
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .AddCommandLine(args)
                .Build();
            bool hostingInHttpSys = false;
            bool useHttps = false;
            try
            {
                if (config["server"].ToLower() != "httpsys")
                {
                    Console.WriteLine("Host is Kestrel");
                    hostingInHttpSys = false;
                }
                else
                {
                    Console.WriteLine("Host is HttpSys");
                    hostingInHttpSys = true;
                }
            }
            catch //Ignore if the option is not provided.
            {
            }

            try
            {
                if (bool.Parse(config["SecurityOption:EnableHTTPS"]))
                {
                    useHttps = true;
                    Console.WriteLine("Enabled HTTPS");
                }
            }
            catch (Exception) //Ignore if the option is not provided.
            {
            }

            var hostBuilder = new WebHostBuilder();
            if (hostingInHttpSys)
            {
                hostBuilder.UseHttpSys(options =>
                {
                    options.Authentication.AllowAnonymous = true;
                });
            }
            else
            {
                hostBuilder.UseKestrel(options =>
                {
                    // options.ThreadCount = 4;
                    // listenOptions.NoDelay = true;
                    // listenOptions.UseConnectionLogging();
                    if (useHttps)
                    {
                        options.Listen(
                            endPoint: null,
                            configure: listenOptions => listenOptions.UseHttps(_httpsCertFile, _httpsCertPwd));
                    }
                });
            }

            var host = hostBuilder.UseConfiguration(config)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            if (GCSettings.IsServerGC)
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

