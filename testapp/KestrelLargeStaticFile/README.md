# Kestrel Large Static File Application

This application is a test application for performance and reliability for kestrel with large static file

## Setup the application
* use following command to publish the applicaiton
	- `dotnet restore`
	- `dotnet publish --configuration Release --frmaework netcoreapp2.0|net46`

* copy the large static file (\\funfile\Scratch\dayu\test\load.png) to published wwwroot directory under Images (replace the small file)	

## Run the application
* Run as stand alone service
	Go to target directory (e.g. bin\Release\netcoreapp2.0\publish) and run `dotnet KestrelLargeStaticFile.dll`. The service listens at http://localhost:5000

* Run under IIS
	- Setup/Configure IIS using default settings.
    - Install AspNetCoreModule as part of installing the .NET Core runtime or as explained at https://github.com/aspnet/AspNetCoreModule#installing-the-latest-aspnet-core-module
	- Copy target directory and all sub-directories to IIS root (c:\inetpub by default)

The client load can follow same setup process as Hello World.