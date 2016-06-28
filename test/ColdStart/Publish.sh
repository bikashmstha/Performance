#!/bin/bash

## Build and publish the application into workspace

## clean up
rm -rf ~/.nuget
rm -rf ~/.dotnet
rm -rf ~/.local

git clean -xdf

targetApp="HelloWorldMvc"
framework="netcoreapp1.0"

while getopts ":t:f:d:p:" opt; do
    case $opt in
        t) targetApp="$OPTARG"
        ;;
        f) framework="$OPTARG"
        ;;
        d) appDir="$OPTARG"
        ;;
        p) precompile="$OPTARG"
        ;;
        \?) echo "Invalid option -$OPTARG" >&2
        ;;
    esac
done

repoRoot=`git rev-parse --show-toplevel`

## run "build pre-clean" to ensure we have the lastest dotnet
## currently this line is not work, need to fix it
$repoRoot/build.sh pre-clean

# Set targetApp name and workspace
scriptRoot="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
source $scriptRoot/SetEnv.sh

if [ -z "$targetApp" ]; then
    echo "Target application is not set"
    exit -1
fi

if [ -z "$workspace" ]; then
    echo "Workspace dir is not set"
    exit -1
fi

if [ -z "$outputFile" ]; then
    echo "Output file is not set"
    exit -1
fi

if [ ! -d "$workspace" ]; then
    echo "Workspace $workspace does not exist"
    exit -1
fi

appLocation="$repoRoot/testapp/$targetApp"

if [ ! -d "$appLocation" ]; then
    if [ -d "$appDir" ]; then
        appLocation=$appDir
    else
        echo "$appLocation is not a valid performance app"
        exit -1
    fi
fi

## publish targeted application
pushd $appLocation

if [ ! -z "$precompile" ]; then
    cp project.json project.json.bak
    nodejs $repoRoot/tools/ProjectModifier/modify-project.js precompile=$precompile
fi

~/.dotnet/dotnet restore --infer-runtimes
rm -rf $publishLocation
appPublishLocation=${publishLocation}/${targetApp}
~/.dotnet/dotnet publish -o $appPublishLocation --configuration release --framework $framework

if [ -f project.json.bak ]; then
    rm project.json
    mv project.json.bak project.json
fi

## archive the lock.json file for record because desktop app does not have .deps.json
outputDir=$(dirname "${outputFile}")
cp -R ${appPublishLocation} ${outputDir}

popd

