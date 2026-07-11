# Betriebskostenabrechnung

Dieser Artikel beschreibt, wie Walter eine Betriebskostenabrechnung für einen Mieter erstellt — von den Eingabedaten bis zum fertigen PDF- oder Word-Dokument.

Die fachliche Berechnung liegt im Projekt `Deeplex.Saverwalter.BetriebskostenabrechnungService`. Die Orchestrierung, das Laden der Daten und die Verbuchung des Ergebnisses übernehmen die Dienste unter `Deeplex.Saverwalter.WebAPI/Services/Abrechnung`.

> **Datengrundlage:** Alle Beträge (Mietzahlungen, Vorauszahlungen, Betriebskostenrechnungen, Abrechnungsergebnis) sind **Buchungssätze** auf den Buchungskonten von Wohnung, Vertrag und Umlage. Eine Betriebskostenrechnung ist ein Buchungssatz auf dem `NkVerrechnungsKonto` der Umlage; ein Abrechnungsresultat ist ein Buchungssatz auf dem `BkAbrechnungsKonto` des Vertrags.

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
9. Bei Heizkosten: **HKVO-Konfiguration** an der Umlage; für den § 9(2)-Warmwasseranteil zusätzlich mindestens ein **Allgemein-Wärmezähler** (Gas/Wärme)

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

### Schritt 2 — Miet-Soll und Miet-Saldo

Je Vertrag führt die Abrechnung zur Information den Miet-Soll und den Miet-Saldo aus den
gebuchten Zeilen des **`MietBuchungskonto`** im Abrechnungsjahr:

```
KaltmieteSoll = Σ Soll-Zeilen              (gestellte Kaltmiete)
MietSaldo     = Σ Soll − Σ Haben           (positiv = offene Miete)
```

Die monatlichen Sollstellungen werden aus der jeweils gültigen `VertragVersion`-Grundmiete
gebucht; die Abrechnung liest die **gebuchten** Beträge, nicht die Versionen selbst.

### Schritt 3 — NK-Vorauszahlungen ermitteln

Die im Abrechnungsjahr geleisteten Nebenkosten-Vorauszahlungen ergeben sich aus den
**Haben-Zeilen auf dem `NkBuchungskonto`** des Vertrags:

```
Vorauszahlung = Σ Haben-Zeilen des Jahres auf dem NkBuchungskonto
```

Dieser Wert wird in Schritt 7 den tatsächlichen Nebenkosten gegenübergestellt.

### Schritt 4 — Abrechnungseinheiten bilden

Die Umlagen der Wohnung werden nach der **Menge der zugehörigen Wohnungen** zusammengefasst: Umlagen, die exakt dieselbe Wohnungsmenge verbinden, bilden eine **Abrechnungseinheit** (`NkEinheit`, gebildet in `ComputeEinheiten` durch Gruppierung der Umlagen über ihre sortierte WohnungId-Menge).

> Nicht zu verwechseln mit der **Abrechnungsgruppe** (`AbrechnungsGruppen`, Union-Find), die alle transitiv über gemeinsame Umlagen verbundenen Wohnungen zu einer im Abrechnungslauf auswählbaren Gruppe zusammenfasst.

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

**Verbrauchsanteil** (`Verbrauch` / `VerbrauchAnteil`): Verbrauch der Wohnung / Gesamtverbrauch aller Wohnungen. Der Verbrauch ergibt sich aus der Differenz der Zählerstände zu Beginn und Ende des Jahres (Anfangsstand höchstens 14 Tage vor dem Stichtag; Endstand der letzte Stand bis zum Stichtag).

**Personenzeitanteil** (`PersonenZeitanteil`): analog zum Flächenanteil, aber auf Basis der `Personenzahl` der VertragVersion, zeitgewichtet.

> **Leerstand / Eigenanteil:** Für Zeiträume, in denen keine Partei die Wohnung belegt, bildet der Service eine *Eigenanteil-Partei* (`NkPartei` mit `Vertrag == null`). Sie erhält dieselben Flächen-Zeitanteile, aber Personenzahl-Anteil 0. Ihr Anteil wird auf das `AufwandsKonto` der Wohnung gebucht. Rundungsdifferenzen werden bevorzugt der letzten Eigenanteil-Partei zugeschlagen.

