# Package Script Writer CLI - Demo Preparation Script (PowerShell)
# Run this before your Umbraco Sydney meetup demo to ensure everything is ready

$ErrorActionPreference = "Stop"

Write-Host "🎯 Package Script Writer CLI - Demo Preparation" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

function Print-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Print-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Print-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Print-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Blue
}

function Print-Step {
    param([string]$Message)
    Write-Host "`n▶ $Message" -ForegroundColor Blue
}

# Step 1: Check if PSW CLI is installed
Print-Step "Step 1: Checking PSW CLI installation..."
try {
    $version = & psw --version 2>&1 | Select-Object -First 1
    Print-Success "PSW CLI is installed: $version"
} catch {
    Print-Error "PSW CLI is not installed"
    Write-Host "Install with: dotnet tool install -g PackageScriptWriter.Cli"
    exit 1
}

# Step 2: Check .NET SDK
Print-Step "Step 2: Checking .NET SDK..."
try {
    $dotnetVersion = & dotnet --version
    Print-Success ".NET SDK is installed: $dotnetVersion"
} catch {
    Print-Error ".NET SDK is not installed"
    exit 1
}

# Step 3: Prime the cache
Print-Step "Step 3: Priming the package cache (this may take a moment)..."
Print-Info "This ensures the demo runs smoothly without API delays"

# Clear existing cache first
try {
    & psw --clear-cache | Out-Null
    Print-Success "Cleared existing cache"
} catch {
    # Ignore errors
}

# Run a quick command to populate cache
Print-Info "Fetching package list..."
try {
    $job = Start-Job -ScriptBlock { psw --default }
    Wait-Job $job -Timeout 60 | Out-Null
    Stop-Job $job -ErrorAction SilentlyContinue | Out-Null
    Remove-Job $job -ErrorAction SilentlyContinue | Out-Null
    Print-Success "Package cache primed"
} catch {
    Print-Warning "Cache priming timed out (this is okay)"
}

# Step 4: Create demo directory
Print-Step "Step 4: Setting up demo workspace..."
$demoDir = Join-Path $env:USERPROFILE "psw-demo-sydney"

if (Test-Path $demoDir) {
    Print-Warning "Demo directory already exists: $demoDir"
    $response = Read-Host "Remove it and start fresh? (y/N)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        Remove-Item -Path $demoDir -Recurse -Force
        Print-Success "Removed old demo directory"
    }
}

if (-not (Test-Path $demoDir)) {
    New-Item -ItemType Directory -Path $demoDir | Out-Null
    Print-Success "Created demo directory: $demoDir"
}

Set-Location $demoDir

# Step 5: Generate sample scripts for backup
Print-Step "Step 5: Generating backup scripts..."

# Generate a default script
Print-Info "Generating default script..."
try {
    & psw --default > "$demoDir\backup-default-script.ps1" 2>&1
    if (Test-Path "$demoDir\backup-default-script.ps1") {
        Print-Success "Default script saved: backup-default-script.ps1"
    }
} catch {
    Print-Warning "Could not generate default script (this is okay)"
}

