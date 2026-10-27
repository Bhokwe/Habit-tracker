# Rebuild personal test packages into .\Releases
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

Write-Host "Cleaning old Releases..." -ForegroundColor Yellow
if (Test-Path ".\Releases") { Remove-Item ".\Releases" -Recurse -Force }
New-Item -ItemType Directory -Force -Path ".\Releases\Windows", ".\Releases\Android", ".\Releases\iOS" | Out-Null

Write-Host "Publishing Windows..." -ForegroundColor Cyan
dotnet publish ".\Habit tracker.csproj" -c Release -f net9.0-windows10.0.19041.0 -o ".\Releases\Windows"
if ($LASTEXITCODE -ne 0) { throw "Windows publish failed" }

Write-Host "Publishing Android APK..." -ForegroundColor Cyan
dotnet publish ".\Habit tracker.csproj" -c Release -f net9.0-android -p:AndroidPackageFormats=apk -o ".\Releases\Android"
if ($LASTEXITCODE -ne 0) { throw "Android publish failed" }

@'
@echo off
cd /d "%~dp0"
start "" "Habit tracker.exe"
'@ | Set-Content -Encoding ASCII ".\Releases\Windows\Start Habit Tracker.bat"

Compress-Archive -Path ".\Releases\Windows\*" -DestinationPath ".\Releases\HabitTracker-Windows.zip" -Force
Copy-Item ".\Releases\Android\com.companyname.habittracker-Signed.apk" ".\Releases\HabitTracker-Android.apk" -Force

Write-Host "Done." -ForegroundColor Green
Get-Item ".\Releases\HabitTracker-Windows.zip", ".\Releases\HabitTracker-Android.apk" |
  Format-Table Name, @{N='MB';E={[math]::Round($_.Length/1MB,2)}} -AutoSize
