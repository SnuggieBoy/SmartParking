# ============================================
# SmartParking Solution Restructure Script
# ============================================
# IMPORTANT: Close Visual Studio before running!
# Run from: E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE\

param(
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

# Colors
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }

Write-Info "============================================"
Write-Info "SmartParking Solution Restructure Script"
Write-Info "============================================"
Write-Host ""

# Check if VS is running
$vsProcess = Get-Process | Where-Object { $_.ProcessName -like "*devenv*" }
if ($vsProcess -and -not $DryRun) {
    Write-Error "ERROR: Visual Studio is running!"
    Write-Warning "Please close Visual Studio before running this script."
    exit 1
}

# Check current location
$currentPath = Get-Location

if ($currentPath.Path -notlike "*Synergy_BE") {
    Write-Warning "Current path: $($currentPath.Path)"
    Write-Warning "Please run this from: E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE\"
    exit 1
}

# Check if old structure exists
if (-not (Test-Path "SmartParking.API")) {
    Write-Error "Error: SmartParking.API folder not found!"
    exit 1
}

if ($DryRun) {
    Write-Warning "DRY RUN MODE - No changes will be made"
    Write-Host ""
}

# Step 1: Rename root folder
Write-Info "Step 1: Renaming root folder..."
if (-not $DryRun) {
    if (Test-Path "SmartParking") {
        Write-Error "Error: 'SmartParking' folder already exists!"
        exit 1
    }
    Rename-Item -Path "SmartParking.API" -NewName "SmartParking"
    Write-Success "✓ Renamed: SmartParking.API → SmartParking"
} else {
    Write-Info "  Would rename: SmartParking.API → SmartParking"
}

# Change to new directory
if (-not $DryRun) {
    Set-Location "SmartParking"
}
$basePath = if ($DryRun) { "SmartParking.API" } else { "SmartParking" }

# Step 2: Rename solution file
Write-Info "Step 2: Renaming solution file..."
$slnOldPath = Join-Path $basePath "SmartParking.API.sln"
$slnNewPath = Join-Path $basePath "SmartParking.sln"

if (-not $DryRun) {
    Rename-Item -Path "SmartParking.API.sln" -NewName "SmartParking.sln"
    Write-Success "✓ Renamed: SmartParking.API.sln → SmartParking.sln"
} else {
    Write-Info "  Would rename: SmartParking.API.sln → SmartParking.sln"
}

# Step 3: Create new folders
Write-Info "Step 3: Creating new folder structure..."
if (-not $DryRun) {
    New-Item -ItemType Directory -Path "src" -Force | Out-Null
    New-Item -ItemType Directory -Path "docs" -Force | Out-Null
    Write-Success "✓ Created: src/ and docs/ folders"
} else {
    Write-Info "  Would create: src/ and docs/ folders"
}

# Step 4: Move projects to src/
Write-Info "Step 4: Moving projects to src/..."
$projects = @(
    "SmartParking.Domain",
    "SmartParking.Application",
    "SmartParking.Infrastructure",
    "SmartParking.API"
)

foreach ($project in $projects) {
    if (-not $DryRun) {
        if (Test-Path $project) {
            Move-Item -Path $project -Destination "src\" -Force
            Write-Success "✓ Moved: $project → src\$project"
        } else {
            Write-Warning "⚠ Project not found: $project"
        }
    } else {
        Write-Info "  Would move: $project → src\$project"
    }
}

# Step 5: Move docs
Write-Info "Step 5: Moving documentation files..."
$docFiles = @("PROJECT_READY.md", "DEPLOYMENT_GUIDE.md", "RESTRUCTURE_GUIDE.md")

foreach ($file in $docFiles) {
    if (-not $DryRun) {
        if (Test-Path $file) {
            Move-Item -Path $file -Destination "docs\" -Force
            Write-Success "✓ Moved: $file → docs\$file"
        }
    } else {
        Write-Info "  Would move: $file → docs\$file (if exists)"
    }
}

# Step 6: Update solution file
Write-Info "Step 6: Updating solution file references..."
if (-not $DryRun) {
    $slnContent = Get-Content "SmartParking.sln" -Raw -Encoding UTF8
    
    # Replace project paths
    $slnContent = $slnContent -replace '"SmartParking\.Domain\\', '"src\SmartParking.Domain\'
    $slnContent = $slnContent -replace '"SmartParking\.Application\\', '"src\SmartParking.Application\'
    $slnContent = $slnContent -replace '"SmartParking\.Infrastructure\\', '"src\SmartParking.Infrastructure\'
    $slnContent = $slnContent -replace '"SmartParking\.API\\', '"src\SmartParking.API\'
    
    Set-Content "SmartParking.sln" -Value $slnContent -NoNewline -Encoding UTF8
    Write-Success "✓ Updated solution file with new paths"
} else {
    Write-Info "  Would update solution file paths"
}

