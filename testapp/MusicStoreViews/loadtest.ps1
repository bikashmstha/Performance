param(
    [int] $iterations = 3000,
    [int] $rps = 20,
    [string][ValidateSet("address", "addressAndPayment", "create", "register")] $variation = "address")

if ($variation -eq "address" -or $variation -eq "addressAndPayment")
{
    $url = "http://127.0.0.1:5000/Home/AddressAndPayment"
}
elseif ($variation -eq "create")
{
    $url = "http://127.0.0.1:5000/Home/Create"
}
else
{
    $url = "http://127.0.0.1:5000/Home/Register"
}

Write-Host -ForegroundColor Green Testing fetch from $url

# First, do a GET to this page to load the form and confirm the site is running.

$response = Invoke-WebRequest -Uri $url -Headers @{"Cache-Control"="no-cache"}
$status_code = $response.StatusCode
if ($status_code -ne 200)
{
    Write-Error Request failed: $status_code
    return;
}

Write-Host -ForegroundColor Green Beginning workload
Write-Host "`& loadtest -k -n $iterations -c 16 --rps $rps $url"
Write-Host

& loadtest -k -n $iterations -c 16 --rps $rps $url