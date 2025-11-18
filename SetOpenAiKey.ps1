# Set OpenAI API Key for OCR Feature
# This script helps you securely configure the OpenAI API key for Aspire AppHost
# The key will be stored in AppHost user secrets and passed to the API at runtime

Write-Host "OpenAI API Key Configuration (Aspire)" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will configure your OpenAI API key for the OCR feature." -ForegroundColor Yellow
Write-Host "The key will be stored securely in Aspire AppHost user secrets." -ForegroundColor Yellow
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

# Set the user secret in AppHost project
try {
    Push-Location "$PSScriptRoot\AppHost"
    
    Write-Host ""
    Write-Host "Initializing user secrets for AppHost (if needed)..." -ForegroundColor Cyan
    
    # Initialize user secrets if not already done
    $null = dotnet user-secrets init 2>&1
    
    Write-Host "Setting OpenAI API key parameter..." -ForegroundColor Cyan
    
    # Set the parameter in AppHost user secrets
    dotnet user-secrets set "Parameters:openai-apikey" $apiKey
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ OpenAI API key configured successfully for Aspire!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "  1. Start Aspire: cd AppHost && dotnet run" -ForegroundColor Gray
        Write-Host "  2. The API will automatically receive the OpenAI key" -ForegroundColor Gray
        Write-Host ""
        Write-Host "You can verify the configuration with:" -ForegroundColor Gray
        Write-Host "  cd AppHost" -ForegroundColor Gray
        Write-Host "  dotnet user-secrets list" -ForegroundColor Gray
        Write-Host ""
        Write-Host "The OCR feature is now ready to use!" -ForegroundColor Green
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
