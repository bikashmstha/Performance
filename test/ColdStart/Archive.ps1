## This script is called when wrapping up a test.
## It would rename the tests/current folder to Results-{Time the Tests Started}
## Also it would take the average of all runs performed during the test in each
## scenario and append it at the end of the file.

param (
    [Alias("t")]$targetApp = "HelloWorldMvc",
    [Alias("f")]$framework = "netcoreapp1.0",
    [Alias("n")]$testName = "test"
)

& (Join-Path $PSScriptRoot SetEnv.ps1)

$outputDir = Split-Path $global:outputFile
$outputsDir = Split-Path $outputDir
$outputFileName = (Get-Item $global:outputFile).BaseName

$lines = Get-Content $global:outputFile
$count = 0

for ($i = 1; $i -lt $lines.Length; $i++) {
    if ([System.String]::IsNullOrEmpty($lines[$i])) {
        continue
    }

    $values = $lines[$i] -split ","

    if (!$averages) {
        $averages = new-object double[] $values.Count
    }

    for ($j = 0; $j -lt $values.Length; $j++) {
        $averages[$j] += $values[$j]
    }

    $count++
}

for ($j = 0; $j -lt $averages.Length; $j++) {
    $averages[$j] /= $count
}

$scriptOutput = [System.String]::Join(",", $averages)
$scriptOutput | Out-File $global:outputFile -Append
$testNameFile = [System.IO.Path]::Combine($outputDir, $testName)
$testName | Out-File $testNameFile

mv $outputDir (Join-Path $outputsDir $outputFileName)

## Friendly output, so the caller can see the averages
Write-Host $scriptOutput
