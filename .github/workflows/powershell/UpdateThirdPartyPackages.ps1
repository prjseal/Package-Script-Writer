<#
.SYNOPSIS
 Update NuGet packages in the RootPath folder, optionally include prerelease and build
 Outputs a console table of package updates and detailed per-solution diagnostics (stdout/stderr and inline error summaries).

.PARAMETER RootPath
 Repository root. It defaults to the current working directory.

.PARAMETER DryRun
 If set, no files are changed and no dotnet commands are executed (logic still runs).

.PARAMETER IncludePrerelease
 If set, uses latest including prerelease versions; otherwise uses latest stable where possible.

.PARAMETER IgnorePackages
 Exact package IDs to skip (case-insensitive). Example: -IgnorePackages "Newtonsoft.Json","Microsoft.NET.Test.Sdk"

.PARAMETER IgnorePatterns
 One or more regex patterns applied to package IDs (case-insensitive). Example: -IgnorePatterns "^Microsoft\.", "Analyzers$"

.PARAMETER InternalPackages
 Exact package IDs considered internal (skip updating), case-insensitive.

.PARAMETER InternalPatterns
 Regex patterns to identify internal packages (skip updating), case-insensitive.

.PARAMETER KillRunning
 If set, the script will stop any running processes that appear to be executing from the RootPath folder before building.
#>
param(
  [string]$RootPath = (Get-Location).Path,
  [switch]$DryRun,
  [switch]$IncludePrerelease,
  [string[]]$IgnorePackages   = @(),
  [string[]]$IgnorePatterns   = @(),
  [string[]]$InternalPackages = @(),
  [string[]]$InternalPatterns = @(),
  [switch]$KillRunning
)

# ----------------------------- Utilities -----------------------------
function Write-Log {
    param(
        [string]$Prefix,
        [string]$Value,
        [string]$Suffix,
        [ValidateSet("Black","DarkBlue","DarkGreen","DarkCyan","DarkRed","DarkMagenta","DarkYellow","Gray","DarkGray","Blue","Green","Cyan","Red","Magenta","Yellow","White")]
        [string]$ValueColor = "White",
        [string]$Level = 'INFO'
    )

    $ts = (Get-Date).ToString('s')
    Write-Host "[${ts}] [$Level] " -NoNewline
    if ($Prefix) { Write-Host $Prefix -NoNewline }
    if ($Value) { Write-Host $Value -ForegroundColor $ValueColor -NoNewline }
    if ($Suffix) { Write-Host $Suffix -NoNewline }
    Write-Host ""  # Move to next line
}


function Fail-Fast {
  param([string]$Message)
  Write-Log -Prefix "" -Value $Message -ValueColor "Red" -Level "ERROR"
  exit 1
}
function Ensure-Directory {
  param(
    [Parameter(Mandatory)]
    [string]$Path
  )
  try {
    if ([string]::IsNullOrWhiteSpace($Path)) {
      throw "Path argument is null or whitespace."
    }
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
      New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
    $resolved = Resolve-Path -LiteralPath $Path -ErrorAction Stop
    return $resolved.Path
  }
  catch {
    Write-Log ("Failed to ensure directory '{0}': {1}" -f $Path, $_.Exception.Message) "ERROR"
    throw
  }
}

# ---------------- Process discovery/termination (NEW) -----------------
function Get-TemplateProcesses {
  param(
    [Parameter(Mandatory)][string]$TemplatePath
  )
  # Use CIM so we can filter on CommandLine reliably
  $tpl = $TemplatePath.ToLowerInvariant()
  try {
    $currentPid = $PID

    $procs = Get-CimInstance Win32_Process | Where-Object {
      # Either the command line references the template folder, or the process name is one we know
      ($_.CommandLine -and $_.CommandLine.ToLower().Contains($tpl)) -and $_.Id -ne $currentPid
    }
    return $procs
  } catch {
    Write-Log ("Failed to enumerate processes: {0}" -f $_.Exception.Message) "WARN"
    return @()
  }
}

