try {
    $r = [System.Net.FtpWebRequest]::Create('ftp://site66731.siteasp.net/')
    $r.Credentials = New-Object System.Net.NetworkCredential('site66731','qA!8G@4e_dY2')
    $r.Method = [System.Net.WebRequestMethods+Ftp]::ListDirectory
    $r.Timeout = 15000
    $resp = $r.GetResponse()
    $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
    Write-Host $sr.ReadToEnd()
    $sr.Close()
    $resp.Close()
    Write-Host "FTP CONNECTIVITY: OK"
} catch {
    Write-Host "FTP CONNECTIVITY: FAILED - $($_.Exception.Message)"
}
