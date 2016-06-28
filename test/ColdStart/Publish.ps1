## Build and publish the application into workspace

param (
    [Alias("t")]
    $targetApp = "HelloWorldMvc",

    [Alias("f")]
    $framework = "netcoreapp1.0",

    [Alias("d")]$appDir,

    [Alias("p")]
    $precompile
)

## cleaning package caches, we are doing this because we are close to release
$pkgCache = [System.IO.Path]::Combine($env:UserProfile, '.nuget')
$nugetCache = [System.IO.Path]::Combine($env:LocalAppData, 'Nuget')
$dotnetInstallation = [System.IO.Path]::Combine($env:LocalAppData, 'Microsoft', 'dotnet')

Remove-Item $pkgCache -Force -Recurse -ErrorAction SilentlyContinue
Remove-Item $nugetCache -Force -Recurse -ErrorAction SilentlyContinue
Remove-Item $dotnetInstallation -Force -Recurse -ErrorAction SilentlyContinue

git clean -xdf

$repoRoot = $(git rev-parse --show-toplevel)

## run "build pre-clean" to ensure we have the lastest dotnet
& (Join-Path $repoRoot build.ps1) pre-clean

# Set targetApp name and workspace
& (Join-Path $PSScriptRoot SetEnv.ps1)

if (! (Test-Path variable:global:workspace)) {
    Write-Error "Workspace dir is not set"
    Exit -1
}

if (! (Test-Path variable:global:outputFile)) {
    Write-Error "Output file is not set"
    Exit -1
}

if (! (Test-Path $global:workspace)) {
    Write-Error "Workspace ${global:workspace} does not exist"
    Exit -1
}

$appLocation = [System.IO.Path]::Combine($repoRoot, "testapp" , $targetApp)

if (! (Test-Path $appLocation) )
{
    if (Test-Path $appDir) {
        $appLocation = $appDir
    }
    else {
        Write-Error "$appLocation is not a valid performance app"
        Exit -1
    }
}

## publish targeted application
pushd $appLocation

if ($precompile)
{
    $modifyJsonScript = [System.IO.Path]::Combine($PSScriptRoot, "..", "..", "tools", "ProjectModifier", "modify-project.js")
    Copy-Item project.json project.json.bak
    node $modifyJsonScript precompile=$precompile
}

dotnet restore --infer-runtimes

if (Test-Path $global:publishLocation) {
    Write-Host "Clearing publish directory ${global:publishLocation}..."
    Remove-Item $global:publishLocation -Force -Recurse
}

$appPublishLocation = Join-Path $global:publishLocation ($targetApp)
dotnet publish -o $appPublishLocation --configuration Release -f $framework

if (Test-Path project.json.bak)
{
    Move-Item project.json.bak project.json -force
}

## archive the lock.json file for record because desktop app does not have .deps.json
$outputDir = Split-Path $global:outputFile
cp -r $appPublishLocation $outputDir

popd
