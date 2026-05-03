# Betriebskostenabrechnung

Dieser Artikel beschreibt den vollständigen Prozess, wie Walter eine Betriebskostenabrechnung für einen Mieter erstellt — von der Dateneingabe bis zum fertigen PDF- oder Word-Dokument.

---

## Grundbegriffe

| Begriff              | Bedeutung                                                                 |
|----------------------|---------------------------------------------------------------------------|
| **Umlage**           | Eine Betriebskostenposition (z.B. Heizung, Wasser) mit Verteilungsschlüssel |
| **Abrechnungseinheit** | Gruppe von Wohnungen, die dieselben Umlagen teilen                      |
| **Umlageschlüssel**  | Formel zur Aufteilung einer Rechnung auf die einzelnen Wohnungen          |
| **Zeitanteil**       | Faktor für Mieter, die nicht das ganze Jahr in der Wohnung wohnten        |
| **Saldo**            | Positiv = Mieter zahlt nach; Negativ = Vermieter erstattet               |

---

## Voraussetzungen

Damit eine Abrechnung korrekt berechnet werden kann, müssen folgende Daten im System vorliegen:

1. **Wohnung** mit Wohnfläche, Nutzfläche, Nutzeinheiten und Miteigentumsanteilen
2. **Wohnung hat einen Besitzer** (Kontakt mit Adresse) — wird als Vermieter im Briefkopf verwendet
3. **Vertrag** mit mindestens einer **VertragVersion** (Grundmiete + Personenzahl)
4. **Mieter** mit Adresse (für den Briefkopf des Dokuments)
5. **Mietzahlungen** (`Miete`-Einträge) für das Abrechnungsjahr
6. **Umlagen** der Wohnung mit passendem Umlagetyp und Verteilungsschlüssel
7. **Betriebskostenrechnungen** für das Abrechnungsjahr, je eine pro Umlage
8. Bei verbrauchsabhängigen Umlagen: **Zähler** mit **Zählerständen** zu Beginn und Ende des Jahres
9. Bei Heizkosten: **HKVO-Konfiguration** an der Umlage sowie ein Allgemein-Gaszähler

---

## Ablauf der Berechnung

### Schritt 1 — Zeitraum ermitteln

Die Abrechnung bezieht sich immer auf ein Kalenderjahr (01.01.–31.12.). Für Mieter, die nicht das ganze Jahr in der Wohnung gewohnt haben, wird ein **Zeitanteil** berechnet:

```
Nutzungsbeginn  = MAX(Vertragsbeginn, 01.01.)
Nutzungsende    = MIN(Vertragsende,   31.12.)
Nutzungszeitraum  = Nutzungsende − Nutzungsbeginn + 1  (in Tagen)
Abrechnungszeitraum = 365 (bzw. 366 in Schaltjahren)
Zeitanteil = Nutzungszeitraum / Abrechnungszeitraum
```

Ein Mieter, der ab dem 01.07. eingezogen ist, hat einen Zeitanteil von ca. 0,503 (184/365).

### Schritt 2 — Kaltmiete berechnen

Die Kaltmiete wird monatsweise aus den **VertragVersionen** summiert. Wenn sich die Grundmiete im Laufe des Jahres geändert hat (neue VertragVersion), wird für jeden Monat die jeweils gültige Version herangezogen:

```
KaltMiete = Summe aller Monate × zugehörige Grundmiete
```

### Schritt 3 — Gezahlte Gesamtmiete ermitteln

Alle `Miete`-Einträge, deren `BetreffenderMonat` im Abrechnungsjahr liegt, werden summiert:

```
GezahlteMiete = Σ Miete.Betrag  (für alle Mieten des Jahres)
```

### Schritt 4 — Abrechnungseinheiten bilden

Die Umlagen der Wohnung werden nach der **Gruppe der zugehörigen Wohnungen** zusammengefasst. Umlagen, die exakt dieselbe Menge an Wohnungen verbinden, bilden eine **Abrechnungseinheit**.

Für jede Abrechnungseinheit werden berechnet:
- Gesamtwohnfläche, Gesamtnutzfläche, Gesamteinheiten, Gesamtmiteigentumsanteile
- Flächenanteile der konkreten Wohnung (mit Zeitfaktor multipliziert):

```
WFZeitanteil  = (Wohnfläche Wohnung / Gesamtwohnfläche) × Zeitanteil
NFZeitanteil  = (Nutzfläche Wohnung / Gesamtnutzfläche) × Zeitanteil
NEZeitanteil  = (Nutzeinheiten Wohnung / Gesamteinheiten) × Zeitanteil
MEAZeitanteil = (MEA Wohnung / Gesamt-MEA) × Zeitanteil
```

