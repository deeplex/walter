# Leitfaden für Buchhalter

Dieser Leitfaden richtet sich an Buchhalter und Verwalter, die mit Walter die
laufende Buchführung und die jährliche Betriebskostenabrechnung (Nebenkosten­abrechnung)
erstellen. Er erklärt, welche Daten wann gepflegt werden und wie ein Abrechnungsjahr
sauber abgeschlossen wird.

Eine Erklärung der Bedienoberfläche (Menüs, Seiten, Schaltflächen) findet sich im
[Bedienungsleitfaden](bedienung.md). Die Rechenlogik der Abrechnung ist in
[Betriebskostenabrechnung](betriebskostenabrechnung.md) beschrieben.

---

## Grundprinzip: Alles ist eine Buchung

Walter führt eine **doppelte Buchführung**. Das ist das zentrale Konzept, das den
gesamten Arbeitsablauf prägt:

- Jeder Geldbetrag ist ein **Buchungssatz** mit einer Soll- und einer Haben-Seite auf
  **Buchungskonten** — eine Miete, eine Betriebskostenrechnung, ein Erhaltungsaufwand.
- Jede Wohnung, jeder Vertrag, jede Umlage und jeder Handwerker-Kontakt besitzt seine
  eigenen Buchungskonten. Walter legt diese automatisch an — man muss sich um den
  Kontenrahmen im Alltag nicht kümmern.
- Geld, das den Kontostand tatsächlich verändert (Zahlungseingang, Überweisung an
  einen Dienstleister), wird als **Transaktion** erfasst. Eine Transaktion entspricht
  einem Kontoauszugseintrag und erzeugt automatisch die passenden Buchungssätze.
- Eine offene Forderung (z.B. eine gestellte, aber noch nicht bezahlte Miete) bleibt
  so lange **offen**, bis eine Zahlung sie über den *Offenen-Posten-Ausgleich* (OPOS)
  deckt.

Praktisch heißt das: Man erfasst überwiegend **Transaktionen** und muss keine
Buchungssätze von Hand konstruieren.

---

## Jahresüberblick

```
Januar – Dezember       Laufende Datenpflege
                         └─ Zahlungseingänge als Transaktionen erfassen
                         └─ Eingangsrechnungen (Betriebskosten, Handwerker) buchen
                         └─ Belege in den Ablagestapel hochladen und zuordnen
                         └─ Zählerstände ablesen und eintragen

Nach Jahresende         Abrechnung & Jahresabschluss
(typisch: Jan – März)    └─ Betriebskostenrechnungen des Jahres vollständig?
                         └─ Zählerstände zu Jahresanfang/-ende prüfen
                         └─ Abrechnungslauf: Vorschau prüfen, dann buchen
                         └─ Dokumente erzeugen und an Mieter versenden
                         └─ Nach-/Rückzahlungen ausgleichen
                         └─ Jahresabschlusskontrolle auf „grün" bringen
```

---

## Stammdaten (einmalig, bei Änderung aktualisieren)

### Wohnung

Für jede Mieteinheit muss hinterlegt sein:

| Wann | Was |
|------|-----|
| Einmalig | Wohnfläche (m²), Nutzfläche (m²), Nutzeinheiten, Miteigentumsanteile — als `WohnungVersion` mit Beginn-Datum |
| Einmalig | Adresse der Wohnung |
| Einmalig | Eigentümer (Kontakt mit vollständiger Postanschrift) — zeitlich als `WohnungEigentuemer` |

> Der Eigentümer erscheint als Vermieter im Briefkopf der Abrechnung. Ohne hinterlegte
> Adresse erscheint dort „Unbekannt".

Ändern sich Flächen oder Anteile, wird eine **neue** `WohnungVersion` mit neuem
Beginn-Datum angelegt — die alte bleibt für vergangene Jahre gültig.

### Umlagen

Für jede Betriebskostenart (Heizung, Wasser, Allgemeinstrom, Grundsteuer …) wird eine
**Umlage** angelegt:

| Feld | Beschreibung |
|------|-------------|
| Umlagetyp | Art der Kosten, z.B. „Heizkosten", „Wasser/Abwasser" |
| Umlageschlüssel | Verteilung auf die Wohnungen (in der `UmlageVersion`, zeitlich gültig) |
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

