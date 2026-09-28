# ADR 0001 – Projectstructuur van de backend

- **Status:** aanvaard, aangevuld door [ADR 0002](0002-monorepo.md) en [ADR 0003](0003-regels-helpers-hosting.md)
- **Datum:** 2026-09-23

## Context

KampConnect bestaat uit elf microservices. Zonder vaste afspraken groeit elke service op zijn eigen manier,
wat onderhoud, debuggen en documenteren moeilijker maakt. Solution folders in Visual Studio zijn bovendien
virtueel en lopen makkelijk uit de pas met de mappen op schijf.

## Beslissing

- Eén solution (`KampConnect.slnx`) in één repository voor alle services.
- Genummerde groepen: 000-Shared, 100-Gateway, 200-Platform, 300-Kamp-Communicatie, 400-Betalingen, 900-Tests.
- Elke service heeft zeven projecten in drie lagen (Data, Business, Service) plus een testproject.
- De mappen op schijf zijn leidend; solution folders worden door `dotnet sln add` afgeleid uit het pad.
- Services worden aangemaakt met `scripts/New-Service.ps1`, gecontroleerd met `scripts/Test-Structure.ps1`.
- Gedeelde instellingen in `Directory.Build.props`, packageversies in `Directory.Packages.props`.

## Gevolgen

- Alle services hebben dezelfde opbouw; wie er één begrijpt, begrijpt ze allemaal.
- De compiler dwingt de laagscheiding af; `Test-Structure.ps1` controleert de afhankelijkheidsregels
  en verbiedt referenties tussen services.
- Veel projecten (±90); de scripts en centrale configuratie vangen het onderhoud op.
- Een wijziging aan deze structuur wordt vastgelegd in een nieuwe ADR.

## Gebaseerd op

De structuur van de Activak-backend, met deze aanpassingen: `Business.Shared` werd `Business.Contracts`,
er kwam een apart `Business.Logic`-project, `Data.Store` is geschrapt, interfaces hebben één vaste plek,
en logbestanden en client-side libraries horen niet in een API-project.
