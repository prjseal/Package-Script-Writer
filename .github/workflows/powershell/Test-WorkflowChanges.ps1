<#
.SYNOPSIS
    Checks if any changes were made by the workflow.

.DESCRIPTION
    This script checks if NuGet packages were updated,
    and outputs a summary if no changes were made.

.PARAMETER WorkspacePath
    The GitHub workspace path

.EXAMPLE
    .\Test-WorkflowChanges.ps1 -WorkspacePath "/workspace"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$WorkspacePath
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Checking for Workflow Changes" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Check if packages were updated by reading the summary file
$summaryPath = "$WorkspacePath\.artifacts\package-summary.txt"
$packagesUpdated = $false
if (Test-Path $summaryPath) {
    $content = Get-Content $summaryPath -Raw
    $packagesUpdated = $content -notmatch 'No packages to update'
    Write-Host "Package summary file found. Packages updated: $packagesUpdated" -ForegroundColor Magenta
}
else {
    Write-Host "Package summary file not found at: $summaryPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Packages updated: $packagesUpdated (boolean)" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

if (-not $packagesUpdated) {
    Write-Host "ENTERING: No changes block" -ForegroundColor Green
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "✅ No Changes Needed - Workflow Complete" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Summary:" -ForegroundColor Cyan
    Write-Host "  • NuGet Packages: All packages are already at their latest versions" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "No branch created, no commits made, no PR needed." -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Setting has_changes=false" -ForegroundColor Cyan

    # Add GitHub Action summary
    $summary = @"
## ✅ No Changes Needed - Workflow Complete

### 📋 Summary
- **NuGet Packages**: All packages are already at their latest versions

### 📌 Result
No branch created, no commits made, no PR needed.
"@
    $summary | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8

    echo "has_changes=false" >> $env:GITHUB_OUTPUT
    exit 0
}
else {
    Write-Host "ENTERING: Changes detected block" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Changes detected - will proceed with commit and PR creation" -ForegroundColor Cyan
    Write-Host "Setting has_changes=true" -ForegroundColor Cyan

    # Add GitHub Action summary
    $summary = @"
## 🔄 Changes Detected

**Changes found**: NuGet packages updated

Proceeding with commit and PR creation...
"@
    $summary | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8

    echo "has_changes=true" >> $env:GITHUB_OUTPUT
}
