$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root 'src\BudeView\BudeView.csproj'
$Output = Join-Path $Root 'publish\win-x64'

Remove-Item $Output -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $Output | Out-Null

dotnet publish $Project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $Output

Write-Host "Published: $Output" -ForegroundColor Green
