# Leitfaden für Buchhalter

Dieser Leitfaden richtet sich an Buchhalter und Verwalter, die mit Walter die jährliche Betriebskostenabrechnung (Nebenkostenabrechnung) erstellen. Er erklärt, welche Daten wann gepflegt werden müssen und wie der Abschluss eines Abrechnungsjahres abläuft.

---

## Jahresüberblick

```
Januar – Dezember       Laufende Datenpflege
                         └─ Mietzahlungen erfassen
                         └─ Zählerstände ablesen und eintragen
                         └─ Eingehende Rechnungen zuordnen

Nach Jahresabschluss    Abrechnung erstellen
(typisch: Jan – März)    └─ Jahresabschluss-Zählerstände prüfen
                         └─ Betriebskostenrechnungen einpflegen
                         └─ Abrechnung berechnen und prüfen
                         └─ Dokument erzeugen und versenden
                         └─ Resultat als „abgesendet" markieren
```

---

## Stammdaten (einmalig, bei Änderung aktualisieren)

Diese Daten werden einmalig angelegt und nur bei Änderungen angepasst.

### Wohnung

Für jede Mieteinheit muss hinterlegt sein:

| Datum | Was |
|-------|-----|
| Einmalig | Wohnfläche (m²), Nutzfläche (m²), Nutzeinheiten, Miteigentumsanteile |
| Einmalig | Adresse der Wohnung |
| Einmalig | Besitzer (Kontakt mit vollständiger Postanschrift) |

> Der Besitzer wird als Vermieter im Briefkopf der Abrechnung verwendet. Ohne hinterlegte Adresse des Besitzers kann kein Dokument erzeugt werden.

### Umlagen

Für jede Betriebskostenart (z.B. Heizung, Wasser, Allgemeinstrom) muss eine **Umlage** angelegt sein:

| Feld | Beschreibung |
|------|-------------|
| Umlagetyp | Art der Kosten, z.B. „Heizkosten", „Wasser/Abwasser" |
| Umlageschlüssel | Wie werden die Kosten verteilt? (siehe unten) |
| Zugeordnete Wohnungen | Alle Wohnungen, die diese Kosten teilen |

**Umlageschlüssel im Überblick:**

| Schlüssel | Wann verwenden |
|-----------|---------------|
| Nach Wohnfläche | Standard für die meisten Positionen (Grundsteuer, Versicherung …) |
| Nach Nutzfläche | Wenn Gemeinschaftsflächen einbezogen werden |
| Nach Nutzeinheit | Gleichmäßige Verteilung unabhängig von der Größe |
| Nach Personenzahl | Wasserabhängige Kosten ohne Zähler |
| Nach Verbrauch | Strom, Wasser, Gas — erfordert Zähler |
| Nach MEA | Miteigentumsanteile (WEG-Abrechnung) |

### Zähler

