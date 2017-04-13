param(
    [int] $iterations = 3000,
    [int] $rps = 50,
    [string] $hostname = '127.0.0.1')
$url = "http://$hostname:5000/Large.html"

Write-Host -ForegroundColor Green loadtest -k -n $iterations -c 16 --rps $rps $url
Write-Host

& loadtest -k -n $iterations -c 16 --rps $rps $url