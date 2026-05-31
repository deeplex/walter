# Betriebskostenabrechnung

Dieser Artikel beschreibt, wie Walter eine Betriebskostenabrechnung für einen Mieter erstellt — von den Eingabedaten bis zum fertigen PDF- oder Word-Dokument.

Die fachliche Berechnung liegt im Projekt `Deeplex.Saverwalter.BetriebskostenabrechnungService`. Die Orchestrierung, das Laden der Daten und die Verbuchung des Ergebnisses übernehmen die Dienste unter `Deeplex.Saverwalter.WebAPI/Services/Abrechnung`.

> **Datengrundlage:** Beträge (Mietzahlungen, Vorauszahlungen, Betriebskostenrechnungen, Abrechnungsergebnis) sind **Buchungssätze** auf den Buchungskonten von Wohnung, Vertrag und Umlage – es gibt keine `Miete`- oder `Betriebskostenrechnung`-Entitäten mehr. Eine „Betriebskostenrechnung“ ist ein Buchungssatz auf dem `NkVerrechnungsKonto` der Umlage; ein „Abrechnungsresultat“ ist ein Buchungssatz auf dem `BkAbrechnungsKonto` des Vertrags.

---

## Grundbegriffe

| Begriff              | Bedeutung                                                                 |
|----------------------|---------------------------------------------------------------------------|
| **Umlage**           | Eine Betriebskostenposition (z. B. Heizung, Wasser) mit Verteilungsschlüssel (in der `UmlageVersion`) |
| **Abrechnungseinheit** | Gruppe von Wohnungen, die dieselben Umlagen teilen                      |
| **Umlageschlüssel**  | Formel zur Aufteilung einer Rechnung auf die einzelnen Wohnungen          |
| **Zeitanteil**       | Faktor für Mieter, die nicht das ganze Jahr in der Wohnung wohnten        |
| **Saldo**            | Positiv = Mieter bekommt zurück; Negativ = Mieter zahlt nach              |

---

## Voraussetzungen

1. **Wohnung** mit mindestens einer `WohnungVersion` (Wohnfläche, Nutzfläche, Nutzeinheiten, Miteigentumsanteile)
2. **Eigentümer** der Wohnung (`WohnungEigentuemer` → Kontakt mit Adresse) — Vermieter im Briefkopf
3. **Vertrag** mit mindestens einer **VertragVersion** (Grundmiete + Personenzahl)
4. **Mieter** mit Adresse (für den Briefkopf)
5. **Mietzahlungen** als Buchungen auf dem Miet-/NK-Konto des Vertrags
6. **Umlagen** der Wohnung mit Umlagetyp und Verteilungsschlüssel
7. **Betriebskostenrechnungen** des Jahres (Buchungssatz je Umlage)
8. Bei `NachVerbrauch`: **Zähler** mit **Zählerständen** zu Beginn und Ende des Jahres
9. Bei Heizkosten: **HKVO-Konfiguration** an der Umlage sowie ein Allgemein-Gaszähler

---

## Ablauf der Berechnung

### Schritt 1 — Zeitraum ermitteln

Die Abrechnung bezieht sich immer auf ein Kalenderjahr (01.01.–31.12.). Für Mieter, die nicht das ganze Jahr gewohnt haben, wird ein **Zeitanteil** berechnet (`Zeitraum`):

```
Nutzungsbeginn      = MAX(Vertragsbeginn, 01.01.)
Nutzungsende        = MIN(Vertragsende,   31.12.)
Nutzungszeitraum    = Nutzungsende − Nutzungsbeginn + 1  (in Tagen)
Abrechnungszeitraum = 365 (bzw. 366 in Schaltjahren)
Zeitanteil          = Nutzungszeitraum / Abrechnungszeitraum
```

Ein Mieter, der ab dem 01.07. eingezogen ist, hat einen Zeitanteil von ca. 0,503 (184/365).

### Schritt 2 — Kaltmiete berechnen

Die Kaltmiete wird monatsweise aus den **VertragVersionen** summiert; pro Monat gilt die jeweils gültige Grundmiete:

```
KaltMiete = Σ (Monat × zugehörige Grundmiete)
```

### Schritt 3 — Gezahlte Gesamtmiete ermitteln

Die im Abrechnungsjahr gezahlte Gesamtmiete ergibt sich aus den **Zahlungsbuchungen** des Vertrags (Haben-Zeilen auf dem Zahlungskonto, abgeglichen über `OffenerPostenAusgleich`):

```
GezahlteMiete = Σ Zahlungen des Jahres
```

### Schritt 4 — Abrechnungseinheiten bilden

Die Umlagen der Wohnung werden nach der **Gruppe der zugehörigen Wohnungen** zusammengefasst. Umlagen, die exakt dieselbe Menge an Wohnungen verbinden, bilden eine **Abrechnungseinheit** (`AbrechnungsGruppen`).

Je Abrechnungseinheit werden Gesamtwohnfläche, Gesamtnutzfläche, Gesamteinheiten und Gesamt-MEA bestimmt und die zeitanteiligen Flächenanteile der Wohnung berechnet:

