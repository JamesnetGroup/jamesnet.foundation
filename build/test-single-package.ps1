[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PackagePath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ResolvedPackagePath = (Resolve-Path $PackagePath).Path
$TemporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) (
    "jamesnet-foundation-package-test-{0}" -f [Guid]::NewGuid().ToString('N')
)

$ExpectedEntries = @(
    'lib/netstandard2.0/Jamesnet.Foundation.dll',
    'lib/netstandard2.0/Jamesnet.Platform.OpenSilver.dll',
    'lib/net8.0-windows/Jamesnet.Foundation.dll',
    'lib/net8.0-windows/Jamesnet.Platform.WPF.dll',
    'lib/net8.0-windows10.0.19041.0/Jamesnet.Foundation.dll',
    'lib/net8.0-windows10.0.19041.0/Jamesnet.Platform.WinUI3.dll',
    'lib/uap10.0/Jamesnet.Foundation.dll',
    'lib/uap10.0/Jamesnet.Platform.Uwp.dll',
    'LICENSE',
    'README.md'
)

try {
    New-Item -ItemType Directory -Force -Path $TemporaryRoot | Out-Null
    Expand-Archive -Path $ResolvedPackagePath -DestinationPath $TemporaryRoot -Force

    $Entries = @(
        Get-ChildItem -Path $TemporaryRoot -Recurse -File |
            ForEach-Object {
                $_.FullName.Substring($TemporaryRoot.Length + 1).Replace('\', '/')
            }
    )

    $MissingEntries = @($ExpectedEntries | Where-Object { $_ -notin $Entries })
    if ($MissingEntries.Count -gt 0) {
        throw "Package entries are missing:`n - $($MissingEntries -join "`n - ")"
    }

    $ForbiddenEntries = @(
        $Entries |
            Where-Object {
                $_ -match '(^|/)(bin|obj|Debug)(/|$)' -or
                $_ -match '\.(user|suo)$'
            }
    )

    if ($ForbiddenEntries.Count -gt 0) {
        throw "Forbidden package entries were found:`n - $($ForbiddenEntries -join "`n - ")"
    }

    $NuSpec = Get-ChildItem -Path $TemporaryRoot -Filter '*.nuspec' -File |
        Select-Object -First 1
    if ($null -eq $NuSpec) {
        throw 'The package does not contain a nuspec manifest.'
    }

    [xml]$NuSpecXml = Get-Content -Raw $NuSpec.FullName
    $NamespaceManager = [System.Xml.XmlNamespaceManager]::new($NuSpecXml.NameTable)
    $NamespaceManager.AddNamespace('n', $NuSpecXml.DocumentElement.NamespaceURI)

    $PackageId = $NuSpecXml.SelectSingleNode(
        '/n:package/n:metadata/n:id',
        $NamespaceManager
    ).InnerText

    if ($PackageId -ne 'Jamesnet.Foundation') {
        throw "Unexpected package ID: $PackageId"
    }

    $ExpectedFrameworks = @(
        'netstandard2.0',
        'net8.0-windows',
        'net8.0-windows10.0.19041.0',
        'uap10.0'
    )
    $ActualFrameworks = @(
        $NuSpecXml.SelectNodes(
            '/n:package/n:metadata/n:dependencies/n:group',
            $NamespaceManager
        ) |
            ForEach-Object { $_.targetFramework }
    )

    $MissingFrameworks = @(
        $ExpectedFrameworks |
            Where-Object { $_ -notin $ActualFrameworks }
    )
    if ($MissingFrameworks.Count -gt 0) {
        throw "Dependency groups are missing:`n - $($MissingFrameworks -join "`n - ")"
    }

    Write-Host "Package validation passed: $ResolvedPackagePath"
    Write-Host "Entries: $($Entries.Count)"
    Get-FileHash $ResolvedPackagePath -Algorithm SHA256 |
        Format-List Algorithm, Hash, Path
}
finally {
    if (Test-Path $TemporaryRoot) {
        Remove-Item -Recurse -Force $TemporaryRoot
    }
}
