<#
.SYNOPSIS
    Maakt een nieuwe KampConnect-microservice aan volgens de vaste projectstructuur.

.DESCRIPTION
    Maakt de zeven projecten van een microservice aan (Data, Business en Service) plus een testproject,
    voegt ze toe aan KampConnect.slnx (solution folders volgen automatisch de mappen op schijf)
    en legt de projectreferenties volgens de afhankelijkheidsregels.

.EXAMPLE
    .\backend\scripts\New-Service.ps1 -Name Camp -Group 300-Camp-Communication -Number 310
#>
[CmdletBinding()]
param(
    # Naam van de service in PascalCase, bv. Camp
    [Parameter(Mandatory)][ValidatePattern('^[A-Z][A-Za-z]+$')][string]$Name,

    # Groepsmap, bv. 300-Camp-Communication
    [Parameter(Mandatory)][ValidatePattern('^[1-8]00-[A-Za-z-]+$')][string]$Group,

    # Nummer van de service binnen de groep, een tiental, bv. 310
    [Parameter(Mandatory)][ValidateRange(110, 890)][int]$Number
)

. (Join-Path $PSScriptRoot 'KampConnect.Common.ps1')

# --- Controles vooraf ---------------------------------------------------------
if ($Number % 10 -ne 0) { throw "Het servicenummer moet een tiental zijn (bv. 310), niet $Number." }
if ([math]::Floor($Number / 100) -ne [int]$Group.Substring(0, 1)) {
    throw "Servicenummer $Number hoort niet bij groep $Group."
}
$groupDir = Join-Path $BackendRoot $Group
if (Test-Path $groupDir) {
    $taken = Get-ChildItem $groupDir -Directory | Where-Object { $_.Name -like "$Number-*" -or $_.Name -like "*-$Name" }
    if ($taken) { throw "Nummer $Number of naam $Name is al in gebruik: $($taken.Name -join ', ')" }
}

# --- Projecten aanmaken --------------------------------------------------------
$p          = "KampConnect.$Name"
$serviceDir = "$Group/$Number-$Name"
$dataDir    = "$serviceDir/$($Number + 1)-Data"
$busDir     = "$serviceDir/$($Number + 2)-Business"
$svcDir     = "$serviceDir/$($Number + 3)-Service"

$entity     = Add-KcProject -Template classlib -Name "$p.Data.Entity"        -Directory $dataDir
$context    = Add-KcProject -Template classlib -Name "$p.Data.Context"       -Directory $dataDir
$interface  = Add-KcProject -Template classlib -Name "$p.Data.Interface"     -Directory $dataDir
$repository = Add-KcProject -Template classlib -Name "$p.Data.Repository"    -Directory $dataDir
$contracts  = Add-KcProject -Template classlib -Name "$p.Business.Contracts" -Directory $busDir
$logic      = Add-KcProject -Template classlib -Name "$p.Business.Logic"     -Directory $busDir
$service    = Add-KcProject -Template webapi   -Name "$p.Service"            -Directory $svcDir `
                            -TemplateArgs '--use-controllers', '--no-openapi'
$tests      = Add-KcProject -Template xunit    -Name "$p.Tests"              -Directory '900-Tests'

$sharedContracts = Join-Path $BackendRoot '000-Shared/KampConnect.Shared.Contracts/KampConnect.Shared.Contracts.csproj'
$serviceDefaults = Join-Path $BackendRoot '000-Shared/KampConnect.Shared.ServiceDefaults/KampConnect.Shared.ServiceDefaults.csproj'

# --- Referenties volgens de afhankelijkheidsregels ----------------------------
Add-KcReference -From $context    -To $entity
Add-KcReference -From $interface  -To $entity
Add-KcReference -From $repository -To $interface, $context, $entity
Add-KcReference -From $logic      -To $contracts, $interface, $entity, $sharedContracts
Add-KcReference -From $service    -To $logic, $contracts, $repository, $context, $serviceDefaults, $sharedContracts
Add-KcReference -From $tests      -To $logic, $contracts, $entity

Write-Host ""
Write-Host "Service '$Name' aangemaakt in $serviceDir." -ForegroundColor Green
Write-Host "Controleer de structuur met: .\backend\scripts\Test-Structure.ps1"