### Schritt 6 — Warme Nebenkosten berechnen (HKVO)

Umlagen mit HKVO-Konfiguration (Heizung, Warmwasser) werden nach der **Heizkostenverordnung** aufgeteilt (`ProcessHkvoUmlage`). Der Rechnungsbetrag wird in eine Heiz- und eine Warmwasser-Fraktion zerlegt:

**Warmwasseranteil nach § 9 Abs. 2** (nur wenn mindestens ein Allgemein-Wärmezähler in `HKVO.AllgemeinWaermeZaehler` existiert):
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
heizAnteil = §7 × Heizverbrauchsanteil + (1 − §7) × NFZeitanteil    (nur NFZeitanteil, falls keine Wärmezähler)
wwAnteil   = §8 × WWVerbrauchsanteil   + (1 − §8) × NFZeitanteil    (nur NFZeitanteil, falls keine WW-Zähler)

Betrag der Partei = betragHZ × heizAnteil + betragWW × wwAnteil
```

- `Heizverbrauchsanteil` = Wärmeverbrauch der Wohnung / Gesamtwärmeverbrauch (Zählertypen `Gas`, `Wärme`).
- `WWVerbrauchsanteil`   = Warmwasserverbrauch der Wohnung / Gesamtwarmwasserverbrauch.
- Der verbrauchsunabhängige Anteil (§ 7/§ 8) wird nach **Nutzfläche** (`NFZeitanteil`) verteilt.

#### Strompauschale (Betriebsstrom → Heizkosten)

Ist an der HKVO der Heizkosten-Umlage eine **Betriebsstrom-Umlage** hinterlegt und
`HKVO.Strompauschale > 0`, wird der auf den Heizungsbetrieb entfallende Anteil des
Allgemein-/Betriebsstroms **vor** der §7/§8-Aufteilung in die Heizkosten umgelegt
(`ApplyStrompauschale`). Voraussetzung: Heizkosten- und Betriebsstrom-Umlage gehören zur
**selben Abrechnungseinheit** (dieselben Wohnungen).

```
delta = round(Heizkosten-Gesamtbetrag × HKVO.Strompauschale, 2)
```

- Die **Heizkosten**-Pläne werden um `delta` hochskaliert; die §7/§8-Proportionen bleiben
  erhalten (es bleibt eine Heizkostenzeile aus BK + Pauschale).
- Die **Betriebsstrom**-Pläne werden anteilig um `delta` gekürzt (Betrag und Anteile),
  sodass der Abzug auch in Vorschau und Druck sichtbar ist.
- Buchhalterisch entsteht eine balancierte **Umbuchung**:
  `Soll Betriebsstrom-NkVerrechnungsKonto / Haben Heizkosten-NkVerrechnungsKonto` über `delta`.

Grenzfälle:

| Fall | Verhalten |
|------|-----------|
| `delta` übersteigt die Allgemeinstromrechnung | wird auf deren Betrag gekürzt (**Warnung**) |
| Betriebsstrom-Umlage hat keine Buchungen im Jahr | Pauschale wird nicht angewandt (**Warnung**) |
| Betriebsstrom-Umlage liegt in einer anderen Einheit | hier nicht behandelt |

### Schritt 7 — Saldo je Partei

Pro Partei und Umlage entsteht ein `NkRechnungsAnteil`. Aufsummiert über alle Umlagen ergibt sich der Nebenkosten-Anteil der Wohnung; abzüglich der geleisteten NK-Vorauszahlungen (Haben auf `NkBuchungskonto`) ergibt sich der **Saldo**.

> **Mietminderung:** Eine zeitanteilige Mietminderung
> (`Σ(Minderung × Tage) / Abrechnungstage`) wird erst im **Abrechnungsdokument** als
> Gutschrift auf die Nebenkosten angerechnet
> (`NebenkostenMietminderung = Nebenkosten × Minderung`, Position „Verrechnung mit
> Mietminderung"). Der `Saldo` der API-Vorschau enthält sie **nicht**.

- Saldo zugunsten des Mieters → Vermieter erstattet.
- Saldo zulasten des Mieters → Mieter zahlt nach.

> **Vorzeichenkonvention (zwei Perspektiven):** Derselbe Saldo wird an zwei Stellen mit
> entgegengesetztem Vorzeichen geführt.
> - **Intern/buchhalterisch — aus Vermietersicht:** Das Feld `Saldo` der API-Vorschau/
>   -Übersicht (`AbrechnungslaufEinheit`) ist `Nebenkosten − Vorauszahlung`. Positiv =
>   offene Forderung gegen den Mieter (Nachzahlung) — konsistent zur übrigen Buchführung,
>   in der eine Forderung (Soll) positiv ist.
> - **Abrechnungsdokument (PDF/Word) — aus Mietersicht:** `Vorauszahlung − Nebenkosten`.
>   Positiv = der Mieter bekommt zurück (so auch in den [Grundbegriffen](#grundbegriffe)).

Beim **Buchen** (`/book`) wird das Ergebnis als Buchungssatz auf dem `BkAbrechnungsKonto` des Vertrags festgehalten und als `Abrechnungsresultat` gespeichert.

---

## Validierungshinweise (Notes)

Während der Berechnung werden Hinweise (`Note`) mit Schweregrad **Error**, **Warning** oder
**Info** gesammelt und in Dokument und API-Antwort ausgegeben:

| Schweregrad | Bedingung |
|-------------|-----------|
| Error   | Kein gültiger Anfangs- oder Endstand eines Zählers im Abrechnungszeitraum |
| Error   | Erster gültiger Zählerstand liegt nach Nutzungsbeginn |
| Error   | Letzter gültiger Zählerstand liegt zu weit vor Nutzungsende |
| Error   | Zähler-Enddatum ist kleiner/gleich dem Beginn der Zählung |
| Error   | Mehrdeutiger Ersatzzähler bei einem Zählerwechsel |
| Error   | § 9(2)-Warmwasseranteil über 100 % (wird auf 100 % begrenzt) |
| Warning | Keine Buchungen (Betriebskostenrechnung) für eine Umlage im Jahr |
| Warning | Allgemein-Wärmezähler ohne Verbrauch bzw. kein Warmwasser-Verbrauch → gesamter Betrag als Heizung (§ 7) |
| Warning | Summe der Wohnungs-Wärmezähler übersteigt den Allgemeinzähler (Q) |
| Warning | Messfenster eines Zählers weicht vom Abrechnungszeitraum ab |
| Warning | Strompauschale nicht anwendbar (kein Betriebsstrom-Umsatz) oder übersteigt die Allgemeinstromrechnung (auf diese gekürzt) |
| Warning | Beim Buchen: Vertrag bereits versendet (übersprungen), Abrechnungsverzicht hinterlegt (nicht gebucht), abweichende Strompauschale-Umbuchung (erst stornieren) oder ein Buchungsfehler eines Vertrags |
| Info    | Zählerwechsel innerhalb des Zeitraums erkannt |

> Beim **Buchen** führen harte Fehler (z. B. eine leere Abrechnungsgruppe) zum Abbruch
> (`InvalidOperationException` → HTTP 409/400), nicht zu einem Hinweis. Fehlt die Adresse
> von Eigentümer oder Mieter, bricht die Erzeugung nicht ab — im Briefkopf erscheint dann
> „Unbekannt".

---

## API-Endpunkte

### Abrechnungslauf (Berechnung)

```
GET  /api/abrechnungslauf/gruppen         ← Abrechnungsgruppen, die der Nutzer vollständig lesen darf
POST /api/abrechnungslauf/preview         ← berechnet das Ergebnis ohne zu buchen (Lese-Recht)
POST /api/abrechnungslauf/book            ← berechnet und bucht das Abrechnungsresultat (Vollmacht)
GET  /api/abrechnungslauf/kontrolle/{jahr} ← prüft, ob eine Neuberechnung das Gebuchte bestätigt (lesend)
POST /api/abrechnungslauf/rueckabwicklung ← nimmt die gebuchte, noch nicht abgesendete Abrechnung zurück
POST /api/abrechnungslauf/storno          ← storniert eine abgesendete Abrechnung (Grund Pflicht)
```
Body von `preview`/`book`: `{ Jahr, Gruppen: [ { WohnungIds: [...] } ] }`.
Body von `rueckabwicklung`/`storno`: `{ WohnungIds: [...], Jahr, Grund? }` — `Grund` ist beim Storno Pflicht.

Autorisierung erfolgt pro Wohnung über `CanAccessAllWohnungen`: `preview`/`print` verlangen
Lese-Recht auf **allen** angeforderten Wohnungen, `book` verlangt Vollmacht (Update),
`rueckabwicklung`/`storno` verlangen Lösch-Recht.

### Dokument erzeugen

```
POST /api/abrechnungslauf/print/pdf    ← PDF (einzeln) bzw. ZIP (mehrere)
POST /api/abrechnungslauf/print/docx   ← Word (einzeln) bzw. ZIP (mehrere)
```
Body: `{ WohnungIds: [...], Jahr, VertragId? }` — `VertragId` optional zum Druck eines einzelnen Vertrags.

### Gespeicherte Resultate & Verzicht

```
GET    /api/abrechnungsresultate/vertrag/{vertragId}/jahr/{jahr}
GET    /api/abrechnungsresultate/{id}
PUT    /api/abrechnungsresultate/{id}          ← Notiz / Abgesendet-Status aktualisieren
DELETE /api/abrechnungsresultate/{id}