### Schritt 5 — Kalte Nebenkosten berechnen

Für jede **kalte** Umlage (ohne HKVO) wird die zugehörige Betriebskostenrechnung des Jahres geladen und der Anteil der Wohnung nach Umlageschlüssel ermittelt:

| Schlüssel           | Anteil                                     |
|---------------------|--------------------------------------------|
| `NachWohnflaeche`   | `Rechnungsbetrag × WFZeitanteil`           |
| `NachNutzflaeche`   | `Rechnungsbetrag × NFZeitanteil`           |
| `NachNutzeinheit`   | `Rechnungsbetrag × NEZeitanteil`           |
| `NachMiteigentumsanteil` | `Rechnungsbetrag × MEAZeitanteil`     |
| `NachPersonenzahl`  | `Rechnungsbetrag × PersonenZeitanteil`     |
| `NachVerbrauch`     | `Rechnungsbetrag × Verbrauchsanteil`       |

**Verbrauchsanteil** (für `NachVerbrauch`):
```
Verbrauchsanteil = Verbrauch der Wohnung / Gesamtverbrauch aller Wohnungen
```
Der Verbrauch ergibt sich aus der Differenz der Zählerstände zu Beginn und Ende des Abrechnungsjahres (Toleranz: Ablesung max. 30 Tage vor/nach Stichtag).

**Personenzeitanteil** (für `NachPersonenzahl`):
Analog zum Flächenanteil, aber basierend auf der in der VertragVersion hinterlegten `Personenzahl` — zeitgewichtet, falls sich die Personenzahl im Jahresverlauf geändert hat.

**Sonderfall Betriebsstrom bei HKVO:**
Ist eine Umlage mit einer HKVO verknüpft (z.B. Allgemeinstrom für die Heizungsanlage), wird ein Anteil des Betriebsstroms von der Allgemeinstromrechnung abgezogen, bevor der Restbetrag auf die Wohnungen umgelegt wird:

```
BereinigterBetrag = Stromrechnung.Betrag − Σ (Heizkosten.Betrag × HKVO.Strompauschale)
```

### Schritt 6 — Warme Nebenkosten berechnen (HKVO)

Umlagen mit einer HKVO-Konfiguration (Heizung, Warmwasser) werden nach der **Heizkostenverordnung** aufgeteilt. Die Berechnung erfolgt in der Klasse `Heizkostenberechnung`:

**Gesamtbetrag inkl. Betriebsstromanteil:**
```
PauschalBetrag = GesamtBetrag + GesamtBetrag × HKVO.Strompauschale
```

**Aufteilung Heizwärme und Warmwasser nach §9 Abs. 2:**
```
V  = Gesamtwarmwasserverbrauch aller Wohnungen [m³]
Q  = Gesamtwärmemenge laut Allgemein-Gaszähler [kWh]
tw = 60 (Warmwassertemperatur in °C, Standardwert)

Para9_2 = 2,5 × (V / Q) × (tw − 10)   → Warmwasseranteil am Gesamtbetrag
```

**Anteil der einzelnen Wohnung:**
```
WaermeAnteilNF    = PauschalBetrag × (1 − Para9_2) × (1 − §7) × NFZeitanteil
WaermeAnteilVerb  = PauschalBetrag × (1 − Para9_2) ×      §7  × Heizkostenverbrauchsanteil
WarmwasserAnteilNF   = PauschalBetrag × Para9_2 × (1 − §8) × NFZeitanteil
WarmwasserAnteilVerb = PauschalBetrag × Para9_2 ×      §8  × Warmwasserverbrauchsanteil

Betrag = WaermeAnteilNF + WaermeAnteilVerb + WarmwasserAnteilNF + WarmwasserAnteilVerb
```

- `§7` und `§8` sind die in der HKVO-Konfiguration hinterlegten Verbrauchsanteile (z.B. 0.5 = 50 %).
- `Heizkostenverbrauchsanteil` = Gasverbrauch der Wohnung / Gesamtgasverbrauch aller Wohnungen
- `Warmwasserverbrauchsanteil` = Warmwasserverbrauch der Wohnung / Gesamtwarmwasserverbrauch

### Schritt 7 — Nebenkosten summieren

```
BetragNebenkosten = Σ (BetragKalt + BetragWarm) über alle Abrechnungseinheiten
```

### Schritt 8 — Mietminderung berechnen

Falls Mietminderungen im Abrechnungsjahr vorlagen, wird eine gewichtete Minderungsquote für das gesamte Jahr berechnet:

```
Mietminderung = Σ (Minderung.Minderung × Minderungstage) / Jahrestage
```

Beispiel: 10 % Minderung für 73 Tage = `0.10 × 73 / 365 = 0.02` (2 % auf das Jahr gerechnet).

