<#
.SYNOPSIS
    Displays summary when PR creation is skipped due to existing PR.

.DESCRIPTION
    This script displays a formatted summary explaining that a PR was not
    created because an existing PR already has identical package updates.

.PARAMETER ExistingPrNumber
    The number of the existing PR

.PARAMETER Repository
    The GitHub repository in owner/repo format

.EXAMPLE
    .\Show-PullRequestSkippedSummary.ps1 -ExistingPrNumber "123" -Repository "owner/repo"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ExistingPrNumber,

    [Parameter(Mandatory = $true)]
    [string]$Repository
)

Write-Host "================================================" -ForegroundColor Yellow
Write-Host "PR CREATION SKIPPED" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "An existing PR (#$ExistingPrNumber) already has identical package updates." -ForegroundColor Cyan
Write-Host "No need to create a duplicate PR." -ForegroundColor Cyan
Write-Host ""
Write-Host "Existing PR: https://github.com/$Repository/pull/$ExistingPrNumber" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Yellow

# Add GitHub Action summary
$prUrl = "https://github.com/$Repository/pull/$ExistingPrNumber"
$summary = @"
## ⚠️ PR Creation Skipped

An existing PR already has identical package updates.

### 📌 Existing PR
[#$ExistingPrNumber]($prUrl)

**Reason**: No need to create a duplicate PR with the same package updates.
"@
$summary | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
