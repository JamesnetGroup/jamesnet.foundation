[CmdletBinding()]
param(
    [Parameter()]
    [ValidatePattern('^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$')]
    [string]$Version = '10.0.0-preview.1',

    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [string]$OutputDirectory,

    [Parameter()]
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($Configuration -ne 'Release') {
    throw 'The unified package must be assembled from Release outputs.'
}

$RepositoryRoot = Split-Path -Parent $PSScriptRoot
$NuSpecPath = Join-Path $PSScriptRoot 'Jamesnet.Foundation.nuspec'

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepositoryRoot 'artifacts\packages'
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

    throw 'nuget.exe was not found. Install NuGet CLI or use NuGet/setup-nuget in GitHub Actions.'
}

Push-Location $RepositoryRoot
try {
    $Commit = (git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($Commit)) {
        throw 'Unable to determine the Git commit for package repository metadata.'
    }

    if (-not $SkipBuild) {
        Invoke-CheckedCommand dotnet @(
            'build',
            'src/Jamesnet.Foundation/Jamesnet.Foundation.csproj',
            '--configuration', $Configuration
        )
        Invoke-CheckedCommand dotnet @(
            'build',
            'src/Jamesnet.Platform.WPF/Jamesnet.Platform.WPF.csproj',
            '--configuration', $Configuration
        )
        Invoke-CheckedCommand dotnet @(
            'build',
            'src/Jamesnet.Platform.OpenSilver/Jamesnet.Platform.OpenSilver.csproj',
            '--configuration', $Configuration
        )
        Invoke-CheckedCommand dotnet @(
            'build',
            'src/Jamesnet.Platform.WinUI3/Jamesnet.Platform.WinUI3.csproj',
            '--configuration', $Configuration
        )

        Invoke-CheckedCommand msbuild @(
            'src/Jamesnet.Platform.UWP/Jamesnet.Platform.Uwp.csproj',
            '/restore',
            '/t:Rebuild',
            "/p:Configuration=$Configuration",
            '/p:Platform=AnyCPU',
            '/m'
        )
    }

    $RequiredOutputs = @(
        'src/Jamesnet.Foundation/bin/Release/netstandard2.0/Jamesnet.Foundation.dll',
        'src/Jamesnet.Platform.WPF/bin/Release/net8.0-windows/Jamesnet.Platform.WPF.dll',
        'src/Jamesnet.Platform.OpenSilver/bin/Release/netstandard2.0/Jamesnet.Platform.OpenSilver.dll',
        'src/Jamesnet.Platform.WinUI3/bin/Release/net8.0-windows10.0.19041.0/Jamesnet.Platform.WinUI3.dll',
        'src/Jamesnet.Platform.UWP/bin/Release/Jamesnet.Platform.Uwp.dll'
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
    Write-Host "Unified package created: $PackagePath"
    Get-FileHash $PackagePath -Algorithm SHA256 |
        Format-List Algorithm, Hash, Path
}
finally {
    Pop-Location
}
