# Bedienungsleitfaden (Oberfläche)

Dieser Leitfaden beschreibt die Weboberfläche von Walter Seite für Seite. Er richtet
sich an alle, die täglich mit der Anwendung arbeiten. Die fachlichen Abläufe (Jahres­ablauf,
Datenpflege) sind im [Leitfaden für Buchhalter](buchhalter.md) beschrieben.

---

## Anmeldung und Rollen

Der Zugang erfolgt über **Anmelden** mit Benutzername und Passwort. Nach der Anmeldung
richtet sich der sichtbare Datenbestand nach der Rolle des Nutzers:

| Rolle | Was sie darf |
|-------|--------------|
| Admin | alles lesen und ändern; Zugriff auf den Adminbereich |
| Eigentümer / Vollmacht | die zugeordneten Wohnungen lesen **und** ändern |
| Nur-Lese-Nutzer | die zugeordneten Wohnungen nur ansehen |

Wird eine Zeile oder ein Feld ohne Änderungsrecht geöffnet, ist es schreibgeschützt.

---

## Navigation (Seitenleiste)

Die Seitenleiste gliedert sich in drei Bereiche:

**Hauptbereich**

| Eintrag | Zweck |
|---------|-------|
| **Startseite** | Offene Forderungen und fällige Aufgaben des Jahres |
| **Abrechnungslauf** | Betriebskostenabrechnung berechnen, buchen, drucken |

**Stammdaten**

| Eintrag | Zweck |
|---------|-------|
| **Kontakte** | Adressbuch aller Personen (Mieter, Eigentümer, Handwerker …) |
| **Wohnungen** | Wohneinheiten mit Versionen, Eigentümern, Zählern, Aufwandsposten |
| **Verträge** | Mietverhältnisse mit Versionen, Kaution, Mietminderungen |
| **Garagen** | Stellplätze und Garagenverträge |

**Erweitert** (aufklappbares Menü)