Für **Heizkosten** nach Heizkostenverordnung (HKVO) wird an der Umlage zusätzlich eine
HKVO-Konfiguration hinterlegt (Verbrauchsanteile § 7/§ 8, Warmwasserberechnung § 9(2)).
Details siehe [Betriebskostenabrechnung](betriebskostenabrechnung.md).

### Zähler

Für verbrauchsabhängige Umlagen ist je Wohnung ein **Zähler** einzutragen:

| Feld | Beschreibung |
|------|-------------|
| Kennnummer | Zählernummer laut Plakette |
| Typ | Warmwasser, Kaltwasser, Strom, Gas oder Wärme |
| Wohnung | Zugehörige Wohneinheit (leer = Allgemeinzähler) |
| Ende | Datum der Außerbetriebnahme (optional) |

> Für die Heizkostenabrechnung nach HKVO wird zusätzlich ein **Allgemein-Zähler**
> (Gas/Wärme) benötigt, der keiner Wohnung zugeordnet ist.

### Mietvertrag

Je Mietverhältnis muss vorhanden sein:

- **Vertrag** mit zugehöriger Wohnung
- Mindestens eine **VertragVersion** mit Grundmiete (Kaltmiete) und Personenzahl,
  gültig ab Vertragsbeginn
- **Mieter** (Kontakt) mit hinterlegter Postanschrift (für den Briefkopf)

Bei Mieterhöhung oder Personenänderung: neue `VertragVersion` mit neuem Beginn-Datum
anlegen — die alte Version bleibt erhalten.

---

## Laufende Datenpflege

### Zahlungseingänge und -ausgänge erfassen (Transaktionen)

Der zentrale Erfassungsweg ist die **Transaktion**. Eine Transaktion bündelt einen
Zahlungsvorgang (ein Kontoauszugseintrag) und besteht aus einer oder mehreren
Positionen. Walter erzeugt daraus automatisch die Buchungssätze und stellt fehlende
Forderungen bei Bedarf glatt.

Unterstützte Positionsarten:

| Position | Wofür | Was Walter bucht |
|----------|-------|------------------|
| **Miete** | Monatliche Mietzahlung des Mieters | Trennt Kaltmiete und NK-Vorauszahlung; legt die Sollstellung an, falls sie fehlte, und gleicht sie aus |
| **Garagenmiete** | Stellplatz-/Garagenmiete | Buchung auf das Garagenvertrag-Konto |
| **Betriebskosten­eingang** | Eingangsrechnung eines Versorgers/Dienstleisters | Rechnung auf dem `NkVerrechnungsKonto` der Umlage + Zahlung an den Dienstleister |
| **Erhaltungsaufwendung** | Handwerker-/Instandhaltungsrechnung | Aufwandsbuchung auf dem `AufwandsKonto` der Wohnung |
| **NK-Anteil** | Direkter Nebenkosten-Anteil (ohne verteilte Rechnung) | Soll auf `NkBuchungskonto` des Vertrags |
| **Abrechnungsausgleich** | Nach-/Rückzahlung zu einer bereits versendeten Jahresabrechnung | Ausgleich des Abrechnungssaldos (Teilzahlungen erlaubt) |
| **Sonstige** | Frei definierbare Buchung | Direkter Buchungssatz mit gewählten Soll-/Haben-Konten |

> Der Mieter zahlt üblicherweise Kaltmiete + Nebenkostenvorauszahlung in **einem**
> Betrag. Dieser Gesamtbetrag wird als **Miete**-Position eingetragen; Walter trennt
> Kaltmiete und Nebenkosten automatisch anhand der hinterlegten Grundmiete.

Die Summe aller Positionen muss dem Transaktionsbetrag entsprechen — sonst wird die
Transaktion abgelehnt.

### Offene Forderungen (Startseite)

Die **Startseite** listet die offenen Kaltmiete-Forderungen des gewählten Jahres.
Beim Öffnen erzeugt Walter fehlende monatliche **Sollstellungen** automatisch
(bis einschließlich des laufenden Monats — nie für zukünftige Monate). Von hier aus
lassen sich Zahlungen direkt erfassen. Zusätzlich zeigt die Startseite:

