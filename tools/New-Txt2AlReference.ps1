<#
.SYNOPSIS
  Produces reference AL output with Microsoft's Txt2Al from the same input as Cal2AL, then compares.
.EXAMPLE
  .\New-Txt2AlReference.ps1 -Txt2Al "C:\...\txt2al.exe" -Source .\tests\Fixtures -Work .\compare
#>
param(
    [Parameter(Mandatory)] [string] $Txt2Al,
    [Parameter(Mandatory)] [string] $Source,
    [string] $Work = ".\compare",
    [string] $Cal2Al = "cal2al"
)
$ErrorActionPreference = "Stop"
$reference = Join-Path $Work "txt2al"
$mine = Join-Path $Work "cal2al"
New-Item -ItemType Directory -Force -Path $reference, $mine | Out-Null

# Txt2Al expects new-syntax exports (finsql Command=ExportToNewSyntax). Export with that command for a fair comparison.
& $Txt2Al --source=$Source --target=$reference --rename
& $Cal2Al convert --source $Source --target $mine --quiet
& $Cal2Al compare --cal2al $mine --reference $reference --report (Join-Path $Work "comparison.json") --verbose
