# Architektur

Walter (Saverwalter) ist eine Webanwendung zur Verwaltung von Wohnungen, Mietverhältnissen und Betriebskostenabrechnungen.

---

## Technologie-Stack

| Schicht       | Technologie                              |
|---------------|------------------------------------------|
| Backend       | .NET 8, ASP.NET Core (REST API)          |
| ORM           | Entity Framework Core                    |
| Datenbank     | PostgreSQL                               |
| Frontend      | Svelte, Carbon Design System, D3.js      |
| Dateiablage   | S3-kompatibel (MinIO im Entwicklungs-Stack) |
| Observability | OpenTelemetry (konfigurierbar)           |
| Tests         | xUnit (Unit/Integration), Playwright (E2E) |
| CI            | GitHub Actions                           |

---

## Projektstruktur

```
Deeplex.Saverwalter.Model/                    ← Datenmodelle & EF Core DbContext
Deeplex.Saverwalter.BetriebskostenabrechnungService/  ← Berechnungslogik Nebenkosten
Deeplex.Saverwalter.ErhaltungsaufwendungService/      ← Logik Erhaltungsaufwendungen
Deeplex.Saverwalter.PrintService/             ← Word- und PDF-Generierung
Deeplex.Saverwalter.WebAPI/                   ← REST API + eingebettetes Svelte-Frontend
Deeplex.Saverwalter.CLI/                      ← Kommandozeilen-Werkzeuge (Datenbankinitialisierung)
Deeplex.Saverwalter.InitiateTestDbs/          ← Testdaten-Seed für Entwicklung und Tests
```

Jedes Service-Projekt hat ein zugehöriges Test-Projekt (Suffix `.Tests`).

---

## Schichten

```
┌──────────────────────────────────────┐
│  Svelte-Frontend (SPA, eingebettet)  │
└──────────────┬───────────────────────┘
               │ HTTP/JSON
┌──────────────▼───────────────────────┐
│  ASP.NET Core REST API               │
│  Controller → Handler/Services       │
└──────────────┬───────────────────────┘
               │
       ┌───────┴────────┐
       │                │
┌──────▼──────┐  ┌──────▼──────────────┐
│ Abrechnung- │  │ EF Core DbContext    │
│ Services    │  │ (PostgreSQL)        │
└─────────────┘  └─────────────────────┘
       │
┌──────▼──────┐
│ PrintService│  (Word/PDF via OOXML)
└─────────────┘
```

---

## Umgebungsvariablen

| Variable            | Beschreibung                                    |
|---------------------|-------------------------------------------------|
| `DATABASE_HOST`     | PostgreSQL-Host                                 |
| `DATABASE_PORT`     | PostgreSQL-Port (Standard: 5432)                |
| `DATABASE_USER`     | Datenbankbenutzer                               |
| `DATABASE_PASSWORD` | Datenbankpasswort                               |
| `DATABASE_NAME`     | Datenbankname                                   |
| `S3_PROVIDER`       | S3-Anbieter (`minio` oder AWS-kompatibel)       |
| `S3_*`              | S3-Verbindungsparameter (Endpoint, Bucket, etc.) |
| `OTEL_ENDPOINT`     | OpenTelemetry Collector Endpoint (optional)     |

---

## Authentifizierung

Walter verwendet ein eigenes Benutzer- und Rechtesystem. Kontakte können mit `UserAccount`-Einträgen verknüpft werden. Die Zugriffskontrolle erfolgt pro Vertrag/Wohnung über `Verwalter`-Einträge.

---

## Weiterführende Dokumentation

- [Datenmodell](datenmodell.md) — alle Entitäten und Felder
- [Betriebskostenabrechnung](betriebskostenabrechnung.md) — Berechnungsprozess im Detail