- offene Betriebskosten (fehlende oder unvollständig verteilte Rechnungen),
- offene Garagenmieten,
- Guthaben,
- fällige Zählerstände (Zähler, die im Jahr noch keinen Stand haben).

### Belege ablegen (Ablagestapel)

Der **Ablagestapel** ist der Eingangskorb für Belege (PDFs, Scans), die noch keiner
Buchung zugeordnet sind. Ist der Stapel nicht leer, zeigt die Seitenleiste ein
Hinweissymbol. Belege können später an den passenden Buchungssatz oder die passende
Entität geheftet werden.

### Zählerstände eintragen

Zählerstände sollten mindestens zu folgenden Terminen erfasst werden:

| Zeitpunkt | Beschreibung |
|-----------|-------------|
| 01.01. (bzw. 31.12.) | Jahresanfang/-ende für die Abrechnung |
| Mieterwechsel | Ablesung bei Ein- und Auszug |
| Auf Anforderung | Zwischenablesung bei Bedarf |

> Für den Jahresanfang wählt die Abrechnung den Zählerstand, der dem Stichtag am nächsten
> liegt — höchstens **14 Tage** davor. Für das Jahresende gilt der letzte Stand bis zum
> Stichtag; weicht ein Messfenster mehr als 14 Tage ab, erscheint eine Warnung. Ein
> Zählerstand vom 28.12. gilt also noch als Jahresabschluss.

---

## Abrechnung erstellen (Jahresabschluss)

### Schritt 1 — Betriebskostenrechnungen vollständig erfassen

Für jede Umlage wird je Abrechnungsjahr die vom Versorger/Dienstleister ausgestellte
Jahresrechnung als **Buchung** erfasst — als Position **Betriebskosteneingang** in
einer Transaktion oder direkt auf der Umlage. Fachlich entsteht dabei ein Buchungssatz
auf dem `NkVerrechnungsKonto` der Umlage (Haben = Kosten, negativer Betrag = Gutschrift).

> Liegt für eine Umlage im Abrechnungsjahr keine Rechnung vor, meldet der
> Abrechnungslauf einen Fehler.

### Schritt 2 — Abrechnungslauf: Vorschau prüfen

Die Abrechnung wird nicht je Vertrag einzeln, sondern je **Abrechnungsgruppe**
gerechnet. Eine Abrechnungsgruppe fasst alle Wohnungen zusammen, die über gemeinsame
Umlagen (direkt oder mittelbar) verbunden sind. Auf der Seite **Abrechnungslauf** wählt
man Jahr und Gruppe und startet die
**Vorschau** (`preview`). Die Vorschau rechnet, **ohne** zu buchen, und zeigt je Partei:

| Posten | Bedeutung |
|--------|-----------|
| Kaltmiete | Grundmiete × Monate (aus den VertragVersionen) |
| Nebenkosten | Anteil der Wohnung an den Betriebskostenrechnungen |
| Vorauszahlungen | Geleistete NK-Vorauszahlungen |
| Mietminderung | Anteiliger Abzug (falls vorhanden) |
| Saldo | Vorauszahlungen − tatsächliche Nebenkosten |

### Schritt 3 — Hinweise prüfen

Die Vorschau gibt **Hinweise** (Notes) aus:

| Art | Bedeutung |
|-----|-----------|
| **Fehler (Error)** | Die Abrechnung ist unvollständig oder rechnerisch inkorrekt. Muss behoben werden. |
| **Warnung (Warning)** | Daten fehlen oder sind unplausibel; die Abrechnung ist dennoch erstellbar. |

Häufige Ursachen:

- Fehlende Zählerstände zu Jahresbeginn/-ende oder unplausible Zähler-Chronologie (Fehler)
- Keine Betriebskostenrechnung für eine Umlage im Abrechnungsjahr (Warnung)
- HKVO § 9(2): Warmwasseranteil über 100 % (Fehler)
- Kein Allgemein-Wärmezähler → kein Warmwasseranteil, der gesamte Betrag wird als Heizung verteilt (Warnung)

### Schritt 4 — Buchen

Ist die Vorschau in Ordnung, wird die Abrechnung **gebucht** (`book`). Dabei entsteht je
Vertrag ein **Abrechnungsresultat**, festgehalten als Buchungssatz auf dem
`BkAbrechnungsKonto` des Vertrags. Die Beträge ergeben sich aus diesem Buchungssatz.