function Stop-TemplateProcesses {
  param(
    [Parameter(Mandatory)][string]$TemplatePath,
    [int]$GraceMs = 1500
  )
  $procs = Get-TemplateProcesses -TemplatePath $TemplatePath
  if (-not $procs -or $procs.Count -eq 0) {
    Write-Log "No running template-related processes detected."
    return
  }
Write-Log -Prefix "Found " -Value $procs.Count -ValueColor Cyan -Suffix " template-related process(es) to stop..."
  foreach ($p in $procs) {
    try {
      Write-Log ("Stopping PID {0} '{1}' (soft)" -f $p.ProcessId, $p.Name)
      Stop-Process -Id $p.ProcessId -ErrorAction SilentlyContinue
    } catch { }
  }
  Start-Sleep -Milliseconds $GraceMs
  # Force kill anything still running
  foreach ($p in Get-TemplateProcesses -TemplatePath $TemplatePath) {
    try {
      Write-Log ("Killing PID {0} '{1}' (hard)" -f $p.ProcessId, $p.Name) "WARN"
      Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
    } catch { }
  }
}

# ----------------------- Console ASCII Table Helper -------------------
function Write-AsciiTable {
<#
      .SYNOPSIS
        Renders a simple ASCII table with borders, similar to benchmarking tools.
      .PARAMETER Rows
        Enumerable of objects that share the same property names as the headers.
      .PARAMETER Headers
        Ordered list of headers (strings) that map to property names in each row.
      .PARAMETER AlignRight
        Property names to right-align (e.g., versions).
      .PARAMETER OutputFile
        Optional file path to write the table to (in addition to console output).
    #>
    param(
        [Parameter(Mandatory)]
        [System.Collections.IEnumerable]$Rows,
        [Parameter(Mandatory)]
        [string[]]$Headers,
        [string[]]$AlignRight = @(),
        [string]$OutputFile
    )

    $rowsArray = @($Rows)
    $outputLines = @()

    if ($rowsArray.Count -eq 0) {
        $msg = "(no rows)"
        Write-Host $msg -ForegroundColor DarkGray
        if ($OutputFile) {
            $msg | Out-File -FilePath $OutputFile -Encoding UTF8
        }
        return
    }

    $columns = foreach ($h in $Headers) {
        [pscustomobject]@{
            Name  = $h
            Width = [math]::Max($h.Length, 1)
            Align = $(if ($AlignRight -contains $h) { 'Right' } else { 'Left' })
        }
    }

    foreach ($row in $rowsArray) {
        foreach ($col in $columns) {
            $val = $row | Select-Object -ExpandProperty $col.Name
            if ($val.Length -gt $col.Width) { $col.Width = $val.Length }
        }
    }

    function Border($columns) {
        $parts = @('+')
        foreach ($c in $columns) {
            $parts += ('{0}' -f ('-' * ($c.Width + 2)))
            $parts += '+'
        }
        return ($parts -join '')
    }

    function Cell($text, $width, $align) {
        $text = [string]$text
        $w = [int]$width
        if ($align -eq 'Right') {
            return ((' {0,'  + $w + '} ') -f $text)
        } else {
            return ((' {0,-' + $w + '} ') -f $text)
        }
    }

    $top    = Border $columns

    # Build header row with pipes between each column (same pattern as data rows)
    $headerParts = @('|')
    foreach ($c in $columns) {
        $headerParts += (Cell $c.Name $c.Width 'Left')
        $headerParts += '|'
    }
    $header = ($headerParts -join '')

    $sep    = Border $columns

    $outputLines += $top
    $outputLines += $header
    $outputLines += $sep

    $lastFile = ''
    foreach ($row in $rowsArray) {
        $file = $row.'File Name'
        if ($lastFile -ne '' -and $file -ne $lastFile) {
            $outputLines += $sep
        }
        $lastFile = $file

        $lineParts = @('|')
        foreach ($c in $columns) {
            $val = $row | Select-Object -ExpandProperty $c.Name
            $lineParts += (Cell $val $c.Width $c.Align)
            $lineParts += '|'
        }
        $outputLines += ($lineParts -join '')
    }

    $outputLines += $top

    # Write to console
    foreach ($line in $outputLines) {
        Write-Host $line
    }

    # Write to file if specified
    if ($OutputFile) {
        $outputLines | Out-File -FilePath $OutputFile -Encoding UTF8
    }
}

