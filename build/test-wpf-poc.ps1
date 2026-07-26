[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PackagePath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ResolvedPackagePath = (Resolve-Path $PackagePath).Path
$TemporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) (
    "jamesnet-wpf-poc-test-{0}" -f [Guid]::NewGuid().ToString('N')
)

try {
    $FeedPath = Join-Path $TemporaryRoot 'feed'
    $ProjectPath = Join-Path $TemporaryRoot 'consumer'
    $InspectionPath = Join-Path $TemporaryRoot 'package'
    New-Item -ItemType Directory -Force -Path $FeedPath, $ProjectPath | Out-Null
    Copy-Item $ResolvedPackagePath $FeedPath
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

    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="WpfPoC" value="$FeedPath" />
    <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -Encoding UTF8 (Join-Path $TemporaryRoot 'NuGet.Config')

    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jamesnet.Foundation" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@ | Set-Content -Encoding UTF8 (Join-Path $ProjectPath 'WpfConsumer.csproj')

    dotnet restore (Join-Path $ProjectPath 'WpfConsumer.csproj') `
        --configfile (Join-Path $TemporaryRoot 'NuGet.Config')
    if ($LASTEXITCODE -ne 0) {
        throw "WPF PoC restore failed with exit code $LASTEXITCODE."
    }

    $AssetsPath = Join-Path $ProjectPath 'obj\project.assets.json'
    $AssetsText = Get-Content -Raw $AssetsPath

    foreach ($RequiredAsset in @(
        'Jamesnet.Foundation.dll',
        'Jamesnet.Platform.WPF.dll'
    )) {
        if ($AssetsText -notmatch [regex]::Escape($RequiredAsset)) {
            throw "Required WPF asset was not selected: $RequiredAsset"
        }
    }

    foreach ($ForbiddenAsset in @(
        'Jamesnet.Platform.OpenSilver.dll',
        'Jamesnet.Platform.WinUI3.dll',
        'Jamesnet.Platform.Uwp.dll'
    )) {
        if ($AssetsText -match [regex]::Escape($ForbiddenAsset)) {
            throw "Unexpected platform asset was selected: $ForbiddenAsset"
        }
    }

    dotnet build (Join-Path $ProjectPath 'WpfConsumer.csproj') `
        --configuration Release `
        --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "WPF PoC consumer build failed with exit code $LASTEXITCODE."
    }

    Write-Host 'WPF PoC package restore, asset selection, and build passed.'
}
finally {
    if (Test-Path $TemporaryRoot) {
        Remove-Item -Recurse -Force $TemporaryRoot
    }
}
