[CmdletBinding()]
param(
    [Parameter()]
    [ValidatePattern('^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$')]
    [string]$Version = '10.0.0-wpf-poc.1',

    [Parameter()]
    [string]$OutputDirectory,

    [Parameter()]
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepositoryRoot = Split-Path -Parent $PSScriptRoot
$NuSpecPath = Join-Path $PSScriptRoot 'Jamesnet.Foundation.WpfPoC.nuspec'

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepositoryRoot 'artifacts\wpf-poc'
}

$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory)]
        [string]$Executable,

        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    Write-Host ">> $Executable $($Arguments -join ' ')"
    & $Executable @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "$Executable failed with exit code $LASTEXITCODE."
    }
}

function Resolve-NuGetCommand {
    foreach ($CommandName in @('nuget.exe', 'nuget')) {
        $Command = Get-Command $CommandName -ErrorAction SilentlyContinue
        if ($null -ne $Command) {
            return $Command.Source
        }
    }

    throw @'
nuget.exe was not found.
Download NuGet CLI from https://www.nuget.org/downloads and put it on PATH.
'@
}

Push-Location $RepositoryRoot
try {
    $Commit = (git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($Commit)) {
        throw 'Unable to determine the Git commit.'
    }

    if (-not $SkipBuild) {
        Invoke-CheckedCommand dotnet @(
            'build',
            'src/Jamesnet.Foundation/Jamesnet.Foundation.csproj',
            '--configuration', 'Release'
        )
        Invoke-CheckedCommand dotnet @(
            'build',
            'src/Jamesnet.Platform.WPF/Jamesnet.Platform.WPF.csproj',
            '--configuration', 'Release'
        )
    }

    $RequiredOutputs = @(
        'src/Jamesnet.Foundation/bin/Release/netstandard2.0/Jamesnet.Foundation.dll',
        'src/Jamesnet.Platform.WPF/bin/Release/net8.0-windows/Jamesnet.Platform.WPF.dll'
    )
    $MissingOutputs = @(
        $RequiredOutputs |
            Where-Object { -not (Test-Path (Join-Path $RepositoryRoot $_) -PathType Leaf) }
    )

    if ($MissingOutputs.Count -gt 0) {
        throw "Required Release outputs are missing:`n - $($MissingOutputs -join "`n - ")"
    }

    $NuGetCommand = Resolve-NuGetCommand
    Invoke-CheckedCommand $NuGetCommand @(
        'pack',
        $NuSpecPath,
        '-Version', $Version,
        '-Properties', "commit=$Commit",
        '-BasePath', $RepositoryRoot,
        '-OutputDirectory', $OutputDirectory,
        '-NoPackageAnalysis',
        '-NonInteractive'
    )

    $PackagePath = Join-Path $OutputDirectory "Jamesnet.Foundation.$Version.nupkg"
    if (-not (Test-Path $PackagePath -PathType Leaf)) {
        throw "Expected package was not created: $PackagePath"
    }

    Write-Host ''
    Write-Host 'WPF PoC package created. This package is for local validation only.'
    Write-Host "Package: $PackagePath"
    Get-FileHash $PackagePath -Algorithm SHA256 |
        Format-List Algorithm, Hash, Path
}
finally {
    Pop-Location
}
