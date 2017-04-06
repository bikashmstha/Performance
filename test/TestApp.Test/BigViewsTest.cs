// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class BigViewsTest : IClassFixture<TestAppFixture<BigViews.Startup>>
    {
        private readonly HttpClient _client;

        public BigViewsTest(TestAppFixture<BigViews.Startup> fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task BigViews_HtmlHelpers()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Home/Index");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task BigViews_TagHelpers()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Home/IndexWithTagHelpers");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task BigViews_TagHelpers_StaticOptions()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Home/IndexWithStaticOptions");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
