# Tests

Walter wird auf vier Ebenen getestet. Alle laufen in CI (`.github/workflows/reusable_ci.yaml`).

| Ebene | Werkzeug | Ort | Ausführen |
|-------|----------|-----|-----------|
| Backend-Unit/Integration | xUnit | `Deeplex.Saverwalter.*.Tests` | `dotnet test Deeplex.Saverwalter.sln` |
| Frontend-Unit | Vitest | `svelte/src/**/*.test.ts` | `yarn vitest run` |
| End-to-End (UI + API) | Playwright | `svelte/tests/e2e/*.spec.ts` | `yarn test:e2e` |
| Voller Stack (lokal/CI) | Skript | `scripts/test-full-stack.sh` | seedet DB+S3, startet Backend, fährt die E2E-Suite |

## Backend-Tests

- `Deeplex.Saverwalter.Model.Tests` — Modell- und Kontext-Invarianten.
- `Deeplex.Saverwalter.PrintService.Tests` — Word-/PDF-Erzeugung.
- `Deeplex.Saverwalter.WebAPI.Tests` — Dienste und Endpunkte, u. a. die
  Nebenkostenberechnung in `NkGruppenAbrechnungsService.Tests` (Mieterwechsel,
  Zeitanteile, HKVO-Aufteilung, Ausschluss bereits ausgeglichener Buchungssätze).

## End-to-End-Suite

Die Playwright-Suite läuft gegen ein laufendes Backend (Standard `http://localhost:5254`,
überschreibbar via `PLAYWRIGHT_BASE_URL`). Sie nutzt die geseedeten Entwicklungs-Nutzer
(`svelte/tests/e2e/credentials.ts`); das Passwort kommt aus `WALTER_E2E_PASSWORD`
(Standard `postgres`, in diesem Devcontainer `admin`).

### Entwicklungs-Nutzer und Rollen

| Nutzer | Rolle | Sichtbarkeit / Rechte |
|--------|-------|------------------------|
| `admin.dev` | Admin | globaler Zugriff, darf alles lesen/ändern |
| `owner.dev` | Owner / Eigentümer | Eigentümer auf einem Teil der Wohnungen → ändern erlaubt |
| `manager.dev` | User / Vollmacht | Vollmacht auf ~½ der Wohnungen → ändern erlaubt |
| `viewer.dev` | User / Keine | nur Lesen auf ~¼ der Wohnungen |
| `limited.dev` | Guest / Keine | nur Lesen auf wenigen Wohnungen |

### Spec-Dateien

- `auth.ts`, `credentials.ts`, `api.ts`, `entities.ts`, `abrechnung.ts` — gemeinsame Helfer
  (kein Test). `api.ts#unwrapList` normalisiert sowohl `{ items, totalCount }`-Antworten
  (paginierte Endpunkte) als auch reine Arrays.
- `api-authz.spec.ts` — Authentifizierung + Wohnungs-Autorisierungsmatrix (Login, Accounts
  nur für Admin, Sichtbarkeit, Detail-403, PUT nur mit Vollmacht, Owner-only-Create).
- `entity-authz.spec.ts` — **datengetriebene Berechtigungsmatrix über alle Sammlungen**
  (siehe unten).
- `abrechnungslauf-authz.spec.ts` — Autorisierung des Abrechnungslaufs: `gruppen` liefert
  nur vollständig verwaltete Gruppen; `preview`/`print` verlangen Lese-Recht, `book`
  Vollmacht auf **allen** angeforderten Wohnungen (sonst 403; unbekannte IDs → 403, ohne
  Existenz-Leak).
- `ui-pages-matrix.spec.ts` — jeder Nutzer öffnet alle Hauptseiten; Admin-Seiten sind für
  Nicht-Admins gesperrt.
- `ui-detail-authz.spec.ts` — UI-Zugriff auf Wohnungs-/Account-Detail- und Anlegeseiten je Rolle.
- `ui-crud.spec.ts` — vollständiger Create → Edit → Delete einer Adresse über die echte UI
  sowie Berechtigungs-Gating der Detailseite (Felder schreibgeschützt ohne Update-Recht).
- `files.spec.ts` — S3-Dateifluss (Upload/List/Download/Trash, Ablagestapel) und
  Betriebskostenabrechnungs-Druck.
- `abrechnung-ui.spec.ts` — Abrechnungslauf-UI: Gruppe wählen → Vorschau → Download.

### Berechtigungsmatrix (`entity-authz.spec.ts`)

Eine zentrale Beschreibung in `entities.ts` (`entitySpecs`) erzeugt pro Sammlung die
passenden Invarianten-Tests. So bleibt „Berechtigungen überall" wartbar statt vielfach
kopiert.

| Sammlung | Sichtbarkeit | Detail-403 außerhalb Scope | Viewer schreibgeschützt |
|----------|--------------|----------------------------|--------------------------|
| wohnungen, vertraege, umlagen, umlagetypen, transaktionen, zaehler | scoped | ja | ja |
| adressen | scoped | nein¹ | ja |
| garagen | scoped | – (im Seed leer) | – |
| kontakte | global (alle sehen alles) | – | nur Verwalter dürfen schreiben² |
| accounts | nur Admin | – | – |

¹ Die Adressliste wird gefiltert, der Detail-Read ist aber bewusst permissiv.
² Kontakte sind ein gemeinsames Adressbuch: für alle lesbar, aber nur mit
Verwaltungsrechten (Admin oder Vollmacht) änderbar.

Geprüfte Invarianten pro scoped-Sammlung: kein Nutzer sieht mehr als den Admin-Scope;
Viewer/Limited sehen ihre Zeilen ohne `update`/`remove`; eine Vollmacht-Rolle kann
mindestens eine Zeile ändern; Detail-Reads außerhalb des Scopes liefern 403, im Scope 200.

## Autorisierungsmodell (Kurzfassung)

- **Abrechnungslauf** wirkt pro Wohnung: Lesen/Drucken verlangt Lese-Recht, Buchen
  Vollmacht, Rückabwicklung/Storno Lösch-Recht — jeweils auf **allen** angeforderten
  Wohnungen (`Utils.CanAccessAllWohnungen`).
- **Kontakte** bilden ein gemeinsames Adressbuch: lesbar für alle, änderbar nur mit
  Verwaltungsrechten (`Utils.HasAnyManagementAuthority`).
- **Beziehungslose Entitäten** (z. B. wohnungslose Zähler) sind lesbar wie üblich,
  aber nur mit Verwaltungsrechten (bzw. Admin) schreibbar.
