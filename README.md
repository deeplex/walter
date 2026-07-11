# Walter

Walter (Saverwalter) ist eine Webanwendung zur Verwaltung von Wohnungen,
Mietverhältnissen und Betriebskostenabrechnungen (Nebenkostenabrechnungen). Jede
Geldbewegung — Miete, Eingangsrechnungen, Erhaltungsaufwand, das Abrechnungsergebnis —
wird als **doppelter Buchungssatz** (`Buchungssatz`) auf Buchungskonten
(`Buchungskonto`) erfasst. Fachentitäten (Wohnung, Vertrag, Umlage, Kontakt) besitzen
ihre Konten, sodass der Kontenrahmen im Alltag nicht stört.

## Funktionsüberblick

- **Stammdaten** — Wohnungen (versionierte Flächen/Anteile, Eigentümer), Verträge
  (versionierte Grundmiete, Kaution, Mietminderungen), Kontakte, Garagen, Zähler.
- **Doppelte Buchführung** — jeder Betrag ist eine Buchung; Korrekturen erfolgen
  ausschließlich per Stornobuchung und halten den Bestand lückenlos (§ 239 HGB).
- **Transaktionen** — ein Kontoauszugseintrag wird einmalig als `Transaktion` mit
  Positionen erfasst (Miete, Eingangsrechnung, Erhaltungsaufwand …); Walter erzeugt
  daraus die Buchungssätze und legt fehlende Forderungen automatisch an.
- **Betriebskostenabrechnung** — je Abrechnungsgruppe, mit zeitanteiliger Verteilung,
  allen Umlageschlüsseln und Heizkosten nach HKVO; Vorschau, Buchen, Drucken (PDF/Word),
  Rückabwicklung oder Storno.
- **Jahresabschlusskontrolle** — Bilanzansicht je Jahr plus Abrechnungsstatus; ein Jahr
  gilt als abgeschlossen, wenn keine Konten mehr offen sind und jede Abrechnung
  abgesendet (oder verzichtet) ist.
- **Zugriffssteuerung** — Rollen je Wohnung (Eigentümer, Vollmacht, Ableser),
  API-weit durchgesetzt.
- **Dateiablage** — Belege an Entitäten und Buchungen, gespeichert in S3/MinIO.

## Technologie

.NET 8 / ASP.NET Core (REST API), Entity Framework Core auf PostgreSQL, ein SvelteKit-
Frontend mit Carbon Design System (in die API eingebettet), Word-/PDF-Erzeugung über
OOXML / MigraDoc sowie eine S3-kompatible Dateiablage (MinIO im Entwicklungs-Stack).

## Dokumentation

Die ausführliche Dokumentation liegt unter [`docs/`](docs/README.md):

- [Architektur](docs/architektur.md) — Stack, Projektstruktur, Schichten, Umgebung
- [Datenmodell](docs/datenmodell.md) — Entitäten, Felder, Beziehungen (ER-Diagramme)
- [Betriebskostenabrechnung](docs/betriebskostenabrechnung.md) — Berechnung & API
- [Leitfaden für Buchhalter](docs/buchhalter.md) — Buchführungs-Workflow & Jahresablauf
- [Bedienungsleitfaden](docs/bedienung.md) — die Weboberfläche, Seite für Seite
- [Tests](docs/testing.md) — Test-Ebenen, Entwicklungs-Nutzer, Berechtigungsmatrix

## Entwicklungsumgebung mit realistischen Demodaten aufsetzen

Das Bootstrap-Skript wird **innerhalb des Devcontainers** ausgeführt und bereitet die
Entwicklungsdaten vor. Es setzt voraus, dass die Devcontainer-Dienste bereits laufen,
installiert dann die Abhängigkeiten und seedet zwei realistische Entwicklungsdatenbanken:

- `walter_dev_generic_db` (Standard: 120 Wohnungen)
- `walter_dev_full_generic_db` (Standard: 320 Wohnungen)

Ausführen:

```bash
./scripts/bootstrap-dev.sh
```

Optionale Feineinstellung:

```bash
WALTER_DEV_TARGET_WOHNUNGEN=90 \
WALTER_DEV_FULL_TARGET_WOHNUNGEN=250 \
WALTER_DEV_RANDOM_SEED=4242 \
./scripts/bootstrap-dev.sh
```

Das Skript gibt anschließend die genauen Startbefehle für `dotnet watch` und das Frontend
aus, dazu eine Zugriffsübersicht der geseedeten Nutzer samt Wohnungs-Sichtbarkeit sowie
einen gut sichtbaren Zugangsdatenblock mit allen Benutzernamen und dem gemeinsamen
Passwort.

Geseedete Entwicklungs-Nutzer (Standardpasswort = `DATABASE_PASS`):

- primärer DB-Nutzer (`DATABASE_USER`)
- `admin.dev`
- `owner.dev`
- `manager.dev`
- `viewer.dev`
- `limited.dev`

