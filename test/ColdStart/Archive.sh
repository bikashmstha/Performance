#!/bin/bash

## This script is called when wrapping up a test.
## It would rename the tests/current folder to Results-{Time the Tests Started}
## Also it would take the average of all runs performed during the test in each
## scenario and append it at the end of the file.

targetApp="HelloWorldMvc"
framework="netcoreapp1.0"
testName="test"

while getopts ":n:t:f:" opt; do
    case $opt in
        t) targetApp="$OPTARG"
        ;;
        f) framework="$OPTARG"
        ;;
        n) testName="$OPTARG"
        ;;
        \?) echo "Invalid option -$OPTARG" >&2
        ;;
    esac
done

source ./SetEnv.sh

outputDir=$( dirname "${outputFile}" )
outputsDir=$( dirname "${outputDir}" )
outputFileName=$( basename "${outputFile}" )

OLDIFS=$IFS
IFS=,

coldSum=0
warmSum=0
nocacheSum=0
count=0

while read Cold Warm NoCache
do
    if [ ! $isHeader ]; then
        isHeader=true
    else
        if [ -z $Cold ]; then
           continue
        fi
        coldSum=`expr $coldSum + $Cold`
        warmSum=`expr $warmSum + $Warm`
        nocacheSum=`expr $nocacheSum + $NoCache`
        count=`expr $count + 1`
    fi
done < $outputFile

coldAvg=`expr $coldSum / $count`
warmAvg=`expr $warmSum / $count`
nocacheAvg=`expr $nocacheSum / $count`

scriptOutput="${coldAvg},${warmAvg},${nocacheAvg}"

echo $scriptOutput >> $outputFile
echo $testName >> $outputDir/$testName

IFS=$OLDIFS

mv $outputDir $outputsDir/$outputFileName

## Friendly output, so the caller can see the averages
echo $scriptOutput
