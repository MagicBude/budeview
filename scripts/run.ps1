param(
    [string]$Image
)

$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root 'src\BudeView\BudeView.csproj'

if ([string]::IsNullOrWhiteSpace($Image)) {
    dotnet run --project $Project
} else {
    dotnet run --project $Project -- $Image
}
