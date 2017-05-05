// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Benchmarks.Utility.Helpers
{
    public class SampleManager : IDisposable
    {
        private readonly Dictionary<Type, List<SampleEntry>> _samples = new Dictionary<Type, List<SampleEntry>>();

        public string GetRestoredSample(string name)
        {
            var sample = GetOrAdd(name, RestoredSample.Create);
            sample.Initialize();
            return sample.Valid ? sample.SamplePath : null;
        }

        public string GetDotNetPublishedSample(string name, string framework)
        {
            var sample = GetOrAdd(DotNetPublishedSample.GetUniqueName(name, framework), DotNetPublishedSample.Create);
            sample.Initialize();
            return sample.Valid ? sample.SamplePath : null;
        }

        public void Dispose()
        {
            foreach (var list in _samples.Values)
            {
                foreach (var sample in list)
                {
                    if (sample.TemporaryPath != null && Directory.Exists(sample.TemporaryPath))
                    {
                        try
                        {
                            Directory.Delete(sample.TemporaryPath, recursive: true);
                        }
                        catch (IOException)
                        {
                            // Ignore cleanup errors.
                        }
                    }
                }
            }
        }

        private SampleEntry GetOrAdd<T>(string name, Func<string, T> factory) where T : SampleEntry
        {
            if (!_samples.TryGetValue(typeof(T), out var samples))
            {
                samples = new List<SampleEntry>();
                _samples[typeof(T)] = samples;
            }

            var sample = samples.FirstOrDefault(entry => string.Equals(name, entry.Name, StringComparison.OrdinalIgnoreCase));
            if (sample == null)
            {
                sample = factory(name);
                samples.Add(sample);
            }

            return sample;
        }

        private abstract class SampleEntry
        {
            private bool _initialized;

            public SampleEntry(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public string SourcePath { get; protected set; }

            public string SamplePath { get; protected set; }

            // Temporary folder containing SamplePath.
            public string TemporaryPath { get; protected set; }

            public bool Valid => SamplePath != null && Directory.Exists(SamplePath);

            public void Initialize()
            {
                if (!_initialized)
                {
                    _initialized = doInitialization();
                }
            }

            protected abstract bool doInitialization();
        }

        private class RestoredSample : SampleEntry
        {
            private static readonly string _pathToNugetConfig = GetPathToNugetConfig();
            private static readonly string _rootFolder =
                PathHelper.GetRootFolder(AppContext.BaseDirectory);
            private static readonly string _buildFolder = Path.Combine(_rootFolder, "build");

            private static string GetPathToNugetConfig()
            {
                // This is a non-exhaustive search for the directory where NuGet.config resides.
                // Typically, it'll be found at ..\..\NuGet.config, but it may vary depending on execution preferences.
                const string nugetConfigFileName = "NuGet.config";
                const int maxRelativeFolderTraversalDepth = 10; // how many ".." will we attempt adding looking for NuGet.config?
                var appbase = AppContext.BaseDirectory;
                var relativePath = nugetConfigFileName;
                for (var i = 1; i < maxRelativeFolderTraversalDepth; i++)
                {
                    var currentTry = Path.GetFullPath(Path.Combine(appbase, relativePath));
                    if (File.Exists(currentTry))
                    {
                        return Path.GetDirectoryName(currentTry);
                    }
                    relativePath = Path.Combine("..", relativePath);
                }

                throw new Exception($"Cannot determine the location of '{nugetConfigFileName}' from base path '{AppContext.BaseDirectory}'");
            }

            private RestoredSample(string name)
                : base(name)
            {
            }

            public static RestoredSample Create(string name) => new RestoredSample(name);

            protected override bool doInitialization()
            {
                SourcePath = PathHelper.GetTestAppFolder(Name);
                if (SourcePath == null)
                {
                    return false;
                }

                TemporaryPath = PathHelper.GetNewTempFolder();
                Directory.CreateDirectory(TemporaryPath); // workaround for Linux
                var target = Path.Combine(TemporaryPath, "app", Name);
                Directory.CreateDirectory(target);
                var buildTarget = Path.Combine(TemporaryPath, "build");

                string copyCommand, copyBuildParameters, copyNugetConfigParameters, copyPropsParameters, copySampleParameters;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    copyCommand = "robocopy";
                    copyBuildParameters = $"\"{_buildFolder}\" \"{buildTarget}\" /S /NC /NP /NJS /NS";
                    copyNugetConfigParameters = $"\"{_pathToNugetConfig}\" \"{TemporaryPath}\" NuGet.config /NC /NP /NJS /NS";
                    copyPropsParameters = $"\"{_rootFolder}\" \"{TemporaryPath}\" *.json *.props *.targets /NC /NP /NJS /NS";
                    copySampleParameters = $"\"{SourcePath}\" \"{target}\" /S /XD bin node_modules obj /NC /NP /NJS /NS";
                }
                else
                {
                    copyCommand = "rsync";
                    copyBuildParameters = $"\"{_buildFolder}/\"*.* \"{buildTarget}\"";
                    copyNugetConfigParameters = $"\"{_pathToNugetConfig}/NuGet.config\" \"{TemporaryPath}/NuGet.config\"";
                    copyPropsParameters = "--include=*.json --include=*.props --include=*.targets --exclude=*.* " +
                        $"\"{_rootFolder}/\"*.* \"{TemporaryPath}\"";
                    copySampleParameters = "--recursive --exclude=bin/ --exclude=node_modules/ --exclude=obj/ " +
                        $"\"{SourcePath}/\" \"{target}/\"";
                }

                var runner = new CommandLineRunner(copyCommand);
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Let the shell expand the wildcards for the content of the build folder as well as the props and
                    // targets in the root folder.
                    runner.UseShellExecute = true;
                }

                runner.Execute(copyBuildParameters);
                runner.Execute(copyNugetConfigParameters);
                runner.Execute(copyPropsParameters);
                runner.Execute(copySampleParameters);
                if (!DotnetHelper.GetDefaultInstance().Restore(target, quiet: true))
                {
                    try
                    {
                        Directory.Delete(target, recursive: true);
                    }
                    catch (IOException)
                    {
                    }

                    return false;
                }

                SamplePath = target;

                return true;
            }
        }

        private class DotNetPublishedSample : SampleEntry
        {
            private const char _separator = '|';

            private DotNetPublishedSample(string name) : base(name) { }

            public static DotNetPublishedSample Create(string name) => new DotNetPublishedSample(name);

            public static string GetUniqueName(string sampleName, string framework) => $"{sampleName}{_separator}{framework}";

            protected override bool doInitialization()
            {
                var parts = Name.Split(_separator);
                SourcePath = PathHelper.GetTestAppFolder(parts[0]);
                if (SourcePath == null)
                {
                    return false;
                }

                var dotnet = DotnetHelper.GetDefaultInstance();
                if (!dotnet.Restore(SourcePath, quiet: true))
                {
                    return false;
                }

                TemporaryPath = PathHelper.GetNewTempFolder();
                var target = Path.Combine(TemporaryPath, parts[0]);
                Directory.CreateDirectory(target);

                if (!dotnet.Publish(SourcePath, target, parts[1]))
                {
                    try
                    {
                        Directory.Delete(target, recursive: true);
                    }
                    catch (IOException)
                    {
                    }

                    return false;
                }

                SamplePath = target;

                return true;
            }
        }
    }
}
