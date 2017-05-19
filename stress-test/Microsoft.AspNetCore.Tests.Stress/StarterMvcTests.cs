// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Stress.Framework;
using Xunit;

namespace Microsoft.AspNetCore.Tests.Stress
{
    [Collection("May share ports")]
    public class StarterMvcTests : StressTestBase
    {
        public static async Task StarterMvc_Warmup(HttpClient client)
        {
            await client.GetAsync("/");
            await client.GetAsync("/Home/About");
            await client.GetAsync("/Home/Contact");
            await client.GetAsync("/Account/Login");
            await client.GetAsync("/Account/LogOff");
            await client.GetAsync("/Account/Register");
            await client.GetAsync("/Manage");
        }

        [Stress("StarterMvc", WarmupMethodName = nameof(StarterMvc_Warmup))]
        public Task StarterMvc()
        {
            return IterateAsync(async client =>
            {
                var response = await client.GetAsync("/");
                response.EnsureSuccessStatusCode();
                response = await client.GetAsync("/Home/About");
                response.EnsureSuccessStatusCode();
                response = await client.GetAsync("/Home/Contact");
                response.EnsureSuccessStatusCode();

                // Register
                var getResponse = await client.GetAsync("/Account/Register");
                getResponse.EnsureSuccessStatusCode();

                var responseContent = await getResponse.Content.ReadAsStringAsync();
                var verificationToken = ExtractVerificationToken(responseContent);

                var testUser = GetUniqueUserId();
                var requestContent = CreateRegisterPost(verificationToken, testUser, "Asd!123$$", "Asd!123$$");

                var postResponse = await client.PostAsync("/Account/Register", requestContent);
                postResponse.EnsureSuccessStatusCode();

                var postResponseContent = await postResponse.Content.ReadAsStringAsync();
                Assert.Contains("Learn how to build ASP.NET apps that can run anywhere.", postResponseContent); // Home page

                // Verify manage page
                var manageResponse = await client.GetAsync("/Manage");
                manageResponse.EnsureSuccessStatusCode();

                var manageContent = await manageResponse.Content.ReadAsStringAsync();
                verificationToken = ExtractVerificationToken(manageContent);

                // Verify Logoff
                var logoffRequestContent = CreateLogOffPost(verificationToken);
                var logoffResponse = await client.PostAsync("/Account/LogOff", logoffRequestContent);
                logoffResponse.EnsureSuccessStatusCode();

                var logOffResponseContent = await logoffResponse.Content.ReadAsStringAsync();
                Assert.Contains("Learn how to build ASP.NET apps that can run anywhere.", postResponseContent); // Home page

                // Verify relogin
                var loginResponse = await client.GetAsync("/Account/Login");
                loginResponse.EnsureSuccessStatusCode();
                var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

                verificationToken = ExtractVerificationToken(responseContent);
                var loginRequestContent = CreateLoginPost(verificationToken, testUser, "Asd!123$$");

                var loginPostResponse = await client.PostAsync("/Account/Login", loginRequestContent);
                loginPostResponse.EnsureSuccessStatusCode();

                var longPostResponseContent = await loginPostResponse.Content.ReadAsStringAsync();
                Assert.DoesNotContain("Invalid login attempt.", longPostResponseContent); // Errored Login page

                // Logoff to get the HttpClient back into a working state.
                manageResponse = await client.GetAsync("/Manage");
                manageResponse.EnsureSuccessStatusCode();
                manageContent = await manageResponse.Content.ReadAsStringAsync();
                verificationToken = ExtractVerificationToken(manageContent);
                logoffRequestContent = CreateLogOffPost(verificationToken);
                logoffResponse = await client.PostAsync("/Account/LogOff", logoffRequestContent);
                logoffResponse.EnsureSuccessStatusCode();
            });
        }

        private HttpContent CreateRegisterPost(
            string verificationToken,
            string email,
            string password,
            string confirmPassword)
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken),
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("ConfirmPassword", confirmPassword)
            };
            var content = new FormUrlEncodedContent(form);

            return content;
        }

        private HttpContent CreateLoginPost(string verificationToken, string email, string password)
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken),
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("RememberMe", false.ToString())
            };
            var content = new FormUrlEncodedContent(form);

            return content;
        }

        private HttpContent CreateLogOffPost(string verificationToken)
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken)
            };
            var content = new FormUrlEncodedContent(form);

            return content;
        }

        private string ExtractVerificationToken(string response)
        {
            var tokenElement = string.Empty;
            var writer = new StreamWriter(new MemoryStream());
            writer.Write(response);

            writer.BaseStream.Position = 0;
            var reader = new StreamReader(writer.BaseStream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().Trim();
                if (line.StartsWith("<input name=\"__RequestVerificationToken\""))
                {
                    tokenElement = line.Replace("</form>", "");
                }
            }

            var root = XElement.Parse(tokenElement);
            return (string)root.Attribute("value");
        }

        private string GetUniqueUserId()
        {
            return string.Format("testUser{0}@ms.com", Guid.NewGuid().ToString());
        }
    }
}