# ---------------------- NuGet Source Discovery ----------------------
function Get-ConfiguredNuGetSources {
  try {
    # Get all configured NuGet sources
    $output = dotnet nuget list source 2>&1
    $sources = @()

    # Parse the output to extract source URLs
    # Format: "  1.  nuget.org [Enabled]"
    #         "      https://api.nuget.org/v3/index.json"
    $lines = $output -split "`n"
    $currentSource = $null

    foreach ($line in $lines) {
      # Check if this is a source name line (starts with number)
      if ($line -match '^\s+\d+\.\s+(.+?)\s+\[Enabled\]') {
        $currentSource = $matches[1].Trim()
      }
      # Check if this is a URL line (starts with spaces and contains http)
      elseif ($currentSource -and $line -match '^\s+(https?://[^\s]+)') {
        $sourceUrl = $matches[1].Trim()

        # For nuget.org, use the known flatcontainer URL
        if ($sourceUrl -match 'api\.nuget\.org') {
          $sources += @{
            Name = $currentSource
            Url = 'https://api.nuget.org/v3-flatcontainer'
          }
        }
        # For all other sources, query the service index to find PackageBaseAddress
        elseif ($sourceUrl -match '/v3/index\.json$' -or $sourceUrl -match '/v3/?$') {
          try {
            Write-Host "Querying service index for $currentSource at $sourceUrl" -ForegroundColor Cyan
            $serviceIndex = Invoke-RestMethod -Uri $sourceUrl -Method Get -TimeoutSec 10 -ErrorAction Stop

            # Look for PackageBaseAddress/3.0.0 resource
            $packageBaseAddress = $serviceIndex.resources | Where-Object {
              $_.'@type' -eq 'PackageBaseAddress/3.0.0'
            } | Select-Object -First 1

            if ($packageBaseAddress -and $packageBaseAddress.'@id') {
              $flatcontainerUrl = $packageBaseAddress.'@id'.TrimEnd('/')
              Write-Host "Found PackageBaseAddress: $flatcontainerUrl" -ForegroundColor Green
              $sources += @{
                Name = $currentSource
                Url = $flatcontainerUrl
              }
            }
            else {
              Write-Log ("No PackageBaseAddress found in service index for '{0}'" -f $currentSource) -Level "WARN"
            }
          }
          catch {
            Write-Log ("Could not query service index for '{0}': {1}" -f $currentSource, $_.Exception.Message) -Level "WARN"
          }
        }
        # If it's already a flatcontainer URL
        elseif ($sourceUrl -match 'flatcontainer') {
          $sources += @{
            Name = $currentSource
            Url = $sourceUrl.TrimEnd('/')
          }
        }
        else {
          Write-Log ("Unsupported source URL format for '{0}': {1}" -f $currentSource, $sourceUrl) -Level "WARN"
        }
        $currentSource = $null
      }
    }

    # Fallback: Always include NuGet.org if not found
    if (-not ($sources | Where-Object { $_.Name -eq 'nuget.org' })) {
      $sources += @{
        Name = 'nuget.org'
        Url = 'https://api.nuget.org/v3-flatcontainer'
      }
    }

    return $sources
  }
  catch {
    Write-Log ("Failed to get configured NuGet sources, using nuget.org only: {0}" -f $_.Exception.Message) -Level "WARN"
    return @(@{
      Name = 'nuget.org'
      Url = 'https://api.nuget.org/v3-flatcontainer'
    })
  }
}

