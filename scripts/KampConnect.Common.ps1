# Gedeelde hulpfuncties voor de KampConnect-scripts.
# Dit bestand wordt ingeladen (dot-sourced) door de andere scripts; niet rechtstreeks uitvoeren.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:RepoRoot      = Split-Path -Parent $PSScriptRoot
$script:Solution      = Join-Path $script:RepoRoot 'KampConnect.slnx'
$script:PackagesProps = Join-Path $script:RepoRoot 'Directory.Packages.props'

# Bestanden die de dotnet-templates aanmaken maar die we niet willen houden.
$script:TemplateLeftovers = @(
    'Class1.cs',
    'UnitTest1.cs',
    'WeatherForecast.cs',
    'Controllers/WeatherForecastController.cs',
    '*.http'
)

function Invoke-Dotnet {
    # Voert een dotnet-commando uit en stopt het script als het faalt.
    & dotnet @args | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($args -join ' ') is mislukt (exitcode $LASTEXITCODE)."
    }
}

function Get-KcRelativePath {
    # Pad relatief t.o.v. de repo-root, altijd met '/' als scheidingsteken.
    param([Parameter(Mandatory)][string]$Path)
    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($script:RepoRoot).TrimEnd('\', '/')
    if (-not $full.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Pad '$full' ligt niet in de repository."
    }
    return $full.Substring($root.Length).TrimStart('\', '/').Replace('\', '/')
}

function Save-KcXml {
    param([Parameter(Mandatory)][xml]$Xml, [Parameter(Mandatory)][string]$Path)
    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Indent = $true
    $settings.OmitXmlDeclaration = $true
    $settings.Encoding = New-Object System.Text.UTF8Encoding($false)
    $writer = [System.Xml.XmlWriter]::Create($Path, $settings)
    try { $Xml.Save($writer) } finally { $writer.Dispose() }
    Add-Content -LiteralPath $Path -Value '' -Encoding UTF8   # afsluitende newline
}

function Add-KcPackageVersion {
    # Zet een packageversie centraal in Directory.Packages.props (als ze er nog niet staat).
    param([Parameter(Mandatory)][string]$Id, [Parameter(Mandatory)][string]$Version)
    [xml]$props = Get-Content -LiteralPath $script:PackagesProps -Raw
    if ($props.SelectSingleNode("//PackageVersion[@Include='$Id']")) { return }
    $group = $props.SelectSingleNode('/Project/ItemGroup')
    if (-not $group) { $group = $props.DocumentElement.AppendChild($props.CreateElement('ItemGroup')) }
    $node = $props.CreateElement('PackageVersion')
    $node.SetAttribute('Include', $Id)
    $node.SetAttribute('Version', $Version)
    [void]$group.AppendChild($node)
    Save-KcXml -Xml $props -Path $script:PackagesProps
}

function Optimize-KcProjectFile {
    # Haalt instellingen weg die centraal geregeld zijn (Directory.Build.props / Directory.Packages.props).
    param([Parameter(Mandatory)][string]$Path)
    [xml]$xml = Get-Content -LiteralPath $Path -Raw
    foreach ($prop in 'TargetFramework', 'ImplicitUsings', 'Nullable') {
        foreach ($node in @($xml.SelectNodes("//PropertyGroup/$prop"))) {
            [void]$node.ParentNode.RemoveChild($node)
        }
    }
    foreach ($ref in @($xml.SelectNodes('//PackageReference[@Version]'))) {
        Add-KcPackageVersion -Id $ref.GetAttribute('Include') -Version $ref.GetAttribute('Version')
        $ref.RemoveAttribute('Version')
    }
    foreach ($group in @($xml.SelectNodes('//PropertyGroup[not(*)]'))) {
        [void]$group.ParentNode.RemoveChild($group)
    }
    Save-KcXml -Xml $xml -Path $Path
}

function Add-KcProject {
    # Maakt een project aan op de juiste plek, ruimt de template op en voegt het toe aan de solution.
    # De solution folder wordt door 'dotnet sln add' automatisch afgeleid uit het pad.
    param(
        [Parameter(Mandatory)][string]$Template,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Directory,   # relatief t.o.v. de repo-root, bv. 300-Kamp-Communicatie/310-Camp/311-Data
        [string[]]$TemplateArgs = @()
    )
    $projectDir = Join-Path $script:RepoRoot (Join-Path $Directory $Name)
    if (Test-Path -LiteralPath $projectDir) { throw "Project bestaat al: $projectDir" }

    Invoke-Dotnet new $Template -n $Name -o $projectDir --no-restore @TemplateArgs

    foreach ($pattern in $script:TemplateLeftovers) {
        Get-ChildItem -Path (Join-Path $projectDir $pattern) -ErrorAction SilentlyContinue | Remove-Item -Force
    }
    $controllers = Join-Path $projectDir 'Controllers'
    if ((Test-Path $controllers) -and -not (Get-ChildItem $controllers)) { Remove-Item $controllers }

    $csproj = Join-Path $projectDir "$Name.csproj"
    Optimize-KcProjectFile -Path $csproj
    Invoke-Dotnet sln $script:Solution add $csproj
    return $csproj
}

function Add-KcReference {
    param([Parameter(Mandatory)][string]$From, [Parameter(Mandatory)][string[]]$To)
    Invoke-Dotnet add $From reference @To
}
