use File::Find;
sub process_file {
return if -d;
    $file = $File::Find::name;
    print "Processing $file\n";
    if($file =~ /\/(.*?)\/wrk\.log/) {
        $scenario = $1;
        $scenario =~ s/ServerGC//sig; 
        open DAT, "< $file";
        while(<DAT>)
        {
             if(/Requests\/sec:\s*(\d*)\./)
             {
                 $myhash{$scenario}{"throughput"} = $1;                 
             }  
             if(/Latency\s*(\d*)\./)
             {
                 $myhash{$scenario}{"latency"} = $1;                 
             }  
             if(/(\d+) requests in/)
             {
                 $myhash{$scenario}{"requests"} = $1;                 
                 $myhash{$scenario}{"reliability"} = ($myhash{$scenario}{"requests"} - $myhash{$scenario}{"errors"})*100/$myhash{$scenario}{"requests"} if( $myhash{$scenario}{"requests"} != 0);              
             }  
             if(/Socker errors/)
             {
                 $myhash{$scenario}{"sockererrors"} = $_;                 
             } 
             if(/Non-(.*?): (\d+)/)
             {
                 $myhash{$scenario}{"errors"} = $2;   
             }  
        }
        close(DAT);
    }
    if($file =~ /\/(.*?)\/cpu\.log/) {
        $scenario = $1; 
        $scenario =~ s/ServerGC//sig; 
        open DAT, "<:encoding(UCS-2le)",$file;
        while(<DAT>)
        {
             if(/\s*(\d+)\s*/)
             {
                 $myhash{$scenario}{"cpu"} = $1;                 
             }  
        }
        close(DAT);
    }
    if($file =~ /\/(.*?)\/vmmap\.csv/) {
        $scenario = $1; 
        $scenario =~ s/ServerGC//sig; 
        open DAT, "< $file";
        while(<DAT>)
        {
             if(/"Total"/)
             {
                 @tokens = split /","/, $_; 
                 $myhash{$scenario}{"memory"} = $tokens[4];   
                 last;              
             }  
        }
        close(DAT);
    }
}
find(\&process_file, @ARGV);
printf "%-50s %-12s %-7s %-14s %-7s %-10s %-10s %-5s %-5s %-5s\n","Scenario","Throughput","Latency", "Reliability", "cpu" , "memory", "requests", "tratio", "lratio", "mratio";
foreach $key (sort keys %myhash)
{
    if($key =~ /BasicKestrel/) {
        $baseline = "BaselineHelloWorldMVC5";
    } 
    elsif($key =~ /HelloWorld/) {
        $baseline = "BaselineHelloWorldMVC5";
    } 
    elsif($key =~ /MusicStore/) {
        $baseline = "BaselineMusicStoreMVC5";
    }
    

    $myhash{$key}{"throughputRatio"} = $myhash{$baseline}{"throughput"}/$myhash{$key}{"throughput"} if($myhash{$key}{"throughput"} != 0);
    $myhash{$key}{"latencyRatio"} = $myhash{$key}{"latency"}/$myhash{$baseline}{"latency"} if($myhash{$baseline}{"latency"} != 0);
    $myhash{$key}{"memoryRatio"} = $myhash{$key}{"memory"}/$myhash{$baseline}{"memory"} if($myhash{$baseline}{"memory"} != 0);    
    
    printf "%-50s %-12s %-7s %-14s %-7s %-10s %-10s %-5.4s %-5.4s %-5.4s\n", $key, $myhash{$key}{"throughput"}, $myhash{$key}{"latency"}, $myhash{$key}{"reliability"}, 
    $myhash{$key}{"cpu"} , $myhash{$key}{"memory"} ,$myhash{$key}{"requests"}, $myhash{$key}{"throughputRatio"},  $myhash{$key}{"latencyRatio"}, $myhash{$key}{"memoryRatio"};
    push( @{$results{$key}} , {$key, $myhash{$key}{"throughput"}, $myhash{$key}{"latency"},$myhash{$key}{"cpu"} ,
    $myhash{$key}{"memory"} ,$myhash{$key}{"requests"}, $myhash{$key}{"throughputRatio"},  $myhash{$key}{"latencyRatio"}, $myhash{$key}{"memoryRatio"}});
    #print $myhash{$scenario}{"sockererrors"}."\n";
}

open DAT, ">summary.csv";
print DAT "Scenario,Throughput,Latency,CPU,Memory,Requests\n";
foreach $key (sort keys %myhash)
{
    printf DAT "%s,%s,%s,%s,%s,%s\n", $key, $myhash{$key}{"throughput"}, $myhash{$key}{"latency"},$myhash{$key}{"cpu"} ,$myhash{$key}{"memory"},$myhash{$key}{"memory"};
}
close(DAT);