# ------------------------- NuGet Version Helper -----------------------
function Get-LatestNuGetVersion {
  param(
    [string]$packageId,
    [hashtable]$cache,
    [switch]$IncludePrerelease
  )
  if (-not ($cache -is [hashtable])) {
    throw "cache parameter must be a [hashtable]"
  }
  $key = $packageId.ToLower()
  if ($cache.ContainsKey($key)) {
    return $cache[$key]
  }

  try {
    # Get all configured NuGet sources
    $sources = Get-ConfiguredNuGetSources
    $allVersions = @()

    Write-Log ("Querying {0} source(s) for package '{1}'" -f $sources.Count, $packageId)

    # Query each source
    foreach ($source in $sources) {
      $url = ('{0}/{1}/index.json' -f $source.Url, $key)
      try {
        Write-Log ("  Querying source '{0}': {1}" -f $source.Name, $url)
        $resp = Invoke-RestMethod -Uri $url -Method Get -TimeoutSec 20 -ErrorAction Stop
        $sourceVersions = @($resp.versions | ForEach-Object { [string]$_ })
        if ($sourceVersions.Count -gt 0) {
          Write-Log ("  Found {0} version(s) in '{1}'" -f $sourceVersions.Count, $source.Name)
          $allVersions += $sourceVersions
        }
        else {
          Write-Log ("  No versions found in '{0}'" -f $source.Name) -Level "WARN"
        }
      }
      catch {
        # Not all sources may have the package, which is expected
        Write-Log ("  Package not found in '{0}': {1}" -f $source.Name, $_.Exception.Message) -Level "WARN"
      }
    }

    # Remove duplicates and process versions
    $versions = @($allVersions | Select-Object -Unique)

    # Exclude nightly build versions (versions containing "-build" in the suffix)
    # This prevents selecting unstable nightly builds like "17.0.0-build.20251124.1"
    $beforeFilter = $versions.Count
    $versions = @($versions | Where-Object { $_ -notmatch '-build' })
    $afterFilter = $versions.Count
    if ($beforeFilter -ne $afterFilter) {
      $excluded = $beforeFilter - $afterFilter
      Write-Log ("Excluded {0} nightly build version(s) for {1}" -f $excluded, $packageId) -Level "INFO"
    }

    if ($versions.Count -eq 0) {
      Write-Log ("No versions found for {0} in any configured source (after filtering)" -f $packageId) -Level "WARN"
      $cache[$key] = $null
      return $null
    }
    Write-Log ("Total unique versions found for {0}: {1}" -f $packageId, $versions.Count)
    if ($IncludePrerelease) {
      # Prefer stable > rc > beta > alpha when including prerelease; respect numeric suffixes (e.g., rc3 > rc2)
      $parsed = $versions | ForEach-Object {
        $v = $_
        $prTag = ''
        $prNum = 0
        # Updated regex to handle 3 or 4 segment versions with optional prerelease (e.g., "72.1.0.3", "8.0.0-beta.1")
        if ($v -match '^([0-9]+\.[0-9]+\.[0-9]+(?:\.[0-9]+)?)(?:-([A-Za-z]+)[\.-]?([0-9]*))?$') {
          $base = $matches[1]
          if ($matches[2]) { $prTag = $matches[2].ToLower() }
          if ($matches[3] -and $matches[3] -ne '') { $prNum = [int]$matches[3] }
        } else {
          # If pattern doesn't match, try to extract just the numeric version part (3 or 4 segments)
          if ($v -match '^([0-9]+\.[0-9]+\.[0-9]+(?:\.[0-9]+)?)') {
            $base = $matches[1]
          } else {
            # Fallback to the original version (may fail later, but preserves behavior)
            $base = $v
          }
        }
        switch ($prTag) {
          'rc'    { $prPriority = 70 }
          'beta'  { $prPriority = 50 }
          'alpha' { $prPriority = 30 }
          default { if ($prTag -ne '') { $prPriority = 40 } else { $prPriority = 100 } }
        }
        [pscustomobject]@{
          Original = $v
          BaseVer   = [Version]$base
          PrTag     = $prTag
          PrNum     = $prNum
          PrPriority = $prPriority
          Effective = $prPriority + $prNum
        }
      }
      $sorted = $parsed | Sort-Object -Property @{Expression={$_.BaseVer}; Descending=$true}, @{Expression={$_.Effective}; Descending=$true}
      $chosen = $sorted[0].Original
      Write-Log ('Selected version {0} for {1} (IncludePrerelease=true, from {2} candidates)' -f $chosen, $packageId, $versions.Count)
    } else {
      $stable = @($versions | Where-Object { $_ -notmatch '-' })
      $chosen = if ($stable.Count -gt 0) { $stable[-1] } else { $versions[-1] }
      Write-Log ('Selected version {0} for {1} (IncludePrerelease=false, {2} stable versions available)' -f $chosen, $packageId, $stable.Count)
    }
    $cache[$key] = $chosen
    return $chosen
  }
  catch {
    Write-Log ('Failed to query NuGet for {0}: {1}' -f $packageId, $_.Exception.Message) -Level "ERROR"
    throw
  }
}

