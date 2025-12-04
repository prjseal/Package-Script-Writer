<#
.SYNOPSIS
    Displays build failure summary from artifacts.

.DESCRIPTION
    This script displays a formatted build failure summary by reading the
    build summary file from the artifacts directory.

.PARAMETER WorkspacePath
    The GitHub workspace path

.EXAMPLE
    .\Show-BuildFailureSummary.ps1 -WorkspacePath "/workspace"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$WorkspacePath
)

Write-Host "================================================" -ForegroundColor Red
Write-Host "❌ UPDATE PACKAGES WORKFLOW FAILED" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Red
Write-Host ""
Write-Host "The UpdateThirdPartyPackages script encountered build failures." -ForegroundColor Yellow
Write-Host ""
Write-Host "📋 Build Summary:" -ForegroundColor Cyan
$buildSummaryPath = "$WorkspacePath\.artifacts\build-summary.txt"
if (Test-Path $buildSummaryPath) {
    $buildSummary = Get-Content $buildSummaryPath -Raw
    Write-Host $buildSummary
}
else {
    Write-Host "Build summary file not found." -ForegroundColor Yellow
}
Write-Host ""
Write-Host "🔍 Check the logs above for detailed error messages." -ForegroundColor Cyan
Write-Host "📂 Build logs are saved in .artifacts/logs/ directory." -ForegroundColor Cyan
Write-Host ""
Write-Host "No PR will be created due to build failures." -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Red

# Add GitHub Action summary
$summary = @"
## ❌ Update Packages Workflow Failed

### 🚨 Build Failures Detected

The UpdateThirdPartyPackages script encountered build failures.

### 📋 Build Summary

"@

$buildSummaryPath = "$WorkspacePath\.artifacts\build-summary.txt"
if (Test-Path $buildSummaryPath) {
    $buildSummary = Get-Content $buildSummaryPath -Raw
    $summary += @"
``````
$buildSummary
``````

"@
}
else {
    $summary += "Build summary file not found.`n`n"
}

$summary += @"
### 🔍 Next Steps
- Check the logs above for detailed error messages
- Build logs are saved in `.artifacts/logs/` directory
- **No PR will be created** due to build failures

Fix the build errors and re-run the workflow.
"@

$summary | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8
