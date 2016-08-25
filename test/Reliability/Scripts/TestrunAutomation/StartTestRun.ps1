Param(
  [string]$scenarioFile, # Mandatory
  [string]$server, # Mandatory
  [bool]$TestPerformance=$false,
  [bool]$TestConnectionDensity=$false,
  [bool]$TestReliability=$false,
  [int]$vc=1,
  [int]$endVC=60000,
  [int]$duration=20,
  [int]$repeat=3,
  [string]$logdir,
  [int]$runid=0,
  [bool]$shutdownAfterfinish=$false
)

# Only one of the following should be $true
# if $TestPerformance is $true, you need to specify $vc (# of virtual clients). $vc, $duration and $repeat are used.
# if $TestConnectionDensity is $true, you dont' need to specify any other argument. $vc, $endVC and $duration are used.
# if $TestReliability is $true, you need to specify $vc (# of virtual clients) and $duration. $vc and $duration are used.

function ProcessScenariofile
{
    $content = (Get-Content $scenarioFile | Out-String);

    if($content -match  'scenario\s*{\s*name\s*=\s*"\s*(.*)\s*"\s*;')
    {
        $global:scenario= $matches[1];
    }

        (Get-Content $scenarioFile).Replace('<Your-Server>', $server) | Set-Content $scenarioFile;       
        
    if($content -match  'port\s*=\s*(.*)\s*;')
    {
        $global:port= $matches[1];
    }

    if(Test-Path "$($env:ProgramFiles)\wcat\wcctl.exe")
    {
       $global:wcatpath = "$($env:ProgramFiles)\wcat";
    }
    else
    {
       Write-Host "wcat not found. Please install 64 bit wcat";
       exit;
    }
}

function cleanupPreviousRun
{
    cmd /c taskkill /f /t /im wcctl
    cmd /c taskkill /f /t /im wcctl.exe
    cmd /c taskkill /f /t /im wcclient.exe
    cmd /c taskkill /f /t /im wcclient
}

function printResults
{
    param(
    [string]$resultFile,
    [string]$summaryFile
    )

    [xml] $content = (Get-Content $resultFile | Out-String);
    $vc = $content.SelectNodes("/report/section[@name='header']/table[@name='collection']/item/data[@name='virtualclients']").Item(0).'#text';
    $throughput = $content.SelectNodes("/report/section[@name='summary']/table[@name='summarydata']/item/data[@name='rps']").Item(0).'#text';
    $medianlatency= $content.SelectNodes("/report/section[@name='details']/table[@name='histogram']/item/data[@name='response_time_50']").Item(0).'#text';
    $p95latency= $content.SelectNodes("/report/section[@name='details']/table[@name='histogram']/item/data[@name='response_time_95']").Item(0).'#text';
    $requests =  $content.SelectNodes("/report/section[@name='details']/table[@name='transstats']/item/data[@name='requests']").Item(0).'#text';
    $errors =  $content.SelectNodes("/report/section[@name='details']/table[@name='transstats']/item/data[@name='errors']").Item(0).'#text';
    if($requests -gt 0)
    {
        $reliability = (($requests-$errors)/$requests) * 100;
    }
    else
    {
        $reliability = 0;
    }
    $txt = $("$vc,$throughput,$medianlatency,$p95latency,$reliability");
    $txt | Out-File $summaryFile -Append;    
}

function runTest
{
    param(
    [int]$virtualClients
    )

    $cmd = "`"$($wcatpath)\wcclient.exe`"" ;
    Start-Process "`"$cmd`""  -ArgumentList "localhost -b"

    $cmd = "`"$($wcatpath)\wcctl.exe`" -t $scenarioFile -s $server -p $port -c 1 -v $virtualClients -u $duration -o `"$logdir\result$virtualClients.xml`" -x -k `"$scenario`" > $logdir\wcat-output.log";
    cmd /c "`"$cmd`""
}

function incrementVirtualClients
{
    param(
    [int]$virtualClients
    )
    if($virtualClients -ge $endVC) {
        $virtualClients = -1 ;
    } 
    elseif($virtualClients -lt  10){
        $virtualClients += 1 ;
    }
    elseif($virtualClients -lt 100){
        $virtualClients += 10 ;
    }
    elseif($virtualClients -lt  1000){
        $virtualClients += 100 ;
    }
    elseif( $virtualClients -lt  10000){
        $virtualClients += 1000 ;
    }
    elseif( $virtualClients -lt  60000){
        $virtualClients += 10000 ;
    }
    else {
        $virtualClients = -1;
    }
    return $virtualClients;
}

ProcessScenariofile;
cleanupPreviousRun;

if(!($logdir))
{
    $tstamp=$(((get-date).ToUniversalTime()).ToString("yyyy-MM-dd"));
    $logdir = "c:\$tstamp-$scenario";
}
mkdir $logdir

$txt = "Virtualclients,Throughput,MedianLatency,P95Latency,Reliability";
$summaryFile = $("$logdir\summary.csv");
$txt | Out-File $summaryFile;

$virtualClients = $vc;
For($i=1;;$i++)
{
    runTest $virtualClients;
    printResults $("$logdir\result$virtualClients.xml") $summaryFile;

    if($TestReliability -eq $true)
    {
        
        break;
    }
    elseif($TestPerformance -eq $true)
    {
        if($i -ge $repeat)
        {
            break;
        }
    }    
    elseif($TestConnectionDensity -eq $true)
    {
        $virtualClients = incrementVirtualClients $virtualClients;

        if($virtualClients -lt 0) 
        {
            break;
        }
    }


}
if($shutdownAfterfinish)
{
    $RGname = $server.Split("-")[0];
    $VMname = $server.Split("-")[1];
    Select-AzureRmProfile -Path "X:\Scripts\profile.json";
    Stop-AzureRmVM -ResourceGroupName $RGname -Name $VMname -Force
    cmd /c shutdown -s -t 0;
}

