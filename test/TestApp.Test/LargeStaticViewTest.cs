// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class LargeStaticViewTest : IClassFixture<TestAppFixture<LargeStaticView.Startup>>
    {
        private readonly HttpClient _client;

        public LargeStaticViewTest(TestAppFixture<LargeStaticView.Startup> fixture)
        {
            _client = fixture.Client;
        }

        [Theory]
        [InlineData("")]
        [InlineData("/TagHelpers")]
        public async Task StaticView_ViewsAreSuccessful(string requestPath)
        {
            // Arrange & Act
            var response = await _client.GetAsync(requestPath);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
