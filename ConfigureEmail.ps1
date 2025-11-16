# Configure Email Notifications Script
# This script helps you securely configure SMTP settings for email notifications

Write-Host "Email Notification Configuration" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will configure SMTP settings for email notifications." -ForegroundColor Yellow
Write-Host "Settings will be stored securely in .NET user secrets." -ForegroundColor Yellow
Write-Host ""

# Prompt for SMTP settings
Write-Host "Choose your email provider:" -ForegroundColor Cyan
Write-Host "1. Gmail (smtp.gmail.com:587)"
Write-Host "2. Outlook/Office365 (smtp.office365.com:587)"
Write-Host "3. SendGrid (smtp.sendgrid.net:587)"
Write-Host "4. Custom SMTP server"
Write-Host ""
$provider = Read-Host "Enter choice (1-4)"

switch ($provider) {
    "1" {
        $smtpHost = "smtp.gmail.com"
        $smtpPort = 587
        Write-Host ""
        Write-Host "For Gmail, you'll need to:" -ForegroundColor Yellow
        Write-Host "1. Enable 2-factor authentication on your Google account" -ForegroundColor Yellow
        Write-Host "2. Create an App Password at https://myaccount.google.com/apppasswords" -ForegroundColor Yellow
        Write-Host "3. Use the App Password (not your regular password) below" -ForegroundColor Yellow
        Write-Host ""
    }
    "2" {
        $smtpHost = "smtp.office365.com"
        $smtpPort = 587
    }
    "3" {
        $smtpHost = "smtp.sendgrid.net"
        $smtpPort = 587
        Write-Host ""
        Write-Host "For SendGrid:" -ForegroundColor Yellow
        Write-Host "- Username: 'apikey' (literal string)" -ForegroundColor Yellow
        Write-Host "- Password: Your SendGrid API key" -ForegroundColor Yellow
        Write-Host ""
    }
    "4" {
        $smtpHost = Read-Host "Enter SMTP host"
        $smtpPort = Read-Host "Enter SMTP port (default 587)"
        if ([string]::IsNullOrWhiteSpace($smtpPort)) { $smtpPort = 587 }
    }
    default {
        Write-Host "Invalid choice. Exiting." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
$smtpUsername = Read-Host "Enter SMTP username/email"
$smtpPassword = Read-Host "Enter SMTP password" -AsSecureString
$smtpPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($smtpPassword))

$fromEmail = Read-Host "Enter 'From' email address (press Enter to use username)"
if ([string]::IsNullOrWhiteSpace($fromEmail)) { $fromEmail = $smtpUsername }

$fromName = Read-Host "Enter 'From' name (default: Warranty App)"
if ([string]::IsNullOrWhiteSpace($fromName)) { $fromName = "Warranty App" }

$useSsl = Read-Host "Use SSL/TLS? (Y/n)"
if ($useSsl -eq "n" -or $useSsl -eq "N") { $useSsl = "false" } else { $useSsl = "true" }

# Set user secrets
try {
    Push-Location "$PSScriptRoot\MyApi"
    
    Write-Host ""
    Write-Host "Configuring user secrets..." -ForegroundColor Cyan
    
    dotnet user-secrets set "Smtp:Host" $smtpHost
    dotnet user-secrets set "Smtp:Port" $smtpPort
    dotnet user-secrets set "Smtp:Username" $smtpUsername
    dotnet user-secrets set "Smtp:Password" $smtpPasswordPlain
    dotnet user-secrets set "Smtp:FromEmail" $fromEmail
    dotnet user-secrets set "Smtp:FromName" $fromName
    dotnet user-secrets set "Smtp:UseSsl" $useSsl
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ Email notification settings configured successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Configuration:" -ForegroundColor Gray
        Write-Host "  Host: $smtpHost" -ForegroundColor Gray
        Write-Host "  Port: $smtpPort" -ForegroundColor Gray
        Write-Host "  Username: $smtpUsername" -ForegroundColor Gray
        Write-Host "  From Email: $fromEmail" -ForegroundColor Gray
        Write-Host "  From Name: $fromName" -ForegroundColor Gray
        Write-Host "  Use SSL: $useSsl" -ForegroundColor Gray
        Write-Host ""
        Write-Host "You can verify with:" -ForegroundColor Gray
        Write-Host "  cd MyApi" -ForegroundColor Gray
        Write-Host "  dotnet user-secrets list" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Email notifications are now enabled!" -ForegroundColor Green
        Write-Host ""
        Write-Host "To test, create a receipt with a warranty expiring soon and wait for the notification check cycle." -ForegroundColor Yellow
    } else {
        Write-Host ""
        Write-Host "Error: Failed to configure user secrets" -ForegroundColor Red
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
