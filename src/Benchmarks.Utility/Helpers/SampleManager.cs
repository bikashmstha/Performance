// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.PlatformAbstractions;

namespace Benchmarks.Utility.Helpers
{
    public class SampleManager
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

        private SampleEntry GetOrAdd<T>(string name, Func<string, T> factory) where T : SampleEntry
        {
            List<SampleEntry> samples;
            if (!_samples.TryGetValue(typeof(T), out samples))
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

            private static string GetPathToNugetConfig()
            {
                // This is a non-exhaustive search for the directory where NuGet.config resides.
                // Typically, it'll be found at ..\..\NuGet.config, but it may vary depending on execution preferences.
                const string nugetConfigFileName = "NuGet.config";
                const int maxRelativeFolderTraversalDepth = 10; // how many ".." will we attempt adding looking for NuGet.config?
                var appbase = PlatformServices.Default.Application.ApplicationBasePath;
                var relativePath = nugetConfigFileName;
                for (int i = 1; i < maxRelativeFolderTraversalDepth; i++)
                {
                    var currentTry = Path.GetFullPath(Path.Combine(appbase, relativePath));
                    if (File.Exists(currentTry))
                    {
                        return Path.GetDirectoryName(currentTry);
                    }
                    relativePath = Path.Combine("..", relativePath);
                }
                throw new Exception($"Cannot determine the location of '{nugetConfigFileName}' from base path '{PlatformServices.Default.Application.ApplicationBasePath}'");
            }

            private RestoredSample(string name) : base(name) { }

            public static RestoredSample Create(string name) => new RestoredSample(name);

            protected override bool doInitialization()
            {
                SourcePath = PathHelper.GetTestAppFolder(Name);
                if (SourcePath == null)
                {
                    return false;
                }

                var tempFolder = PathHelper.GetNewTempFolder();
                Directory.CreateDirectory(tempFolder); // workaround for Linux
                var target = Path.Combine(tempFolder, Name);
                Directory.CreateDirectory(target);

                string copyCommand, copySampleParameters, copyNugetConfigParameters, copyGlobalJsonParameters;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    copyCommand = "robocopy";
                    copySampleParameters = $"\"{SourcePath}\" \"{target}\" /E /S /XD node_modules /XF project.lock.json";
                    copyNugetConfigParameters = $"\"{_pathToNugetConfig}\" \"{target}\" NuGet.config";
                    copyGlobalJsonParameters = $"\"{_pathToNugetConfig}\" \"{target}\" global.json";
                }
                else
                {
                    copyCommand = "rsync";
                    copySampleParameters = $"--recursive --exclude=node_modules --exclude=project.lock.json \"{SourcePath}/\" \"{target}/\"";
                    copyNugetConfigParameters = $"\"{_pathToNugetConfig}/NuGet.config\" \"{target}/NuGet.config\"";
                    copyGlobalJsonParameters = $"\"{_pathToNugetConfig}/global.json\" \"{target}/global.json\"";
                }
                var runner = new CommandLineRunner(copyCommand);
                runner.Execute(copySampleParameters);
                runner.Execute(copyNugetConfigParameters);
                runner.Execute(copyGlobalJsonParameters);
                if (!DotnetHelper.GetDefaultInstance().Restore(target, quiet: true))
                {
                    Directory.Delete(target, recursive: true);
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

                var target = Path.Combine(PathHelper.GetNewTempFolder(), parts[0]);
                Directory.CreateDirectory(target);

                if (!dotnet.Publish(SourcePath, target, parts[1]))
                {
                    Directory.Delete(target, recursive: true);
                    return false;
                }

                SamplePath = target;
                return true;
            }
        }
    }
}