# ---------------------- .csproj Package Updating ----------------------
function Update-Csproj-PackageReferences {
  param(
    [string]$csprojPath,
    [hashtable]$versionCache,
    [switch]$DryRunFlag,
    [switch]$IncludePrerelease,
    [string[]]$IgnorePackages   = @(),
    [string[]]$IgnorePatterns   = @(),
    [string[]]$InternalPackages = @(),
    [string[]]$InternalPatterns = @()
  )

  $result = [ordered]@{
    Path    = $csprojPath
    Updated = $false
    Changes = @()
    Errors  = @()
  }

  try { [xml]$xml = Get-Content -Path $csprojPath -Raw }
  catch {
    $result.Errors += ('Failed to read XML: {0}' -f $_.Exception.Message)
    return $result
  }

  # Consider ALL PackageReference nodes
  $prNodes = $xml.SelectNodes("//PackageReference")
  foreach ($pr in $prNodes) {
    $pkgId = $pr.Include
    if (-not $pkgId) { continue }

    # Only attempt updates if there is a Version *attribute* (skip <Version> child and CPM)
    if (-not $pr.HasAttribute("Version")) { continue }

    # ---- EARLY SKIPS: DO NOT call NuGet for any of the following ----

    $idLower = $pkgId.ToLowerInvariant()

    # Ignore: exact
    $ignoredByExact = $false
    if ($IgnorePackages.Count -gt 0) {
      $ignoredByExact = $IgnorePackages |
        ForEach-Object { $_.ToLowerInvariant() } |
        Where-Object { $_ -eq $idLower } |
        Select-Object -First 1
    }

    # Ignore: regex
    $ignoredByPattern = $false
    foreach ($pat in $IgnorePatterns) {
      if ([string]::IsNullOrWhiteSpace($pat)) { continue }
      if ($pkgId -imatch $pat) { $ignoredByPattern = $true; break }
    }

    if ($ignoredByExact -or $ignoredByPattern) { continue }

    # Internal: exact
    $internalByExact = $false
    if ($InternalPackages.Count -gt 0) {
      $internalByExact = $InternalPackages |
        ForEach-Object { $_.ToLowerInvariant() } |
        Where-Object { $_ -eq $idLower } |
        Select-Object -First 1
    }

    # Internal: regex
    $internalByPattern = $false
    foreach ($pat in $InternalPatterns) {
      if ([string]::IsNullOrWhiteSpace($pat)) { continue }
      if ($pkgId -imatch $pat) { $internalByPattern = $true; break }
    }

    if ($internalByExact -or $internalByPattern) { continue }

    # ---- ONLY NOW query NuGet ----
    $existingVersion = $pr.Version
    $latest = Get-LatestNuGetVersion -packageId $pkgId -cache $versionCache -IncludePrerelease:$IncludePrerelease
    if (-not $latest) {
      Write-Log ('No latest version found for {0}' -f $pkgId) -Level "WARN"
      continue
    }
    if ($existingVersion -and ($existingVersion -ieq $latest)) {
      Write-Log ('Package {0} is already at latest version {1}' -f $pkgId, $latest) -Level "INFO"
      continue
    }
    Write-Log ('Package {0}: {1} -> {2}' -f $pkgId, $existingVersion, $latest) -Level "INFO"

    if (-not $DryRunFlag) {
      $pr.SetAttribute("Version", $latest)
    }

    $result.Updated = $true
    $result.Changes += [ordered]@{
      Package    = $pkgId
      OldVersion = $existingVersion
      NewVersion = $latest
    }
  }

  if ($result.Updated -and -not $DryRunFlag) {
    try { $xml.Save($csprojPath) }
    catch { $result.Errors += ('Failed to save XML: {0}' -f $_.Exception.Message) }
  }

  return $result
}

# ----------------------------- dotnet Runner --------------------------
function Run-DotNet {
  param(
    [string]$Command, # e.g., 'build "My.sln" -c Release'
    [string]$Path,
    [switch]$DryRunFlag,
    [string]$StdOutFile,
    [string]$StdErrFile
  )
  $result = [ordered]@{
    Command  = $Command
    Path     = $Path
    Success  = $true
    ExitCode = 0
    StdOut   = ""
    StdErr   = ""
  }
  if ($DryRunFlag) {
    $result.StdOut = "DryRun"
    return $result
  }
  try {
    Push-Location $Path
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dotnet"
    $psi.Arguments = $Command
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow  = $true
    $proc = New-Object System.Diagnostics.Process
    $proc.StartInfo = $psi
    [void]$proc.Start()
    $stdOut = $proc.StandardOutput.ReadToEnd()
    $stdErr = $proc.StandardError.ReadToEnd()
    $proc.WaitForExit()
    $result.ExitCode = $proc.ExitCode
    $result.Success  = ($proc.ExitCode -eq 0)
    $result.StdOut   = $stdOut
    $result.StdErr   = $stdErr
    if ($StdOutFile) { $stdOut | Out-File -FilePath $StdOutFile -Encoding UTF8 }
    if ($StdErrFile) { $stdErr | Out-File -FilePath $StdErrFile -Encoding UTF8 }
  }
  catch {
    $result.Success = $false
    $result.StdErr  = $_.Exception | Out-String
  }
  finally {
    Pop-Location
  }
  return $result
}