| Eintrag | Zweck |
|---------|-------|
| **Transaktionen** | Zahlungsvorgänge erfassen; erzeugt die Buchungssätze |
| **Jahresabschluss** | Jahresabschlusskontrolle: Bilanz + Abrechnungsstatus je Jahr |
| **Umlagen** | Betriebskostenpositionen mit Verteilungsschlüssel |
| **Umlagetypen** | Klassifikation der Umlagen (z. B. „Heizkosten") |
| **Zähler** | Zähler und Zählerstände |
| **Adressen** | Adressen von Wohnungen und Kontakten |

**Nutzermenü** (unten)

| Eintrag | Zweck |
|---------|-------|
| **Ablagestapel** | Eingangskorb für noch nicht zugeordnete Belege; zeigt ein Symbol, solange etwas offen ist |
| **Nutzereinstellungen** | eigenes Konto und Passwort |
| **Adminbereich** | nur für Admins: Benutzerverwaltung |
| **Abmelden** | Sitzung beenden |

---

## Startseite

Die Startseite ist die tägliche Aufgabenliste für ein gewähltes Jahr. Sie zeigt in
klickbaren Kacheln:

- **Offene Kaltmiete-Forderungen** — fehlende monatliche Sollstellungen werden beim
  Öffnen automatisch erzeugt (bis einschließlich des laufenden Monats, nie in die
  Zukunft). Ein Klick auf eine Zeile öffnet die Erfassung der zugehörigen Zahlung.
- **Offene Betriebskosten** — Umlagen ohne vollständige Rechnung/Verteilung.
- **Offene Garagenmieten**.
- **Guthaben**.
- **Fällige Zählerstände** — Zähler, die im Jahr noch keinen Stand haben.

Alle Kacheln sind direkt bebuchbar: Ein Klick auf eine Zeile öffnet die passende
Schnellerfassung.

---

## Transaktionen

Eine **Transaktion** entspricht einem Kontoauszugseintrag und ist der zentrale
Erfassungsweg für Geldbewegungen. Beim Anlegen wählt man Zahlungsdatum, ggf. Zahler/
Empfänger (Bankkonto) und fügt eine oder mehrere **Positionen** hinzu:

| Position | Wofür |
|----------|-------|
| **Miete** | Monatliche Mietzahlung (Kaltmiete + NK-Vorauszahlung + evtl. Garagen) |
| **Garagenmiete** | Stellplatz-/Garagenmiete |
| **Betriebskosteneingang** | Eingangsrechnung eines Versorgers/Dienstleisters |
| **Erhaltungsaufwendung** | Handwerker-/Instandhaltungsrechnung einer Wohnung |
| **NK-Anteil** | Direkter Nebenkosten-Anteil ohne verteilte Rechnung |
| **Abrechnungsausgleich** | Nach-/Rückzahlung zu einer versendeten Jahresabrechnung |
| **Sonstige** | Freie Buchung mit gewählten Soll-/Haben-Konten |

Die Summe der Positionen muss dem Transaktionsbetrag entsprechen. Walter erzeugt daraus
automatisch die Buchungssätze und legt fehlende Forderungen an. Die entstehenden
Buchungssätze sind über die Transaktions-Detailseite und die jeweilige Fachentität
einsehbar.

---

## Wohnungen

Die Wohnungs-Detailseite bündelt alles zu einer Einheit:

- **Versionen** — zeitlich gültige Flächen und Anteile (Wohnfläche, Nutzfläche,
  Nutzeinheiten, Miteigentumsanteile). Bei Änderung eine neue Version anlegen.
- **Eigentümer** — zeitlich gültige Eigentümer-Zuordnung (Kontakt).
- **Wohnungen an derselben Adresse** — Nachbarschaftsübersicht.
- **Zähler** — die Zähler der Wohnung.
- **Verträge** — die Mietverhältnisse der Wohnung.
- **Umlagen** — die zugeordneten Betriebskostenpositionen.
- **Konten** — die Buchungskonten der Wohnung mit ihren Buchungen (u. a. Miet­ertrag und
  Aufwand wie Erhaltungsaufwendungen und Leerstandskosten).
- **Dateien** — an die Wohnung geheftete Belege.

---

## Verträge

Die Vertrags-Detailseite zeigt:

- **Stammdaten & Kaution** — Vertragsdaten samt Kautionsbetrag, Eingangs-/Rückgabedatum
  und Art (in einem aufklappbaren Abschnitt).
- **Mieter** — die zugeordneten Kontakte.
- **Nachträge** — die Vertragsversionen (Grundmiete/Kaltmiete und Personenzahl, zeitlich
  gültig).
- **Mietminderungen** — Quote und Zeitraum.
- **Garagenverträge** — dem Vertrag zugeordnete Stellplätze.
- **NK-Anteile** — direkte Nebenkosten-Anteile des Vertrags.
- **Abrechnungsresultate** — die gebuchten Jahresabrechnungen.
- **Konten** — die Buchungskonten des Vertrags (Miete, NK-Vorauszahlung, Abrechnung,
  Zahlung, Mietminderung) mit ihren Buchungen und Offenen Posten.
- **Dateien** — an den Vertrag geheftete Belege.

Neue Verträge werden meist aus der Wohnung heraus angelegt, damit die Wohnung bereits
vorbelegt ist.

---

## Umlagen und Umlagetypen

- Ein **Umlagetyp** benennt eine Kostenart (z. B. „Heizkosten", „Wasser/Abwasser").
- Eine **Umlage** verbindet einen Umlagetyp mit einer Gruppe von Wohnungen. Der
  **Verteilungsschlüssel** liegt in der zeitlich gültigen `UmlageVersion`.
- Für **Heizkosten** wird an der Umlage eine **HKVO-Konfiguration** hinterlegt
  (Verbrauchsanteile § 7/§ 8, Warmwasserberechnung § 9(2)).
- Die Umlage-Detailseite zeigt die eingebuchten Betriebskostenrechnungen des Jahres und
  ihren Verteilungsstatus (bezahlt/verteilt).

---

## Zähler

Zähler werden mit Kennnummer, Typ (Warmwasser, Kaltwasser, Strom, Gas, Wärme), zugehöriger
Wohnung (leer = Allgemeinzähler) und optionalem Ende-Datum geführt. **Zählerstände**
werden mit Datum und Stand erfasst; die Abrechnung nutzt zum Jahresanfang den
nächstgelegenen Stand (höchstens 14 Tage vor dem Stichtag) und zum Jahresende den letzten
Stand bis zum Stichtag.

---

## Abrechnungslauf

Der Abrechnungslauf erstellt die Betriebskostenabrechnung je **Abrechnungsgruppe**
(alle über gemeinsame Umlagen verbundenen Wohnungen):

1. **Jahr und Gruppe** wählen.
2. **Vorschau** — rechnet ohne zu buchen und zeigt je Vertrag Kaltmiete, Nebenkosten,
   Vorauszahlungen, Mietminderung und Saldo sowie Hinweise (Fehler/Warnungen).
3. **Buchen** — hält je Vertrag ein Abrechnungsresultat als Buchungssatz fest.
4. **Drucken** — erzeugt PDF- oder Word-Dokumente (einzeln oder als ZIP für die Gruppe).
5. Nach dem Versand das Resultat als **Abgesendet** markieren.

Korrekturen: **Rückabwicklung** (solange nicht abgesendet) oder **Storno** (nach dem
Versand, mit Pflicht-Grund). Für Verträge, die bewusst nicht abgerechnet werden, lässt
sich ein **Abrechnungsverzicht** mit Grund hinterlegen.

Details der Berechnung: [Betriebskostenabrechnung](betriebskostenabrechnung.md).

---

## Jahresabschluss

Die **Jahresabschlusskontrolle** ist die Endprüfung eines Jahres. Sie zeigt je
Buchungsjahr:

- eine Bilanzansicht je Konto (Jahres-Soll/-Haben, Saldovortrag, Endsaldo, offene Posten),
- den Abrechnungsstatus je Vertrag (Resultat vorhanden? abgesendet? ausgeglichen? oder
  Verzicht?).

Ein Jahr gilt als **abgeschlossen**, wenn keine Konten mehr offen sind und alle
Abrechnungen fertig (abgesendet oder verzichtet) sind. Die Seite ist rein informativ und
erzeugt keine Buchungen.

---

## Ablagestapel

Der Ablagestapel ist der Eingangskorb für Belege (PDFs, Scans), die noch keiner Buchung
zugeordnet sind. Dateien werden per Drag-and-drop hochgeladen und später an den passenden
Buchungssatz oder die passende Entität geheftet. Solange der Stapel nicht leer ist, zeigt
die Seitenleiste ein Hinweissymbol.

---

## Dateien

An vielen Detailseiten (Wohnung, Vertrag, Umlage, Adresse, Kontakt, Buchungssatz …) gibt
es ein **Datei-Panel**. Belege werden dort hochgeladen, aufgelistet, heruntergeladen und
in den Papierkorb verschoben. Die Ablage erfolgt in einem S3-kompatiblen Speicher
(im Entwicklungs-Stack MinIO).

---

## Adminbereich

Für Admins: Verwaltung der Benutzerkonten (`UserAccount`) — anlegen, Rollen/Wohnungs­zu­ord­nungen
(`Verwalter`) pflegen, Passwörter zurücksetzen.
