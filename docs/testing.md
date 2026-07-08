# Tests

Walter wird auf vier Ebenen getestet. Alle laufen in CI (`.github/workflows/reusable_ci.yaml`).

| Ebene | Werkzeug | Ort | Ausführen |
|-------|----------|-----|-----------|
| Backend-Unit/Integration | xUnit | `Deeplex.Saverwalter.*.Tests` | `dotnet test Deeplex.Saverwalter.sln` |
| Frontend-Unit | Vitest | `svelte/src/**/*.test.ts` | `yarn vitest run` |
| End-to-End (UI + API) | Playwright | `svelte/tests/e2e/*.spec.ts` | `yarn test:e2e` |
| Voller Stack (lokal/CI) | Skript | `scripts/test-full-stack.sh` | seedet DB+S3, startet Backend, fährt die E2E-Suite |

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
- `ui-pages-matrix.spec.ts` — jeder Nutzer öffnet alle Hauptseiten; Admin-Seiten sind für
  Nicht-Admins gesperrt.
- `ui-detail-authz.spec.ts` — UI-Zugriff auf Wohnungs-/Account-Detail- und Anlegeseiten je Rolle.
- `ui-crud.spec.ts` — vollständiger Create → Edit → Delete einer Adresse über die echte UI
  sowie Berechtigungs-Gating der Detailseite (Felder schreibgeschützt ohne Update-Recht).
- `files.spec.ts` — S3-Dateifluss (Upload/List/Download/Trash, Stack) und
  Betriebskostenabrechnungs-Druck.
- `abrechnung-ui.spec.ts` — Abrechnungslauf-UI: Gruppe wählen → Vorschau → Download.

### Berechtigungsmatrix (`entity-authz.spec.ts`)

Eine zentrale Beschreibung in `entities.ts` (`entitySpecs`) erzeugt pro Sammlung die
passenden Invarianten-Tests. So ist „Berechtigungen überall" wartbar statt 10× kopiert.

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
Viewer/Limited sehen ihre Zeilen ohne `update`/`remove`; eine Vollmacht-Rolle kann mindestens
eine Zeile ändern; Detail-Reads außerhalb des Scopes liefern 403, im Scope 200.

## Autorisierungs-Korrekturen (Sicherheitslücken)

`abrechnungslauf-authz.spec.ts` und die Kontakt-/Zähler-Regeln oben sind Regressionstests für
folgende behobene Lücken (alle vorgefunden):

1. **Abrechnungslauf-IDOR:** `/api/abrechnungslauf/{gruppen,preview,print/*,book}` wirkten auf
   beliebige `wohnungIds` ohne Pro-Wohnung-Autorisierung — jeder eingeloggte Nutzer konnte
   fremde Betriebskostenabrechnungen lesen, drucken und sogar **buchen**. Jetzt: `gruppen`
   liefert nur vollständig verwaltete Gruppen, `preview`/`print` verlangen Lese-Recht und
   `book` Vollmacht auf **allen** angeforderten Wohnungen (sonst 403; unbekannte IDs → 403,
   kein Existenz-Leak). Umgesetzt über `Utils.CanAccessAllWohnungen`.
2. **Kontakte:** jeder (auch Gast) konnte beliebige Kontakte ändern/löschen. Jetzt nur mit
   Verwaltungsrechten (`Utils.HasAnyManagementAuthority`); der Detail-`permissions`-Flag wird
   konsistent berechnet statt hartkodiert.
3. **Garagen / wohnungslose Zähler / beziehungslose Entitäten:** „immer erlaubt"-Handler bzw.
   leere Wohnungs-Collection erlaubten jedem Schreibzugriff. Jetzt: lesbar wie zuvor, Schreiben
   nur mit Verwaltungsrechten (bzw. Admin bei beziehungslosen Entitäten).

## Bekannte, offene Punkte (vorgefunden, nicht durch die Test-Arbeit verursacht)

1. **Backend:** Zwei Tests in `NkGruppenAbrechnungsService.Tests` schlagen fehl
   (`VollstaendigVerteilterBS_WirdNichtNochmalVerteilt`,
   `NkAnteilBuchungssatz_WirdNichtAlsBkRechnungVerteilt`). Test und Service wurden im selben
   Commit (`feat: update abrechnung to fit updated model`) geändert — die Tests beschreiben
   das **gewünschte** neue Verhalten (ein vollständig verteilter Buchungssatz erzeugt keinen
   weiteren Rechnungsplan), das `BuildRechnungsplaene` noch nicht umsetzt. Fachliche
   Entscheidung des Autors.
2. **Formatierung:** `yarn prettier --check .` meldet ~24 bereits vorhandene Verstöße in
   nicht von den Tests berührten Dateien (`yarn prettier --write .` behebt sie).
