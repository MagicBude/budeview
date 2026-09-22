param(
    [Parameter(Mandatory = $true)]
    [string]$Image
)

$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root 'src\BudeView\BudeView.csproj'
$TraceDir = Join-Path $Root 'results'
$Trace = Join-Path $TraceDir 'viewer-trace.jsonl'

New-Item -ItemType Directory -Force $TraceDir | Out-Null
Remove-Item $Trace -Force -ErrorAction SilentlyContinue

dotnet run --project $Project -- --trace-file $Trace $Image

Write-Host "Trace: $Trace" -ForegroundColor Green
