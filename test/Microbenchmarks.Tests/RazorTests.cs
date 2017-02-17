// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Benchmarks.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Primitives;
using Xunit;
using SourceLocation = Microsoft.AspNetCore.Razor.SourceLocation;
using TagHelperDescriptor = Microsoft.AspNetCore.Razor.Compilation.TagHelpers.TagHelperDescriptor;
using TagHelperDescriptorResolver = Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperDescriptorResolver;
using TagHelperDirectiveDescriptor = Microsoft.AspNetCore.Razor.Compilation.TagHelpers.TagHelperDirectiveDescriptor;

namespace Microbenchmarks.Tests.Razor
{
    public class RazorTests : BenchmarkTestBase
    {
        [Benchmark]
        [BenchmarkVariation("Runtime", false)]
        [BenchmarkVariation("Design Time", true)]
        public void TagHelperResolution(bool designTime)
        {
            var descriptorResolver = new TagHelperDescriptorResolver(designTime);
            var errorSink = new ErrorSink();
            var addTagHelperDirective = new TagHelperDirectiveDescriptor
            {
                DirectiveText = "*, Microsoft.AspNetCore.Mvc.TagHelpers",
                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                Location = SourceLocation.Zero
            };
            var resolutionContext = new TagHelperDescriptorResolutionContext(
                new[] { addTagHelperDirective },
                errorSink);
            IEnumerable<TagHelperDescriptor> descriptors;

            using (Collector.StartCollection())
            {
                descriptors = descriptorResolver.Resolve(resolutionContext);
            }

            Assert.NotEmpty(descriptors);
            Assert.Empty(errorSink.Errors);
        }

        [Benchmark]
        [BenchmarkVariation("Runtime", false)]
        public void ViewParsing(bool designTime)
        {
            // Arrange
            var services = ConfigureDefaultServices(Directory.GetCurrentDirectory());
            var compilationService = (RazorCompilationService)services.GetRequiredService<IRazorCompilationService>();

            var assembly = typeof(RazorTests).GetTypeInfo().Assembly;
            var assemblyName = assembly.GetName().Name;
            var stream = assembly.GetManifestResourceStream($"{assemblyName}.compiler.resources.RazorTests.TestFile.cshtml");

            var codeDocument = compilationService.CreateCodeDocument("test.cshtml", stream);

            RazorCSharpDocument result;

            // Act
            using (Collector.StartCollection())
            {
                result = compilationService.ProcessCodeDocument(codeDocument);
            }

            // Assert
            Assert.Empty(result.Diagnostics);
        }

        private static IServiceProvider ConfigureDefaultServices(string basePath)
        {
            var services = new ServiceCollection();

            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(PlatformServices.Default.Application);
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName = "Microbenchmarks.Tests",
                WebRootFileProvider = new PhysicalFileProvider(basePath)
            });
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(new PhysicalFileProvider(basePath));
            });
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddLogging();
            services.AddMvc();

            services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());

            return services.BuildServiceProvider();
        }

        private class TestFileProvider : IFileProvider
        {
            public virtual IDirectoryContents GetDirectoryContents(string subpath)
            {
                throw new NotSupportedException();
            }

            public virtual IFileInfo GetFileInfo(string subpath)
            {
                return new NotFoundFileInfo();
            }

            public virtual IChangeToken Watch(string filter)
            {
                return new TestFileChangeToken();
            }

            private class NotFoundFileInfo : IFileInfo
            {
                public bool Exists => false;

                public bool IsDirectory
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public DateTimeOffset LastModified
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public long Length
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public string Name
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public string PhysicalPath
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public Stream CreateReadStream()
                {
                    throw new NotImplementedException();
                }
            }

            private class TestFileChangeToken : IChangeToken
            {
                public bool ActiveChangeCallbacks => false;

                public bool HasChanged { get; set; }

                public IDisposable RegisterChangeCallback(Action<object> callback, object state)
                {
                    return new NullDisposable();
                }

                private class NullDisposable : IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
            }
        }
    }
}
