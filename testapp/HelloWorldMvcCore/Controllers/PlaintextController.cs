// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelloWorldMvcCore
{
    public class PlaintextController : ControllerBase
    {
        private static readonly byte[] _bytes = Encoding.UTF8.GetBytes("Hello, World!");

        [HttpGet("Plaintext")]
        public Task Index()
        {
            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/plain";
            Response.Headers["Content-Length"] = "13";
            return Response.Body.WriteAsync(_bytes, 0, _bytes.Length);
        }
    }
}
