$PSToolsPath="D:\Users\INDGAUNT\Desktop\PSTools";
$ServerName="\\aapt-perf-043";
#$LinuxServer="aapt-perf-044l";
$LinuxServer="10.194.217.113";
$DotPath="D:\Users\INDGAUNT\AppData\Local\Microsoft\dotnet\dotnet.exe";
$PowerShellPath="D:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
$ColdStartPSPath="D:\auto\ColdStart.ps1";
$date=$(((get-date).ToUniversalTime()).ToString("yyyy-MM-dd"));
$ResultSharePath="\\aaptperffs\labshare\AspnetCore\AzFileShare\Performance\$date\StartupTime";
$timeoutPeriod = 180;

function RunRemotelyOnServerNoWait{
    Param(
    [string]$Command
    )
    $a=Get-Date;
    $random=$a.Hour.ToString()+$a.Minute.ToString()+$a.Second.ToString()+$a.Millisecond.ToString();
    New-Item C:\temp\$random.txt -ItemType file -Value $Command;
    Start-Process -FilePath "D:\Users\INDGAUNT\Desktop\plink.exe" -ArgumentList "-ssh -l asplab -pw iis6!dfu -m C:\temp\$random.txt aapt-perf-044l" -NoNewWindow -RedirectStandardError "$ResultSharePath\error.log";;
}

function RestartServer
{     
     Restart-Computer -ComputerName aapt-perf-043 -Force -Wait -For PowerShell -Timeout 600 -Delay 2;
     Start-Sleep -s 20 | Out-Null;
}

function RestartLinuxServer{
    RunRemotelyOnServerNoWait 'echo -e "iis6!dfu\n" | sudo -S reboot';
    Start-Sleep -s 10 | Out-Null;
    $timer= [Diagnostics.Stopwatch]::StartNew()
    $success = $false
    ForEach ($ping in 1..$timeoutPeriod){
        [string]$r = ping $LinuxServer;
        if ($r.Contains("time<1ms")) {
            $success = $true;
            break;
        }
        Start-Sleep -s 10 | Out-Null;
    }
    $timer.Stop();
    if($success){
        Start-Sleep -s 50 | Out-Null;
        return;
    }
    else{
        Write-Host "Timeout! Restart not success!";
    }
}

function RunLinuxScenarioNginx{
    param(
        [string]$port,
        [string]$TestappName
    )
    if($TestappName -eq "MusicStore"){
        $PhysicalPath="~/musicStore/samples/MusicStore/bin/release/netcoreapp1.0/publish";
    }
    else{
        $PhysicalPath="~/Performance/testapp/$TestappName/bin/release/netcoreapp1.0/publish";
    }   
    [string]$ScenarioName="LinuxNginx" + $TestappName;
    if(!(Test-Path "$ResultSharePath\$ScenarioName")){
        mkdir "$ResultSharePath\$ScenarioName";
    }

    RunRemotelyOnServerNoWait "~/.dotnet/dotnet $PhysicalPath/$TestappName.dll";
    #RunRemotelyOnServerNoWait "~/.dotnet/dotnet ~/Performance/testapp/HelloWorldMvc/bin/release/netcoreapp1.0/publish/HelloWorldMvc.dll";
    Start-Sleep -s 50 | Out-Null;
    
    Start-Process -FilePath "$PSToolsPath\PsExec.exe" -ArgumentList "$ServerName $PowerShellPath $ColdStartPSPath  -port $port -hostname http://$LinuxServer" -Wait -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\StartupTime.log";
}


function RunLinuxScenarioKestrel{
    param(
        [string]$port,
        [string]$TestappName
    )
    if($TestappName -eq "MusicStore"){
        $PhysicalPath="~/musicStore/samples/MusicStore/bin/release/netcoreapp1.0/publish";
    }
    else{
        $PhysicalPath="~/Performance/testapp/$TestappName/bin/release/netcoreapp1.0/publish";
    }   
    [string]$ScenarioName="LinuxKestrel" + $TestappName;
    if(!(Test-Path "$ResultSharePath\$ScenarioName")){
        mkdir "$ResultSharePath\$ScenarioName";
    }

    RunRemotelyOnServerNoWait "~/.dotnet/dotnet $PhysicalPath/$TestappName.dll --server.urls=http://$LinuxServer:5000";
    Start-Sleep -s 50 | Out-Null;
    
    Start-Process -FilePath "$PSToolsPath\PsExec.exe" -ArgumentList "$ServerName $PowerShellPath $ColdStartPSPath -port $port -hostname http://$LinuxServer" -Wait -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\StartupTime.log";
}

