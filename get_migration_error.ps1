try {
    $response = Invoke-WebRequest -Uri 'https://optiflowsystem101.runasp.net/api/migration/run' -UseBasicParsing -ErrorAction Ignore
    Write-Host "Status: $($response.StatusCode)"
    Write-Host "Body: $($response.Content)"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