# =============================== MAIN =================================
Write-Log ('Starting package update... IncludePrerelease={0}, DryRun={1}' -f $IncludePrerelease, $DryRun)

$templatePath = $RootPath
if (-not (Test-Path $templatePath)) {
  Fail-Fast ("Template folder not found at '{0}'." -f $templatePath)
}

if ($KillRunning) {
    Write-Log "Checking for running template processes to stop..."

    # Get current process ID
    $currentPid = $PID

    # Find processes related to the template path
    $processes = Get-Process | Where-Object {
        $_.Path -like "*$templatePath*" -and $_.Id -ne $currentPid
    }

    foreach ($proc in $processes) {
        Write-Log "Stopping process: $($proc.ProcessName) (PID: $($proc.Id))"
        Stop-Process -Id $proc.Id -Force
    }
}


# Artifacts layout
$artifactsRoot = Join-Path $RootPath ".artifacts"
$logsDir       = Join-Path $artifactsRoot "logs"
$null = Ensure-Directory $artifactsRoot
$null = Ensure-Directory $logsDir

Write-Log ('Scanning for csproj files in template folder...')
$csprojFiles = Get-ChildItem -Path $templatePath -Filter *.csproj -Recurse -File
if ($csprojFiles.Count -eq 0) {
  Fail-Fast ("No .csproj files found in '{0}'." -f $templatePath)
}
Write-Log -Prefix "Found " -Value $csprojFiles.Count -ValueColor Cyan -Suffix " csproj files."

$versionCache  = @{}
$updateResults = @()

foreach ($csproj in $csprojFiles) {
  $updateResults += Update-Csproj-PackageReferences `
    -csprojPath $csproj.FullName `
    -versionCache $versionCache `
    -DryRunFlag:$DryRun `
    -IncludePrerelease:$IncludePrerelease `
    -IgnorePackages $IgnorePackages `
    -IgnorePatterns $IgnorePatterns `
    -InternalPackages $InternalPackages `
    -InternalPatterns $InternalPatterns
}

# ---------------------- PACKAGE UPDATE RESULTS (TABLE) ---------------------

# Flatten changes into rows for the table
$packageChanges = New-Object System.Collections.Generic.List[object]

foreach ($result in $updateResults) {
    $projName = [System.IO.Path]::GetFileName($result.Path)

    foreach ($change in $result.Changes) {
        $packageChanges.Add([pscustomobject]@{
            'File Name'    = $projName
            'Package Name' = $change.Package
            'Old Version'  = $change.OldVersion
            'New Version'  = $change.NewVersion
        })
    }

    # Surface any XML errors per file (rare but useful)
    foreach ($err in $result.Errors) {
        Write-Host ('XML Error in {0}: {1}' -f $projName, $err) -ForegroundColor Red
    }
}


# ------------------------------ BUILD SECTION -----------------------------

