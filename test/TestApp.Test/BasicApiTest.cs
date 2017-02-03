// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Benchmarks.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class BasicApiTest
    {
        private static readonly TestServer Server;
        private static readonly HttpClient Client;

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

        static BasicApiTest()
        {
            try
            {
                var builder = new WebHostBuilder();
                builder.UseStartup<BasicApi.Startup>();
                builder.UseProjectOf<BasicApi.Startup>();
                Server = new TestServer(builder);
                Client = Server.CreateClient();
            }
            catch (System.Reflection.TargetInvocationException)
            {
                // ignore platform not supported exception, as we're skipping those platforms
                // silently with OSSkipCondition attributes
            }
        }

        public async Task<string> GetAuthorizationToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/token?username=writer@example.com");
            request.Headers.Add("Cache-Control", new [] {"no-cache"});
            var response = await Client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [OSSkipCondition(OperatingSystems.MacOSX)]
        [OSSkipCondition(OperatingSystems.Linux)]
        [Benchmark(Iterations = 1, WarmupIterations = 0)]
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

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
