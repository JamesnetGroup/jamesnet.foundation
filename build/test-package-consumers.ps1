[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PackagePath,

    [Parameter()]
    [string]$EvidenceDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ResolvedPackagePath = (Resolve-Path $PackagePath).Path
$RepositoryRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) {
    $EvidenceDirectory = Join-Path $RepositoryRoot 'artifacts\validation\consumer-assets'
}

$EvidenceDirectory = [System.IO.Path]::GetFullPath($EvidenceDirectory)
New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null

$TemporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) (
    "jamesnet-foundation-consumer-test-{0}" -f [Guid]::NewGuid().ToString('N')
)
$LocalFeed = Join-Path $TemporaryRoot 'feed'

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

function Assert-AssetSelection {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectName,

        [Parameter(Mandatory)]
        [string]$AssetsPath,

        [Parameter(Mandatory)]
        [string[]]$RequiredPatterns,

        [Parameter(Mandatory)]
        [string[]]$ForbiddenPatterns
    )

    if (-not (Test-Path $AssetsPath -PathType Leaf)) {
        throw "$ProjectName did not produce project.assets.json."
    }

    $AssetsText = Get-Content -Raw $AssetsPath
    foreach ($Pattern in $RequiredPatterns) {
        if ($AssetsText -notmatch [regex]::Escape($Pattern)) {
            throw "$ProjectName is missing required asset '$Pattern'."
        }
    }

    foreach ($Pattern in $ForbiddenPatterns) {
        if ($AssetsText -match [regex]::Escape($Pattern)) {
            throw "$ProjectName contains forbidden asset '$Pattern'."
        }
    }

    Copy-Item $AssetsPath (
        Join-Path $EvidenceDirectory "$ProjectName.project.assets.json"
    )
    Write-Host "$ProjectName asset selection passed."
}

try {
    New-Item -ItemType Directory -Force -Path $LocalFeed | Out-Null
    Copy-Item $ResolvedPackagePath $LocalFeed

    $InspectionPath = Join-Path $TemporaryRoot 'package'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory(
        $ResolvedPackagePath,
        $InspectionPath
    )
    $Manifest = Get-ChildItem $InspectionPath -Filter '*.nuspec' -File |
        Select-Object -First 1
    if ($null -eq $Manifest) {
        throw 'The package manifest was not found.'
    }

    [xml]$ManifestXml = Get-Content -Raw $Manifest.FullName
    $NamespaceManager = [System.Xml.XmlNamespaceManager]::new(
        $ManifestXml.NameTable
    )
    $NamespaceManager.AddNamespace('n', $ManifestXml.DocumentElement.NamespaceURI)
    $PackageVersion = $ManifestXml.SelectSingleNode(
        '/n:package/n:metadata/n:version',
        $NamespaceManager
    ).InnerText

    $NuGetConfig = Join-Path $TemporaryRoot 'NuGet.Config'
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="UnifiedLocal" value="$LocalFeed" />
    <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
    <add key="OpenSilver" value="https://www.myget.org/F/opensilver/api/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -Encoding UTF8 $NuGetConfig

    $Projects = @(
        @{
            Name = 'Wpf'
            Project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jamesnet.Foundation" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@
            Required = @(
                'Jamesnet.Foundation.dll',
                'Jamesnet.Platform.WPF.dll'
            )
            Forbidden = @(
                'Jamesnet.Platform.OpenSilver.dll',
                'Jamesnet.Platform.WinUI3.dll',
                'Jamesnet.Platform.Uwp.dll'
            )
        },
        @{
            Name = 'OpenSilver'
            Project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <OpenSilverType>7</OpenSilverType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jamesnet.Foundation" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@
            Required = @(
                'Jamesnet.Foundation.dll',
                'Jamesnet.Platform.OpenSilver.dll'
            )
            Forbidden = @(
                'Jamesnet.Platform.WPF.dll',
                'Jamesnet.Platform.WinUI3.dll',
                'Jamesnet.Platform.Uwp.dll'
            )
        },
        @{
            Name = 'WinUI3'
            Project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
    <RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jamesnet.Foundation" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@
            Required = @(
                'Jamesnet.Foundation.dll',
                'Jamesnet.Platform.WinUI3.dll'
            )
            Forbidden = @(
                'Jamesnet.Platform.WPF.dll',
                'Jamesnet.Platform.OpenSilver.dll',
                'Jamesnet.Platform.Uwp.dll'
            )
        }
    )

    foreach ($Definition in $Projects) {
        $ProjectDirectory = Join-Path $TemporaryRoot $Definition.Name
        New-Item -ItemType Directory -Force -Path $ProjectDirectory | Out-Null
        $ProjectPath = Join-Path $ProjectDirectory "$($Definition.Name).csproj"
        $Definition.Project | Set-Content -Encoding UTF8 $ProjectPath

        Invoke-CheckedCommand dotnet @(
            'restore',
            $ProjectPath,
            '--configfile', $NuGetConfig,
            '--packages', (Join-Path $TemporaryRoot 'packages')
        )

        Assert-AssetSelection `
            -ProjectName $Definition.Name `
            -AssetsPath (Join-Path $ProjectDirectory 'obj\project.assets.json') `
            -RequiredPatterns $Definition.Required `
            -ForbiddenPatterns $Definition.Forbidden
    }

    Write-Host 'Consumer restore and asset-selection validation passed.'
}
finally {
    if (Test-Path $TemporaryRoot) {
        Remove-Item -Recurse -Force $TemporaryRoot
    }
}
