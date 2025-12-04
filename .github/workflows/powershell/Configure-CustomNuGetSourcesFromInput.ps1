<#
.SYNOPSIS
    Configures custom NuGet sources from comma-separated input.

.DESCRIPTION
    This script takes a comma-separated list of NuGet source URLs and adds them
    to the local NuGet configuration with auto-generated names.

.PARAMETER SourcesInput
    Comma-separated list of NuGet source URLs

.EXAMPLE
    .\Configure-CustomNuGetSourcesFromInput.ps1 -SourcesInput "https://www.myget.org/F/umbraco-dev/api/v3/index.json,https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json"
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$SourcesInput = ""
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Configuring Custom NuGet Sources" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

if ([string]::IsNullOrWhiteSpace($SourcesInput)) {
    Write-Host "No custom NuGet sources provided." -ForegroundColor Yellow
    exit 0
}

# Split comma-separated sources and trim whitespace
$sources = $SourcesInput -split ',' | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

if ($sources.Count -eq 0) {
    Write-Host "No valid custom NuGet sources provided." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($sources.Count) custom NuGet source(s):" -ForegroundColor Green

# Add each NuGet source
$sourceIndex = 1
foreach ($sourceUrl in $sources) {
    $sourceName = "CustomSource$sourceIndex"

    Write-Host "`nAdding NuGet source:" -ForegroundColor Cyan
    Write-Host "  Name: $sourceName" -ForegroundColor White
    Write-Host "  URL:  $sourceUrl" -ForegroundColor White

    try {
        # Check if source already exists and remove it
        $existingSources = dotnet nuget list source
        if ($existingSources -match $sourceName) {
            Write-Host "  Removing existing source..." -ForegroundColor Yellow
            dotnet nuget remove source $sourceName
        }

        # Add the source
        dotnet nuget add source $sourceUrl --name $sourceName

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Successfully added $sourceName" -ForegroundColor Green
        }
        else {
            Write-Host "  ⚠️  Failed to add $sourceName (exit code: $LASTEXITCODE)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "  ⚠️  Error adding source: $($_.Exception.Message)" -ForegroundColor Red
    }

    $sourceIndex++
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "NuGet Source Configuration Complete" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# List all configured sources
Write-Host "`nAll configured NuGet sources:" -ForegroundColor Yellow
dotnet nuget list source
