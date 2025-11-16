# Set OpenAI API Key for OCR Feature
# This script helps you securely configure the OpenAI API key using .NET user secrets

Write-Host "OpenAI API Key Configuration" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will configure your OpenAI API key for the OCR feature." -ForegroundColor Yellow
Write-Host "The key will be stored securely in .NET user secrets (not in source control)." -ForegroundColor Yellow
Write-Host ""

# Prompt for API key
$apiKey = Read-Host "Enter your OpenAI API key (starts with 'sk-')"

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "Error: API key cannot be empty" -ForegroundColor Red
    exit 1
}

if (-not $apiKey.StartsWith("sk-")) {
    Write-Host "Warning: API key should typically start with 'sk-'" -ForegroundColor Yellow
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne "y") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit 0
    }
}

# Set the user secret
try {
    Push-Location "$PSScriptRoot\MyApi"
    
    Write-Host ""
    Write-Host "Setting user secret..." -ForegroundColor Cyan
    
    dotnet user-secrets set "OpenAI:ApiKey" $apiKey
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ OpenAI API key configured successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "You can verify the configuration with:" -ForegroundColor Gray
        Write-Host "  cd MyApi" -ForegroundColor Gray
        Write-Host "  dotnet user-secrets list" -ForegroundColor Gray
        Write-Host ""
        Write-Host "The OCR feature is now ready to use." -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "Error: Failed to set user secret" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
