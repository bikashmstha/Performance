Import-module .\Invoke-Parallel.ps1 -Force;
#Add-AzureRmAccount
$SubscriptionName = "Microsoft Azure Internal Consumption";

Select-AzureRmSubscription -SubscriptionName $SubscriptionName;

[System.Collections.ArrayList]$vmrunninglist = @();

function ShutdownVMs
{
    $rgs = Get-AzureRmResourceGroup
    $srgs= $rgs| Where{$_.ResourceGroupName -like "*cell*"}
    Foreach($rg in $srgs)
    {
        $ResourceGroupName = $rg.ResourceGroupName;
        $vms = Get-AzureRMVM -ResourceGroupName $ResourceGroupName;

        Foreach($vm in $vms)
        {
            $vmstatus = Get-AzureRMVM -ResourceGroupName $ResourceGroupName -name $vm.name -Status       
            $PowerState = (get-culture).TextInfo.ToTitleCase(($vmstatus.statuses)[1].code.split("/")[1])
            write-host "VM: '"$vm.Name"' is" $PowerState
            $object = New-Object System.Object;
            $object | Add-Member -type NoteProperty -name ResourceName -Value $ResourceGroupName;
            $object | Add-Member -type NoteProperty -name MachineName -Value $vm.Name;
            if ($Powerstate -eq 'Running')
            {
            $vmrunninglist.Add( $object);
            }
        }
    }
    $vmrunninglist | Invoke-Parallel -ImportVariables -NoCloseOnTimeout -ScriptBlock {
        Stop-AzureRmVM -ResourceGroupName $_.ResourceName -Name $_.MachineName -Verbose -Force 
        }
}

ShutdownVMs;