Ist MinIO unter `s3:9000` erreichbar (Devcontainer-Standard), lädt das Bootstrap zusätzlich
realistische Platzhalter-PDFs zu einer Auswahl von `vertraege`, `wohnungen`,
`erhaltungsaufwendungen`, `adressen` und `kontakte` hoch, damit die Datei-Panels in der
Oberfläche nicht leer sind. Das Ziel lässt sich über `S3_PROVIDER=...` überschreiben oder
das Datei-Seeding mit `WALTER_DEV_SKIP_FILE_SEED=1` ganz überspringen.

## Dateiablage (S3 / MinIO)

Der Entwicklungs-Compose-Stack betreibt MinIO als S3-kompatibles Datei-Backend. Sowohl
Walter als auch MinIO stehen unter AGPL-3.0, das Bündeln erzeugt also keinen
Lizenzkonflikt.

- API-Endpunkt: `http://s3:9000` (im Devcontainer) /
  `http://localhost:9000` (vom Host)
- Standard-Bucket: `saverwalter`
- Walter-API-Basis-URL: `http://s3:9000/saverwalter`
  (als `S3_PROVIDER` für das Backend gesetzt)
- Konsole: `http://localhost:9001` (Login `minioadmin` / `minioadmin`)

Der Bucket wird vom `s3-init`-Sidecar-Dienst angelegt und auf eine öffentliche, anonyme
Policy geschaltet. Das ist ausschließlich für die Entwicklung akzeptabel;
Produktionsumgebungen müssen MinIO (oder einen anderen S3-Anbieter) hinter echte
Zugangsdaten stellen.

## Vollständigen Dev-Watch-Stack starten (im Devcontainer)

Dieses Skript startet Backend und Frontend im Watch-Modus, verbunden mit der geseedeten
Entwicklungsdatenbank:

```bash
./scripts/dev-watch-stack.sh
```

Standardwerte:

- Backend-DB: `walter_dev_generic_db`
- Frontend-URL: `http://localhost:5173`

Nützliche Optionen:

```bash
DATABASE_NAME=walter_dev_full_generic_db ./scripts/dev-watch-stack.sh
```

```bash
WALTER_BOOTSTRAP=1 ./scripts/dev-watch-stack.sh
```

`WALTER_BOOTSTRAP=1` führt zuerst das vollständige Bootstrap-Skript aus, bevor der
Watch-Modus startet. Bei jedem Lauf stellt das Watch-Skript außerdem sicher, dass die
Entwicklungs-Nutzer/Rollen in der gewählten DB existieren, gibt eine Zugriffsübersicht
sowie einen eigenen Zugangsdatenblock (Benutzerliste + gemeinsames Passwort) zum schnellen
Kopieren aus.

## Playwright-E2E (UI- + API-Autorisierung)

Aus dem Frontend-Ordner:

```bash
cd Deeplex.Saverwalter.WebAPI/svelte
yarn test:e2e
```

Die Suite enthält Browser- und API-Tests und prüft u. a.:

- rollenbasierte Anmeldung aller geseedeten Testnutzer
- Admin-only-Zugriff auf `/api/accounts`
- Owner-only-Policy für `POST /api/wohnungen`
- unterschiedliche Wohnungs-Sichtbarkeit für Manager-/Viewer-/Limited-Rollen
- UI-Zugriff auf `/admin` für Admins und Fehlerdarstellung für Nicht-Admins
- UI-Zugriff auf dynamische Detail-/Anlege-Routen wie `/wohnungen/:id`, `/wohnungen/new`,
  `/accounts/:id` und `/accounts/new`
- Änderungsbeschränkungen auf Entitätsebene anhand der Backend-Berechtigungsflags
- Datei-Roundtrip (Upload/List/Download/Trash) an einer Wohnung gegen MinIO
- Sichtbarkeit geseedeter Beispieldateien, Viewer-Schreibschutz und den Ablagestapel-Endpunkt
- Erzeugung einer Betriebskostenabrechnungs-PDF über die API
- UI-Download-Fluss im Abrechnungslauf (PDF-Erzeugung, Override-Modal, S3-Persistenz)

Voraussetzungen vor dem Testlauf:

- Backend erreichbar unter `PLAYWRIGHT_API_BASE_URL` (Standard `http://localhost:5254`)
- Frontend erreichbar unter `PLAYWRIGHT_BASE_URL` (Standard `http://localhost:5173`)
- gemeinsames Testpasswort in `WALTER_E2E_PASSWORD` (Standard `postgres`)

## Skript für den vollständigen Stack-Test

Den gesamten Stack lokal im Devcontainer ausführen:

```bash
./scripts/test-full-stack.sh
```

Das Skript:

- wartet auf Postgres
- bootstrappt und seedet die Entwicklungsdatenbanken
- führt alle `.NET`-Tests aus
- führt die Frontend-Unit-Tests aus
- startet Backend und Frontend lokal
- fährt die Playwright-API- + -UI-Autorisierungssuite gegen den laufenden Stack

Die CI enthält einen Full-Stack-Test-Job, der dieses Skript in jedem Test-Workflow auf
Main- und Feature-Branches ausführt.

Ebenfalls verfügbar sind VS-Code-Tasks:

- `dev_watch_stack` startet Backend + Frontend im Watch-Modus direkt
- `dev_watch_stack_with_bootstrap` führt zuerst das Bootstrap aus, dann den Watch-Stack
