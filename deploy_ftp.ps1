# FTP Deploy Script for MonsterASP.net
# Uploads all published files to the hosting server via FTP

$ftpServer = "ftp://site66731.siteasp.net"
$ftpUser = "site66731"
$ftpPass = '4Sf%Q!9q6Ra='
$localPublishPath = "C:\Users\Default.LAPTOP-VJOB6R5U\source\repos\Casan_IT15_Project\Casan_IT15_Project\bin\Release\net10.0\publish"
$remotePath = "/wwwroot"

# Load .NET FTP classes
Add-Type -AssemblyName System.Net

function Upload-FtpFile {
    param(
        [string]$localFile,
        [string]$remoteFile
    )
    
    try {
        $ftpUri = "$ftpServer$remoteFile"
        $request = [System.Net.FtpWebRequest]::Create($ftpUri)
        $request.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
        $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPass)
        $request.UseBinary = $true
        $request.UsePassive = $true
        $request.EnableSsl = $false
        $request.KeepAlive = $false
        
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
        Write-Host "  ERROR uploading $remoteFile : $_" -ForegroundColor Red
        return $false
    }
}

function Ensure-FtpDirectory {
    param(
        [string]$remoteDirPath
    )
    
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
    catch {
        # Directory likely already exists, ignore
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  OptiFlow FTP Deployment" -ForegroundColor Cyan
Write-Host "  Target: $ftpServer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get all files from publish folder
$allFiles = Get-ChildItem -Path $localPublishPath -Recurse -File
$totalFiles = $allFiles.Count
$uploaded = 0
$failed = 0

Write-Host "Found $totalFiles files to upload..." -ForegroundColor Yellow
Write-Host ""

# First, create all directories
$allDirs = Get-ChildItem -Path $localPublishPath -Recurse -Directory
foreach ($dir in $allDirs) {
    $relativePath = $dir.FullName.Substring($localPublishPath.Length).Replace("\", "/")
    $remoteDir = "$remotePath$relativePath"
    Ensure-FtpDirectory -remoteDirPath $remoteDir
}

# Then upload all files
$counter = 0
foreach ($file in $allFiles) {
    $counter++
    $relativePath = $file.FullName.Substring($localPublishPath.Length).Replace("\", "/")
    $remoteFile = "$remotePath$relativePath"
    
    Write-Host "[$counter/$totalFiles] Uploading: $relativePath" -ForegroundColor Gray -NoNewline
    
    $result = Upload-FtpFile -localFile $file.FullName -remoteFile $remoteFile
    if ($result) {
        Write-Host " OK" -ForegroundColor Green
        $uploaded++
    }
    else {
        $failed++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "  Uploaded: $uploaded / $totalFiles" -ForegroundColor Green
if ($failed -gt 0) {
    Write-Host "  Failed: $failed" -ForegroundColor Red
}
Write-Host "  URL: http://optiflowsystem101.runasp.net" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
