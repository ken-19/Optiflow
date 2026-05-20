# Remove app_offline.htm from production FTP
$ftpUrl = "ftp://site66731.siteasp.net/app_offline.htm"
$username = "site66731"
$password = "qA!8G@4e_dY2"

try {
    $request = [System.Net.FtpWebRequest]::Create($ftpUrl)
    $request.Credentials = New-Object System.Net.NetworkCredential($username, $password)
    $request.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
    $request.UseBinary = $true
    $request.KeepAlive = $false
    $response = $request.GetResponse()
    Write-Host "SUCCESS: app_offline.htm deleted"
    $response.Close()
} catch {
    Write-Host "INFO: $($_.Exception.Message)"
}

# Wait for app to restart
Write-Host "Waiting 15 seconds for app to restart..."
Start-Sleep -Seconds 15

# Test site availability
try {
    $web = Invoke-WebRequest -Uri "http://optiflowsystem101.runasp.net/" -TimeoutSec 30 -UseBasicParsing
    Write-Host "Site status: $($web.StatusCode)"
} catch {
    Write-Host "Site check: $($_.Exception.Message)"
}
