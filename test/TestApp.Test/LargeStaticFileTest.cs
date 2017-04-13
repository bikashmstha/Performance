// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class LargeStaticFileTest : IClassFixture<TestAppFixture<LargeStaticFile.Startup>>
    {
        private readonly HttpClient _client;

        public LargeStaticFileTest(TestAppFixture<LargeStaticFile.Startup> fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task StaticFile_DownloadIsSuccessful()
        {
            // Arrange & Act
            var response = await _client.GetAsync("Large.html");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