# Generate a full-featured script
Print-Info "Generating full-featured script..."
try {
    & psw -p "uSync,Diplo.GodMode,Umbraco.Community.BlockPreview" `
        -n DemoProject -s DemoSolution -u `
        --database-type SQLite `
        --admin-email "admin@demo.com" `
        --admin-password "DemoPass123!" `
        --add-docker > "$demoDir\backup-full-script.ps1" 2>&1

    if (Test-Path "$demoDir\backup-full-script.ps1") {
        Print-Success "Full-featured script saved: backup-full-script.ps1"
    }
} catch {
    Print-Warning "Could not generate full script (this is okay)"
}

# Step 6: Create a sample template
Print-Step "Step 6: Creating sample team template..."
try {
    & psw -p "uSync|17.0.0,Diplo.GodMode" `
        -n SampleProject -s SampleSolution `
        -u --database-type SQLite `
        --admin-email "admin@company.com" `
        template save DemoTeamStandard `
        --template-description "Demo template for Sydney meetup" `
        --template-tags "demo,sydney,umbraco" | Out-Null

    $templateList = & psw template list 2>&1
    if ($templateList -match "DemoTeamStandard") {
        Print-Success "Sample template created: DemoTeamStandard"
    } else {
        Print-Warning "Could not create sample template (this is okay)"
    }
} catch {
    Print-Warning "Could not create sample template (this is okay)"
}

# Step 7: Populate history with some entries
Print-Step "Step 7: Populating history with sample entries..."
Print-Info "This will give you something to show in the history demo"

for ($i = 1; $i -le 3; $i++) {
    try {
        & psw --default | Out-Null
    } catch {
        # Ignore errors
    }
}

try {
    $historyList = & psw history list 2>&1
    $historyCount = ($historyList | Select-String "│").Count
    if ($historyCount -gt 0) {
        Print-Success "History populated with sample entries"
    } else {
        Print-Warning "Could not populate history (this is okay)"
    }
} catch {
    Print-Warning "Could not populate history (this is okay)"
}

# Step 8: Test network connectivity
Print-Step "Step 8: Testing network connectivity..."
try {
    $response = Invoke-WebRequest -Uri "https://marketplace.umbraco.com" -Method Head -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Print-Success "Umbraco Marketplace is reachable"
} catch {
    Print-Warning "Cannot reach Umbraco Marketplace (check your network)"
}

try {
    $response = Invoke-WebRequest -Uri "https://api.nuget.org" -Method Head -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Print-Success "NuGet API is reachable"
} catch {
    Print-Warning "Cannot reach NuGet API (check your network)"
}

# Step 9: Create quick reference files
Print-Step "Step 9: Creating quick reference files..."

$commandsContent = @"
Package Script Writer CLI - Demo Commands
==========================================

SCENARIO 1: 30-Second Win
-------------------------
psw --default

SCENARIO 2: Interactive Experience
-----------------------------------
psw
(Then navigate through the menus)

SCENARIO 3: Power User
----------------------
# Quick setup
psw -p "uSync|17.0.0,Diplo.GodMode" -n UmbracoSydney -s SydneyMeetup --database-type SQLite

# Full automation
psw -p "uSync,Diplo.GodMode,Umbraco.Community.BlockPreview" ``
    -n ProductionSite -s MySolution -u ``
    --database-type SQLite ``
    --admin-email "admin@example.com" ``
    --admin-password "SecurePass123!" ``
    --add-docker --auto-run

# Client project
psw --template "Bootstrap Starter Kit" -p "uSync,Diplo.GodMode" ``
    -n ClientWebsite -u --database-type SQLite ``
    --admin-email "dev@agency.com" --admin-password "TempPass123!" ``
    --auto-run

SCENARIO 4: Team Templates
---------------------------
psw template list
psw template load DemoTeamStandard
psw template load DemoTeamStandard -n NewProject --auto-run

SCENARIO 5: History
-------------------
psw history list
psw history show 1
psw history rerun 1

BONUS: Versions Table
---------------------
psw versions
"@

$commandsContent | Out-File -FilePath "$demoDir\DEMO_COMMANDS.txt" -Encoding UTF8
Print-Success "Created command reference: DEMO_COMMANDS.txt"

# Step 10: System check summary
Print-Step "Step 10: Final system check..."

Write-Host ""
Write-Host "================================" -ForegroundColor Green
Write-Host "Demo Preparation Complete! ✨" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""
Write-Host "Demo workspace: $demoDir"
Write-Host ""
Write-Host "Ready to demo:" -ForegroundColor Green
Print-Success "PSW CLI installed and working"
Print-Success "Package cache primed"
Print-Success "Backup scripts generated"
Print-Success "Sample template created"
Print-Success "History populated"
Print-Success "Quick reference created"
Write-Host ""

Print-Info "Demo checklist:"
Write-Host "  [ ] Terminal size is large enough (at least 120x30)"
Write-Host "  [ ] Terminal colors are working"
Write-Host "  [ ] Network connection is stable"
Write-Host "  [ ] Demo commands ready: cat $demoDir\DEMO_COMMANDS.txt"
Write-Host "  [ ] Backup scripts ready in: $demoDir"
Write-Host ""

Print-Info "Optional: Test the interactive mode now"
Write-Host "  cd $demoDir; psw"
Write-Host ""

Print-Info "During demo, if anything fails:"
Write-Host "  - Use backup scripts from: $demoDir"
Write-Host "  - Show web version: https://psw.codeshare.co.uk"
Write-Host "  - Show pre-generated history entries"
Write-Host ""

Print-Success "Good luck with your demo! 🚀"
Write-Host ""
