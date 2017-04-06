// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class BigModelBindingTest : IClassFixture<TestAppFixture<BigModelBinding.Startup>>
    {
        private static readonly HttpContent Content;
        private static readonly byte[] ValidBytes = new UTF8Encoding(false).GetBytes(@"
{
  ""category"" : {
    ""id"" : 2,
    ""name"" : ""Cats""
  },
  ""name"" : ""fluffy"",
  ""status"" : ""available""
}");
        private readonly HttpClient _client;

        static BigModelBindingTest()
        {
            var inputFile = Path.Combine(HostingStartup.GetProjectDirectoryOf<BigModelBinding.Startup>(), "postdata.txt");
            var input = File.ReadAllText(inputFile);
            Content = new StringContent(input, Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        public BigModelBindingTest(TestAppFixture<BigModelBinding.Startup> fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task BigModelBinding()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = Content
            };

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