# Step 7: Create README if not exists
Write-Info "Step 7: Creating README.md..."
if (-not $DryRun) {
    if (-not (Test-Path "README.md")) {
        $readmeContent = @"
# SmartParking Backend API

Modern Smart Parking Management System built with ASP.NET Core 8.0

## 🏗️ Architecture

This project follows Clean Architecture with 3-layer separation:

- **Domain**: Entities, Enums, Constants
- **Application**: Business Logic, Services, DTOs
- **Infrastructure**: Data Access, External Services
- **API**: Controllers, Endpoints

## 🚀 Quick Start

``````bash
# 1. Restore dependencies
dotnet restore

# 2. Update connection string in appsettings.json

# 3. Run migrations (see Database/README.md)

# 4. Run application
cd src/SmartParking.API
dotnet run
``````

## 📁 Project Structure

``````
SmartParking/
├── src/
│   ├── SmartParking.Domain/
│   ├── SmartParking.Application/
│   ├── SmartParking.Infrastructure/
│   └── SmartParking.API/
├── Database/
├── docs/
└── README.md
``````

## 📚 Documentation

- [Project Ready Guide](docs/PROJECT_READY.md)
- [Deployment Guide](docs/DEPLOYMENT_GUIDE.md)
- [Database Setup](Database/README.md)

## 🔑 Features

- JWT Authentication + Refresh Tokens
- Google OAuth Integration
- Vehicle Management
- Parking Lot Management
- Booking System
- VNPay Payment Integration
- Role-based Authorization

## 🛠️ Tech Stack

- .NET 8.0
- Entity Framework Core 8.0
- SQL Server
- JWT Bearer Authentication
- BCrypt Password Hashing
- VNPay Payment Gateway

## 📞 API Endpoints

- **Auth**: `/api/auth/*`
- **Vehicles**: `/api/vehicles/*`
- **Parking Lots**: `/api/parking-lots/*`
- **Bookings**: `/api/bookings/*`
- **Payments**: `/api/payments/*`

View full API documentation at: `https://localhost:7000/swagger`

---

**Status**: Production Ready  
**Version**: 1.0  
**Last Updated**: January 25, 2026
"@
        Set-Content "README.md" -Value $readmeContent -Encoding UTF8
        Write-Success "✓ Created README.md"
    } else {
        Write-Info "  README.md already exists (skipped)"
    }
} else {
    Write-Info "  Would create README.md"
}

# Summary
Write-Host ""
Write-Info "============================================"
if (-not $DryRun) {
    Write-Success "Restructure Complete! ✓"
} else {
    Write-Warning "DRY RUN Complete - No changes made"
}
Write-Info "============================================"
Write-Host ""

if (-not $DryRun) {
    Write-Info "New structure:"
    Write-Host ""
    Write-Host "SmartParking/" -ForegroundColor White
    Write-Host "├── SmartParking.sln" -ForegroundColor Green
    Write-Host "├── README.md" -ForegroundColor Green
    Write-Host "├── src/" -ForegroundColor Cyan
    Write-Host "│   ├── SmartParking.Domain/" -ForegroundColor Yellow
    Write-Host "│   ├── SmartParking.Application/" -ForegroundColor Yellow
    Write-Host "│   ├── SmartParking.Infrastructure/" -ForegroundColor Yellow
    Write-Host "│   └── SmartParking.API/" -ForegroundColor Yellow
    Write-Host "├── Database/" -ForegroundColor Magenta
    Write-Host "│   ├── Migration_*.sql"
    Write-Host "│   └── SeedData.sql"
    Write-Host "└── docs/" -ForegroundColor Magenta
    Write-Host "    ├── PROJECT_READY.md"
    Write-Host "    └── DEPLOYMENT_GUIDE.md"
    Write-Host ""
    
    Write-Info "✅ Next steps:"
    Write-Host "1. Open Visual Studio"
    Write-Host "2. Open: SmartParking\SmartParking.sln"
    Write-Host "3. Build > Rebuild Solution"
    Write-Host "4. Run and test"
    Write-Host ""
    Write-Success "All done! 🎉"
} else {
    Write-Host ""
    Write-Warning "This was a DRY RUN - No changes were made"
    Write-Host ""
    Write-Info "To apply changes, run:"
    Write-Host "  .\Restructure-Solution.ps1" -ForegroundColor Yellow
    Write-Host ""
    Write-Info "Or run without opening PowerShell:"
    Write-Host "  powershell -ExecutionPolicy Bypass -File .\Restructure-Solution.ps1" -ForegroundColor Yellow
}
