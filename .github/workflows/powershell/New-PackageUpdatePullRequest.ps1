<#
.SYNOPSIS
    Creates a pull request for package updates.

.DESCRIPTION
    This script creates a pull request with details about package updates,
    including custom NuGet sources if provided.

.PARAMETER PackageSummary
    The package summary content

.PARAMETER IncludePrerelease
    Boolean string indicating if prerelease versions were included

.PARAMETER NuGetSources
    Comma-separated custom NuGet source URLs

.PARAMETER BranchName
    The branch name to create the PR from

.PARAMETER WorkspacePath
    The GitHub workspace path

.EXAMPLE
    .\New-PackageUpdatePullRequest.ps1 -PackageSummary "..." -IncludePrerelease "false" -NuGetSources "" -BranchName "update-nuget-packages-20250101120000" -WorkspacePath "/workspace"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$PackageSummary,

    [Parameter(Mandatory = $true)]
    [string]$IncludePrerelease,

    [Parameter(Mandatory = $false)]
    [string]$NuGetSources = "",

    [Parameter(Mandatory = $true)]
    [string]$BranchName,

    [Parameter(Mandatory = $true)]
    [string]$WorkspacePath
)

# Create PR body in a file to avoid escaping issues
$prBodyFile = "$WorkspacePath\.artifacts\pr-body.md"

# Determine what was updated
$summaryContent = $PackageSummary
$packagesUpdated = ($summaryContent -notmatch 'No package summary found') -and ($summaryContent -notmatch 'No packages to update')

# Write PR body content line by line
"This PR updates the following:" | Out-File -FilePath $prBodyFile -Encoding UTF8
"" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append

if ($packagesUpdated) {
    "- ✅ **NuGet Packages** - Updated to latest versions" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
}

"" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
"**Triggered by:** $env:GITHUB_ACTOR" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append

# Add custom NuGet sources if provided
if (-not [string]::IsNullOrWhiteSpace($NuGetSources)) {
    "" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "## Custom NuGet Sources" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "This PR was created with custom NuGet sources. The PR build will automatically use these sources:" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append

    # Split comma-separated sources and add each as a nuget-source line
    $sources = $NuGetSources -split ',' | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($sourceUrl in $sources) {
        "nuget-source: $sourceUrl" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    }
}

if ($packagesUpdated) {
    "" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "**IncludePrerelease:** $IncludePrerelease" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "### Updated Packages:" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "``````" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "$PackageSummary" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
    "``````" | Out-File -FilePath $prBodyFile -Encoding UTF8 -Append
}

$prOutput = gh pr create `
    --title "Update NuGet packages" `
    --body-file $prBodyFile `
    --base main `
    --head $BranchName

# Extract PR URL from output
$prUrl = $prOutput | Select-Object -Last 1
$prNumber = ""
if ($prUrl -match '/pull/(\d+)') {
    $prNumber = $matches[1]
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "✅ Pull Request Created Successfully" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "PR URL: $prUrl" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Green

# Add GitHub Action summary
$summary = @"
## ✅ Pull Request Created

### 📋 Summary
"@

if ($packagesUpdated) {
    $summary += "`n- **NuGet Packages**: Updated to latest versions"
}

$summary += @"

### 🔗 Pull Request
[$prNumber]($prUrl)

"@

if ($packagesUpdated) {
    $summary += @"
### 📦 Updated Packages
``````
$PackageSummary
``````

"@
}

if (-not [string]::IsNullOrWhiteSpace($NuGetSources)) {
    $summary += @"
### 🔧 Custom NuGet Sources
This PR was created with custom NuGet sources.

"@
}

$summary += "**Include Prerelease**: $IncludePrerelease"

$summary | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
