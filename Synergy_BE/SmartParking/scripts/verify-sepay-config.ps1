# ===================================
# SEPAY CONFIGURATION VERIFICATION
# ===================================
# Verifies SePay environment variables and configuration

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SEPAY CONFIGURATION VERIFICATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check environment variables
Write-Host "1. Checking environment variables..." -ForegroundColor Yellow
Write-Host ""

$apiKey = $env:SEPAY_API_KEY
$webhookSecret = $env:SEPAY_WEBHOOK_SECRET

if ($apiKey) {
    Write-Host "✅ SEPAY_API_KEY is set" -ForegroundColor Green
    Write-Host "   Value: $($apiKey.Substring(0, [Math]::Min(15, $apiKey.Length)))..." -ForegroundColor Gray
} else {
    Write-Host "❌ SEPAY_API_KEY is NOT set" -ForegroundColor Red
    Write-Host "   Run: `$env:SEPAY_API_KEY = 'your-api-key'" -ForegroundColor Yellow
}

Write-Host ""

if ($webhookSecret) {
    Write-Host "✅ SEPAY_WEBHOOK_SECRET is set" -ForegroundColor Green
    Write-Host "   Length: $($webhookSecret.Length) characters" -ForegroundColor Gray
    
    if ($webhookSecret.Length -lt 32) {
        Write-Host "   ⚠️  WARNING: Secret should be at least 32 characters" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ SEPAY_WEBHOOK_SECRET is NOT set" -ForegroundColor Red
    Write-Host "   Run: `$env:SEPAY_WEBHOOK_SECRET = 'your-32-char-secret'" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check appsettings.json
Write-Host "2. Checking appsettings.Development.json..." -ForegroundColor Yellow
Write-Host ""

$appsettingsPath = "src\SmartParking.API\appsettings.Development.json"

if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $sepay = $appsettings.SePay
    
    if ($sepay) {
        Write-Host "✅ SePay section found" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "   Enabled: $($sepay.Enabled)" -ForegroundColor Gray
        Write-Host "   MerchantId: $($sepay.MerchantId)" -ForegroundColor Gray
        Write-Host "   ApiKey: $(if ($sepay.ApiKey) { 'HARDCODED (should be empty)' } else { 'Empty (correct)' })" -ForegroundColor $(if ($sepay.ApiKey) { 'Yellow' } else { 'Green' })
        Write-Host "   WebhookSecret: $(if ($sepay.WebhookSecret) { 'HARDCODED (should be empty)' } else { 'Empty (correct)' })" -ForegroundColor $(if ($sepay.WebhookSecret) { 'Yellow' } else { 'Green' })
        Write-Host "   Bank.Code: $($sepay.Bank.Code)" -ForegroundColor Gray
        Write-Host "   Bank.AccountNumber: $($sepay.Bank.AccountNumber)" -ForegroundColor Gray
        Write-Host "   Bank.AccountName: $($sepay.Bank.AccountName)" -ForegroundColor Gray
        Write-Host "   Urls.WebhookUrl: $($sepay.Urls.WebhookUrl)" -ForegroundColor Gray
        Write-Host "   Webhook.RequireHttps: $($sepay.Webhook.RequireHttps)" -ForegroundColor Gray
    } else {
        Write-Host "❌ SePay section NOT found" -ForegroundColor Red
    }
} else {
    Write-Host "❌ appsettings.Development.json not found at: $appsettingsPath" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check .gitignore
Write-Host "3. Checking .gitignore..." -ForegroundColor Yellow
Write-Host ""

if (Test-Path ".gitignore") {
    $gitignore = Get-Content ".gitignore" -Raw
    
    if ($gitignore -match "\.env") {
        Write-Host "✅ .env files are ignored in git" -ForegroundColor Green
    } else {
        Write-Host "⚠️  .env is NOT in .gitignore (should add it)" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ .gitignore not found" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Summary
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host ""

$envVarsSet = $apiKey -and $webhookSecret
$secretsNotHardcoded = -not $sepay.ApiKey -and -not $sepay.WebhookSecret
$configValid = $sepay -and $sepay.Bank.Code -and $sepay.Bank.AccountNumber

if ($envVarsSet -and $secretsNotHardcoded -and $configValid) {
    Write-Host "✅ Configuration is READY for production" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. dotnet build" -ForegroundColor Gray
    Write-Host "2. dotnet run" -ForegroundColor Gray
    Write-Host "3. Check logs for: 'SePay configuration validated successfully'" -ForegroundColor Gray
} else {
    Write-Host "❌ Configuration needs attention" -ForegroundColor Red
    Write-Host ""
    Write-Host "Issues:" -ForegroundColor Yellow
    
    if (-not $envVarsSet) {
        Write-Host "- Environment variables not set" -ForegroundColor Gray
    }
    
    if (-not $secretsNotHardcoded) {
        Write-Host "- Secrets are hardcoded in appsettings (should be empty)" -ForegroundColor Gray
    }
    
    if (-not $configValid) {
        Write-Host "- Bank configuration incomplete" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "See: docs/SEPAY_ENVIRONMENT_VARIABLES.md" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
