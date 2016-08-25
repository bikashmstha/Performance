Param(
[bool]$ServerGC=$true
)

$GitPath = "$env:ProgramFiles\Git";
$RepoPath= "$env:SystemDrive\Repo";
$DotnetPath="$env:LOCALAPPDATA\Microsoft\dotnet\";
$GitUrl = "-b release https://github.com/aspnet/Performance.git";
$RepoRoot = "$RepoPath\Performance";
$ClientHostName="aapt-perf-045l";
$ServerHostName="aapt-perf-043";
$CurrentMachineIP="";
$Port5000=":5000";
$Port80=":80";
$GCMode="";
$date=$(((get-date).ToUniversalTime()).ToString("yyyy-MM-dd"));
$SharePath="\\aaptperffs\labshare\AspnetCore\FastFileShare";
$ResultSharePath="\\aaptperffs\labshare\AspnetCore\FastFileShare\Performance\$date";
$ip=get-WmiObject Win32_NetworkAdapterConfiguration|Where {$_.Ipaddress.length -gt 1};
$CurrentMachineIP=$ip.ipaddress[0];

if($ServerGC)
{
    $GCMode="ServerGC";
}
else
{
    $GCMode="WorkStationGC";
}



function KillProcess
{
    param(
    [string]$Name
    )

    $ret = Get-Process $Name -ErrorAction SilentlyContinue;
    if($ret)
    {
        Stop-Process -processname $Name -Force;
    }
}

function KillProcesses {
    KillProcess "BasicKestrel";
    KillProcess "HelloWorldMvc";
    KillProcess "MusicStore";
    KillProcess "dotnet";
    KillProcess "w3wp" ;
    KillProcess "vmmap" ;
}

