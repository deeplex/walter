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

Die Seitenleiste trennt die Fachsicht von der Buchhaltung:

**Hauptbereich**

| Eintrag | Zweck |
|---------|-------|
| **Startseite** | Offene Forderungen und fällige Aufgaben des Jahres |
| **Abrechnungslauf** | Betriebskostenabrechnung berechnen, buchen, drucken |

**Vermietung**

| Eintrag | Zweck |
|---------|-------|
| **Kontakte** | Adressbuch aller Personen (Mieter, Eigentümer, Handwerker …) |
| **Wohnungen** | Wohneinheiten mit Versionen, Eigentümern, Zählern, Erhaltungsaufwand |
| **Verträge** | Mietverhältnisse mit Mietkonto, Kaution, Nebenkostenabrechnungen |
| **Garagen** | Stellplätze und Garagenverträge |

**Stammdaten** (aufklappbares Menü)

| Eintrag | Zweck |
|---------|-------|
| **Umlagen** | Betriebskostenpositionen mit Verteilungsschlüssel und Rechnungen |
| **Umlagetypen** | Klassifikation der Umlagen (z. B. „Heizkosten") |
| **Zähler** | Zähler und Zählerstände |
| **Adressen** | Adressen von Wohnungen und Kontakten |

**Buchhaltung** (aufklappbares Menü) — die Ebene unter der Fachsicht

| Eintrag | Zweck |
|---------|-------|
| **Transaktionen** | alle Zahlungen mit Zuordnung; Erfassen mit allen Positionstypen |
| **Buchungskonten** | alle Konten mit Funktion, Besitzer, Saldo und Status |
| **Jahresabschluss** | Strukturprüfung, Bilanz und Abrechnungsstatus je Jahr |

**Nutzermenü** (unten)

| Eintrag | Zweck |
|---------|-------|
| **Ablagestapel** | Eingangskorb für noch nicht zugeordnete Belege; zeigt ein Symbol, solange etwas offen ist |
| **Nutzereinstellungen** | eigenes Konto und Passwort |
| **Adminbereich** | nur für Admins: Benutzerverwaltung |
| **Abmelden** | Sitzung beenden |

---

## Begriffe

Die Detailseiten zeigen zuerst, was fachlich wichtig ist (fällig, bezahlt, offen). Wie das
gebucht ist, steht im eingeklappten Abschnitt **Buchhaltung** ganz unten auf jeder
Detailseite (Kontakt, Wohnung, Vertrag, Garage, Garagenvertrag, Umlage) und auf den Seiten
*Buchungskonten* und *Buchungssätze*. Die Begriffe entsprechen sich so:

| Auf den Detailseiten | In der Buchhaltung |
|----------------------|--------------------|
| Fällig | Sollstellung |
| Bezahlt | Ausgeglichen (Offener-Posten-Ausgleich) |
| Offen | Offener Posten |
| Guthaben | Haben-Saldo auf einem Personenkonto |
| Zahlung | Transaktion mit ihren Buchungssätzen |
| Rückgängig machen | Löschen bzw. Stornobuchung |
| Rückgängig gemacht | Storniert |
| Mietkonto | Miet-, NK-Vorauszahlungs- und Garagenkonten des Vertrags |
| Kaution | Buchungen auf dem Kautionskonto |
| Nachzahlung / Erstattung | Saldo des Abrechnungskontos |

Der Zahlungsstatus hat überall dieselben Farben:

| Status | Farbe |
|--------|-------|
| Offen | rot |
| Teilweise bezahlt | lila |
| Bezahlt | grün |
| Guthaben | blau |
| Rückgängig gemacht | grau |

---

## Startseite

Die Startseite ist die tägliche Aufgabenliste für ein gewähltes Jahr:

- **Offene Mieten** — Monate mit offener Kaltmiete (und Garagenmiete) je Vertrag. Fehlende
  monatliche Sollstellungen werden beim Öffnen automatisch erzeugt (bis einschließlich des
  laufenden Monats, nie in die Zukunft). Die Zeile führt zum **Mietkonto** des Vertrags.
- **Offene Garagenmieten** — Garagenverträge ohne Wohnungsvertrag; die Zeile führt zum
  Garagenvertrag.
- **Fehlende Betriebskostenrechnungen** — Umlagen ohne Rechnung im Jahr.
- **Offene Betriebskostenrechnungen** — erfasste, noch nicht (ganz) bezahlte Rechnungen.
- **Guthaben** — Monate, in denen mehr gezahlt wurde als fällig war; die Zeile führt zum
  Mietkonto.
- **Zählerstände fällig** — Zähler, die im Jahr noch keinen Stand haben.

Der Button **+** an einer Zeile öffnet **Zahlung erfassen** (wie im Mietkonto): Datum,
Betrag (vorbelegt mit dem offenen Rest), optional das Bankkonto, dazu die Vorschau.
**Erweitert …** öffnet das volle Transaktionsformular. Bei Rechnungen genügt ein Klick auf
die Zeile.

---

## Transaktionen

Eine **Transaktion** entspricht einem Kontoauszugseintrag und ist der zentrale
Erfassungsweg für Geldbewegungen. Beim Anlegen wählt man Zahlungsdatum, ggf. Zahler/
Empfänger (Bankkonto) und fügt eine oder mehrere **Positionen** hinzu:

| Position | Bedeutung |
|----------|-------|
| **Miete** | Monatliche Mietzahlung (Kaltmiete + NK-Vorauszahlung + evtl. Garagen) |
| **Garagenmiete** | Stellplatz-/Garagenmiete |
| **Betriebskosteneingang** | Eingangsrechnung eines Versorgers/Dienstleisters |
| **Erhaltungsaufwendung** | Handwerker-/Instandhaltungsrechnung einer Wohnung |
| **NK-Anteil** | Direkter Nebenkosten-Anteil ohne verteilte Rechnung |
| **Abrechnungsausgleich** | Nach-/Rückzahlung zu einer versendeten Jahresabrechnung |
| **Sonstige** | Freie Buchung mit gewählten Soll-/Haben-Konten |

Die Summe der Positionen muss dem Transaktionsbetrag entsprechen. Walter erzeugt daraus
automatisch die Buchungssätze und legt fehlende Forderungen an. Unter dem Formular zeigt
die **Vorschau**, was gebucht würde (in Klartext, aufklappbar die Buchungssätze);
gespeichert werden kann erst, wenn die Vorschau die Eingabe akzeptiert. Die entstehenden
Buchungssätze sind über die Transaktions-Detailseite und die jeweilige Fachentität
einsehbar.

**Zuordnung** — in Transaktionslisten (Transaktionen, Kontakt, Bankkonto) als Spalte und
auf der Detailseite oben (dort mit Betrag): wofür eine Zahlung war, z. B. „Kaltmiete Mär
2026 – 3 OG, Hauptstr. 5“, mit Link zum Mietkonto, zur Umlage oder zur Wohnung. Walter
leitet das allein aus den Buchungen ab, nicht aus dem Verwendungszweck: aus den bebuchten
Konten und dem Posten, für den eine Zahlung gebucht wurde (Sollstellung, Rechnung,
Abrechnung). Diese Zuordnung bleibt auch nach dem Rückgängigmachen erhalten; die Zahlung
steht dann mit dem Zusatz „(rückgängig gemacht)“. Hängt die Transaktion an einer Rechnung
(ohne eigene Zahlungsbuchung), ist die Zuordnung die Rechnung. Zahlungen ohne Bezug (z. B.
Umbuchung zwischen eigenen Konten) sind **nicht zugeordnet**.

Auf der Detailseite macht **Rückgängig machen** die ganze Transaktion rückgängig. Was dabei
passiert, entscheidet Walter: Hängt nichts daran, wird sie vollständig entfernt; sonst wird
sie mit einer Gegenbuchung storniert (mit Pflicht-Grund, beides bleibt sichtbar). Sind
Abrechnungen betroffen, zeigt der Dialog sie vorher an; sie werden nach Bestätigung mit
zurückgenommen. Die Buchungssätze stehen eingeklappt im Abschnitt **Buchhaltung**.

---

## Kontakte und Bankkonten

- Die Kontakt-Detailseite eines Mieters zeigt oben **Offen als Mieter**: je Vertrag die
  offene Miete und offene Nachzahlungen aus versendeten Nebenkostenabrechnungen, mit
  Summe. Die Zeile führt zum Mietkonto bzw. zu den Abrechnungen.
- Kontakt und Bankkonto listen ihre **Transaktionen** mit der Spalte **Zuordnung**
  (seitenweise vom Server, wie die Transaktionsliste).

---

## Wohnungen

Die Wohnungs-Detailseite bündelt alles zu einer Einheit:

- **Erhaltungsaufwand** — je Rechnung Betrag, bezahlt, offen und Status; im Kopf, was
  insgesamt noch offen ist. **+ Rechnung** erfasst eine Handwerkerrechnung (noch nicht
  bezahlt), **+ Zahlung** bezahlt sie (Bankkonto des Zahlers ist vorbelegt mit dem
  Bankkonto des Eigentümers). Aufgeklappt stehen die Zahlungen mit **Rückgängig
  machen**; direkt beim Kauf bezahlte Rechnungen (Transaktion mit
  Erhaltungsaufwendung) erscheinen gleich als bezahlt. Darunter der **Jahresüberblick**:
  Mieteinnahmen (Kaltmiete laut Sollstellung), Erhaltungsaufwand und nicht verteilte
  Betriebskosten (z. B. bei Leerstand) je Jahr.
- **Versionen** — zeitlich gültige Flächen und Anteile (Wohnfläche, Nutzfläche,
  Nutzeinheiten, Miteigentumsanteile). Bei Änderung eine neue Version anlegen.
- **Eigentümer** — zeitlich gültige Eigentümer-Zuordnung (Kontakt).
- **Wohnungen an derselben Adresse** — Nachbarschaftsübersicht.
- **Zähler** — die Zähler der Wohnung.
- **Verträge** — die Mietverhältnisse der Wohnung.
- **Umlagen** — die zugeordneten Betriebskostenpositionen.
- **Buchhaltung** (eingeklappt, ganz unten) — die Buchungskonten der Wohnung mit ihren
  Buchungen (u. a. Miet­ertrag und Aufwand wie Erhaltungsaufwendungen und Leerstandskosten).
- **Dateien** — an die Wohnung geheftete Belege.

---

## Verträge

Die Vertrags-Detailseite zeigt:

- **Stammdaten** — Vertragsdaten.
- **Mietkonto** — was fällig war, was bezahlt wurde und was offen ist:
  - Kaltmiete (und Garagenmiete) **je Monat**. Fällig ist die Miete laut Vertrag;
    eine Zahlung zählt für den Monat, für den sie erfasst wurde, nicht für den Monat ihres
    Zahlungsdatums. Monate in der Zukunft erscheinen erst, wenn dafür etwas gebucht ist.
  - **NK-Vorauszahlungen je Jahr**, gezählt wie in der Abrechnung (einschließlich
    Miet-Korrekturen).
  - Aufgeklappt zeigt eine Zeile die einzelnen Zahlungen mit Link zur Transaktion und
    **Rückgängig machen** (macht die ganze Zahlung rückgängig, also auch ihre anderen Teile).
  - **+ Zahlung** an einem Monat erfasst eine Zahlung mit Datum, Betrag und optional dem
    Bankkonto des Zahlers. Walter teilt den Betrag auf: zuerst die offene Kaltmiete, dann
    offene Garagenmieten, der Rest ist NK-Vorauszahlung. Die **Vorschau** zeigt vor dem
    Buchen, was passiert; **Erweitert …** öffnet das volle Transaktionsformular.
  - Darunter, falls vorhanden: **Guthaben** (Zahlungen ohne Monat) und **rückgängig gemachte
    Zahlungen**.
  - Weicht die gebuchte Kaltmiete vom Vertrag ab (z. B. rückwirkend erfasste
    Mieterhöhung), erscheint oben ein Hinweis mit **Differenz buchen**.
- **Kaution** — die gehaltene Kaution (Saldo des KautionsKontos) und die bisherigen Eingänge
  und Rückzahlungen. Über **Kaution erhalten** und **Kaution zurückzahlen** werden sie mit
  Datum und Betrag gebucht; eine Rückzahlung darf die gehaltene Kaution nicht übersteigen.
  Jede Buchung ist eine eigene Transaktion und lässt sich direkt in der Liste rückgängig
  machen.
- **Nebenkostenabrechnungen** — je Abrechnung die **Nachzahlung** des Mieters bzw. die
  **Erstattung** an den Mieter, was davon bezahlt ist und was offen ist. Der Name der
  Zeile führt zur Abrechnung. Nicht versendete Abrechnungen tragen den Status „Nicht
  versendet“ und lassen sich noch nicht ausgleichen; stornierte bleiben mit „Storniert“
  als Beleg stehen. Bei versendeten, offenen Abrechnungen erfasst **+ Zahlung** die
  Nachzahlung bzw. Erstattung (bei einer Erstattung ist das optionale Bankkonto das des
  Mieters als Empfänger). Aufgeklappt zeigt eine Zeile die Zahlungen mit **Rückgängig
  machen**.
- **Mieter** — die zugeordneten Kontakte.
- **Nachträge** — die Vertragsversionen (Grundmiete/Kaltmiete und Personenzahl, zeitlich
  gültig).
- **Mietminderungen** — Quote und Zeitraum.
- **Garagenverträge** — dem Vertrag zugeordnete Stellplätze.
- **NK-Anteile** — direkte Nebenkosten-Anteile des Vertrags.
- **Buchhaltung** (eingeklappt, ganz unten) — die Buchungskonten des Vertrags (Miete,
  NK-Vorauszahlung, Abrechnung, Zahlung, Mietminderung, Kaution) mit ihren Buchungen und
  Offenen Posten.
- **Dateien** — an den Vertrag geheftete Belege.

Neue Verträge werden meist aus der Wohnung heraus angelegt, damit die Wohnung bereits
vorbelegt ist.

### Garagenverträge

Die Garagenvertrags-Detailseite zeigt unter den Stammdaten das **Garagenmietkonto**: die
Garagenmiete je Monat, nach denselben Regeln wie das Mietkonto eines Vertrags (fällig,
bezahlt, offen; aufklappen, **+ Zahlung**, **Rückgängig machen**, Guthaben, rückgängig
gemachte Zahlungen). NK-Vorauszahlungen gibt es hier nicht.

Gehört die Garage zu einem Wohnungsvertrag, erscheinen dieselben Monate auch im Mietkonto
des Vertrags; ein Hinweis verlinkt dorthin. Eine Zahlung für Wohnung und Garage zusammen
wird im Mietkonto des Vertrags erfasst, eine Zahlung nur für die Garage geht auch hier.

---

## Umlagen und Umlagetypen

- Ein **Umlagetyp** benennt eine Kostenart (z. B. „Heizkosten", „Wasser/Abwasser").
- Eine **Umlage** verbindet einen Umlagetyp mit einer Gruppe von Wohnungen. Der
  **Verteilungsschlüssel** liegt in der zeitlich gültigen `UmlageVersion`.
- Für **Heizkosten** wird an der Umlage eine **HKVO-Konfiguration** hinterlegt
  (Verbrauchsanteile § 7/§ 8, Warmwasserberechnung § 9(2)).
- Die Umlage-Detailseite zeigt unter **Rechnungen** je Betriebskostenrechnung Betrag,
  bezahlt, offen, Status und wie viel davon auf die Mieter verteilt ist (verteilt wird im
  Abrechnungslauf). **+ Rechnung** erfasst eine Rechnung, **+ Zahlung** bezahlt sie;
  aufgeklappt stehen die Zahlungen mit **Rückgängig machen**. Der Link einer Zeile führt
  zum Buchungssatz der Rechnung.

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
   Vorauszahlungen, Mietminderung und Saldo sowie Hinweise (Fehler/Warnungen). Darüber
   steht das Mietkonto des Vertrags für das Abrechnungsjahr.
3. **Buchen** — hält je Vertrag ein Abrechnungsresultat als Buchungssatz fest.
4. **Drucken** — erzeugt PDF- oder Word-Dokumente (einzeln oder als ZIP für die Gruppe).
5. Nach dem Versand das Resultat als **Abgesendet** markieren. Danach lässt sich die
   Nachzahlung bzw. Erstattung auf der Abrechnung oder im Abschnitt
   **Nebenkostenabrechnungen** des Vertrags erfassen.

Korrekturen: **Rückabwicklung** (solange nicht abgesendet) oder **Storno** (nach dem
Versand, mit Pflicht-Grund). Nach einem Storno gilt das Jahr wieder als offen: Daten
korrigieren, neu buchen und die neue Abrechnung versenden. Die stornierte Abrechnung
bleibt mit dem Status „Storniert“ als Beleg sichtbar. Für Verträge, die bewusst nicht abgerechnet werden, lässt
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

Oben steht die **Strukturprüfung** (unabhängig vom Jahr). Sie listet, was die Zuordnung
von Zahlungen unmöglich macht, statt es zu überspielen:

- **Konten mit mehreren Besitzern** — ein Buchungskonto, das zu mehr als einem Vertrag,
  einer Umlage, Wohnung o. ä. gehört. Buchungen darauf erscheinen nur mit dem Konto.
- **Zahlungen ohne Zuordnung** — Bestand, bei dem nicht festgehalten wurde, für welchen
  Monat, welche Abrechnung bzw. Rechnung gezahlt wurde; die Zahlung gleicht nichts aus.
  Prüfen und gegebenenfalls rückgängig machen und neu erfassen. Bereits rückgängig
  gemachte Zahlungen ohne Zuordnung stehen nur zur Information da.

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
(im Entwicklungs-Stack SeaweedFS).

---

## Adminbereich

Für Admins: Verwaltung der Benutzerkonten (`UserAccount`) — anlegen, Rollen/Wohnungs­zu­ord­nungen
(`Verwalter`) pflegen, Passwörter zurücksetzen.
