# SmartParking Solution Restructure Script
# Run from: E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE\

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "SmartParking Solution Restructure Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if VS is running
$vsProcess = Get-Process -Name "devenv" -ErrorAction SilentlyContinue
if ($vsProcess) {
    Write-Host "ERROR: Visual Studio is running!" -ForegroundColor Red
    Write-Host "Please close Visual Studio before running this script." -ForegroundColor Yellow
    exit 1
}

# Check current location
$currentPath = Get-Location
if ($currentPath.Path -notlike "*Synergy_BE*") {
    Write-Host "Current path: $($currentPath.Path)" -ForegroundColor Yellow
    Write-Host "Please run from Synergy_BE folder" -ForegroundColor Red
    exit 1
}

# Check if old structure exists
if (-not (Test-Path "SmartParking.API")) {
    Write-Host "Error: SmartParking.API folder not found!" -ForegroundColor Red
    exit 1
}

# Check if new structure already exists
if (Test-Path "SmartParking") {
    Write-Host "Error: SmartParking folder already exists!" -ForegroundColor Red
    Write-Host "Please remove it first or choose a different name." -ForegroundColor Yellow
    exit 1
}

Write-Host "Step 1: Renaming root folder..." -ForegroundColor Cyan
Rename-Item -Path "SmartParking.API" -NewName "SmartParking"
Write-Host "OK - Renamed: SmartParking.API -> SmartParking" -ForegroundColor Green

Set-Location "SmartParking"

Write-Host "Step 2: Renaming solution file..." -ForegroundColor Cyan
Rename-Item -Path "SmartParking.API.sln" -NewName "SmartParking.sln"
Write-Host "OK - Renamed: SmartParking.API.sln -> SmartParking.sln" -ForegroundColor Green

Write-Host "Step 3: Creating new folders..." -ForegroundColor Cyan
New-Item -ItemType Directory -Path "src" -Force | Out-Null
New-Item -ItemType Directory -Path "docs" -Force | Out-Null
Write-Host "OK - Created: src/ and docs/ folders" -ForegroundColor Green

Write-Host "Step 4: Moving projects to src/..." -ForegroundColor Cyan
$projects = @("SmartParking.Domain", "SmartParking.Application", "SmartParking.Infrastructure", "SmartParking.API")
foreach ($project in $projects) {
    if (Test-Path $project) {
        Move-Item -Path $project -Destination "src\" -Force
        Write-Host "OK - Moved: $project" -ForegroundColor Green
    }
}

Write-Host "Step 5: Moving documentation..." -ForegroundColor Cyan
$docs = @("PROJECT_READY.md", "DEPLOYMENT_GUIDE.md", "RESTRUCTURE_GUIDE.md")
foreach ($doc in $docs) {
    if (Test-Path $doc) {
        Move-Item -Path $doc -Destination "docs\" -Force
        Write-Host "OK - Moved: $doc" -ForegroundColor Green
    }
}

Write-Host "Step 6: Updating solution file..." -ForegroundColor Cyan
$slnContent = Get-Content "SmartParking.sln" -Raw -Encoding UTF8
$slnContent = $slnContent -replace '"SmartParking\.Domain\\', '"src\SmartParking.Domain\'
$slnContent = $slnContent -replace '"SmartParking\.Application\\', '"src\SmartParking.Application\'
$slnContent = $slnContent -replace '"SmartParking\.Infrastructure\\', '"src\SmartParking.Infrastructure\'
$slnContent = $slnContent -replace '"SmartParking\.API\\', '"src\SmartParking.API\'
Set-Content "SmartParking.sln" -Value $slnContent -NoNewline -Encoding UTF8
Write-Host "OK - Updated solution paths" -ForegroundColor Green

Write-Host "Step 7: Creating README.md..." -ForegroundColor Cyan
$readme = @"
# SmartParking Backend API

Modern Smart Parking Management System built with ASP.NET Core 8.0

## Architecture

Clean Architecture with 3-layer separation:
- **Domain**: Entities, Enums, Constants
- **Application**: Business Logic, Services, DTOs
- **Infrastructure**: Data Access, External Services
- **API**: Controllers, Endpoints

## Quick Start

1. Open SmartParking.sln in Visual Studio
2. Update connection string in appsettings.json
3. Run database migration (see Database/README.md)
4. Press F5 to run
5. Navigate to https://localhost:7000/swagger

## Documentation

- [Project Status](docs/PROJECT_READY.md)
- [Deployment Guide](docs/DEPLOYMENT_GUIDE.md)

## Features

- JWT Authentication + Refresh Tokens
- Google OAuth Integration
- Vehicle Management
- Booking System
- VNPay Payment Integration

**Status**: Production Ready | **Version**: 1.0
"@
Set-Content "README.md" -Value $readme -Encoding UTF8
Write-Host "OK - Created README.md" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Restructure Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "New structure:" -ForegroundColor Cyan
Write-Host "SmartParking/"
Write-Host "├── SmartParking.sln" -ForegroundColor Green
Write-Host "├── README.md" -ForegroundColor Green
Write-Host "├── src/" -ForegroundColor Yellow
Write-Host "│   ├── SmartParking.Domain/"
Write-Host "│   ├── SmartParking.Application/"
Write-Host "│   ├── SmartParking.Infrastructure/"
Write-Host "│   └── SmartParking.API/"
Write-Host "├── Database/"
Write-Host "└── docs/"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Open Visual Studio"
Write-Host "2. Open: SmartParking\SmartParking.sln"
Write-Host "3. Build > Rebuild Solution"
Write-Host ""
Write-Host "Done!" -ForegroundColor Green
