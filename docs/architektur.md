# Architektur

Walter (Saverwalter) ist eine Webanwendung zur Verwaltung von Wohnungen, Mietverhältnissen und Betriebskostenabrechnungen. Sämtliche Geld­bewegungen werden seit dem Zahlungsmodell-Umbau über eine **doppelte Buchführung** (Buchungssätze auf Buchungskonten) abgebildet.

---

## Technologie-Stack

| Schicht       | Technologie                              |
|---------------|------------------------------------------|
| Backend       | .NET 8, ASP.NET Core (REST API)          |
| ORM           | Entity Framework Core (Npgsql)           |
| Datenbank     | PostgreSQL                               |
| DI-Container  | Simple Injector                          |
| Frontend      | SvelteKit, Carbon Design System, D3.js   |
| Dateiablage   | S3-kompatibel (MinIO im Entwicklungs-Stack) |
| Observability | OpenTelemetry (optional, via `OTEL_ENDPOINT`) |
| Tests         | xUnit (Backend), Vitest + Playwright (Frontend) |
| CI            | GitHub Actions                           |

---

## Projektstruktur

```
Deeplex.Saverwalter.Model/                            ← EF-Core-Entitäten, DbContext, Migrationen
Deeplex.Saverwalter.BetriebskostenabrechnungService/  ← Berechnungslogik Nebenkosten (rein fachlich)
Deeplex.Saverwalter.PrintService/                     ← Word- und PDF-Generierung (OOXML / MigraDoc)
Deeplex.Saverwalter.WebAPI/                           ← REST API + eingebettetes SvelteKit-Frontend (svelte/)
Deeplex.Saverwalter.CLI/                              ← Kommandozeilen-Werkzeuge (Datenbankinitialisierung)
Deeplex.Saverwalter.InitiateTestDbs/                  ← Testdaten-Seed für Entwicklung und Tests
```

Test-Projekte (xUnit): `Deeplex.Saverwalter.Model.Tests`, `…PrintService.Tests`, `…WebAPI.Tests`.

> **Hinweis:** Das frühere `Deeplex.Saverwalter.ErhaltungsaufwendungService`-Projekt wurde entfernt. Erhaltungsaufwendungen sind heute reine Buchungssätze auf dem `AufwandsKonto` der Wohnung (siehe `WebAPI/Services/Buchungen/ErhaltungsaufwendungBuchungsService`).

### Aufbau der WebAPI

```
Deeplex.Saverwalter.WebAPI/
├── Controllers/              ← HTTP-Endpunkte (ein Controller je Ressource)
│   └── Utils/                ← Selection-Listen, Datei-Handling, Paging
├── Services/
│   ├── DbServices/           ← CRUD-/Lade-Logik je Entität (von Controllern aufgerufen)
│   ├── Buchungen/            ← Erzeugen von Buchungssätzen (Transaktion, Erhaltungs-
│   │                            aufwendung, Betriebskostenrechnung, NK-Anteil, Storno, …)
│   └── Abrechnung/           ← Abrechnungslauf (Berechnung + Druck + DTOs)
├── Utils/                    ← Querschnitt (FileHandling, AccountExtensions, …)
└── svelte/                   ← SvelteKit-Frontend (Quelle); Build landet in wwwroot/
```

---

## Schichten

```
┌──────────────────────────────────────┐
│  SvelteKit-Frontend (SPA, eingebettet)│
└──────────────┬───────────────────────┘
               │ HTTP/JSON
┌──────────────▼───────────────────────┐
│  ASP.NET Core REST API                │
│  Controller → DbService / BuchungsService
└──────────────┬───────────────────────┘
               │
       ┌───────┴─────────────┐
       │                     │
┌──────▼───────────┐  ┌──────▼──────────────┐
│ Abrechnungs- /   │  │ EF Core DbContext    │
│ Buchungs-Services│  │ (PostgreSQL)         │
└──────┬───────────┘  └─────────────────────┘
       │
┌──────▼──────┐
│ PrintService│  (Word/PDF via OOXML / MigraDoc)
└─────────────┘
```

---

## Buchführungs-Kern

Alle Geldbewegungen laufen über vier Modell-Typen (Details siehe [Datenmodell](datenmodell.md)):

- **Buchungskonto** — ein Konto im Kontenrahmen (Aktiv / Passiv / Aufwand / Ertrag). Z. B. das `AufwandsKonto` einer Wohnung, das `NkBuchungskonto`/`MietBuchungskonto` eines Vertrags oder das `VerbindlichkeitsKonto` eines Kontakts.
- **Buchungssatz** — ein vollständiger, gemäß § 239 HGB lückenlos nummerierter Buchungssatz mit Buchungsjahr und optionalem Belegpfad. Korrekturen erfolgen ausschließlich per **Stornobuchung** (`StornoVon`).
- **Buchungszeile** — eine Soll- oder Haben-Zeile eines Buchungssatzes auf genau einem Buchungskonto.
- **OffenerPostenAusgleich** — verbindet eine offene Forderung (Soll-Zeile) mit ihrer ausgleichenden Zahlung (Haben-Zeile) auf demselben Konto (OPOS-Prinzip).

Eine importierte oder erfasste **Transaktion** (Kontoauszugseintrag) wird über `Buchungssaetze` mit den passenden Buchungen verknüpft.

---

## Umgebungsvariablen

| Variable            | Beschreibung                                    |
|---------------------|-------------------------------------------------|
| `DATABASE_HOST`     | PostgreSQL-Host                                 |
| `DATABASE_PORT`     | PostgreSQL-Port (Standard: 5432)                |
| `DATABASE_USER`     | Datenbankbenutzer                               |
| `DATABASE_PASS`     | Datenbankpasswort                               |
| `DATABASE_NAME`     | Datenbankname                                   |
| `S3_*`              | S3/MinIO-Verbindungsparameter (Endpoint, Bucket, Keys) |
| `OTEL_ENDPOINT`     | OpenTelemetry Collector Endpoint (optional)     |

---

## Authentifizierung & Autorisierung

Walter verwendet ein eigenes Benutzer- und Rechtesystem. Ein `UserAccount` (mit `Username`, `IsAdmin`) kann mit einem `Kontakt` verknüpft sein. Der Zugriff wird pro Wohnung/Vertrag über `Verwalter`-Einträge gesteuert (`VerwalterRolle`: `Eigentuemer`, `Vollmacht`, `Ableser`). Die API setzt dies über `IAuthorizationHandler`-Implementierungen je Ressource durch; die Authentifizierung erfolgt über einen Token-Scheme-Handler (`TokenAuthenticationHandler`).

---

## Weiterführende Dokumentation

- [Datenmodell](datenmodell.md) — alle Entitäten und Felder
- [Betriebskostenabrechnung](betriebskostenabrechnung.md) — Berechnungsprozess im Detail
- [Leitfaden für Buchhalter](buchhalter.md) — Jahresablauf und Datenpflege
