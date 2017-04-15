// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;

namespace Benchmarks.Utility.Helpers
{
    /// <summary>
    /// Test manager helps test cases to deploy a web project to IIS Express.
    /// </summary>
    public class IISTestManager : IDisposable
    {
        private bool _initalized = false;
        private readonly SampleManager _sampleManager = new SampleManager();
        private readonly List<IDisposable> _deployer = new List<IDisposable>();
        private readonly Dictionary<Tuple<string, RuntimeFlavor>, DeploymentResult> _deployments
            = new Dictionary<Tuple<string, RuntimeFlavor>, DeploymentResult>();

        public void Initialize(ILoggerFactory loggerFactory)
        {
            if (_initalized)
            {
                return;
            }

            var sampleList = new Tuple<string, RuntimeFlavor>[]
            {
                Tuple.Create("StarterMvc", RuntimeFlavor.Clr),
                Tuple.Create("StarterMvc", RuntimeFlavor.CoreClr)
            };

            foreach (var sample in sampleList)
            {
                var source = _sampleManager.GetRestoredSample(sample.Item1);
                var parameters = new DeploymentParameters(source, ServerType.IISExpress, sample.Item2, RuntimeArchitecture.x64)
                {
                    TargetFramework = Runtimes.GetFrameworkName(sample.Item2),
                };

                // This is a quick fix to turn around the build before the fix in Hosting eventually goes online
                parameters.ApplicationBaseUriHint = "http://localhost:0";

                var deployer = ApplicationDeployerFactory.Create(parameters, loggerFactory);

                var result = deployer.DeployAsync().Result;
                _deployments[sample] = result;
                _deployer.Add(deployer);
            }

            _initalized = true;
        }

        public void Dispose()
        {
            _deployer.ForEach(d => d.Dispose());
        }

        public string GetSite(string sampleName, RuntimeFlavor runtimeFlavor, bool restart)
        {
            // restart site is not implemented
            if (_deployments.TryGetValue(Tuple.Create(sampleName, runtimeFlavor), out var deployment))
            {
                return deployment.ApplicationBaseUri;
            }

            return null;
        }
    }
}
