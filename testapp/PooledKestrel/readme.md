The pooling levels are controled by ASPNETCORE_ environment variables or settings on the command line.

This project has a `PooledHttpContext` and `PooledHttpContextFactory` in the project, which pool the `DefaultHttpRequest` and `DefaultHttpResponse` objects.

The pooling of the HttpContext checks the following setting:
- `hosting.maxPooledContexts` which is the maximum number that will be pooled. The default value is 256. Set it to 0 to disable pooling.
