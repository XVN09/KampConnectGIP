# KampConnect — API-documentatie

Eén map per microservice, met dezelfde nummering en volgorde als in de GIP-bundel en in de map `Databases`.
Elke servicemap heeft twee submappen, allebei met versienummer in de bestandsnaam (zoals bij de ERD's):

```
API/
├── 00. Alle PDFs/    kopie van alle PDF's samen (01. IdentityServiceApiV1.pdf, …)
├── 01. Identity Service/
│   ├── OpenAPI/IdentityServiceOpenApiV1.yaml
│   └── PDF/IdentityServiceApiV1.pdf
├── 02. Tenant Service/
│   ├── OpenAPI/TenantServiceOpenApiV1.yaml
│   └── PDF/TenantServiceApiV1.pdf
├── …
├── 11. Media Service/
├── _assets/          logo en lettertypes voor de PDF's
├── gen.py            genereert de OpenAPI-specs 02 t.e.m. 11
└── build_pdfs.py     maakt van elke spec een PDF
```

| Nr | Service | Basispad | Endpoints |
|---|---|---|---|
| 01 | Identity | `auth.kampconnect.be` | 11 |
| 02 | Tenant | `/api/tenants`, `/api/invitations` | 19 |
| 03 | User | `/api/users`, `/api/guardian-links` | 19 |
| 04 | Camp | `/api/camps` | 22 |
| 05 | Messaging | `/api/conversations`, `/api/messages` + hub `/hubs/chat` | 13 |
| 06 | Calling | `/api/calls` + hub `/hubs/calls` | 8 |
| 07 | Billing | `/api/billing` | 8 |
| 08 | Payment | `/api/payments`, `/api/webhooks` | 5 |
| 09 | Notification | `/api/notifications` | 7 |
| 10 | Compliance | `/api/compliance` | 11 |
| 11 | Media | `/api/files` | 5 |

De API Gateway (YARP) heeft geen eigen endpoints en dus geen specificatie.

## Nieuwe versie maken

1. Zet `VERSION = 2` bovenaan `gen.py` en pas de specs aan. Voer daarna `python gen.py` uit.
   Voor de Identity-spec kopieer je `IdentityServiceOpenApiV1.yaml` naar `…V2.yaml` en pas je die aan.
2. Voer `python build_pdfs.py` uit. Het script maakt een PDF van de **hoogste** versie in elke `OpenAPI`-map.
   Oudere versies blijven staan.

Vereisten: `pip install pyyaml markdown pypdf playwright` en `playwright install chromium`.

## Afspraken

- Paden in kebab-case en in het Engels (5.0.2). JSON-velden in camelCase (standaard in ASP.NET Core).
- De organisatie komt uit de claim `tenant_id` van het access token en staat niet in de URL.
- Elk endpoint vermeldt welke rollen het mogen aanroepen (`guardian`, `staff`, `organisation_admin`, `platform_admin`).
- Fouten volgen RFC 7807 (Problem Details), de OAuth-endpoints volgen RFC 6749.
- Realtime verkeer (chat, signaling) loopt via SignalR. De hubs staan in de beschrijving van de service.

## Bekijken in Swagger

- Snel: plak een `.yaml` in <https://editor.swagger.io>.
- In de code: zet het bestand in `wwwroot/swagger/` van de service en voeg in `Program.cs` dit toe:
  `app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/TenantServiceOpenApiV1.yaml", "Tenant Service V1"));`
