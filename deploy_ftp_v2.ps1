# FTP Deploy Script v2 for MonsterASP.net
# Uses app_offline.htm to stop the app before uploading (unlocks DLLs)

$ftpServer = "ftp://site66731.siteasp.net"
$ftpUser = "site66731"
$ftpPass = '4Sf%Q!9q6Ra='
$localPublishPath = "C:\Users\Default.LAPTOP-VJOB6R5U\source\repos\Casan_IT15_Project\Casan_IT15_Project\bin\Release\net10.0\publish"
$remotePath = "/wwwroot"

function Upload-FtpFile {
    param([string]$localFile, [string]$remoteFile)
    $maxRetries = 3
    for ($i = 1; $i -le $maxRetries; $i++) {
        try {
            $ftpUri = "$ftpServer$remoteFile"
            $request = [System.Net.FtpWebRequest]::Create($ftpUri)
            $request.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
            $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPass)
            $request.UseBinary = $true
            $request.UsePassive = $true
            $request.EnableSsl = $false
            $request.KeepAlive = $false
            $request.Timeout = 30000
            
            $fileContent = [System.IO.File]::ReadAllBytes($localFile)
            $request.ContentLength = $fileContent.Length
            $stream = $request.GetRequestStream()
            $stream.Write($fileContent, 0, $fileContent.Length)
            $stream.Close()
            $response = $request.GetResponse()
            $response.Close()
            return $true
        }
        catch {
            if ($i -eq $maxRetries) {
                Write-Host "  FAILED after $maxRetries attempts: $_" -ForegroundColor Red
                return $false
            }
            Start-Sleep -Seconds 2
        }
    }
}

function Upload-FtpContent {
    param([string]$content, [string]$remoteFile)
    try {
        $ftpUri = "$ftpServer$remoteFile"
        $request = [System.Net.FtpWebRequest]::Create($ftpUri)
        $request.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
        $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPass)
        $request.UseBinary = $true
        $request.UsePassive = $true
        $request.EnableSsl = $false
        $request.KeepAlive = $false
        
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($content)
        $request.ContentLength = $bytes.Length
        $stream = $request.GetRequestStream()
        $stream.Write($bytes, 0, $bytes.Length)
        $stream.Close()
        $response = $request.GetResponse()
        $response.Close()
        return $true
    }
    catch {
        Write-Host "  ERROR: $_" -ForegroundColor Red
        return $false
    }
}

function Delete-FtpFile {
    param([string]$remoteFile)
    try {
        $ftpUri = "$ftpServer$remoteFile"
        $request = [System.Net.FtpWebRequest]::Create($ftpUri)
        $request.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
        $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPass)
        $request.UsePassive = $true
        $request.EnableSsl = $false
        $request.KeepAlive = $false
        $response = $request.GetResponse()
        $response.Close()
        return $true
    }
    catch {
        Write-Host "  ERROR deleting: $_" -ForegroundColor Red
        return $false
    }
}

function Ensure-FtpDirectory {
    param([string]$remoteDirPath)
    try {
        $ftpUri = "$ftpServer$remoteDirPath"
        $request = [System.Net.FtpWebRequest]::Create($ftpUri)
        $request.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
        $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPass)
        $request.UsePassive = $true
        $request.EnableSsl = $false
        $request.KeepAlive = $false
        $response = $request.GetResponse()
        $response.Close()
    }
    catch { }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  OptiFlow FTP Deployment v2" -ForegroundColor Cyan
Write-Host "  Target: $ftpServer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# STEP 1: Upload app_offline.htm to STOP the app and release DLL locks
Write-Host ""
Write-Host "[STEP 1] Taking app offline..." -ForegroundColor Yellow
$offlineHtml = @"
<!DOCTYPE html>
<html>
<head><title>Updating...</title></head>
<body><h1>OptiFlow is being updated. Please wait...</h1></body>
</html>
"@
$result = Upload-FtpContent -content $offlineHtml -remoteFile "$remotePath/app_offline.htm"
if ($result) {
    Write-Host "  app_offline.htm uploaded - app is now stopped" -ForegroundColor Green
} else {
    Write-Host "  WARNING: Could not upload app_offline.htm" -ForegroundColor Red
}

# Wait for the app to fully shut down and release file locks
Write-Host "  Waiting 10 seconds for app to shut down..." -ForegroundColor Gray
Start-Sleep -Seconds 10

# STEP 2: Create all directories first
Write-Host ""
Write-Host "[STEP 2] Creating directories..." -ForegroundColor Yellow
$allDirs = Get-ChildItem -Path $localPublishPath -Recurse -Directory
foreach ($dir in $allDirs) {
    $relativePath = $dir.FullName.Substring($localPublishPath.Length).Replace("\", "/")
    Ensure-FtpDirectory -remoteDirPath "$remotePath$relativePath"
}
Write-Host "  Directories ready" -ForegroundColor Green

# STEP 3: Upload all files
Write-Host ""
Write-Host "[STEP 3] Uploading files..." -ForegroundColor Yellow
$allFiles = Get-ChildItem -Path $localPublishPath -Recurse -File
$totalFiles = $allFiles.Count
$uploaded = 0
$failed = 0
$counter = 0

foreach ($file in $allFiles) {
    $counter++
    $relativePath = $file.FullName.Substring($localPublishPath.Length).Replace("\", "/")
    $remoteFile = "$remotePath$relativePath"
    
    $sizeKB = [math]::Round($file.Length / 1024, 1)
    Write-Host "  [$counter/$totalFiles] $relativePath (${sizeKB}KB)" -NoNewline
    
    $result = Upload-FtpFile -localFile $file.FullName -remoteFile $remoteFile
    if ($result) {
        Write-Host " OK" -ForegroundColor Green
        $uploaded++
    } else {
        $failed++
    }
}

# STEP 4: Remove app_offline.htm to restart the app
Write-Host ""
Write-Host "[STEP 4] Bringing app back online..." -ForegroundColor Yellow
$result = Delete-FtpFile -remoteFile "$remotePath/app_offline.htm"
if ($result) {
    Write-Host "  app_offline.htm removed - app is restarting!" -ForegroundColor Green
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "  Uploaded: $uploaded / $totalFiles" -ForegroundColor Green
if ($failed -gt 0) {
    Write-Host "  Failed:   $failed" -ForegroundColor Red
}
Write-Host "  URL: http://optiflowsystem101.runasp.net" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
