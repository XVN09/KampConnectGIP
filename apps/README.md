# Apps

De frontends van KampConnect. Elke app krijgt een eigen map:

| Map | App | Voor wie |
|---|---|---|
| `mobile/` | Flutter-app | kinderen, ouders, animatoren en kampleiding |
| `dashboard/` | Webdashboard | organisaties en kampleiding: inschrijven, kampen opzetten en koppelen |
| `parent-portal/` | Ouderportaal op `ouder.kampconnect.be` | ouders die de app niet gebruiken |

Maak een app aan met de tooling van het framework, bv. `flutter create apps/mobile`.
Die tooling zet ook de `.gitignore` van de app klaar.

De apps praten enkel met de API-gateway, nooit rechtstreeks met een service.
