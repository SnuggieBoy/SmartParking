# ============================================
# SmartParking Solution Restructure Script
# ============================================
# IMPORTANT: Run this from PowerShell as Administrator
# Location: E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE\

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

# Check current location
$currentPath = Get-Location
$expectedPath = "E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE"

if ($currentPath.Path -ne $expectedPath) {
    Write-Warning "Current path: $($currentPath.Path)"
    Write-Warning "Expected path: $expectedPath"
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne "y") {
        Write-Error "Aborted."
        exit 1
    }
}

# Check if old structure exists
if (-not (Test-Path "SmartParking.API")) {
    Write-Error "Error: SmartParking.API folder not found!"
    Write-Error "Make sure you're running this from: $expectedPath"
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
        Write-Error "Error: SmartParking folder already exists!"
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

# Step 2: Rename solution file
Write-Info "Step 2: Renaming solution file..."
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
            Move-Item -Path $project -Destination "src\$project" -Force
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
$docFiles = @(
    "PROJECT_READY.md",
    "DEPLOYMENT_GUIDE.md"
)

foreach ($file in $docFiles) {
    if (-not $DryRun) {
        if (Test-Path $file) {
            Move-Item -Path $file -Destination "docs\$file" -Force
            Write-Success "✓ Moved: $file → docs\$file"
        } else {
            Write-Info "  File not found (skipped): $file"
        }
    } else {
        Write-Info "  Would move: $file → docs\$file"
    }
}

# Step 6: Update solution file
Write-Info "Step 6: Updating solution file references..."
if (-not $DryRun) {
    $slnContent = Get-Content "SmartParking.sln" -Raw
    
    # Replace project paths
    $slnContent = $slnContent -replace 'SmartParking\.Domain\\', 'src\SmartParking.Domain\'
    $slnContent = $slnContent -replace 'SmartParking\.Application\\', 'src\SmartParking.Application\'
    $slnContent = $slnContent -replace 'SmartParking\.Infrastructure\\', 'src\SmartParking.Infrastructure\'
    $slnContent = $slnContent -replace 'SmartParking\.API\\', 'src\SmartParking.API\'
    
    Set-Content "SmartParking.sln" -Value $slnContent -NoNewline
    Write-Success "✓ Updated solution file with new paths"
} else {
    Write-Info "  Would update solution file paths"
}

# Step 7: Summary
Write-Host ""
Write-Info "============================================"
Write-Success "Restructure Complete!"
Write-Info "============================================"
Write-Host ""

if (-not $DryRun) {
    Write-Info "New structure:"
    Write-Host "SmartParking/"
    Write-Host "├── SmartParking.sln"
    Write-Host "├── src/"
    Write-Host "│   ├── SmartParking.Domain/"
    Write-Host "│   ├── SmartParking.Application/"
    Write-Host "│   ├── SmartParking.Infrastructure/"
    Write-Host "│   └── SmartParking.API/"
    Write-Host "├── Database/"
    Write-Host "└── docs/"
    Write-Host ""
    
    Write-Info "Next steps:"
    Write-Host "1. Open Visual Studio"
    Write-Host "2. Open: SmartParking\SmartParking.sln"
    Write-Host "3. Build > Rebuild Solution"
    Write-Host "4. Verify all projects build successfully"
    Write-Host ""
} else {
    Write-Warning "This was a DRY RUN - no changes were made."
    Write-Info "Run without -DryRun flag to apply changes:"
    Write-Host "  .\Restructure-Solution.ps1"
}

Write-Success "Done!"
