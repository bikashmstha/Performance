// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Benchmarks.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;

namespace Microbenchmarks.Tests.Razor
{
    public class RazorTests : BenchmarkTestBase
    {
        [Benchmark]
        [BenchmarkVariation("Runtime", false)]
        [BenchmarkVariation("Design Time", true)]
        public void TagHelperResolution(bool designTime)
        {
            // Arrange
            var compilation = CreateCompilation();

            // Act
            IEnumerable<TagHelperDescriptor> descriptors;
            using (Collector.StartCollection())
            {
                descriptors = TagHelpers.GetTagHelpers(compilation);
            }

            // Assert
            Assert.NotEmpty(descriptors);
        }

        [Benchmark]
        [BenchmarkVariation("Runtime", false)]
        public void ViewParsing(bool designTime)
        {
            // Arrange
            var assembly = typeof(RazorTests).GetTypeInfo().Assembly;
            var services = ConfigureDefaultServices(assembly);
            var razorEngine = services.GetRequiredService<RazorEngine>();
            var razorProject = services.GetRequiredService<RazorProject>();
            var templateEngine = new MvcRazorTemplateEngine(razorEngine, razorProject)
            {
                Options =
                {
                    ImportsFileName = "_ViewImports.cshtml",
                },
            };

            var codeDocument = templateEngine.CreateCodeDocument("/compiler/resources/RazorTests.TestFile.cshtml");

            // Act
            RazorCSharpDocument result;
            using (Collector.StartCollection())
            {
                result = templateEngine.GenerateCode(codeDocument);
            }

            // Assert
            Assert.Empty(result.Diagnostics);
        }

        private static Compilation CreateCompilation()
        {
            var currentAssembly = typeof(RazorTests).GetTypeInfo().Assembly;
            var dependencyContext = DependencyContext.Load(currentAssembly);

            // Performance is expected to decrease as MVC or this project add tag helpers or dependencies
            // containing tag helpers. Would narrow graph to Microsoft.AspNet.Mvc.TagHelpers and its
            // dependencies if that were easier.
            var references = dependencyContext.CompileLibraries
                .SelectMany(library => library.ResolveReferencePaths())
                .Select(referencePath => MetadataReference.CreateFromFile(referencePath));

            return CSharpCompilation.Create("TestAssembly", references: references);
        }

        private static IServiceProvider ConfigureDefaultServices(Assembly assembly)
        {
            var services = new ServiceCollection();

            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(applicationEnvironment);

            var assemblyName = assembly.GetName().Name;
            var fileProvider = new EmbeddedFileProvider(assembly, assemblyName);
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName = "Microbenchmarks.Tests",
                WebRootFileProvider = fileProvider,
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(fileProvider);
            });

            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddLogging();
            services.AddMvc();
            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());

            return services.BuildServiceProvider();
        }
    }
}