function Cleanup{


    if(Test-Path "$RepoPath\Performance\")
    {
        Start-Process -FilePath "$GitPath\cmd\git.exe" -ArgumentList "clean -xdf" -WorkingDirectory "$RepoPath\Performance\" -NoNewWindow -Wait;
        #rmdir -Recurse -Path $RepoPath\Performance\;
        Remove-Item -Recurse -Force $RepoPath\Performance;
    }

    if(Test-Path "$RepoPath\Build\BasicKestrel")
    {
        rmdir -Recurse -Path $RepoPath\Build\BasicKestrel;
    }

    if(Test-Path "$RepoPath\Build\HelloWorldMvc")
    {
        rmdir -Recurse -Path $RepoPath\Build\HelloWorldMvc;
    }
    if(Test-Path "$RepoPath\Build\MusicStore")
    {
        rmdir -Recurse -Path $RepoPath\Build\MusicStore;
    }

    if(Test-Path "$($env:LOCALAPPDATA)\NUGET\v3-cache\")
    {
        rmdir -Recurse -Path $env:LOCALAPPDATA\NUGET\v3-cache\;
    }
    if(Test-Path "$($env:LOCALAPPDATA)\NUGET\cache\")
    {
        rmdir -Recurse -Path $env:LOCALAPPDATA\NUGET\cache\;
    }
    if(Test-Path "$($env:USERPROFILE)\.dnx\packages\")
    {
        rmdir -Recurse -Path $env:USERPROFILE\.dnx\packages\;
    }
    if(Test-Path "$($env:USERPROFILE)\.nuget\packages\")
    {
        rmdir -Recurse -Path $env:USERPROFILE\.nuget\packages\;
    }
    if(Test-Path "$($env:LOCALAPPDATA)\Microsoft\dotnet")
    {
        rmdir -Recurse -Path $env:LOCALAPPDATA\Microsoft\dotnet\;
    }
    if(Test-Path "$($env:ProgramW6432)\dotnet\")
    {
        rmdir -Recurse -Path $env:ProgramW6432\dotnet\;
    }
}

function AssignUserPermission
{
     param(
    [string]$path
    )
    $icaclspath = [System.Environment]::GetFolderPath('System')+"\icacls.exe";
    $nugetpath="$env:USERPROFILE\.nuget";
    Start-Process -FilePath $icaclspath -ArgumentList "$DotnetPath /grant Everyone:(OI)(CI)F" -Wait;
    Start-Process -FilePath $icaclspath -ArgumentList "$nugetpath /grant Everyone:(OI)(CI)F" -Wait;
    Start-Process -FilePath $icaclspath -ArgumentList "$path /grant Everyone:(OI)(CI)F" -Wait;
}

function CloneAndBuild{

    #echo $GitPath
    if(!(Test-Path $RepoPath)){
        mkdir $RepoPath;
    }

    Start-Process -FilePath "$GitPath\cmd\git.exe" -ArgumentList "clone $GitUrl" -WorkingDirectory $RepoPath -NoNewWindow -Wait;

    cd $RepoRoot;
    cmd /c .\build.cmd clean;
    
    # if($RepoPath -contains "MusicStore") {
        
    #     cd MusicStore 
    #     Start-Process -FilePath "$DotnetPath\dotnet.exe" -ArgumentList ("restore") -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\MusicStoreRestore.log"
    # }
    # else {        
    #     cd testapp\HelloWorldMvc
    #     Start-Process -FilePath "$DotnetPath\dotnet.exe" -ArgumentList ("restore") -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\HelloWorldMvcRestore.log"
    #     Start-Process -FilePath "$DotnetPath\dotnet.exe" -ArgumentList ("publish", "-c", "release") -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\HelloWorldMvcPublish.log"
    #     cd ../BasicKestrel
    #     Start-Process -FilePath "$DotnetPath\dotnet.exe" -ArgumentList ("restore") -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\BasicKestrelRestore.log"
    #     Start-Process -FilePath "$DotnetPath\dotnet.exe" -ArgumentList ("publish", "-c", "release") -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\BasicKestrelPublish.log"
    # }
}

function RunScenarios{
    if($ServerGC)
    {
        ChangeIntoServerGC;
    }
    else
    {
        ChangeIntoWorkStationGC;
    }
    RunScenarioBase "BaselineHelloWorldMVC5" ":8082";

     RunScenarioBase "BaselineMusicStoreMVC5" ":8092";

    RunScenarioBase "BaselineCustomHandlerMVC5" ":8099";

    RunScenarioIIS "HelloWorldMvc" ":8085" $true;

     RunScenarioIIS "BasicKestrel" ":8083" $true;
    
    RunScenarioIIS "HelloWorldMvc" ":8086"  $false;

    RunScenarioIIS "BasicKestrel" ":8084" $false;
    RunScenarioIIS "MusicStore" ":8093" $true;

     RunScenarioIIS "MusicStore" ":8090" $false;

    RunScenarioKestrel "HelloWorldMvc" $true;

    RunScenarioKestrel "BasicKestrel" $true;

   RunScenarioKestrel "HelloWorldMvc" $false;

   RunScenarioKestrel "BasicKestrel" $false;

    RunScenarioKestrel "MusicStore" $true;

     RunScenarioKestrel "MusicStore" $false;
}

function ChangeIntoServerGC{
    $BasicKestrelPath="$RepoPath\Build\BasicKestrel\net451\win7-x64\publish\BasicKestrel.exe.config"  
    if(!(Test-Path $BasicKestrelPath ))
    {  
        New-Item $BasicKestrelPath -type file
        Add-Content $BasicKestrelPath '<?xml version="1.0" encoding="utf-8"?>'
        Add-Content $BasicKestrelPath '<configuration>'      
        Add-Content $BasicKestrelPath '</configuration>'     
        [xml] $doc = Get-Content($BasicKestrelPath)
        $child = $doc.CreateElement("runtime")
        $child.InnerXML = '<gcServer enabled="true"/>'
        $doc.DocumentElement.AppendChild($child)
        $doc.Save($BasicKestrelPath)
    }
    
    $HelloWorldMvcPath = "$RepoPath\Build\HelloWorldMvc\net451\win7-x64\publish\HelloWorldMvc.exe.config"  
    [xml] $doc = Get-Content($HelloWorldMvcPath)
    $child=$doc.CreateElement("gcServer")
    $doc.configuration.runtime.AppendChild($child)      
    $doc.Save($HelloWorldMvcPath)
    (get-content "$HelloWorldMvcPath") | foreach-object {$_ -replace ' <gcServer />', '<gcServer enabled="true"/>'} | set-content  "$HelloWorldMvcPath" 
     
}

function ChangeIntoWorkStationGC{
    $HelloWorldJsonFilePath="$RepoPath\Build\HelloWorldMvc\netcoreapp1.0\publish"
    $BasicJsonFilePath="$RepoPath\Build\BasicKestrel\netcoreapp1.0\publish"
    (get-content "$HelloWorldJsonFilePath\HelloWorldMvc.runtimeconfig.json") | foreach-object {$_ -replace '"System.GC.Server": true', ''} | set-content  "$HelloWorldJsonFilePath\HelloWorldMvc.runtimeconfig.json"
    (get-content "$BasicJsonFilePath\BasicKestrel.runtimeconfig.json") | foreach-object {$_ -replace '"System.GC.Server": true', ''} | set-content  "$BasicJsonFilePath\BasicKestrel.runtimeconfig.json"
}

function RunScenarioBase{
    param(
    [string]$TestappName,
    [string]$PortNumber
    )

    KillProcesses;
    if($TestappName -Match "MusicStore") {
        Delete-AllTables;
        $tempCommand="wrk -c 32 -t 10 -d 40 --latency http://$ServerHostName$PortNumber"

    } else
    {
        $tempCommand="wrk -c 256 -t 32 -d 40 --latency http://$ServerHostName$PortNumber"
    }
    cmd /c $env:windir\System32\inetsrv\appcmd start apppool /apppool.name:"DefaultAppPool"
    Write-Host "Baseline" + $TestappName -foregroundcolor red -backgroundcolor yellow

 #  $SitePhysicalPath = "$RepoPath\Build\$TestappName";

   # (Get-Content "$SitePhysicalPath\web.config").Replace(
    #".\stdout.log", "$ResultSharePath\$ScenarioName\kestrel.log"   ) | Set-Content "$SitePhysicalPath\web.config";  

    $processList = @();
    $processList += "w3wp";

    RunTest $tempCommand $TestappName $processList;
}

function RunScenarioIIS{
    param(
    [string]$TestappName,
    [string]$PortNumber,
    [bool]$CoreCLR
    )
    #$ScenarioName = "IIS-"+$TestappName+$GCMode+$CLRMode;
    KillProcesses;
    cmd /c $env:windir\System32\inetsrv\appcmd start apppool /apppool.name:"CoreCLR"

    $processList = @();
    $processList += "w3wp";
    
    if($CoreCLR){
        $SitePhysicalPath = "$RepoPath\Build\$TestappName\netcoreapp1.0\publish\wwwroot";
        $CLRMode="CoreCLR";
        $processList += "dotnet";

        # (Get-Content "$SitePhysicalPath\web.config").Replace(
        # "..\", "..\publish\"   ) | Set-Content "$SitePhysicalPath\web.config";
    }
    else{
        $SitePhysicalPath = "$RepoPath\Build\$TestappName\net451\win7-x64\publish\wwwroot";
        $CLRMode="DesktopCLR";
        $processList += $TestappName;

        # (Get-Content "$SitePhysicalPath\web.config").Replace(
        # "dotnet.exe", "..\publish\$Testappname.exe"   ) | Set-Content "$SitePhysicalPath\web.config";  
    }

    $ScenarioName = $TestappName+"-"+$CLRMode + "-IIS";
    Write-Host $TestappName+$GCMode+$CLRMode -foregroundcolor red -backgroundcolor yellow
    #SetPhysicalPath $SitePhysicalPath;
    #AssignUserPermission $SitePhysicalPath;
    cmd /c $env:windir\System32\inetsrv\appcmd.exe set vdir "$TestappName$CLRMode/" -physicalPath:$SitePhysicalPath

    if($TestappName -Match "MusicStore") {
        Delete-AllTables;
        Copy-Item $SitePhysicalPath\..\web.config $SitePhysicalPath\web.config;

        (Get-Content "$SitePhysicalPath\web.config").Replace(
        ".\logs\stdout", "$ResultSharePath\$ScenarioName\kestrel.log"   ) | Set-Content "$SitePhysicalPath\web.config";  
        
        if($CoreCLR) {
            (Get-Content "$SitePhysicalPath\web.config").Replace(
            '".\'+$TestappName+'.dll', '"..\'+$TestappName+'.dll'   ) | Set-Content "$SitePhysicalPath\web.config";   
            
        } else {
            (Get-Content "$SitePhysicalPath\web.config").Replace(
            '".\'+$TestappName+'.exe', '"..\' +$TestappName+'.exe'   ) | Set-Content "$SitePhysicalPath\web.config";
        }   
        $tempCommand="wrk -c 32 -t 10 -d 40 --latency http://$ServerHostName$PortNumber"
    }
    else {
        
        (Get-Content "$SitePhysicalPath\web.config").Replace(
        ".\stdout.log", "$ResultSharePath\$ScenarioName\kestrel.log"   ) | Set-Content "$SitePhysicalPath\web.config";  

        if(!($CoreCLR)) {
            (Get-Content "$SitePhysicalPath\web.config").Replace(
            '"dotnet.exe', '"..\'+$TestappName+'.exe'   ) | Set-Content "$SitePhysicalPath\web.config";

            (Get-Content "$SitePhysicalPath\web.config").Replace(
            '"..\'+$TestappName+'.dll', '"'   ) | Set-Content "$SitePhysicalPath\web.config";
        }   
        $tempCommand="wrk -c 256 -t 32 -d 40 --latency http://$ServerHostName$PortNumber"
    }

    # enable stdout
    (Get-Content "$SitePhysicalPath\web.config").Replace(
    "false", "true"   ) | Set-Content "$SitePhysicalPath\web.config";

    RunTest $tempCommand $ScenarioName $processList;
}

function RunScenarioKestrel{
    param(
    [string]$TestappName,
    [bool]$CoreCLR
    )
    KillProcesses;
    if($TestappName -Match "MusicStore") {
        Delete-AllTables;
         $tempCommand="wrk -c 32 -t 10 -d 40 --latency http://$CurrentMachineIP$Port5000"
    }
    else {
        $tempCommand="wrk -c 256 -t 32 -d 40 --latency http://$CurrentMachineIP$Port5000"
    }
    $processList = @();

    #$ScenarioName = ("Kestrel-"+$TestappName+$GCMode+$CLRMode);
    
    if($CoreCLR){
        $SitePhysicalPath = "$RepoPath\Build\$TestappName\netcoreapp1.0\publish";
        $CLRMode="CoreCLR";
        $processList += "dotnet";
        Write-Host "Kestrel"+$TestappName+$GCMode+$CLRMode -foregroundcolor red -backgroundcolor yellow;
        Write-Host "$SitePhysicalPath\$TestappName.dll";
        $p = Start-Process -FilePath "$DotnetPath\dotnet.exe" -ArgumentList ("$SitePhysicalPath\$TestappName.dll", "--server.urls=http://$CurrentMachineIP$Port5000") -WorkingDirectory $SitePhysicalPath -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\kestrel.log" -RedirectStandardError "$ResultSharePath\$ScenarioName\kestrel.err" ;
        Start-Sleep -s 5;
    }
    else{
        $SitePhysicalPath = "$RepoPath\Build\$TestappName\net451\win7-x64\publish";
        $CLRMode="DesktopCLR";
        $processList += $TestappName;
        Write-Host "Kestrel"+$TestappName+$GCMode+$CLRMode -foregroundcolor red -backgroundcolor yellow;
        Start-Process -FilePath "$SitePhysicalPath\$TestappName.exe" -ArgumentList "--server.urls=http://$CurrentMachineIP$Port5000" -WorkingDirectory $SitePhysicalPath -NoNewWindow  -RedirectStandardOutput "$ResultSharePath\$ScenarioName\kestrel.log" -RedirectStandardError "$ResultSharePath\$ScenarioName\kestrel.err" ;
        Start-Sleep -s 5;
    }
    $ScenarioName = $TestappName+"-"+$CLRMode;

    RunTest $tempCommand $ScenarioName $processList;
}

function SetPhysicalPath{
    param(
    [string]$PhysicalPath
    )
    cmd /c $env:windir\System32\inetsrv\appcmd.exe set vdir "Default Web Site/" -physicalPath:$PhysicalPath
}

function RunTest{
    param(
    [string]$Command,
    [string]$ScenarioName,
    [string[]]$processList
    )
    Write-Host "Executing Command: $Command";
    $a=Get-Date;
    $random=$a.Hour.ToString()+$a.Minute.ToString()+$a.Second.ToString()+$a.Millisecond.ToString();
    New-Item C:\temp\$random.txt -ItemType file -Value $Command;

    if(!(Test-Path "$ResultSharePath\$ScenarioName")){
        mkdir "$ResultSharePath\$ScenarioName";
    }

    Start-Process -FilePath "plink.exe" -ArgumentList "-ssh -l asplab -pw iis6!dfu -m C:\temp\$random.txt $ClientHostName" -NoNewWindow -wait;
    Start-Process -FilePath "plink.exe" -ArgumentList "-ssh -l asplab -pw iis6!dfu -m C:\temp\$random.txt $ClientHostName" -NoNewWindow -Wait;
    Start-Process -FilePath "plink.exe" -ArgumentList "-ssh -l asplab -pw iis6!dfu -m C:\temp\$random.txt $ClientHostName" -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\wrk.log" -RedirectStandardError "$ResultSharePath\$ScenarioName\wrkerr.log";    
    Start-Sleep -s 10;
    Get-WmiObject win32_processor | Measure-Object -property LoadPercentage -Average | Select Average | out-file -FilePath "$ResultSharePath\$ScenarioName\cpu.log";
    $totalMemoryUsage = 0;
    foreach ($process in $processList) 
    {
        $proc = Get-WMIObject Win32_Process -Filter "Name='$process.exe'";
        $totalMemoryUsage += $proc.WS;
#        Start-Process -FilePath "vmmap.exe" -ArgumentList ("-p","$process", "$ResultSharePath\$ScenarioName\$process-vmmap.csv") -NoNewWindow;
    }   
    "MemoryUsage: $totalMemoryUsage" | Out-File $ResultSharePath\$ScenarioName\$process-details.txt;
    Start-Sleep -s 35;
    #foreach ($process in $processList) 
    #{
    #    KillProcess $process;
    #}

}

function ChangeDBForMusicStore{
    (get-content "$RepoPath\Build\MusicStore\netcoreapp1.0\publish\config.json" ).Replace('"ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=MusicStore;Trusted_Connection=True;MultipleActiveResultSets=true;Connect Timeout=30;"', '"ConnectionString":"Server=aapt-perf-041;Database=MusicStoreHome;User ID=sa;Password=ASP+Rocks4U;TrustServerCertificate=False;Connection Timeout=30;"') | set-content  "$RepoPath\Build\MusicStore\netcoreapp1.0\publish\config.json"
    (get-content "$RepoPath\Build\MusicStore\net451\win7-x64\publish\config.json" ).Replace('"ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=MusicStore;Trusted_Connection=True;MultipleActiveResultSets=true;Connect Timeout=30;"', '"ConnectionString":"Server=aapt-perf-041;Database=MusicStoreHome;User ID=sa;Password=ASP+Rocks4U;TrustServerCertificate=False;Connection Timeout=30;"') | set-content  "$RepoPath\Build\MusicStore\net451\win7-x64\publish\config.json"
}


function CopyBinaries{

    cmd /c xcopy /s /i /y "$SharePath\Build\HelloWorldMvc" "$RepoPath\Build\HelloWorldMvc";
    cmd /c xcopy /s /i /y "$SharePath\Build\BasicKestrel" "$RepoPath\Build\BasicKestrel";
    cmd /c xcopy /s /i /y "$SharePath\Build\MusicStore" "$RepoPath\Build\MusicStore";
    ChangeDBForMusicStore;
    AssignUserPermission "$SharePath\Build";
}

function Invoke-SQL {
    param(
        [string] $sqlCommand = $(throw "Please specify a query.")
      )

    $connectionString = "Server=aapt-perf-041;Database=MusicStoreHome;User ID=sa;Password=ASP+Rocks4U;TrustServerCertificate=False;Connection Timeout=30;";

    $connection = new-object system.data.SqlClient.SQLConnection($connectionString)
    $command = new-object system.data.sqlclient.sqlcommand($sqlCommand,$connection)
    $connection.Open()

    $adapter = New-Object System.Data.sqlclient.sqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataSet) | Out-Null

    $connection.Close()

}
function Delete-AllTables {
    Invoke-SQL("DROP TABLE [dbo].[__MigrationHistory]");
    Invoke-SQL("DROP TABLE [dbo].[AspNetRoleClaims]");
    Invoke-SQL("DROP TABLE [dbo].[AspNetUserClaims]");
    Invoke-SQL("DROP TABLE [dbo].[AspNetUserLogins]");
    Invoke-SQL("DROP TABLE [dbo].[AspNetUserRoles]");
    Invoke-SQL("DROP TABLE [dbo].[AspNetUserTokens]");
    Invoke-SQL("DROP TABLE [dbo].[CartItems]");
    Invoke-SQL("DROP TABLE [dbo].[OrderDetails]");
    Invoke-SQL("DROP TABLE [dbo].[AspNetRoles]");
    Invoke-SQL("DROP TABLE [dbo].[AspNetUsers]");
    Invoke-SQL("DROP TABLE [dbo].[Orders]");
    Invoke-SQL("DROP TABLE [dbo].[Carts]");
    Invoke-SQL("DROP TABLE [dbo].[Albums]");
    Invoke-SQL("DROP TABLE [dbo].[Artists]");
    Invoke-SQL("DROP TABLE [dbo].[Genres]");
}


KillProcesses;
#Cleanup;
#CloneAndBuild;
#CopyBinaries;

RunScenarios;

perl \\aaptperffs\labshare\AspnetCore\FastFileShare\Scripts\SummarizePerfResults.pl $ResultSharePath