##MultiPart Post
MutipartPOST is a test that attempts to upload a set of very large files by means of a multipart POST message. It exercises three basic scenarios:

```
1. Small text part + large text part: 10MB/100MB/1GB [5:3:1]
2. Small text part + large binary part: 10MB/100MB/1GB [5:3:1]
3. A number of large parts: text/binary [2:1] totalling more than 4GB (to test 32-bit limit)
```

Additionally, it has the following characteristics:
  * No-auth middleware scenario.
  * All messages start with a small JSON text part.
  * File payload is generated dynamically.

##Components
###MultipartPOST
Under directory `MultipartPOST` you can find the server side of this test. It exposes two endpoints:
  * `~/`: where it displays a simple text message indicating the project is running. This is the "home" endpoint.
  * `~/api/upload`: the actual upload endpoint, accepts POST messages with multipart content.
The MultipartPOST consumes the network stream into a buffer and then prints the number of bytes read.

###MultipartPOSTClient
Under directory `MultipartPOSTClient` you can find the client side of this test. It attempts to contact the home endpoint, and upon success it starts a number of iterations around scenarios 1, 2 and 3, as described above. Scenario 3 requires a large memory buffer that cannot be created under a 32-bit process, so scenario 3 only runs when the app is running in a 64-bit process.

##How to run
The most basic execution scenario can be done with the `runtest.ps1` PowerShell script. It simply starts the server and then runs the client to excercise all the scenarios.

If you'd like to run it manually, you can:
  1. Go to folder `MultipartPOST`
  2. Execute: `dotnet restore`
  3. Execute: `dotnet run`
  4. Go to folder `MultipartPOSTClient`
  5. Execute: `dotnet restore`
  6. Execute: `dotnet run`
