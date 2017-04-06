// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class StarterMvcTest : IClassFixture<TestAppFixture<StarterMvc.Startup>>
    {
        private readonly HttpClient _client;

        public StarterMvcTest(TestAppFixture<StarterMvc.Startup> fixture)
        {
            _client = fixture.Client;
        }

        [Theory]
        [InlineData("")]
        [InlineData("/Home/About")]
        [InlineData("/Home/Contact")]
        public async Task OtherPages_ViewsAreSuccessful(string requestPath)
        {
            // Arrange & Act
            var response = await _client.GetAsync(requestPath);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
