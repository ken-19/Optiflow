$user = 'site66731'
$pass = '4Sf%Q!9q6Ra='
$ftpUrl = 'ftp://site66731.siteasp.net/wwwroot/app_offline.htm'

Write-Host "Attempting to delete $ftpUrl"
$ftp = [System.Net.FtpWebRequest]::Create($ftpUrl)
$ftp.Credentials = New-Object System.Net.NetworkCredential($user, $pass)
$ftp.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile

try {
    $resp = $ftp.GetResponse()
    Write-Host "Response: $($resp.StatusDescription)"
    $resp.Close()
    Write-Host "Successfully deleted app_offline.htm"
} catch {
    Write-Host "Failed to delete app_offline.htm: $($_.Exception.Message)"
}
