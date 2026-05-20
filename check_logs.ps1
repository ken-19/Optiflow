$user = 'site66731'
$pass = '4Sf%Q!9q6Ra='
$ftpUrl = 'ftp://site66731.siteasp.net/wwwroot/logs/'

Write-Host "Checking for logs in $ftpUrl"
$ftp = [System.Net.FtpWebRequest]::Create($ftpUrl)
$ftp.Credentials = New-Object System.Net.NetworkCredential($user, $pass)
$ftp.Method = [System.Net.WebRequestMethods+Ftp]::ListDirectoryDetails

try {
    $resp = $ftp.GetResponse()
    $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
    $files = $sr.ReadToEnd()
    $sr.Close()
    $resp.Close()
    Write-Host $files
} catch {
    Write-Host "Failed to find logs: $($_.Exception.Message)"
}