POST   /api/abrechnungsverzicht                ← Verzicht dokumentieren (Grund Pflicht)
DELETE /api/abrechnungsverzicht/{vertragId}/{jahr}
```

### Betriebskostenrechnungen (als Buchungssatz)

```
GET    /api/betriebskostenrechnungen
POST   /api/betriebskostenrechnungen
GET    /api/betriebskostenrechnungen/{id}
PUT    /api/betriebskostenrechnungen/{id}
DELETE /api/betriebskostenrechnungen/{id}
```

### Jahresabschlusskontrolle

```
GET    /api/jahresabschluss           ← Status aller Buchungsjahre (offene Konten, offene Abrechnungen)
GET    /api/jahresabschluss/{jahr}    ← Bilanzansicht eines Jahres je Konto + Abrechnungsstatus je Vertrag
```

---

## Beteiligte Quellcodedateien

| Datei | Beschreibung |
|-------|-------------|
| [BetriebskostenabrechnungService/NkGruppenAbrechnungsService.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/NkGruppenAbrechnungsService.cs) | Kern der Nebenkostenberechnung (kalt + warm/HKVO) |
| [BetriebskostenabrechnungService/AbrechnungsGruppen.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/AbrechnungsGruppen.cs) | Bildet die auswählbaren Abrechnungsgruppen (Union-Find über gemeinsame Umlagen) |
| [BetriebskostenabrechnungService/Zeitraum.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Zeitraum.cs) | Nutzungszeitraum und Zeitanteil |
| [BetriebskostenabrechnungService/Verbrauch.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Verbrauch.cs) · [VerbrauchAnteil.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/VerbrauchAnteil.cs) | Verbrauchsermittlung aus Zählerständen |
| [BetriebskostenabrechnungService/PersonenZeitanteil.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/PersonenZeitanteil.cs) | Zeitgewichteter Personenanteil |
| [BetriebskostenabrechnungService/Note.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Note.cs) | Validierungshinweise |
| [WebAPI/Services/Abrechnung/AbrechnungslaufService.cs](../Deeplex.Saverwalter.WebAPI/Services/Abrechnung/AbrechnungslaufService.cs) | Lädt Daten, ruft Berechnung auf, baut DTOs |
| [WebAPI/Services/Abrechnung/AbrechnungslaufPrintService.cs](../Deeplex.Saverwalter.WebAPI/Services/Abrechnung/AbrechnungslaufPrintService.cs) | Erzeugt Word-/PDF-Dokument |
| [WebAPI/Controllers/AbrechnungslaufController.cs](../Deeplex.Saverwalter.WebAPI/Controllers/AbrechnungslaufController.cs) · [AbrechnungslaufPrintController.cs](../Deeplex.Saverwalter.WebAPI/Controllers/AbrechnungslaufPrintController.cs) | HTTP-Endpunkte |
| [PrintService/](../Deeplex.Saverwalter.PrintService/) | Word-/PDF-Erzeugung (OOXML / MigraDoc) |
