param(
    [Parameter(Mandatory = $true)]
    [string]$Image,

    [int]$Runs = 5
)

$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$PublishScript = Join-Path $PSScriptRoot 'publish-win-x64.ps1'
$Exe = Join-Path $Root 'publish\win-x64\BudeView.exe'
$ResultDir = Join-Path $Root 'artifacts\perf\v0.3-startup'

if (-not (Test-Path -LiteralPath $Image -PathType Leaf)) {
    throw "Image not found: $Image"
}

if ($Runs -lt 1) {
    throw 'Runs must be at least 1.'
}

Write-Host '===== Publish Native AOT =====' -ForegroundColor Cyan
& $PublishScript
if ($LASTEXITCODE -ne 0) {
    throw "Publish failed with exit code $LASTEXITCODE."
}

Remove-Item $ResultDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $ResultDir | Out-Null

$records = @()

Write-Host "`n===== Startup benchmark =====" -ForegroundColor Cyan

for ($run = 1; $run -le $Runs; $run++) {
    $trace = Join-Path $ResultDir ("run-{0:D2}.jsonl" -f $run)

    $argumentList = @(
        '--benchmark-once',
        '--trace-file',
        "`"$trace`"",
        "`"$Image`""
    )

    $process = Start-Process `
        -FilePath $Exe `
        -ArgumentList $argumentList `
        -PassThru

    $peak = 0L

    while (-not $process.HasExited) {
        try {
            $process.Refresh()
            if ($process.WorkingSet64 -gt $peak) {
                $peak = $process.WorkingSet64
            }
        }
        catch {
        }

        Start-Sleep -Milliseconds 10
    }

    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "Run $run failed with exit code $($process.ExitCode)."
    }

    if (-not (Test-Path $trace)) {
        throw "Run $run did not produce a trace."
    }

    $events = Get-Content $trace |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { $_ | ConvertFrom-Json }

    $window = $events |
        Where-Object Event -eq 'window_created' |
        Select-Object -First 1

    $rendered = $events |
        Where-Object Event -eq 'image_rendered' |
        Select-Object -First 1

    $decodeStart = $events |
        Where-Object {
            $_.Event -eq 'decode_start' -and
            $_.Details.preload -eq 'False'
        } |
        Select-Object -First 1

    $decodeEnd = $events |
        Where-Object {
            $_.Event -eq 'decode_end' -and
            $_.Details.preload -eq 'False'
        } |
        Select-Object -First 1

    if ($null -eq $window -or
        $null -eq $rendered -or
        $null -eq $decodeStart -or
        $null -eq $decodeEnd) {
        throw "Run $run trace is missing required events."
    }

    $record = [pscustomobject]@{
        Run = $run
        WindowMs = [double]$window.ElapsedMilliseconds
        FirstImageMs = [double]$rendered.ElapsedMilliseconds
        DecodeMs = [double]$decodeEnd.ElapsedMilliseconds -
            [double]$decodeStart.ElapsedMilliseconds
        PeakMiB = [math]::Round($peak / 1MB, 1)
    }

    $records += $record

    Write-Host (
        "Run {0}: window={1:N1} ms, first-image={2:N1} ms, decode={3:N1} ms, peak={4:N1} MiB" -f
        $record.Run,
        $record.WindowMs,
        $record.FirstImageMs,
        $record.DecodeMs,
        $record.PeakMiB
    )
}

function Get-Percentile {
    param(
        [double[]]$Values,
        [double]$Percentile
    )

    $sorted = @($Values | Sort-Object)
    $index = [math]::Ceiling($Percentile * $sorted.Count) - 1
    $index = [math]::Max(0, [math]::Min($index, $sorted.Count - 1))
    return [double]$sorted[$index]
}

$windowValues = [double[]]($records | ForEach-Object WindowMs)
$firstImageValues = [double[]]($records | ForEach-Object FirstImageMs)
$decodeValues = [double[]]($records | ForEach-Object DecodeMs)
$peakValues = [double[]]($records | ForEach-Object PeakMiB)

Write-Host "`n===== Summary =====" -ForegroundColor Green
Write-Host ("Window        P50={0:N1} ms  P95={1:N1} ms" -f
    (Get-Percentile $windowValues 0.50),
    (Get-Percentile $windowValues 0.95))
Write-Host ("First image   P50={0:N1} ms  P95={1:N1} ms" -f
    (Get-Percentile $firstImageValues 0.50),
    (Get-Percentile $firstImageValues 0.95))
Write-Host ("Decode        P50={0:N1} ms  P95={1:N1} ms" -f
    (Get-Percentile $decodeValues 0.50),
    (Get-Percentile $decodeValues 0.95))
Write-Host ("Peak WS       P50={0:N1} MiB P95={1:N1} MiB" -f
    (Get-Percentile $peakValues 0.50),
    (Get-Percentile $peakValues 0.95))

$csv = Join-Path $ResultDir 'summary.csv'
$records | Export-Csv $csv -NoTypeInformation -Encoding UTF8

Write-Host "`nCSV: $csv"
