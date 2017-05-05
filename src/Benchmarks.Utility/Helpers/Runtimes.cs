// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit;

namespace Benchmarks.Utility.Helpers
{
    public static class Runtimes
    {
        public static string GetFrameworkName(string runtimeType)
        {
            if (string.Equals(runtimeType, "CoreCLR", StringComparison.OrdinalIgnoreCase))
            {
                return "netcoreapp2.0";
            }

            Assert.False(true, $"Unknown framework {runtimeType}");

            return null;
        }

        public static string GetFrameworkName(RuntimeFlavor runtimeFlavor)
        {
            switch (runtimeFlavor)
            {
                case RuntimeFlavor.CoreClr:
                    return "netcoreapp2.0";

                default:
                    Assert.False(true, $"Unknown runtime flavor {runtimeFlavor}");
                    return null;
            }
        }
    }
}
