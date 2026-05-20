# Running Casan IT15 Project

## Quick Start in Visual Studio

### Option 1: Full Stack (Easiest - Recommended)
1. In Visual Studio, find the debug dropdown (top toolbar)
2. Select **"Full Stack"** from the dropdown
3. Click the green **"Start"** button (or press F5)
4. This will automatically start:
   - React dev server on `http://localhost:5173`
   - ASP.NET Core backend on `https://localhost:7144`
5. Your browser will open to `http://localhost:5173/login`

### Option 2: Backend Only
1. Select **"https"** from the debug dropdown
2. Click **"Start"** (F5)
3. Runs backend on `https://localhost:7144`
4. Manually open `http://localhost:5173` in another terminal if needed

### Option 3: From PowerShell
```powershell
.\run-fullstack.ps1
```

## What's Running

- **React Frontend**: http://localhost:5173 (Vite dev server)
- **ASP.NET Core Backend**: https://localhost:7144 (API)
- **API Endpoints**: https://localhost:7144/api/*
- **SignalR Hubs**: https://localhost:7144/hubs/*

## Important Ports

- **5173**: React dev server (frontend)
- **5115**: HTTP backend (dev)
- **7144**: HTTPS backend (dev)

## Troubleshooting

### Port Already In Use
If you get "address already in use" error:
```powershell
# Kill process on port 5173 (React)
Get-Process | Where-Object {$_.Id -eq (Get-NetTCPConnection -LocalPort 5173).OwningProcess} | Stop-Process -Force

# Kill process on port 5115 (Backend HTTP)
Get-Process | Where-Object {$_.Id -eq (Get-NetTCPConnection -LocalPort 5115).OwningProcess} | Stop-Process -Force
```

### React Server Won't Start
```powershell
cd client-app
npm install
npm run dev
```

### Backend Build Issues
```powershell
dotnet clean
dotnet build
dotnet run --project Casan_IT15_Project/Casan_IT15_Project.csproj
```
