try {
    $response = Invoke-WebRequest -Uri 'http://optiflowsystem101.runasp.net/' -TimeoutSec 15 -UseBasicParsing
    Write-Host "HTTP Status: $($response.StatusCode)"
    Write-Host "Content Length: $($response.Content.Length)"
} catch {
    Write-Host "HTTP Error: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "--- Testing FTP ---"
try {
    $r = [System.Net.FtpWebRequest]::Create('ftp://site66731.siteasp.net/')
    $r.Credentials = New-Object System.Net.NetworkCredential('site66731','4Sf%Q!9q6Ra=')
    $r.Method = [System.Net.WebRequestMethods+Ftp]::ListDirectory
    $r.Timeout = 15000
    $resp = $r.GetResponse()
    $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
    $files = $sr.ReadToEnd()
    $sr.Close()
    $resp.Close()
    Write-Host "FTP OK"
    Write-Host "Files on server:"
    Write-Host $files
} catch {
    Write-Host "FTP Error: $($_.Exception.Message)"
}
