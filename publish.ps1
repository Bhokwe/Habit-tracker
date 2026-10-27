# Rebuild personal test packages into .\Releases
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

New-Item -ItemType Directory -Force -Path ".\Releases\Windows", ".\Releases\Android", ".\Releases\iOS" | Out-Null

Write-Host "Publishing Windows..." -ForegroundColor Cyan
dotnet publish ".\Habit tracker.csproj" -c Release -f net9.0-windows10.0.19041.0 -p:WindowsPackageType=None -o ".\Releases\Windows"
if ($LASTEXITCODE -ne 0) { throw "Windows publish failed" }

Write-Host "Publishing Android APK..." -ForegroundColor Cyan
dotnet publish ".\Habit tracker.csproj" -c Release -f net9.0-android -p:AndroidPackageFormats=apk -o ".\Releases\Android"
if ($LASTEXITCODE -ne 0) { throw "Android publish failed" }

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "  Windows: Releases\Windows\Habit tracker.exe  (copy whole folder)"
Write-Host "  Android: Releases\Android\com.companyname.habittracker-Signed.apk"
Write-Host "  iOS:     needs a Mac — see Releases\HOW_TO_INSTALL.txt"
