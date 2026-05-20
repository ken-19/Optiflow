#!/usr/bin/env pwsh

# Start React Dev Server and ASP.NET Core Backend for Visual Studio debugging

$clientAppPath = "$PSScriptRoot\client-app"
$dotnetProject = "$PSScriptRoot\Casan_IT15_Project\Casan_IT15_Project.csproj"

Write-Host "Starting development servers..." -ForegroundColor Green

# Start React dev server in background
Write-Host "Starting React dev server on http://localhost:5173..." -ForegroundColor Cyan
Start-Process pwsh -ArgumentList @(
    '-NoExit'
    '-Command'
    "cd '$clientAppPath'; npm run dev"
) -WindowStyle Minimized

# Give React time to start
Start-Sleep -Seconds 3

# Start .NET backend
Write-Host "Starting ASP.NET Core backend on https://localhost:7144..." -ForegroundColor Cyan
& dotnet run --project $dotnetProject