Für verbrauchsabhängige Umlagen (Schlüssel „Nach Verbrauch") muss je Wohnung ein **Zähler** eingetragen sein:

| Feld | Beschreibung |
|------|-------------|
| Kennnummer | Zählernummer laut Plakette |
| Typ | Warmwasser, Kaltwasser, Strom oder Gas |
| Wohnung | Welcher Wohneinheit gehört der Zähler? (leer = Allgemeinzähler) |

> Für die Heizkostenabrechnung nach HKVO wird zusätzlich ein **Allgemein-Gaszähler** benötigt, der keiner Wohnung zugeordnet ist.

### Mietvertrag

Je Mieter muss vorhanden sein:

- **Vertrag** mit zugehöriger Wohnung
- Mindestens eine **VertragVersion** mit Grundmiete (Kaltmiete) und Personenzahl, gültig ab Vertragsbeginn
- **Mieter** (Kontakt) mit hinterlegter Postanschrift (für Briefkopf)

Bei Mieterhöhung oder Personenänderung: Neue VertragVersion mit neuem `Beginn`-Datum anlegen — die alte Version bleibt erhalten.

---

## Laufende Datenpflege

### Mietzahlungen erfassen

Jeden Monat, wenn der Mieter zahlt:

| Feld | Was eintragen |
|------|--------------|
| Zahlungsdatum | Datum des Zahlungseingangs |
| Betreffender Monat | Der Monat, für den gezahlt wird (z.B. 01.03.) |
| Betrag | Den tatsächlich eingegangenen Betrag (Warmmiete) |

> Der Mieter zahlt üblicherweise Kaltmiete + Nebenkostenvorauszahlung in einem Betrag. Dieser **Gesamtbetrag** wird hier eingetragen. Die Abrechnung trennt Kaltmiete und Nebenkosten automatisch anhand der hinterlegten Grundmiete.

### Zählerstände eintragen

Zählerstände sollten mindestens zu folgenden Terminen erfasst werden:

| Zeitpunkt | Beschreibung |
|-----------|-------------|
| 01.01. (bzw. 31.12.) | Jahresanfang/-ende für die Abrechnung |
| Mieterwechsel | Ablesung bei Ein- und Auszug |
| Auf Anforderung | Zwischenablesung bei Bedarf |

> Die Abrechnung sucht automatisch den letzten Zählerstand, der **nicht mehr als 30 Tage** vor dem Stichtag liegt. Ein Zählerstand vom 28.12. gilt also noch als Jahresabschluss.

---

## Abrechnung erstellen (Jahresabschluss)

### Schritt 1 — Betriebskostenrechnungen einpflegen

Für jede Umlage und jedes Abrechnungsjahr wird genau **eine Betriebskostenrechnung** erwartet. Diese entspricht der vom Versorger/Dienstleister ausgestellten Jahresrechnung.

| Feld | Was eintragen |
|------|--------------|
| Umlage | Welche Kostenposition (z.B. Heizkosten) |
| Betrag | Gesamtbetrag der Rechnung in Euro |
| Datum | Rechnungsdatum |
| Betreffendes Jahr | Das Abrechnungsjahr (z.B. 2024) |

> Wichtig: Für jede Umlage muss für das Abrechnungsjahr genau eine Rechnung vorliegen, sonst erscheint eine Warnmeldung in der Abrechnung.

### Schritt 2 — Abrechnung berechnen lassen

Die Abrechnung wird über die API (oder die Benutzeroberfläche) für einen bestimmten **Vertrag** und ein **Jahr** abgerufen. Walter berechnet automatisch:

| Posten | Berechnung |
|--------|-----------|
| **Kaltmiete** | Grundmiete × Anzahl der Monate (aus VertragVersionen) |
| **Nebenkosten** | Anteil der Wohnung an den Betriebskostenrechnungen |
| **Bezahlte Nebenkosten** | Gesamtzahlungen − Kaltmiete |
| **Mietminderung** | Anteiliger Abzug (falls vorhanden) |
| **Saldo** | Bezahlte Nebenkosten − tatsächliche Nebenkosten |

### Schritt 3 — Hinweise prüfen

Die Abrechnung gibt **Hinweise** (Notes) aus, die auf fehlende oder fehlerhafte Daten hinweisen:

| Art | Bedeutung |
|-----|-----------|
| **Fehler (Error)** | Die Abrechnung ist unvollständig oder rechnerisch inkorrekt. Muss behoben werden. |
| **Warnung (Warning)** | Daten fehlen oder sind unplausibel, die Abrechnung kann aber trotzdem erstellt werden. |

Häufige Ursachen für Fehler:

- Kein Besitzer oder keine Adresse beim Besitzer hinterlegt
- Keine Betriebskostenrechnung für eine Umlage im Abrechnungsjahr
- Fehlende Zählerstände zu Jahresbeginn oder Jahresende
- HKVO: Kein Allgemein-Gaszähler definiert oder Gaszählerstand fehlt

### Schritt 4 — Dokument erzeugen und versenden

Über die entsprechende Schaltfläche in der Oberfläche (technisch: `GET /api/abrechnungslauf/{jahr}/vertrag/{vertragId}/{format}` mit `format` = `pdf` oder `word`) wird das Dokument erzeugt. Das **Abrechnungsresultat** wird als Buchungssatz auf dem `BkAbrechnungsKonto` des Vertrags festgehalten.

Das Resultat umfasst:

| Feld | Bedeutung |
|------|----------|
| Jahr | Abrechnungsjahr |
| Saldo | **Positiv** = Mieter bekommt Geld zurück / **Negativ** = Mieter muss nachzahlen |
| Abgesendet | Wird nach dem Versand auf `true` gesetzt |

Die Beträge (Kaltmiete, Vorauszahlung, Rechnungsbetrag) ergeben sich aus dem verknüpften Buchungssatz und werden nicht zusätzlich gespeichert.

> **Fristen:** Die Betriebskostenabrechnung muss dem Mieter spätestens **12 Monate nach Ende des Abrechnungszeitraums** zugegangen sein (§ 556 BGB). Bei einem Abrechnungsjahr 2024 also spätestens bis zum 31.12.2025.

---

## Mietminderung

Wenn ein Mieter wegen eines Mangels die Miete mindert, wird eine **Mietminderung** eingetragen:

| Feld | Was eintragen |
|------|--------------|
| Beginn | Datum, ab dem die Minderung gilt |
| Ende | Datum, bis zu dem die Minderung gilt (leer = läuft noch) |
| Minderung | Minderungsquote als Dezimalzahl (z.B. `0.10` für 10 %) |

Die Minderung wird bei der Abrechnung **proportional zum Zeitraum** berechnet und sowohl von der Kaltmiete als auch von den Nebenkosten abgezogen.

**Beispiel:** 10 % Minderung für 3 Monate (92 Tage) bei 365 Tagen:
```
Jahresminderung = 0,10 × 92 / 365 = 2,52 %
```

---

## Checkliste: Vollständige Abrechnung

- [ ] Wohnung hat Wohnfläche, Nutzfläche, Nutzeinheiten und Miteigentumsanteile
- [ ] Wohnung hat einen Besitzer mit vollständiger Postanschrift
- [ ] Mieter hat eine hinterlegte Postanschrift
- [ ] Für jede Umlage liegt eine Betriebskostenrechnung für das Abrechnungsjahr vor
- [ ] Alle Mietzahlungen des Jahres sind erfasst
- [ ] Zählerstände zu Jahresbeginn und Jahresende sind vorhanden (bei Verbrauchsabrechnung)
- [ ] Bei Heizkosten: Allgemein-Gaszähler ist definiert und hat Jahresstände
- [ ] Die Abrechnung zeigt keine Fehler (Error-Hinweise)
- [ ] Dokument per `POST` erzeugt (speichert das Resultat)
- [ ] Dokument an Mieter versandt
- [ ] Resultat als „Abgesendet" markiert

---

## Häufige Fragen

**Was passiert, wenn ein Mieter im Laufe des Jahres eingezogen ist?**
Walter berechnet automatisch einen **Zeitanteil** — der Mieter zahlt nur den Anteil der Nebenkosten, der auf seinen Nutzungszeitraum entfällt.

**Was passiert, wenn sich die Miete im Laufe des Jahres erhöht hat?**
Es muss eine neue **VertragVersion** mit dem neuen Betrag und dem Gültigkeitsdatum angelegt werden. Die Kaltmiete wird dann monatsweise aus der jeweils gültigen Version berechnet.

**Was bedeutet ein positiver Saldo?**
Der Mieter hat durch seine monatlichen Vorauszahlungen **mehr gezahlt als die tatsächlichen Nebenkosten**. Der Vermieter muss den Differenzbetrag erstatten.

**Was bedeutet ein negativer Saldo?**
Der Mieter hat weniger gezahlt als die tatsächlichen Nebenkosten betragen. Der Mieter muss den Differenzbetrag **nachzahlen**.

**Welches Dokument wird erzeugt?**
Walter erzeugt wahlweise ein Word-Dokument (`.docx`) oder eine PDF-Datei. Beide enthalten denselben Inhalt mit Briefkopf, Abrechnungsdetails und Saldo.
