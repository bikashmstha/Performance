// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Extensions.PlatformAbstractions;

namespace Benchmarks.Utility.Helpers
{
    public class PathHelper
    {
        private static readonly string TestAppFolder = "testapp";
        private static readonly string RootFileName = "build.sh";

        public static string GetRootFolder(string projectFolder)
        {
            var folder = new DirectoryInfo(projectFolder);
            while (folder.Parent != null)
            {
                var rootFilePath = Path.Combine(folder.FullName, RootFileName);
                if (File.Exists(rootFilePath))
                {
                    return folder.FullName;
                }

                folder = folder.Parent;
            }

            // If we don't find any files then make the project folder the root
            return projectFolder;
        }

        public static string GetNuGetConfig()
        {
            var rootFolder = GetRootFolder(PlatformServices.Default.Application.ApplicationBasePath);

            return Path.Combine(rootFolder, "NuGet.config");
        }

        public static string GetTestAppFolder(string sampleName)
        {
            var rootFolder = GetRootFolder(PlatformServices.Default.Application.ApplicationBasePath);
            var sampleFolder = Path.Combine(rootFolder, TestAppFolder, sampleName);

            if (Directory.Exists(sampleFolder))
            {
                return sampleFolder;
            }
            else
            {
                return null;
            }
        }

        public static string GetNewTempFolder()
        {
            var result = Path.GetTempFileName();
            File.Delete(result);
            Directory.CreateDirectory(result);

            return result;
        }
    }
}