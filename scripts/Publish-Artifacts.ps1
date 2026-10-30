<#
.SYNOPSIS
    Builds and packages Cal2AL release artifacts for GitHub Releases.

.DESCRIPTION
    For each runtime (win-x64, linux-x64) the script publishes cal2al as a self-contained single-file executable
    (Release, .NET 10), packages it with README.md and LICENSE, and writes SHA256SUMS.txt. Output:

        artifacts/
          cal2al-<version>-win-x64.zip
          cal2al-<version>-linux-x64.tar.gz
          SHA256SUMS.txt

    The archives contain a top-level folder cal2al-<version>-<rid>/ with the executable, README.md and LICENSE.
    The script stops on the first failing command and exits with a non-zero code.

.PARAMETER Version
    Release version, e.g. 0.1.0 or 0.2.0-beta.1. Used in the archive names and stamped into the executable.

.PARAMETER Runtimes
    Runtime identifiers to build. Default: win-x64, linux-x64.

.PARAMETER SkipTests
    Do not run the test suite before packaging.

.EXAMPLE
    ./scripts/Publish-Artifacts.ps1 -Version 0.1.0

.EXAMPLE
    pwsh ./scripts/Publish-Artifacts.ps1 -Version 0.2.0-beta.1 -SkipTests
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$')]
    [string] $Version,

    [ValidateSet('win-x64', 'linux-x64')]
    [string[]] $Runtimes = @('win-x64', 'linux-x64'),

    [switch] $SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false   # exit codes are checked explicitly below

$repoRoot     = Split-Path -Parent $PSScriptRoot
$project      = Join-Path $repoRoot 'cal2al/cal2al.csproj'
$solution     = Join-Path $repoRoot 'Cal2AL.slnx'
$artifactsDir = Join-Path $repoRoot 'artifacts'
$stagingDir   = Join-Path $artifactsDir '.staging'
$extraFiles   = @('README.md', 'LICENSE')

function Write-Step([string] $message) { Write-Host "`n==> $message" -ForegroundColor Cyan }
function Write-Ok([string] $message)   { Write-Host "    $message" -ForegroundColor Green }
function Write-Info([string] $message) { Write-Host "    $message" }

function Invoke-Checked([string] $exe, [string[]] $arguments) {
    Write-Info "$exe $($arguments -join ' ')"
    & $exe @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "'$exe $($arguments[0])' failed with exit code $LASTEXITCODE."
    }
}

