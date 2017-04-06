// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class LocalizedViewsTest : IClassFixture<TestAppFixture<LocalizedViews.Startup>>
    {
        private static readonly byte[] ValidBytes = new UTF8Encoding(false).GetBytes("name=Joey&age=15&birthdate=9-9-1985");
        private readonly HttpClient _client;

        public LocalizedViewsTest(TestAppFixture<LocalizedViews.Startup> fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task LocalizedViews_HtmlHelpers()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = new ByteArrayContent(ValidBytes),
            };

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LocalizedViews_TagHelpers()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/TagHelpers")
            {
                Content = new ByteArrayContent(ValidBytes),
            };

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