Daraus ergeben sich:
```
NebenkostenMietminderung = BetragNebenkosten × Mietminderung
KaltMietminderung        = KaltMiete × Mietminderung
```

### Schritt 9 — Saldo berechnen

```
BezahltNebenkosten = GezahlteMiete − KaltMiete + KaltMietminderung
Result = BezahltNebenkosten − BetragNebenkosten + NebenkostenMietminderung
```

**Interpretation des Saldos (`Result`):**
- `Result > 0` → Der Mieter hat zu viel gezahlt (Vorauszahlung > tatsächliche Nebenkosten) → Vermieter erstattet
- `Result < 0` → Der Mieter hat zu wenig gezahlt → Mieter zahlt nach

---

## Validierungshinweise (Notes)

Während der Berechnung werden Hinweise gesammelt, die im Dokument und in der API-Antwort ausgegeben werden:

| Schweregrad | Bedingung                                                                  |
|-------------|----------------------------------------------------------------------------|
| Error       | Kein Besitzer/Ansprechpartner der Wohnung hinterlegt                       |
| Error       | Ansprechpartner hat keine Adresse                                          |
| Error       | Keine Betriebskostenrechnung für eine Umlage gefunden                      |
| Error       | Kein Allgemein-Gaszähler für Heizkosten definiert                          |
| Error       | §9(2)-Berechnung ergibt Wert > 100 % oder < 0 %                           |
| Error       | Gesamtzähler zählt weniger als die Summe der Einzelzähler                  |
| Warning     | Kein Mieter hat eine hinterlegte Adresse                                   |
| Warning     | Betriebsstrom-Pauschale übersteigt die Allgemeinstromrechnung               |
| Warning     | Keine Rechnung für einen bestimmten Umlagetyp gefunden                     |

---

## API-Endpunkte

### Abrechnung abrufen / generieren

```
GET  /api/betriebskostenabrechnung/{vertragId}/{jahr}
```
Gibt die berechneten Daten als JSON zurück, ohne etwas zu speichern.

```
GET  /api/betriebskostenabrechnung/{vertragId}/{jahr}/pdf_document
GET  /api/betriebskostenabrechnung/{vertragId}/{jahr}/word_document
```
Gibt das fertige Dokument als Datei zurück, ohne ein Resultat zu speichern.

```
POST /api/betriebskostenabrechnung/{vertragId}/{jahr}/pdf_document
POST /api/betriebskostenabrechnung/{vertragId}/{jahr}/word_document
```
Gibt das Dokument zurück **und** speichert das Abrechnungsresultat in der Datenbank.

### Gespeicherte Resultate verwalten

```
GET    /api/abrechnungsresultate
GET    /api/abrechnungsresultate/{id}
GET    /api/abrechnungsresultate/vertrag/{vertragId}/jahr/{jahr}
PUT    /api/abrechnungsresultate/{id}      ← Notiz / Abgesendet-Status aktualisieren
DELETE /api/abrechnungsresultate/{id}
```

### Betriebskostenrechnungen (CRUD)

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
| [BetriebskostenabrechnungService/Betriebskostenabrechnung.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Betriebskostenabrechnung.cs) | Hauptklasse: orchestriert alle Berechnungsschritte |
| [BetriebskostenabrechnungService/Abrechnungseinheit.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Abrechnungseinheit.cs) | Bildet Wohnungsgruppen und berechnet kalte Nebenkosten |
| [BetriebskostenabrechnungService/Heizkostenberechnung.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Heizkostenberechnung.cs) | HKVO-Berechnung für warme Nebenkosten |
| [BetriebskostenabrechnungService/Zeitraum.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Zeitraum.cs) | Berechnet Nutzungszeitraum und Zeitanteil |
| [BetriebskostenabrechnungService/Utils.cs](../Deeplex.Saverwalter.BetriebskostenabrechnungService/Utils.cs) | Hilfsmethoden: Mietzahlungen, Kaltmiete, Mietminderung |
| [WebAPI/Controllers/Utils/BetriebskostenabrechnungController.cs](../Deeplex.Saverwalter.WebAPI/Controllers/Utils/BetriebskostenabrechnungController.cs) | HTTP-Endpunkte für Abrechnung und Dokumenterzeugung |
| [WebAPI/Services/Betriebskostenabrechnung.cs](../Deeplex.Saverwalter.WebAPI/Services/Betriebskostenabrechnung.cs) | Handler: lädt Vertrag, ruft Service auf, speichert Resultat |
| [PrintService/](../Deeplex.Saverwalter.PrintService/) | Erzeugt Word- und PDF-Dokumente aus den Berechnungsdaten |
