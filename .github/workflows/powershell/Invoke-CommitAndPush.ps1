<#
.SYNOPSIS
    Commits and pushes changes to a new branch.

.DESCRIPTION
    This script creates a new branch, commits changes with an appropriate message
    based on what was updated, and pushes to the remote repository.

.PARAMETER WorkspacePath
    The GitHub workspace path

.PARAMETER Repository
    The GitHub repository in owner/repo format

.PARAMETER PatToken
    Personal Access Token for authentication

.EXAMPLE
    .\Invoke-CommitAndPush.ps1 -WorkspacePath "/workspace" -Repository "owner/repo" -PatToken "token"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$WorkspacePath,

    [Parameter(Mandatory = $true)]
    [string]$Repository,

    [Parameter(Mandatory = $true)]
    [string]$PatToken
)

# Early exit: Check if there are actually any changes to commit
$summaryPath = "$WorkspacePath\.artifacts\package-summary.txt"
$packagesUpdated = $false
if (Test-Path $summaryPath) {
    $content = Get-Content $summaryPath -Raw
    $packagesUpdated = $content -notmatch 'No packages to update'
}

Write-Host "Commit check - Packages updated: $packagesUpdated (boolean)"

if (-not $packagesUpdated) {
    Write-Host "No changes detected (packages not updated). Exiting without creating branch." -ForegroundColor Yellow
    exit 0
}

$branchName = "update-nuget-packages-$(Get-Date -Format 'yyyyMMddHHmmss')"
echo "branchName=$branchName" >> $env:GITHUB_OUTPUT

git config user.name "github-actions"
git config user.email "github-actions@github.com"
git checkout -b $branchName
git add .
if (git diff --cached --quiet) {
    Write-Host "No changes detected in git. Skipping commit and PR."
    exit 0
}

$commitMessage = "Update NuGet packages"

git commit -m $commitMessage
git push https://x-access-token:$PatToken@github.com/$Repository.git $branchName