### Schritt 5 — Dokument erzeugen und versenden

Aus der gebuchten Abrechnung werden **PDF-** oder **Word-Dokumente** erzeugt (einzeln
oder als ZIP für die ganze Gruppe). Das Dokument enthält Briefkopf, Abrechnungsdetails
und Saldo:

| Saldo | Bedeutung |
|-------|-----------|
| **Positiv** | Mieter hat mehr vorausgezahlt als tatsächliche Nebenkosten → Vermieter erstattet |
| **Negativ** | Mieter hat zu wenig gezahlt → Mieter zahlt nach |

Nach dem Versand wird das Abrechnungsresultat als **Abgesendet** markiert.

> **Frist:** Die Betriebskostenabrechnung muss dem Mieter spätestens **12 Monate nach
> Ende des Abrechnungszeitraums** zugehen (§ 556 BGB). Für das Abrechnungsjahr 2024 also
> bis spätestens 31.12.2025.

### Schritt 6 — Nach-/Rückzahlung ausgleichen

Zahlt der Mieter nach oder erhält er eine Erstattung, wird das als Transaktion mit der
Position **Abrechnungsausgleich** erfasst (Teilzahlungen sind erlaubt). Damit gilt der
Abrechnungssaldo als gedeckt.

---

## Korrekturen: Rückabwicklung und Storno

Eine gebuchte Abrechnung wird nie überschrieben. Es gibt zwei Wege zurück:

| Weg | Wann | Wirkung |
|-----|------|---------|
| **Rückabwicklung** | Solange **nicht** abgesendet | Nimmt Resultate, Verteilungs- und Umbuchungen der Gruppe für das Jahr vollständig zurück |
| **Storno** | Wenn **bereits abgesendet** | Stornobuchung; ein **Grund ist Pflicht**. Der Originalsatz bleibt erhalten |

Einzelne Buchungssätze lassen sich bearbeiten oder löschen, solange sie **frei** sind —
also nicht in eine Betriebskostenabrechnung eingeflossen und ohne Offene-Posten-Ausgleich.
Sobald ein Satz mit einer Zahlung ausgeglichen wurde, ist statt Löschen ein **Storno**
nötig; abrechnungsrelevante Sätze sind gesperrt und nur über die Rückabwicklung/Storno des
Abrechnungslaufs veränderbar.

---

## Abrechnungsverzicht

Für Verträge, die für ein Jahr bewusst **nicht** abgerechnet werden (Bestandsübernahme,
Zeitraum vor Programmeinführung, kein Vorschuss vereinnahmt, Verjährung), wird ein
**Abrechnungsverzicht** dokumentiert:

| Feld | Was eintragen |
|------|--------------|
| Jahr | Abrechnungsjahr, für das nicht abgerechnet wird |
| Grund | Pflicht — der Beleg für die Entscheidung |
| Datum | Datum, zu dem der Verzicht festgehalten wurde |

Ein Verzicht ist bewusst **ohne** Buchungssatz — er dokumentiert eine geschäftliche
Entscheidung. Die Jahresabschlusskontrolle behandelt einen so markierten Vertrag als
erledigt.

---

## Jahresabschlusskontrolle

Die **Jahresabschlusskontrolle** ist die Endprüfung eines Jahres. Sie ist rein lesend
und erzeugt keine Buchungen. Sie prüft pro Buchungsjahr, ob

- alle ausgleichbaren Konten ausgeglichen sind (keine offenen Posten),
- für alle Verträge eine Abrechnung erstellt **und** abgesendet wurde — oder ein
  Abrechnungsverzicht vorliegt.

Zusätzlich zeigt sie je Konto die Jahresverkehrszahlen (Soll/Haben), den Saldovortrag
aus Vorjahren und den Endsaldo. Ein Jahr gilt als **abgeschlossen**, wenn keine Konten
mehr offen sind und alle Abrechnungen fertig (abgesendet oder verzichtet) sind.

Ergänzend prüft der **Abrechnungslauf** über die Kontrolle (`kontrolle/{jahr}`), ob eine
erneute Abrechnung dasselbe ergäbe wie das bereits Gebuchte — als Regressionsschutz
gegen nachträglich veränderte Grunddaten.

