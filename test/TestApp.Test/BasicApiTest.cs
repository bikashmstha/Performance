// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class BasicApiTest : IClassFixture<TestAppFixture<BasicApi.Startup>>
    {
        private static readonly byte[] ValidBytes = new UTF8Encoding(false).GetBytes(@"
{
  ""category"" : {
    ""name"" : ""Cats""
  },
  ""images"": [
    {
        ""url"": ""http://example.com/images/fluffy1.png""
    },
    {
        ""url"": ""http://example.com/images/fluffy2.png""
    },
  ],
  ""tags"": [
    {
        ""name"": ""orange""
    },
    {
        ""name"": ""kitty""
    }
  ],
  ""age"": 2,
  ""hasVaccinations"": ""true"",
  ""name"" : ""fluffy"",
  ""status"" : ""available""
}");
        private readonly HttpClient _client;

        public BasicApiTest(TestAppFixture<BasicApi.Startup> fixture)
        {
            _client = fixture.Client;
        }

        public async Task<string> GetAuthorizationToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/token?username=writer@example.com");
            request.Headers.Add("Cache-Control", new [] {"no-cache"});

            var response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Linux, SkipReason = "No LocalDB on Linux.")]
        [OSSkipCondition(OperatingSystems.MacOSX, SkipReason = "No LocalDB on OSX.")]
        public async Task BasicApi()
        {
            var authToken = await GetAuthorizationToken();
            var request = new HttpRequestMessage(HttpMethod.Post, "/pet");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", .9));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", .6));
            request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            request.Headers.Add("Authorization", new [] {"Bearer " + authToken});

            request.Content = new ByteArrayContent(ValidBytes);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Linux, SkipReason = "No LocalDB on Linux.")]
        [OSSkipCondition(OperatingSystems.MacOSX, SkipReason = "No LocalDB on OSX.")]
        public async Task BasicApi_GetsJson()
        {
            var authToken = await GetAuthorizationToken();

            // Get a lion.
            var request = new HttpRequestMessage(HttpMethod.Get, "/pet/findByCategory/4");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", .9));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", .6));
            request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            request.Headers.Add("Authorization", new[] { "Bearer " + authToken });

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.NotNull(response.Content.Headers.ContentLength);
            Assert.NotEqual(0, response.Content.Headers.ContentLength);
        }
    }
}
