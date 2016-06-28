## This script wraps up an IIS scenario test. Other than calling Archive.ps1
## it would also remove the app and appPools

param (
    [Alias("t")]$targetApp = "HelloWorldMvc",
    [Alias("f")]$framework = "netcoreapp1.0",
    [Alias("n")]$testName = "test"
)

$coldSiteName = "${targetApp}Cold"
$warmSiteName = "${targetApp}WarmPkgCache"

$coldAppPoolName = "aspnetCold"
$warmAppPoolName = "aspnetWarm"

[Environment]::SetEnvironmentVariable("DOTNET_PACKAGES_CACHE", $null, "Machine")

cmd /c $env:windir\System32\inetsrv\APPCMD DELETE SITE "${coldSiteName}"
cmd /c $env:windir\System32\inetsrv\APPCMD DELETE SITE "${warmSiteName}"

cmd /c $env:windir\System32\inetsrv\APPCMD DELETE APPPOOL /apppool.name:$coldAppPoolName
cmd /c $env:windir\System32\inetsrv\APPCMD DELETE APPPOOL /apppool.name:$warmAppPoolName

& (Join-Path $PSScriptRoot Archive.ps1) -t $targetApp -f $ framework -n $testName