# Skip build if there are no packages to update
if ($packageChanges.Count -eq 0) {
    Write-Log "No packages to update - skipping build section" -Level "INFO"
    $buildResults = @()
    $buildSummaryRows = New-Object System.Collections.Generic.List[object]
} else {
    Write-Log ('Scanning for sln files...')
    $slnFiles = Get-ChildItem -Path $RootPath -Filter *.sln -Recurse -File
    Write-Log -Prefix "Found " -Value $slnFiles.Count -ValueColor Cyan -Suffix " sln files."

    $buildResults = @()
    $timestamp = (Get-Date).ToString('yyyyMMdd_HHmmss')
    $buildSummaryRows = New-Object System.Collections.Generic.List[object]

foreach ($sln in $slnFiles) {
    $slnName = [System.IO.Path]::GetFileNameWithoutExtension($sln.FullName)
    $safeName = ($slnName -replace '[^\w\.-]','_')

    $slnLogBase   = Join-Path $logsDir    ("{0}__{1}" -f $safeName, $timestamp)
    $stdoutClean  = ("{0}.clean.stdout.txt" -f $slnLogBase)
    $stderrClean  = ("{0}.clean.stderr.txt" -f $slnLogBase)
    $stdoutBuild  = ("{0}.build.stdout.txt" -f $slnLogBase)
    $stderrBuild  = ("{0}.build.stderr.txt" -f $slnLogBase)

    # CLEAN
    Write-Log ('Cleaning {0}' -f $sln.FullName)
    $cleanCmd = ('clean "{0}" -clp:Summary -v:m' -f $sln.FullName)
    $cleanResult = Run-DotNet -Command $cleanCmd -Path $sln.DirectoryName -DryRunFlag:$DryRun -StdOutFile $stdoutClean -StdErrFile $stderrClean

    if($cleanResult.Success) {
        Write-Log ('Clean succeeded for {0}' -f $sln.Name)
    } else {
        Write-Log ('Clean FAILED for {0}' -f $sln.Name) "WARN"
    }

    # BUILD (no binlog per your preference)
    Write-Log ('Building {0}' -f $sln.FullName)
    $buildCmd = ('build "{0}" -clp:Summary -v:m' -f $sln.FullName)
    $buildResult = Run-DotNet -Command $buildCmd -Path $sln.DirectoryName -DryRunFlag:$DryRun -StdOutFile $stdoutBuild -StdErrFile $stderrBuild

    if($buildResult.Success) {
        Write-Log ('Build succeeded for {0}' -f $sln.Name)
    } else {
        Write-Log ('Build FAILED for {0}' -f $sln.Name) "WARN"
    }


    # Extract top error lines from both streams
    $errorLines = @(
        ($buildResult.StdErr -split "`r?`n"),
        ($buildResult.StdOut -split "`r?`n")
    ) | Where-Object { $_ -match "(:\s*error\s*[A-Z]?\d{3,}|^error\s)" } |
        Select-Object -Unique -First 20

    $buildResults += [ordered]@{
        Solution     = $sln.FullName
        CleanSuccess = $cleanResult.Success
        BuildSuccess = $buildResult.Success
        ExitCode     = $buildResult.ExitCode
        StdOutFile   = $stdoutBuild
        StdErrFile   = $stderrBuild
        ErrorLines   = $errorLines
    }

    
    # --- Parse Errors/Warnings from build output ---
    # Prefer the MSBuild summary counts; fallback to counting diagnostics if missing.
    $combinedOut = ($buildResult.StdOut + "`n" + $buildResult.StdErr)

    # Try summary-style extraction first (e.g., "X Warning(s)", "Y Error(s)")
    $errCount = 0
    $warnCount = 0

    $errSummaryMatch = [regex]::Match($combinedOut, '(?mi)^\s*(\d+)\s+Error\(s\)')
    $warnSummaryMatch = [regex]::Match($combinedOut, '(?mi)^\s*(\d+)\s+Warning\(s\)')

    if ($errSummaryMatch.Success) { $errCount = [int]$errSummaryMatch.Groups[1].Value }
    if ($warnSummaryMatch.Success) { $warnCount = [int]$warnSummaryMatch.Groups[1].Value }

    # Fallback: count diagnostics if summary lines are not found
    if (-not $errSummaryMatch.Success) {
        $errCount = ([regex]::Matches($combinedOut, '(?i)(^|\s)error\s[A-Z]?\d{3,}\b')).Count
    }
    if (-not $warnSummaryMatch.Success) {
        $warnCount = ([regex]::Matches($combinedOut, '(?i)(^|\s)warning\s[A-Z]?\d{3,}\b')).Count
    }

    
    $buildSummaryRows.Add([pscustomobject]@{
        'Solution Name' = [System.IO.Path]::GetFileName($sln.FullName)
        'Clean Result'  = if ($cleanResult.Success) { 'Success' } else { 'Failed' }
        'Build Result'  = if ($buildResult.Success) { 'Success' } else { 'Failed' }
        'Errors'        = $errCount
        'Warnings'      = $warnCount
    })


    # # Inline summary per solution
    # $status = if ($buildResult.Success) { "SUCCESS" } else { "FAILED" }
    # $color  = if ($buildResult.Success) { "Green" } else { "Red" }
    # Write-Host ('{0}: {1} (ExitCode={2})' -f $slnName, $status, $buildResult.ExitCode) -ForegroundColor $color

    if (-not $buildResult.Success) {
        Write-Host "`n========================================" -ForegroundColor Red
        Write-Host "BUILD FAILED: $($sln.Name)" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red

        if ($cleanResult.Success -eq $false) {
            Write-Host "`nClean failed for this solution." -ForegroundColor Yellow
        }

        if ($errorLines.Count -gt 0) {
            Write-Host "`nTop error lines:" -ForegroundColor Yellow
            foreach ($line in $errorLines) {
                Write-Host "  $line" -ForegroundColor Red
            }
        } else {
            Write-Host "`nNo error lines matched the error pattern." -ForegroundColor Yellow
        }

        # Show last 50 lines of build output to capture errors
        Write-Host "`nBuild output (last 50 lines):" -ForegroundColor Yellow
        $buildOutputLines = ($buildResult.StdOut + "`n" + $buildResult.StdErr) -split "`r?`n"
        $lastLines = $buildOutputLines | Select-Object -Last 50
        foreach ($line in $lastLines) {
            if ($line -match "error") {
                Write-Host "  $line" -ForegroundColor Red
            } elseif ($line -match "warning") {
                Write-Host "  $line" -ForegroundColor Yellow
            } else {
                Write-Host "  $line" -ForegroundColor Gray
            }
        }

        Write-Host "`nFull logs saved to:" -ForegroundColor Cyan
        Write-Host "  StdOut: $stdoutBuild" -ForegroundColor Cyan
        Write-Host "  StdErr: $stderrBuild" -ForegroundColor Cyan
        Write-Host "========================================`n" -ForegroundColor Red
    }
}

    if ($KillRunning) {
        Write-Log "Checking for running template processes to stop..."

        # Get current process ID
        $currentPid = $PID

        # Find processes related to the template path
        $processes = Get-Process | Where-Object {
            $_.Path -like "*$templatePath*" -and $_.Id -ne $currentPid
        }

        foreach ($proc in $processes) {
            Write-Log "Stopping process: $($proc.ProcessName) (PID: $($proc.Id))"
            Stop-Process -Id $proc.Id -Force
        }
    }
}

