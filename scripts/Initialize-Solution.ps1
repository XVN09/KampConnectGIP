<#
.SYNOPSIS
    Zet de volledige KampConnect-solution op (eenmalig).

.DESCRIPTION
    Maakt de gedeelde projecten (000-Shared), de API-gateway (100-Gateway) en alle microservices aan.
    Dit script is eenmalig uitgevoerd bij de start van het project; het blijft in de repository
    zodat de opbouw van de solution reproduceerbaar en gedocumenteerd is.
    Nieuwe services voeg je later toe met New-Service.ps1.
#>
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'KampConnect.Common.ps1')

if (Test-Path (Join-Path $RepoRoot '000-Shared')) {
    throw 'De solution is al opgezet. Gebruik New-Service.ps1 om een service toe te voegen.'
}

# --- 000 - Shared ----------------------------------------------------------------
$sharedContracts = Add-KcProject -Template classlib -Name 'KampConnect.Shared.Contracts'       -Directory '000-Shared'
$serviceDefaults = Add-KcProject -Template classlib -Name 'KampConnect.Shared.ServiceDefaults' -Directory '000-Shared'

# --- 100 - Gateway ---------------------------------------------------------------
$gateway = Add-KcProject -Template web -Name 'KampConnect.Gateway' -Directory '100-Gateway'
Add-KcReference -From $gateway -To $serviceDefaults

# --- Microservices ---------------------------------------------------------------
$services = @(
    @{ Name = 'Identity';     Group = '200-Platform';          Number = 210 }
    @{ Name = 'Tenant';       Group = '200-Platform';          Number = 220 }
    @{ Name = 'User';         Group = '200-Platform';          Number = 230 }
    @{ Name = 'Compliance';   Group = '200-Platform';          Number = 240 }
    @{ Name = 'Camp';         Group = '300-Kamp-Communicatie'; Number = 310 }
    @{ Name = 'Messaging';    Group = '300-Kamp-Communicatie'; Number = 320 }
    @{ Name = 'Calling';      Group = '300-Kamp-Communicatie'; Number = 330 }
    @{ Name = 'Media';        Group = '300-Kamp-Communicatie'; Number = 340 }
    @{ Name = 'Notification'; Group = '300-Kamp-Communicatie'; Number = 350 }
    @{ Name = 'Billing';      Group = '400-Betalingen';        Number = 410 }
    @{ Name = 'Payment';      Group = '400-Betalingen';        Number = 420 }
)

foreach ($s in $services) {
    & (Join-Path $PSScriptRoot 'New-Service.ps1') -Name $s.Name -Group $s.Group -Number $s.Number
}

& (Join-Path $PSScriptRoot 'Test-Structure.ps1')
