# StressSimpleMvc
A simple test app built based StartMvc app but without depending on SQL, security, etc. so that it can be used for stress easily.

## Goals

* Test app to be used to stress Kestrel/Mvc functionality easily
* Work cross-platform
*  Low overhead: with very minimum fake business logic
* Exercise all basic functionalities
 * Kestrel server
 * Basic Mvc models and routings
 * Different type of contents (json, plain text, etc.)
 * Different type of HTTP methods
 * Different size of contents
 * Different status code

## How to run the server 
* Install the dotnet runtime
* dotnet restore
* dotnet run --configuration release

## Other configuration

* By default it listens on `localhost:5000`. To listen on other url, run as:

```
dotnet run server.urls=http://*5000 --configuration release
```

* By default it uses `Kestrel` as hosting server. To use HttpSys, run as:

```
dotnet run --server HttpSys --configuration release
```

* If using other http server as reverse proxy, you may need to update configuration to allow large content to be sent by client

## How to run the client

* Use the WCAT scripts under [test\reliability\BasicKestrel](../../test/Reliability/BasicKestrel/)
