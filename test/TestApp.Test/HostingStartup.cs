// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace MvcBenchmarks.InMemory
{
    public static class HostingStartup
    {
        private const string TestAppFolderName = "testapp";
        private const int MaxRelativeFolderTraversalDepth = 10; // how many ".." will we attempt adding looking for the TestAppFolderName folder?
        private static string _testAppRelativeFolder;

        public static string GetProjectDirectoryOf<TStartup>()
        {
            return GetProjectDirectoryOf(typeof(TStartup).GetTypeInfo().Assembly);
        }

        public static string GetProjectDirectoryOf(Assembly assembly)
        {
            var applicationName = assembly.GetName().Name;
            return Path.GetFullPath(Path.Combine(GetTestAppRelativeFolder(), applicationName));
        }

        private static string GetTestAppRelativeFolder()
        {
            if (_testAppRelativeFolder == null)
            {
                var appbase = AppContext.BaseDirectory;
                var relativePath = TestAppFolderName;
                for (int i = 1; i < MaxRelativeFolderTraversalDepth; i++)
                {
                    relativePath = Path.Combine("..", relativePath);
                    var currentTry = Path.GetFullPath(Path.Combine(appbase, relativePath));
                    if (Directory.Exists(currentTry))
                    {
                        _testAppRelativeFolder = currentTry;
                        break;
                    }
                }

                if (_testAppRelativeFolder == null)
                {
                    throw new Exception($"Cannot determine the location of the '{TestAppFolderName}' folder");
                }
            }

            return _testAppRelativeFolder;
        }

        public static WebHostBuilder UseProjectOf<TStartup>(this WebHostBuilder builder)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;
            var webRoot = GetProjectDirectoryOf(startupAssembly);

            builder.ConfigureServices(services =>
            {
                var hostingEnvironment = new HostingEnvironment();
                hostingEnvironment.Initialize(
                    applicationName,
                    webRoot,
                    new WebHostOptions
                    {
                        Environment = "Production"
                    });
                services.AddSingleton<IHostingEnvironment>(hostingEnvironment);

                var manager = new ApplicationPartManager();
                manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));
                services.AddSingleton(manager);
            });

            return builder;
        }
    }
}
