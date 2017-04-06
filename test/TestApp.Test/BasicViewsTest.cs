// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace MvcBenchmarks.InMemory
{
    public class BasicViewsTest : IClassFixture<TestAppFixture<BasicViews.Startup>>
    {
        private readonly HttpClient _client;

        public BasicViewsTest(TestAppFixture<BasicViews.Startup> fixture)
        {
            _client = fixture.Client;
        }

        private async Task<string[]> GetAntiforgeryToken(string requestUri)
        {
            var result = new string[]
            {
                string.Empty, string.Empty
            };

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await _client.SendAsync(request);
            foreach (var item in response.Headers.GetValues("Set-Cookie"))
            {
                result[0] = item.Substring(0, item.IndexOf(';'));
                break;
            }
            var content = await response.Content.ReadAsStringAsync();
            var reader = new StringReader(content);
            var line = reader.ReadLine()?.TrimStart();
            while (line != null)
            {
                if (line.StartsWith(@"<input name=""__RequestVerificationToken"))
                {
                    var start = line.IndexOf(@"value=""");
                    if(start == -1) continue;
                    start += @"value=""".Length;
                    var end = line.LastIndexOf(@"""");
                    result[1] = line.Substring(start, end - start);
                    break;
                }
                line = reader.ReadLine()?.TrimStart();
            }

            return result;
        }

        private byte[] GetValidBytes(string antiforgeryToken = null)
        {
            var message = "name=Joey&age=15&birthdate=9-9-1985";
            if (!string.IsNullOrEmpty(antiforgeryToken))
            {
                message += "&__RequestVerificationToken=" + antiforgeryToken;
            }
            return new UTF8Encoding(false).GetBytes(message);
        }

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Linux, SkipReason = "No LocalDB on Linux.")]
        [OSSkipCondition(OperatingSystems.MacOSX, SkipReason = "No LocalDB on OSX.")]
        public async Task BasicViews_HtmlHelpers()
        {
            var antiforgeryToken = await GetAntiforgeryToken("/Home/HtmlHelpers");
            var request = new HttpRequestMessage(HttpMethod.Post, "/Home/HtmlHelpers");
            request.Headers.Add("Cookie", new [] {antiforgeryToken[0]});
            request.Content = new ByteArrayContent(GetValidBytes(antiforgeryToken[1]));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Linux, SkipReason = "No LocalDB on Linux.")]
        [OSSkipCondition(OperatingSystems.MacOSX, SkipReason = "No LocalDB on OSX.")]
        public async Task BasicViews_TagHelpers()
        {
            var antiforgeryToken = await GetAntiforgeryToken("/");
            var request = new HttpRequestMessage(HttpMethod.Post, "/");
            request.Headers.Add("Cookie", new [] {antiforgeryToken[0]});
            request.Content = new ByteArrayContent(GetValidBytes(antiforgeryToken[1]));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