function RunScenarioIIS
{
    param(
        [string]$port,
        [string]$ScenarioName
    )   
    if(!(Test-Path "$ResultSharePath\$ScenarioName")){
        mkdir "$ResultSharePath\$ScenarioName";
    }
    
     Start-Process -FilePath "$PSToolsPath\PsExec.exe" -ArgumentList "$ServerName $PowerShellPath $ColdStartPSPath -port $port" -Wait -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$ScenarioName\StartupTime.log";

}

function RunScenarioWithoutIIS
{
    param(
    [string]$port,
    [bool]$CoreCLR,
    [string]$TestappName
    )
    [string]$SitePhysicalPath;
    [string]$FolderName;
    if($CoreCLR){
        $SitePhysicalPath = "D:\Repo\Build\$TestappName\netcoreapp1.0\publish\$TestappName"+".dll"; 
        $FolderName="Kestrel"+$TestappName+"ServerGCCoreCLR";
        Start-Process -FilePath "$PSToolsPath\PsExec.exe" -ArgumentList "$ServerName $DotPath $SitePhysicalPath --server.urls=http://*:5001" -NoNewWindow;
        Start-Sleep -s 15 | Out-Null;
       
    }
    else{
        $SitePhysicalPath = "D:\Repo\Build\$TestappName\net451\win7-x64\publish\$TestappName"+".exe";  
        $FolderName="Kertrel"+$TestappName+"ServerGCDesktopCLR";
        Start-Process -FilePath "$PSToolsPath\PsExec.exe" -ArgumentList "$ServerName $SitePhysicalPath --server.urls=http://*:5001" -NoNewWindow;
        Start-Sleep -s 15 | Out-Null;     
       
    }    
    if(!(Test-Path "$ResultSharePath\$FolderName")){
        mkdir "$ResultSharePath\$FolderName";
    }
    Start-Process -FilePath "$PSToolsPath\PsExec.exe" -ArgumentList "$ServerName $PowerShellPath $ColdStartPSPath -port $port" -Wait -NoNewWindow -RedirectStandardOutput "$ResultSharePath\$FolderName\StartupTime.log";
}

function RunScenarioLinux{
    #RestartLinuxServer;
    #RunLinuxScenarioNginx "8080" "HelloWorldMvc";
    #RestartLinuxServer;
    #RunLinuxScenarioNginx "8080" "BasicKestrel";
    #RestartLinuxServer;
    #RunLinuxScenarioNginx "8080" "MusicStore";
    #RestartLinuxServer;
    #RunLinuxScenarioKestrel "5000" "HelloWorldMvc";
    #RestartLinuxServer;
    #RunLinuxScenarioKestrel "5000" "BasicKestrel";
    RestartLinuxServer;
    RunLinuxScenarioKestrel "5000" "MusicStore";
}

function RunScenarioWindows{
    RestartServer;
    RunScenarioIIS "8083" "IISBasicKestrelServerGCCoreCLR";
   # RestartServer;
   # RunScenarioIIS "8084" "IISBasicKestrelServerGCDesktopCLR";
    RestartServer;
    RunScenarioIIS "8085" "IISHelloWorldMvcServerGCCoreCLR";
   # RestartServer;
   # RunScenarioIIS "8086" "IISHelloWorldMvcServerGCDesktopCLR";
    RestartServer;
    RunScenarioIIS "8093" "IISMusicStoreHomeServerGCCoreCLR";
   # RestartServer;
   # RunScenarioIIS "8090" "IISMusicStoreHomeServerDestopCLR";
    RestartServer;
    RunScenarioWithoutIIS "5000" $true "HelloWorldMvc";
   # RestartServer;
   # RunScenarioWithoutIIS "5000" $false "HelloWorldMvc";
    RestartServer;
    RunScenarioWithoutIIS "5001" $true "MusicStore";
   # RestartServer;
   # RunScenarioWithoutIIS "5001" $false "MusicStore";
    RestartServer;
    RunScenarioWithoutIIS "5001" $true "BasicKestrel";
   # RestartServer;
   # RunScenarioWithoutIIS "5001" $false "BasicKestrel";
}

#RunScenarioWindows;
RunScenarioLinux;   