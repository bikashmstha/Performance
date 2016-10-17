param(
    [int] $iterations = 3000,
    [int] $rps = 250)

$url = "http://127.0.0.1:5000/Home/SetTempData"

Write-Host -ForegroundColor Green Beginning workload
Write-Host "`& loadtest -k -n $iterations -c 32 --rps $rps $url"
Write-Host

& loadtest -k -n $iterations -c 32 --rps $rps $url