try {
    $started = Get-Date
    Write-Host "Cal2AL release packaging - version $Version" -ForegroundColor White

    # ---- Preconditions ------------------------------------------------------------------------------
    Write-Step 'Checking prerequisites'
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw 'The .NET SDK (dotnet) was not found on PATH. Install the .NET 10 SDK.'
    }
    $sdk = (& dotnet --version).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $sdk.StartsWith('10.')) {
        throw ".NET 10 SDK required; 'dotnet --version' reports '$sdk'."
    }
    Write-Ok ".NET SDK $sdk"

    foreach ($file in @($project, $solution) + ($extraFiles | ForEach-Object { Join-Path $repoRoot $_ })) {
        if (-not (Test-Path $file -PathType Leaf)) {
            throw "Required file not found: $file"
        }
    }
    Write-Ok "Found $($extraFiles -join ', ')"

    # ---- .gitignore ---------------------------------------------------------------------------------
    $gitignore = Join-Path $repoRoot '.gitignore'
    $ignored = (Test-Path $gitignore) -and (Select-String -Path $gitignore -Pattern '^\s*/?artifacts/?\s*$' -Quiet)
    if (-not $ignored) {
        Add-Content -Path $gitignore -Value "`n# Release packages built by scripts/Publish-Artifacts.ps1`nartifacts/"
        Write-Ok "Added 'artifacts/' to .gitignore"
    }
    else {
        Write-Ok "'artifacts/' is already ignored by git"
    }

    # ---- Clean output -------------------------------------------------------------------------------
    Write-Step "Preparing $artifactsDir"
    if (Test-Path $artifactsDir) { Remove-Item $artifactsDir -Recurse -Force }
    New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

    # ---- Tests --------------------------------------------------------------------------------------
    if (-not $SkipTests) {
        Write-Step 'Running tests (Release)'
        Invoke-Checked dotnet @('test', $solution, '--configuration', 'Release', '--nologo', '--verbosity', 'minimal')
        Write-Ok 'All tests passed'
    }

    # ---- Publish and package ------------------------------------------------------------------------
    $assemblyVersion = ($Version -split '-')[0]
    $packages = @()
    foreach ($rid in $Runtimes) {
        $name       = "cal2al-$Version-$rid"
        $publishDir = Join-Path $stagingDir $name

        Write-Step "Publishing $rid"
        Invoke-Checked dotnet @(
            'publish', $project,
            '--configuration', 'Release',
            '--runtime', $rid,
            '--self-contained', 'true',
            '--output', $publishDir,
            '--nologo',
            '-p:PublishSingleFile=true',
            '-p:IncludeNativeLibrariesForSelfExtract=true',
            '-p:DebugType=none',
            '-p:DebugSymbols=false',
            "-p:Version=$Version",
            "-p:AssemblyVersion=$assemblyVersion.0",
            "-p:FileVersion=$assemblyVersion.0",
            "-p:InformationalVersion=$Version"
        )

        $exeName = if ($rid -like 'win-*') { 'cal2al.exe' } else { 'cal2al' }
        $exePath = Join-Path $publishDir $exeName
        if (-not (Test-Path $exePath -PathType Leaf)) {
            throw "Publish for $rid did not produce $exeName."
        }

        # Keep only the executable (single-file publish may leave symbol or config files).
        Get-ChildItem $publishDir -File | Where-Object Name -ne $exeName | Remove-Item -Force
        foreach ($file in $extraFiles) {
            Copy-Item (Join-Path $repoRoot $file) -Destination $publishDir
        }
        Write-Ok ("{0} ({1:N1} MB)" -f $exeName, ((Get-Item $exePath).Length / 1MB))

        Write-Step "Packaging $rid"
        if ($rid -like 'win-*') {
            $archive = Join-Path $artifactsDir "$name.zip"
            Compress-Archive -Path $publishDir -DestinationPath $archive -CompressionLevel Optimal
        }
        else {
            if (-not (Get-Command tar -ErrorAction SilentlyContinue)) {
                throw "'tar' is required to create .tar.gz archives (included in Windows 10+ and Linux)."
            }
            if (-not $IsWindows) {
                Invoke-Checked chmod @('+x', $exePath)
            }
            $archive = Join-Path $artifactsDir "$name.tar.gz"
            # Windows has no executable bit, so an archive built there needs "chmod +x cal2al" after extraction.
            Push-Location $stagingDir
            try {
                Invoke-Checked tar @('-czf', $archive, $name)
            }
            finally {
                Pop-Location
            }
        }
        if (-not (Test-Path $archive -PathType Leaf)) {
            throw "Archive was not created: $archive"
        }
        Write-Ok ("{0} ({1:N1} MB)" -f (Split-Path $archive -Leaf), ((Get-Item $archive).Length / 1MB))
        $packages += $archive
    }

    # ---- Checksums ----------------------------------------------------------------------------------
    Write-Step 'Writing SHA256SUMS.txt'
    $sumsFile = Join-Path $artifactsDir 'SHA256SUMS.txt'
    # Format compatible with `sha256sum -c SHA256SUMS.txt`: "<hash>  <file>" with LF line endings.
    $lines = foreach ($package in $packages) {
        $hash = (Get-FileHash -Path $package -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $(Split-Path $package -Leaf)"
    }
    [System.IO.File]::WriteAllText($sumsFile, (($lines -join "`n") + "`n"), [System.Text.UTF8Encoding]::new($false))
    $lines | ForEach-Object { Write-Info $_ }

    Remove-Item $stagingDir -Recurse -Force

    # ---- Summary ------------------------------------------------------------------------------------
    Write-Step 'Done'
    Get-ChildItem $artifactsDir -File | Sort-Object Name | ForEach-Object {
        Write-Ok ("{0,-40} {1,10:N1} MB" -f $_.Name, ($_.Length / 1MB))
    }
    Write-Host ("`nRelease $Version packaged in {0:N0} s. Upload the files in '{1}' to the GitHub release." -f ((Get-Date) - $started).TotalSeconds, $artifactsDir) -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
