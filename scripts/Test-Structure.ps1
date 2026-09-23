<#
.SYNOPSIS
    Controleert of de solution en de mappenstructuur nog overeenkomen en of de afhankelijkheidsregels
    gerespecteerd worden.

.DESCRIPTION
    Controles:
      1. Elk .csproj-bestand op schijf staat in KampConnect.slnx, en omgekeerd.
      2. De map van een project heeft dezelfde naam als het project.
      3. De solution folder van een project is gelijk aan zijn pad op schijf.
      4. Projectnamen volgen de conventie KampConnect.<...>.
      5. Projectreferenties volgen de afhankelijkheidsregels (geen referenties tussen services,
         lagen enkel naar binnen).
    Het script eindigt met exitcode 1 als er iets fout is, zodat het ook in CI gebruikt kan worden.

.EXAMPLE
    .\scripts\Test-Structure.ps1
#>
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'KampConnect.Common.ps1')

$errors = New-Object System.Collections.Generic.List[string]

# Toegelaten referenties per laag (binnen dezelfde service) en naar 000-Shared.
$allowed = @{
    'Data.Entity'        = @()
    'Data.Context'       = @('Data.Entity')
    'Data.Interface'     = @('Data.Entity')
    'Data.Repository'    = @('Data.Interface', 'Data.Context', 'Data.Entity')
    'Business.Contracts' = @()
    'Business.Logic'     = @('Business.Contracts', 'Data.Interface', 'Data.Entity', 'Shared.Contracts')
    'Service'            = @('Business.Logic', 'Business.Contracts', 'Data.Repository', 'Data.Context',
                             'Shared.ServiceDefaults', 'Shared.Contracts')
    'Tests'              = @('Business.Logic', 'Business.Contracts', 'Data.Entity', 'Data.Interface',
                             'Data.Repository', 'Data.Context', 'Service', 'Shared.Contracts')
}
$allowedShared = @{
    'Shared.Contracts'       = @()
    'Shared.ServiceDefaults' = @()
    'Gateway'                = @('Shared.ServiceDefaults')
}

function Split-KcName([string]$ProjectName) {
    # KampConnect.Camp.Data.Entity -> Service = Camp, Layer = Data.Entity
    # KampConnect.Shared.Contracts / KampConnect.Gateway -> Service = '', Layer = Shared.Contracts / Gateway
    $rest = $ProjectName -replace '^KampConnect\.', ''
    if ($rest -like 'Shared.*' -or $rest -eq 'Gateway') { return @{ Service = ''; Layer = $rest } }
    $parts = $rest -split '\.', 2
    if ($parts.Count -lt 2) { return @{ Service = $rest; Layer = '' } }
    return @{ Service = $parts[0]; Layer = $parts[1] }
}

# --- 1. Solution vs. schijf ------------------------------------------------------
[xml]$sln = Get-Content -LiteralPath $Solution -Raw
$slnProjects = @{}   # relatief pad -> solution folder
foreach ($node in @($sln.SelectNodes('//Project'))) {
    $path   = $node.GetAttribute('Path').Replace('\', '/')
    $parent = $node.ParentNode
    # LocalName i.p.v. Name: bij XML geeft PowerShell voor .Name het attribuut 'Name' terug.
    $folder = if ($parent.LocalName -eq 'Folder') { $parent.GetAttribute('Name') } else { '/' }
    $slnProjects[$path] = $folder
}

$diskProjects = @(
    Get-ChildItem -Path $RepoRoot -Recurse -Filter *.csproj -File |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj|\.git|\.vs)[\\/]' } |
        ForEach-Object { Get-KcRelativePath $_.FullName }
)

foreach ($path in $diskProjects) {
    if (-not $slnProjects.ContainsKey($path)) { $errors.Add("Staat op schijf maar niet in de solution: $path") }
}
foreach ($path in $slnProjects.Keys) {
    if ($diskProjects -notcontains $path) { $errors.Add("Staat in de solution maar niet op schijf: $path") }
}

# --- 2 t/m 5. Per project ----------------------------------------------------------
foreach ($path in $diskProjects) {
    $name      = [System.IO.Path]::GetFileNameWithoutExtension($path)
    $dirs      = $path.Split('/')
    $folderName = $dirs[$dirs.Count - 2]

    if ($folderName -ne $name) { $errors.Add("Mapnaam '$folderName' verschilt van projectnaam '$name' ($path)") }
    if ($name -notmatch '^KampConnect\.[A-Z]') { $errors.Add("Projectnaam volgt de conventie KampConnect.<...> niet: $name") }

    if ($slnProjects.ContainsKey($path)) {
        if ($dirs.Count -lt 3) { $expected = '/' }
        else { $expected = '/' + (($dirs[0..($dirs.Count - 3)]) -join '/') + '/' }
        if ($slnProjects[$path] -ne $expected) {
            $errors.Add("Solution folder '$($slnProjects[$path])' komt niet overeen met map '$expected' voor $name")
        }
    }

    $me = Split-KcName $name
    $table = if ($me.Service -eq '') { $allowedShared } else { $allowed }
    if (-not $table.ContainsKey($me.Layer)) { $errors.Add("Onbekende laag '$($me.Layer)' in project $name"); continue }
    $rules = @($table[$me.Layer])

    [xml]$proj = Get-Content -LiteralPath (Join-Path $RepoRoot $path) -Raw
    foreach ($ref in @($proj.SelectNodes('//ProjectReference'))) {
        $refName = [System.IO.Path]::GetFileNameWithoutExtension($ref.GetAttribute('Include').Replace('\', '/'))
        $other   = Split-KcName $refName
        if ($other.Service -ne '' -and $other.Service -ne $me.Service) {
            $errors.Add("$name verwijst naar een andere service: $refName")
        }
        elseif ($rules -notcontains $other.Layer) {
            $errors.Add("$name mag niet verwijzen naar $refName (laag $($other.Layer))")
        }
    }
}

# --- Resultaat ---------------------------------------------------------------------
if ($errors.Count -eq 0) {
    Write-Host "Structuur OK: $($diskProjects.Count) projecten gecontroleerd." -ForegroundColor Green
    exit 0
}
Write-Host "Er zijn $($errors.Count) problemen gevonden:" -ForegroundColor Red
$errors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
exit 1