```
WFZeitanteil  = (Wohnfläche  Wohnung / Gesamtwohnfläche)  × Zeitanteil
NFZeitanteil  = (Nutzfläche  Wohnung / Gesamtnutzfläche)  × Zeitanteil
NEZeitanteil  = (Nutzeinheiten Wohnung / Gesamteinheiten) × Zeitanteil
MEAZeitanteil = (MEA         Wohnung / Gesamt-MEA)        × Zeitanteil
```

### Schritt 5 — Kalte Nebenkosten berechnen

Für jede **kalte** Umlage (ohne HKVO) wird der als Buchungssatz erfasste Jahresbetrag geladen und nach Umlageschlüssel (aus der zum Stichtag gültigen `UmlageVersion`) auf die Wohnung verteilt:

| Schlüssel                | Anteil                                |
|--------------------------|---------------------------------------|
| `NachWohnflaeche`        | `Rechnungsbetrag × WFZeitanteil`      |
| `NachNutzflaeche`        | `Rechnungsbetrag × NFZeitanteil`      |
| `NachNutzeinheit`        | `Rechnungsbetrag × NEZeitanteil`      |
| `NachMiteigentumsanteil` | `Rechnungsbetrag × MEAZeitanteil`     |
| `NachPersonenzahl`       | `Rechnungsbetrag × PersonenZeitanteil`|
| `NachVerbrauch`          | `Rechnungsbetrag × Verbrauchsanteil`  |

**Verbrauchsanteil** (`Verbrauch` / `VerbrauchAnteil`): Verbrauch der Wohnung / Gesamtverbrauch aller Wohnungen. Der Verbrauch ergibt sich aus der Differenz der Zählerstände zu Beginn und Ende des Jahres (Ablesung max. 30 Tage vor/nach Stichtag).

**Personenzeitanteil** (`PersonenZeitanteil`): analog zum Flächenanteil, aber auf Basis der `Personenzahl` der VertragVersion, zeitgewichtet.

> **Leerstand / Eigenanteil:** Für Zeiträume, in denen keine Partei die Wohnung belegt, bildet der Service eine *Eigenanteil-Partei* (`NkPartei` mit `Vertrag == null`). Sie erhält dieselben Flächen-Zeitanteile, aber Personenzahl-Anteil 0. Ihr Anteil wird auf das `AufwandsKonto` der Wohnung gebucht. Rundungsdifferenzen werden bevorzugt der letzten Eigenanteil-Partei zugeschlagen.

### Schritt 6 — Warme Nebenkosten berechnen (HKVO)

Umlagen mit HKVO-Konfiguration (Heizung, Warmwasser) werden nach der **Heizkostenverordnung** aufgeteilt (`ProcessHkvoUmlage`). Der Rechnungsbetrag wird in eine Heiz- und eine Warmwasser-Fraktion zerlegt:

**Warmwasseranteil nach § 9 Abs. 2** (nur wenn ein Allgemein-Wärmezähler `HKVO.AllgemeinWaerme` existiert):
```
V  = Gesamtwarmwasserverbrauch aller Wohnungszähler [m³]
Q  = Wärmemenge laut Allgemein-Wärmezähler [kWh]
tw = 60 (Warmwassertemperatur in °C, Standardwert)

Para9_2 = 2,5 × (V / Q) × (tw − 10)     (0, falls Q oder V = 0)

betragHZ = Betrag × (1 − Para9_2)        → Heizfraktion
betragWW = Betrag × Para9_2              → Warmwasserfraktion
```

**Anteil je Partei** (`§7`/`§8` = `HKVO_P7`/`HKVO_P8`):
```
heizAnteil = §7 × Heizverbrauchsanteil + (1 − §7) × WFZeitanteil    (nur WFZeitanteil, falls keine Wärmezähler)
wwAnteil   = §8 × WWVerbrauchsanteil   + (1 − §8) × WFZeitanteil    (nur WFZeitanteil, falls keine WW-Zähler)

Betrag der Partei = betragHZ × heizAnteil + betragWW × wwAnteil
```

- `Heizverbrauchsanteil` = Wärmeverbrauch der Wohnung / Gesamtwärmeverbrauch (Zählertypen `Gas`, `Wärme`).
- `WWVerbrauchsanteil`   = Warmwasserverbrauch der Wohnung / Gesamtwarmwasserverbrauch.
- Der statische Rückfall erfolgt über die **Wohnfläche** (`WFZeitanteil`), nicht die Nutzfläche.

> Eine Strompauschale / ein Betriebsstrom-Abzug wird in der gruppenbasierten Berechnung **nicht** angewendet (anders als in der früheren Implementierung).

### Schritt 7 — Saldo je Partei

Pro Partei und Umlage entsteht ein `NkRechnungsAnteil`. Aufsummiert über alle Umlagen ergibt sich der Nebenkosten-Anteil der Wohnung; abzüglich der geleisteten NK-Vorauszahlungen (Haben auf `NkBuchungskonto`) und unter Berücksichtigung etwaiger Mietminderungen ergibt sich der **Saldo**.

