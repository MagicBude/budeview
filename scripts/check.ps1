$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root 'src\BudeView\BudeView.csproj'

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Command,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE."
    }
}

Write-Host '===== .NET =====' -ForegroundColor Cyan
Invoke-CheckedCommand -Name '.NET version check' -Command {
    dotnet --version
}

Write-Host "`n===== Restore =====" -ForegroundColor Cyan
Invoke-CheckedCommand -Name 'Restore' -Command {
    dotnet restore $Project
}

Write-Host "`n===== Build =====" -ForegroundColor Cyan
Invoke-CheckedCommand -Name 'Build' -Command {
    dotnet build $Project -c Release --no-restore
}

Write-Host "`n===== Self-tests =====" -ForegroundColor Cyan
Invoke-CheckedCommand -Name 'Self-tests' -Command {
    dotnet run --project $Project -c Release --no-build -- --self-test
}

Write-Host "`nBudeView V0.2 checks passed." -ForegroundColor Green
