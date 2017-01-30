// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class LocalizedViewsTest
    {
        private static readonly TestServer Server;
        private static readonly HttpClient Client;

        private static readonly byte[] ValidBytes = new UTF8Encoding(false).GetBytes("name=Joey&age=15&birthdate=9-9-1985");

        static LocalizedViewsTest()
        {
            var builder = new WebHostBuilder();
            Server = new TestServer(builder);
            Client = Server.CreateClient();
        }

        [Fact(Skip = "https://github.com/dotnet/core-setup/issues/1221")]
        public async Task LocalizedViews_HtmlHelpers()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/");
            request.Content = new ByteArrayContent(ValidBytes);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(Skip = "https://github.com/dotnet/core-setup/issues/1221")]
        public async Task LocalizedViews_TagHelpers()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/TagHelpers");
            request.Content = new ByteArrayContent(ValidBytes);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