- Saldo zugunsten des Mieters → Vermieter erstattet.
- Saldo zulasten des Mieters → Mieter zahlt nach.

Beim **Buchen** (`/book`) wird das Ergebnis als Buchungssatz auf dem `BkAbrechnungsKonto` des Vertrags festgehalten und als `Abrechnungsresultat` gespeichert.

---

## Validierungshinweise (Notes)

Während der Berechnung werden Hinweise (`Note`) mit Schweregrad gesammelt und in Dokument und API-Antwort ausgegeben. Typische Fälle:

| Schweregrad | Bedingung (Beispiele)                                                |
|-------------|---------------------------------------------------------------------|
| Error       | Kein Eigentümer/Ansprechpartner der Wohnung hinterlegt              |
| Error       | Ansprechpartner hat keine Adresse                                   |
| Error       | Keine Betriebskostenrechnung für eine Umlage gefunden               |
| Error       | Kein Allgemein-Gaszähler für Heizkosten definiert                   |
| Error       | § 9(2)-Berechnung ergibt Wert > 100 % oder < 0 %                    |
| Warning     | Kein Mieter hat eine hinterlegte Adresse                            |
| Warning     | Betriebsstrom-Pauschale übersteigt die Stromrechnung               |
| Warning     | Fehlende Zählerstände zu Jahresbeginn/-ende                        |

---

## API-Endpunkte

### Abrechnungslauf (Berechnung)

```
GET  /api/abrechnungslauf/gruppen     ← verfügbare Abrechnungsgruppen
POST /api/abrechnungslauf/preview     ← berechnet das Ergebnis ohne zu buchen
POST /api/abrechnungslauf/book        ← berechnet und bucht das Abrechnungsresultat
```
Body von `preview`/`book`: `{ Jahr, Gruppen: [ { WohnungIds: [...] } ] }`.

### Dokument erzeugen

```
POST /api/abrechnungslauf/print/pdf    ← PDF (einzeln) bzw. ZIP (mehrere)
POST /api/abrechnungslauf/print/docx   ← Word (einzeln) bzw. ZIP (mehrere)
```
Body: `{ WohnungIds: [...], Jahr, VertragId? }` — `VertragId` optional zum Druck eines einzelnen Vertrags.

### Gespeicherte Resultate

```
GET    /api/abrechnungsresultate/vertrag/{vertragId}/jahr/{jahr}
GET    /api/abrechnungsresultate/{id}
PUT    /api/abrechnungsresultate/{id}      ← Notiz / Abgesendet-Status aktualisieren
DELETE /api/abrechnungsresultate/{id}
```

### Betriebskostenrechnungen (als Buchungssatz)

```
GET    /api/betriebskostenrechnungen
POST   /api/betriebskostenrechnungen
GET    /api/betriebskostenrechnungen/{id}
PUT    /api/betriebskostenrechnungen/{id}
DELETE /api/betriebskostenrechnungen/{id}
```

---

## Beteiligte Quellcodedateien

| Datei | Beschreibung |
|-------|-------------|
| [BetriebskostenabrechnungService/NkGruppenAbrechnungsService.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/NkGruppenAbrechnungsService.cs) | Kern der Nebenkostenberechnung (kalt + warm/HKVO) |
| [BetriebskostenabrechnungService/AbrechnungsGruppen.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/AbrechnungsGruppen.cs) | Bildet Wohnungsgruppen (Abrechnungseinheiten) |
| [BetriebskostenabrechnungService/Zeitraum.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Zeitraum.cs) | Nutzungszeitraum und Zeitanteil |
| [BetriebskostenabrechnungService/Verbrauch.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Verbrauch.cs) · [VerbrauchAnteil.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/VerbrauchAnteil.cs) | Verbrauchsermittlung aus Zählerständen |
| [BetriebskostenabrechnungService/PersonenZeitanteil.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/PersonenZeitanteil.cs) | Zeitgewichteter Personenanteil |
| [BetriebskostenabrechnungService/Note.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Note.cs) | Validierungshinweise |
| [WebAPI/Services/Abrechnung/AbrechnungslaufService.cs](../Deeplex.Saverwalter.WebAPI/Services/Abrechnung/AbrechnungslaufService.cs) | Lädt Daten, ruft Berechnung auf, baut DTOs |
| [WebAPI/Services/Abrechnung/AbrechnungslaufPrintService.cs](../Deeplex.Saverwalter.WebAPI/Services/Abrechnung/AbrechnungslaufPrintService.cs) | Erzeugt Word-/PDF-Dokument |
| [WebAPI/Controllers/AbrechnungslaufController.cs](../Deeplex.Saverwalter.WebAPI/Controllers/AbrechnungslaufController.cs) · [AbrechnungslaufPrintController.cs](../Deeplex.Saverwalter.WebAPI/Controllers/AbrechnungslaufPrintController.cs) | HTTP-Endpunkte |
| [PrintService/](../Deeplex.Saverwalter.PrintService/) | Word-/PDF-Erzeugung (OOXML / MigraDoc) |