---

## Mietminderung

Mindert ein Mieter wegen eines Mangels die Miete, wird eine **Mietminderung** eingetragen:

| Feld | Was eintragen |
|------|--------------|
| Beginn | Datum, ab dem die Minderung gilt |
| Ende | Datum, bis zu dem die Minderung gilt (leer = läuft noch) |
| Minderung | Minderungsquote als Dezimalzahl (z.B. `0.10` für 10 %) |

Die Minderung wird **proportional zum Zeitraum** berechnet und in der
Betriebskostenabrechnung als Gutschrift auf die Nebenkosten angerechnet (im Dokument als
Position „Verrechnung mit Mietminderung").

**Beispiel:** 10 % Minderung für 92 Tage bei 365 Tagen:
```
Jahresminderung = 0,10 × 92 / 365 = 2,52 %
```

---

## Checkliste: Vollständige Abrechnung

- [ ] Wohnung hat Wohnfläche, Nutzfläche, Nutzeinheiten und Miteigentumsanteile
- [ ] Wohnung hat einen Eigentümer mit vollständiger Postanschrift
- [ ] Mieter hat eine hinterlegte Postanschrift
- [ ] Für jede Umlage ist die Betriebskostenrechnung des Jahres gebucht
- [ ] Alle Zahlungseingänge des Jahres sind als Transaktionen erfasst
- [ ] Zählerstände zu Jahresbeginn und Jahresende sind vorhanden (bei Verbrauchsabrechnung)
- [ ] Bei Heizkosten: Allgemein-Zähler ist definiert und hat Jahresstände
- [ ] Abrechnungslauf-Vorschau zeigt keine Fehler
- [ ] Abrechnung gebucht
- [ ] Dokumente erzeugt und an Mieter versandt
- [ ] Resultate als „Abgesendet" markiert
- [ ] Nach-/Rückzahlungen ausgeglichen
- [ ] Jahresabschlusskontrolle zeigt das Jahr als abgeschlossen

---

## Häufige Fragen

**Wie erfasse ich eine Mietzahlung?**
Als **Transaktion** mit einer **Miete**-Position. Den vom Mieter gezahlten Gesamtbetrag
(Kaltmiete + NK-Vorauszahlung) eintragen — Walter trennt beides automatisch und gleicht
die Sollstellung aus.

**Wie buche ich eine Handwerkerrechnung (Erhaltungsaufwand)?**
Als Transaktion mit einer **Erhaltungsaufwendung**-Position. Sie wird auf das
`AufwandsKonto` der Wohnung gebucht.

**Ein Mieter ist im Laufe des Jahres eingezogen — was ist zu tun?**
Nichts Besonderes. Walter berechnet automatisch einen **Zeitanteil**; der Mieter zahlt
nur den auf seinen Nutzungszeitraum entfallenden Anteil der Nebenkosten.

**Die Miete hat sich im Laufe des Jahres erhöht — was ist zu tun?**
Eine neue **VertragVersion** mit neuem Betrag und Gültigkeitsdatum anlegen. Die Kaltmiete
wird dann monatsweise aus der jeweils gültigen Version berechnet.

**Ich habe mich bei einer gebuchten Abrechnung vertan.**
Noch nicht versendet → **Rückabwicklung**. Bereits versendet → **Storno** mit Grund.
Danach neu buchen.

**Ich möchte einen Vertrag für ein Jahr gar nicht abrechnen.**
Einen **Abrechnungsverzicht** mit Grund hinterlegen — die Jahresabschlusskontrolle wertet
den Vertrag dann als erledigt.

**Was bedeutet ein positiver bzw. negativer Saldo?**
Positiv = der Mieter hat mehr vorausgezahlt als die tatsächlichen Nebenkosten und bekommt
erstattet. Negativ = der Mieter hat zu wenig gezahlt und muss nachzahlen.

**Welches Dokument wird erzeugt?**
Wahlweise ein Word-Dokument (`.docx`) oder eine PDF-Datei; bei mehreren Verträgen einer
Gruppe ein ZIP-Archiv. Alle enthalten Briefkopf, Abrechnungsdetails und Saldo.
