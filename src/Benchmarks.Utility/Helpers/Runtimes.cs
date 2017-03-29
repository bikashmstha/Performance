// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Xunit;

namespace Benchmarks.Utility.Helpers
{
    public static class Runtimes
    {
        public static RuntimeFlavor GetRuntimeFlavor(string runtimeType)
        {
            if (string.Equals(runtimeType, "CLR", StringComparison.OrdinalIgnoreCase))
            {
                return RuntimeFlavor.Clr;
            }

            if (string.Equals(runtimeType, "CoreCLR", StringComparison.OrdinalIgnoreCase))
            {
                return RuntimeFlavor.CoreClr;
            }

            // Remaining possibility is Mono and that's not currently supported.
            Assert.False(true, $"Unknown framework {runtimeType}");

            return RuntimeFlavor.Clr;
        }

        public static string GetFrameworkName(string runtimeType)
        {
            if (string.Equals(runtimeType, "CLR", StringComparison.OrdinalIgnoreCase))
            {
                return "net46";
            }

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
                case RuntimeFlavor.Clr:
                    return "net46";

                case RuntimeFlavor.CoreClr:
                    return "netcoreapp2.0";

                default:
                    Assert.False(true, $"Unknown runtime flavor {runtimeFlavor}");
                    return null;
            }
        }
    }
}
