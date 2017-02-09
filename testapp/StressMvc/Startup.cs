// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
                builder.AddUserSecrets<Startup>();
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

            TextContentRelayController.UseSingletonClient = Boolean.Parse(Configuration["TextContentRelayControllerOptions:UseSingletonClient"]);
        }

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            // Use Kestrel if config key not provided.
            var hostingInHttpSys = false;
            if (config["server"]?.ToLower() == "httpsys")
            {
                hostingInHttpSys = true;
            }

            var hostBuilder = new WebHostBuilder();
            if (hostingInHttpSys)
            {
                Console.WriteLine("Host is HttpSys");
                hostBuilder.UseHttpSys();
            }
            else
            {
                var urls = config["urls"];
                Console.WriteLine("Host is Kestrel");

                var useHttps = bool.Parse(config["SecurityOption:EnableHTTPS"]);
                hostBuilder.UseKestrel(options =>
                {
                    // options.ThreadCount = 4;
                    // listenOptions.NoDelay = true;
                    // listenOptions.UseConnectionLogging();
                    if (string.IsNullOrEmpty(urls))
                    {
                        options.Listen(
                            IPAddress.Loopback,
                            5000,
                            listenOptions =>
                            {
                                if (useHttps)
                                {
                                    Console.WriteLine("Enabled HTTPS");
                                    listenOptions.UseHttps(_httpsCertFile, _httpsCertPwd);
                                }
                            });
                    }
                    else
                    {
                        foreach (var url in urls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var endPoint = CreateIPEndPoint(url);
                            options.Listen(
                                endPoint,
                                listenOptions =>
                                {
                                    if (url.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        listenOptions.UseHttps(_httpsCertFile, _httpsCertPwd);
                                    }
                                });
                        }
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

        private static IPEndPoint CreateIPEndPoint(string url)
        {
            var uri = new Uri(url);

            IPAddress ip;
            if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                ip = IPAddress.Loopback;
            }
            else if (!IPAddress.TryParse(uri.Host, out ip))
            {
                ip = IPAddress.IPv6Any;
            }

            return new IPEndPoint(ip, uri.Port);
        }
    }
}

