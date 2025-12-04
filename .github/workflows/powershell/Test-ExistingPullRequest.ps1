<#
.SYNOPSIS
    Checks for existing pull requests with identical package updates.

.DESCRIPTION
    This script checks if there's already an open PR with the same package updates
    to avoid creating duplicate PRs.

.PARAMETER WorkspacePath
    The GitHub workspace path

.EXAMPLE
    .\Test-ExistingPullRequest.ps1 -WorkspacePath "/workspace"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$WorkspacePath
)

# Get current package summary from file
$summaryPath = "$WorkspacePath\.artifacts\package-summary.txt"
$currentSummary = Get-Content $summaryPath -Raw

# List all open PRs with branches matching our pattern
$existingPRs = gh pr list --state open --json number,headRefName,body --limit 100 | ConvertFrom-Json

$skipPR = $false
$matchingPR = $null

foreach ($pr in $existingPRs) {
    if ($pr.headRefName -like "update-nuget-packages-*") {
        # Extract the package table from the PR body
        $prBody = $pr.body

        # Extract content between triple backticks (the package table)
        if ($prBody -match '(?s)```(.+?)```') {
            $prPackages = $matches[1].Trim()

            # Compare with current summary (normalize whitespace for comparison)
            $currentNormalized = $currentSummary.Trim() -replace '\s+', ' '
            $prNormalized = $prPackages -replace '\s+', ' '

            if ($currentNormalized -eq $prNormalized) {
                $skipPR = $true
                $matchingPR = $pr.number
                Write-Host "Found existing PR #$matchingPR with identical package updates. Skipping PR creation."
                break
            }
        }
    }
}

echo "skip=$skipPR" >> $env:GITHUB_OUTPUT
echo "existing_pr_number=$matchingPR" >> $env:GITHUB_OUTPUT
