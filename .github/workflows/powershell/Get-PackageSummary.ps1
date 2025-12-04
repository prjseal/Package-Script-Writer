<#
.SYNOPSIS
    Reads package summary from artifacts.

.DESCRIPTION
    This script reads the package summary file from the artifacts directory
    and outputs it for use in subsequent workflow steps.

.PARAMETER WorkspacePath
    The GitHub workspace path

.EXAMPLE
    .\Get-PackageSummary.ps1 -WorkspacePath "/workspace"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$WorkspacePath
)

$summaryPath = "$WorkspacePath\.artifacts\package-summary.txt"
if (Test-Path $summaryPath) {
    $content = Get-Content $summaryPath -Raw
    echo "summary<<EOF" >> $env:GITHUB_OUTPUT
    echo "$content" >> $env:GITHUB_OUTPUT
    echo "EOF" >> $env:GITHUB_OUTPUT
}
else {
    echo "summary<<EOF" >> $env:GITHUB_OUTPUT
    echo "No package summary found." >> $env:GITHUB_OUTPUT
    echo "EOF" >> $env:GITHUB_OUTPUT
}
