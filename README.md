ASP.NET Performance Tests
=========================

AppVeyor: [![AppVeyor](https://ci.appveyor.com/api/projects/status/8e82uv0i2xi3dnv7/branch/dev?svg=true)](https://ci.appveyor.com/project/aspnetci/Performance/branch/dev)

Travis:   [![Travis](https://travis-ci.org/aspnet/Performance.svg?branch=dev)](https://travis-ci.org/aspnet/Performance)

Performance tests and infrastructure for ASP.NET.

This project is part of ASP.NET 5. You can find samples, documentation and getting started instructions for ASP.NET 5 at the [Home](https://github.com/aspnet/home) repo.

## Dependencies
Your system must have `gulp` and `bower` readily available from the command line. Feel free to install them in any way you want. If you don't know how to go about it, here are some suggestions.

### Windows
  * [npm](https://nodejs.org/en/)
  * gulp: `npm install -g gulp`
  * bower: `npm install -g bower`

To build, execute `.\build.cmd`.

### Ubuntu
  * nodejs-legacy: `sudo apt-get install nodejs-legacy`
  * npm: `sudo apt-get install npm`
  * gulp: `npm install -g gulp`
  * bower: `npm install -g bower`
  * [Mono](http://www.mono-project.com/docs/getting-started/install/linux/#debian-ubuntu-and-derivatives) (use package mono-complete)

To build, execute `./build.sh`.

### OSX
  * [brew](http://brew.sh/)
  * npm: `brew install npm`
  * gulp: `npm install -g gulp`
  * bower: `npm install -g bower`
  * [Mono](http://www.mono-project.com/docs/getting-started/install/mac/)

To build, execute `./build.sh`.

## [Microbenchmark tests](testapp)
These are targeted tests covering specific feature areas. The tests are quite self explanatory. You can use the `loadtests.ps1` script to load test the specific scenario. We use the [loadtest npm module](https://www.npmjs.com/package/loadtest) in these scripts to keep them as simple as possible.

For e.g. here is how to run the Hello World MVC microbenchmark.

```powershell
cd testapp\HelloWorldMvc
dotnet restore
dotnet run
```

Run the load test client
```powershell
.\loadtest.ps1
```

A `loadtest.sh` script will also be added to cover cross platform testing.