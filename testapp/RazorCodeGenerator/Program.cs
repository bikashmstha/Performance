// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.PlatformAbstractions;

namespace RazorCodeGenerator
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var application = new CommandLineApplication(throwOnUnexpectedArg: true)
            {
                Name = "RazorCodeGenerator",
                FullName = "Razor Code Generation Perf Testing Tool",
                Description = "Supports focused testing of Razor code generation and compilation",
            };

            application.HelpOption("-?|-h|--help");
            var basePathOption = application.Option("-b|--base-path", "base path for resolving views", CommandOptionType.SingleValue);
            var coldOption = application.Option("-c|--cold", "skip warmup - include warmup in timings data", CommandOptionType.NoValue);
            var dumpOption = application.Option("-d|--dump", "output generated code files", CommandOptionType.NoValue);
            var iterationsOption = application.Option("-i|--iterations", "number of iterations", CommandOptionType.SingleValue);
            var nonInteractiveOption = application.Option("-n|--non-interactive", "run code generation without prompting", CommandOptionType.NoValue);
            var viewEngineOption = application.Option("-v|--view-engine", "use view engine (compile and load)", CommandOptionType.NoValue);
            var sourcesArgument = application.Argument("sources", "source .cshtml files", multipleValues: true);

            application.OnExecute(() =>
            {

                string basePath;
                if (basePathOption.HasValue())
                {
                    basePath = Path.GetFullPath(basePathOption.Value());
                    if (!Directory.Exists(basePath))
                    {
                        application.ShowHelp();
                        Console.WriteLine(" ");
                        Console.WriteLine($"Error: Could not find directory {basePath}");

                        return -10;
                    }
                }
                else
                {
                    basePath = Directory.GetCurrentDirectory();
                }

                var cold = coldOption.HasValue();
                var dump = dumpOption.HasValue();

                int iterations;
                if (iterationsOption.HasValue())
                {
                    if (!int.TryParse(iterationsOption.Value(), out iterations))
                    {
                        application.ShowHelp();
                        Console.WriteLine(" ");
                        Console.WriteLine($"Error: Could not parse {iterations}");

                        return -20;
                    }
                }
                else
                {
                    iterations = 100;
                }

                var nonInteractive = nonInteractiveOption.HasValue();

                var sources = new List<string>();
                if (sourcesArgument.Values.Count == 0)
                {
                    application.ShowHelp();
                    Console.WriteLine(" ");
                    Console.WriteLine($"Error: No sources provided");

                    return -30;
                }

                if (sourcesArgument.Values.Count == 1 && sourcesArgument.Values[0] == "*")
                {
                    sources.AddRange(Directory.EnumerateFiles(basePath, "*.cshtml", SearchOption.AllDirectories));
                    if (sources.Count == 0)
                    {
                        Console.WriteLine(" ");
                        Console.WriteLine($"Error: No sources found");

                        return -35;
                    }
                }
                else
                {
                    foreach (var sourceValue in sourcesArgument.Values)
                    {
                        var source = Path.GetFullPath(sourceValue);
                        if (!File.Exists(source))
                        {
                            application.ShowHelp();
                            Console.WriteLine(" ");
                            Console.WriteLine($"Error: Could not find file {source}");

                            return -40;
                        }

                        sources.Add(source);
                    }
                }

                var useViewEngine = viewEngineOption.HasValue();

                return new Program(basePath).Run(sources, useViewEngine, iterations, cold, dump, nonInteractive);
            });

            return application.Execute(args);
        }

        public Program(string basePath)
        {
            BasePath = basePath;

            var services = ConfigureDefaultServices(basePath);
            ViewEngine = services.GetRequiredService<IRazorViewEngine>();
            var host = (MvcRazorHost)services.GetRequiredService<IMvcRazorHost>();
            TemplateEngine = new RazorTemplateEngine(host);
        }

        private string BasePath { get; }

        private RazorTemplateEngine TemplateEngine { get; }

        private IRazorViewEngine ViewEngine { get; }

        public int Run(List<string> sources, bool useViewEngine, int iterations, bool cold, bool dump, bool nonInteractive)
        {
            Console.WriteLine($"Generating code for {string.Join(", ", sources)} x{iterations}");

            if (!cold)
            {
                Console.WriteLine("Doing warmup.");
                if (!GenerateCode(sources, useViewEngine, iterations: 1))
                {
                    return -1;
                }
            }

            if (!nonInteractive)
            {
                Console.WriteLine("Press the ENTRY key to start.");
                Console.ReadLine();
            }

            var timer = Stopwatch.StartNew();
            Console.WriteLine();
            Console.WriteLine("Starting...");

            if (!GenerateCode(sources, useViewEngine, iterations))
            {
                return -1;
            }

            Console.WriteLine($"Completed after {timer.Elapsed}");
            Console.WriteLine();
            Console.WriteLine();

            if (!nonInteractive)
            {
                Console.WriteLine("Press the ENTRY key to exit.");
                Console.ReadLine();
            }

            if (dump)
            {
                GenerateCode(sources, useViewEngine: false, iterations: 1, dump: true);
            }

            return 0;
        }

        private bool GenerateCode(IList<string> sources, bool useViewEngine, int iterations, bool dump = false)
        {
            if (useViewEngine)
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    var source = sources[i];

                    var relativePath = source.Substring(BasePath.Length).Replace('\\', '/');

                    Console.WriteLine($"Creating view {relativePath}");

                    for (var j = 0; j < iterations; j++)
                    {
                        var view = ViewEngine.GetView(null, relativePath, isMainPage: true);
                        view.EnsureSuccessful(new string[0]);
                        GC.KeepAlive(view.View);

                        if (j > 0 && j % 10 == 0)
                        {
                            Console.WriteLine($"Completed iteration {j}");
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    var source = sources[i];

                    var fileNameNoExtension = Path.GetFileNameWithoutExtension(source);

                    Console.WriteLine($"Generating {source}");

                    using (var stream = new FileStream(source, FileMode.Open))
                    {
                        for (var j = 0; j < iterations; j++)
                        {
                            var result = TemplateEngine.GenerateCode(
                                stream,
                                className: fileNameNoExtension,
                                rootNamespace: ManglePath(BasePath, Path.GetDirectoryName(source)),
                                sourceFileName: source.Substring(BasePath.Length));

                            if (!result.Success)
                            {
                                Console.WriteLine($"Code generation failed for {source}");
                                foreach (var error in result.ParserErrors)
                                {
                                    Console.WriteLine("\t" + error);
                                }

                                return false;
                            }

                            if (j > 0 && j % 10 == 0)
                            {
                                Console.WriteLine($"Completed iteration {j}");
                            }

                            if (dump && j == iterations - 1)
                            {
                                var output = Path.ChangeExtension(source, ".cs");
                                Console.WriteLine($"Dumping generated code to {output}");
                                File.WriteAllText(output, result.GeneratedCode);
                            }
                        }

                        stream.Seek(0L, SeekOrigin.Begin);
                    }
                }
            }

            return true;
        }

        private static IServiceProvider ConfigureDefaultServices(string basePath)
        {
            var services = new ServiceCollection();

            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(PlatformServices.Default.Application);
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName = "RazorCodeGenerator",
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

        private static string ManglePath(string basePath, string path)
        {
            if (!path.StartsWith(basePath))
            {
                return "Test";
            }

            var projectName = Path.GetFileName(basePath);

            var @namespace = new StringBuilder(projectName);

            var tokens = path.Substring(basePath.Length).Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                @namespace.Append('.');
                @namespace.Append(token);
            }

            return @namespace.ToString();
        }
    }
}