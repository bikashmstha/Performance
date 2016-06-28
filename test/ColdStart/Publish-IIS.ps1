## Build and publish the application into workspace, and register IIS sites
param (
    [Alias("t")]
    $targetApp = "HelloWorldMvc",

    [Alias("f")]
    $framework = "netcoreapp1.0",

    [Alias("d")]
    $appDir,

    [Alias("p")]
    $precompile
)

$params = @{ "targetApp" = $targetApp; "framework" = $framework; "appDir" = $appDir }
if ($precompile)
{
    $params.precompile = $precompile
}

## prereq: IIS enabled, aspnetCoreModule installed, uCRT, , .NET Framework 4.5.1 Developer Pack
$publishScript = Join-Path $PSScriptRoot Publish.ps1
& $publishScript @params

$dotnetHome = [System.IO.Path]::Combine($env:LocalAppData, "Microsoft" , "dotnet")
$dotnetExe = Join-Path $dotnetHome "dotnet.exe"

if (!(Test-Path $dotnetExe)) {
    Write-Error "$dotnetExe does not exist"
    Exit -1
}

$sysPath = [Environment]::GetEnvironmentVariable("PATH", "Machine")
$sysPathToken = $sysPath.Split(";")
if (! $sysPathToken.Contains($dotnetHome)) {
    [Environment]::SetEnvironmentVariable("PATH", "${sysPath};${dotnetHome}", "Machine")
}

[Environment]::SetEnvironmentVariable("DOTNET_PACKAGES_CACHE", "${env:USERPROFILE}\.nuget\packages", "Machine")

$appLocation = Join-Path $global:publishLocation $targetApp

$wwwroot = Join-Path $appLocation "wwwroot"
$webconfigOverride = Join-Path $wwwroot "web.config.${framework}"

if (Test-Path $webconfigOverride) {
    $webConfig = Join-Path $wwwroot "web.config"
    Remove-Item $webConfig
    Rename-Item $webconfigOverride $webConfig
}

$coldSiteName = "${targetApp}Cold"
$warmSiteName = "${targetApp}WarmPkgCache"
$coldSiteLocation = Join-Path $global:publishLocation $coldSiteName
$warmSiteLocation = Join-Path $global:publishLocation $warmSiteName

if (! (Test-Path $coldSiteLocation)) {
    Write-Host "Moving published app ${appLocation} to test location ${coldSiteLocation}..."
    Move-Item $appLocation $coldSiteLocation
}

if (! (Test-Path $warmSiteLocation)) {
    Write-Host "Warm site location ${warmSiteLocation} does not exist, copying..."
    Copy-Item $coldSiteLocation $warmSiteLocation -Force -Recurse
}

Icacls $dotnetHome /grant IIS_IUSRS:`(OI`)`(CI`)F /T
Icacls $global:publishLocation /grant IIS_IUSRS:`(OI`)`(CI`)F /T

$coldAppPoolName = "aspnetCold"
$warmAppPoolName = "aspnetWarm"

cmd /c $env:windir\System32\inetsrv\APPCMD DELETE SITE "${coldSiteName}"
cmd /c $env:windir\System32\inetsrv\APPCMD DELETE SITE "${warmSiteName}"

cmd /c $env:windir\System32\inetsrv\APPCMD DELETE APPPOOL /apppool.name:$coldAppPoolName
cmd /c $env:windir\System32\inetsrv\APPCMD DELETE APPPOOL /apppool.name:$warmAppPoolName

cmd /c $env:windir\System32\inetsrv\APPCMD ADD APPPOOL /name:$coldAppPoolName /managedRuntimeVersion:""
cmd /c $env:windir\System32\inetsrv\APPCMD ADD APPPOOL /name:$warmAppPoolName /managedRuntimeVersion:""

$coldwwwroot = Join-Path $coldSiteLocation "wwwroot"
$warmwwwroot = Join-Path $warmSiteLocation "wwwroot"

$ServerName = (Get-WmiObject win32_computersystem).DNSHostName
cmd /c $env:windir\System32\inetsrv\APPCMD ADD SITE /name:$coldSiteName /bindings:http/*:5001:$ServerName /physicalPath:$coldwwwroot /applicationDefaults.applicationPool:$coldAppPoolName
cmd /c $env:windir\System32\inetsrv\APPCMD ADD SITE /name:$warmSiteName /bindings:http/*:5002:$ServerName /physicalPath:$warmwwwroot /applicationDefaults.applicationPool:$warmAppPoolName