Write-Log "Package update script completed."
Write-Host ""
Write-Host "`n===== PACKAGE UPDATE RESULTS ====="
$packageUpdateFile = Join-Path $artifactsRoot "package-summary.txt"
if ($packageChanges.Count -gt 0) {

$sorted = @($packageChanges |
    Sort-Object `
        @{ Expression = 'File Name';    Ascending = $true }, `
        @{ Expression = 'Package Name'; Ascending = $true })

    Write-AsciiTable -Rows $sorted `
                     -Headers @('File Name','Package Name','Old Version','New Version') `
                     -AlignRight @('Old Version','New Version') `
                     -OutputFile $packageUpdateFile
} else {
    $msg = "No packages to update"
    Write-Host $msg -ForegroundColor DarkGray
    $msg | Out-File -FilePath $packageUpdateFile -Encoding UTF8
}
Write-Host ""

Write-Host "`n===== BUILD SUMMARY ====="
$buildSummaryFile = Join-Path $artifactsRoot "build-summary.txt"
if ($buildSummaryRows.Count -gt 0) {
    $summarySorted = $buildSummaryRows | Sort-Object 'Solution Name'
    Write-AsciiTable -Rows $summarySorted `
        -Headers @('Solution Name','Clean Result','Build Result','Errors','Warnings') `
        -AlignRight @('Errors','Warnings') `
        -OutputFile $buildSummaryFile

    # Check if any builds failed
    $failedBuilds = $summarySorted | Where-Object { $_.'Build Result' -eq 'Failed' }
    if ($failedBuilds.Count -gt 0) {
        Write-Host "`n================================================" -ForegroundColor Red
        Write-Host "ERROR: $($failedBuilds.Count) solution(s) failed to build" -ForegroundColor Red
        Write-Host "================================================" -ForegroundColor Red
        foreach ($failed in $failedBuilds) {
            Write-Host "  - $($failed.'Solution Name')" -ForegroundColor Red
        }
        Write-Host "`nPlease review the error details above." -ForegroundColor Yellow
        Write-Host "Build logs are saved in: $logsDir" -ForegroundColor Cyan
        Write-Host "================================================`n" -ForegroundColor Red

        # Exit with error code to fail the workflow
        exit 1
    } else {
        Write-Host "`nAll builds completed successfully! ✅" -ForegroundColor Green
    }
} else {
    $msg = "(no solutions found)"
    Write-Host $msg -ForegroundColor DarkGray
    $msg | Out-File -FilePath $buildSummaryFile -Encoding UTF8
}

