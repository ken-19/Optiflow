$user = 'site66731'
$pass = '4Sf%Q!9q6Ra='
$ftpUrl = 'ftp://site66731.siteasp.net/wwwroot/migration_error.txt'

Write-Host "Reading $ftpUrl"
$ftp = [System.Net.FtpWebRequest]::Create($ftpUrl)
$ftp.Credentials = New-Object System.Net.NetworkCredential($user, $pass)
$ftp.Method = [System.Net.WebRequestMethods+Ftp]::DownloadFile

try {
    $resp = $ftp.GetResponse()
    $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
    $content = $sr.ReadToEnd()
    $sr.Close()
    $resp.Close()
    Write-Host "Error log found:"
    Write-Host $content
} catch {
    Write-Host "Failed: $($_.Exception.Message)"
}
