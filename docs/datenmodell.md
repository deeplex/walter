# Datenmodell

Walter verwendet PostgreSQL als Datenbank und Entity Framework Core als ORM. Alle Entitäten befinden sich im Projekt `Deeplex.Saverwalter.Model` (Ordner `model/`, Authentifizierung in `Auth/`).

Zwei Dinge prägen das Modell:

1. **Versionierung statt Überschreiben.** Veränderliche Stammdaten (Wohnungsflächen, Miethöhe, Umlageschlüssel, Eigentümer) werden als zeitlich gültige *Versionen* bzw. Zuordnungen mit `Beginn`/`Ende` geführt, nicht direkt auf der Hauptentität.
2. **Doppelte Buchführung.** Geldbeträge stehen nicht mehr als Felder auf Fachentitäten (es gibt keine `Miete`, `Betriebskostenrechnung` oder `Erhaltungsaufwendung` mehr), sondern als **Buchungssätze** mit Soll-/Haben-**Buchungszeilen** auf **Buchungskonten**. Fachentitäten verweisen auf „ihre“ Konten.

---

## Entity-Relationship-Diagramm (Kern)

```mermaid
erDiagram
    Buchungskonto {
        int     BuchungskontoId  PK
        string  Kontonummer
        string  Bezeichnung
        enum    Kontotyp
    }
    Buchungssatz {
        guid    BuchungssatzId   PK
        date    Buchungsdatum
        int     Buchungsnummer
        int     Buchungsjahr
        string  Beschreibung
        string  Belegpfad
        guid    StornoVonId      FK
        guid    TransaktionId    FK
    }
    Buchungszeile {
        guid    BuchungszeileId  PK
        guid    BuchungssatzId   FK
        int     BuchungskontoId  FK
        enum    SollHaben
        decimal Betrag
    }
    OffenerPostenAusgleich {
        guid    Id          PK
        guid    SollZeileId  FK
        guid    HabenZeileId FK
    }
    Transaktion {
        guid    TransaktionId  PK
        int     ZahlerId       FK
        int     EmpfaengerId   FK
        date    Zahlungsdatum
        decimal Betrag
        string  Verwendungszweck
    }
    Bankkonto {
        int     BankkontoId    PK
        string  Bank
        string  Iban
        int     BuchungsKontoId FK
    }

    Buchungssatz   ||--|{ Buchungszeile         : "besteht aus"
    Buchungskonto  ||--o{ Buchungszeile         : "bebucht durch"
    Buchungssatz   |o--o| Buchungssatz          : "storniert (StornoVon)"
    Transaktion    ||--o{ Buchungssatz          : "verbucht als"
    Bankkonto      ||--|| Buchungskonto         : "geführt auf"
    Buchungszeile  ||--o{ OffenerPostenAusgleich : "Soll/Haben-Ausgleich"
```

```mermaid
erDiagram
    Wohnung {
        int     WohnungId       PK
        string  Bezeichnung
        int     MietErtragskontoId FK
        int     AufwandsKontoId    FK
        int     AdresseId          FK
    }
    WohnungVersion {
        int     WohnungVersionId PK
        int     WohnungId        FK
        date    Beginn
        decimal Wohnflaeche
        decimal Nutzflaeche
        decimal Miteigentumsanteile
        int     Nutzeinheit
    }
    WohnungEigentuemer {
        int     Id        PK
        int     WohnungId FK
        int     KontaktId FK
        date    Beginn
        date    Ende
    }
    Vertrag {
        int     VertragId          PK
        int     WohnungId          FK
        int     AnsprechpartnerId  FK
        date    Ende
        decimal KautionBetrag
    }
    VertragVersion {
        int     VertragVersionId PK
        int     VertragId        FK
        date    Beginn
        double  Grundmiete
        int     Personenzahl
    }
    Mietminderung {
        int     MietminderungId PK
        int     VertragId       FK
        date    Beginn
        date    Ende
        double  Minderung
    }
    Umlage {
        int     UmlageId          PK
        int     TypId             FK
        int     NkVerrechnungsKontoId FK
        date    Ende
    }
    UmlageVersion {
        int     UmlageVersionId PK
        int     UmlageId        FK
        date    Beginn
        enum    Schluessel
    }
    Umlagetyp {
        int     UmlagetypId PK
        string  Bezeichnung
    }
    HKVO {
        int     HKVOId        PK
        double  HKVO_P7
        double  HKVO_P8
        enum    HKVO_P9
        double  Strompauschale
        int     HeizkostenId  FK
        int     BetriebsstromId FK
    }
    Zaehler {
        int     ZaehlerId PK
        string  Kennnummer
        enum    Typ
        int     WohnungId FK
        int     AdresseId FK
        date    Ende
    }
    Zaehlerstand {
        int     ZaehlerstandId PK
        int     ZaehlerId      FK
        date    Datum
        double  Stand
    }
    Abrechnungsresultat {
        guid    AbrechnungsresultatId PK
        int     VertragId   FK
        guid    BuchungssatzId FK
        bool    Abgesendet
    }
    Kontakt {
        int     KontaktId PK
        string  Name
        enum    Rechtsform
        int     VerbindlichkeitsKontoId FK
    }

    Adresse        ||--o{ Wohnung            : "liegt in"
    Wohnung        ||--|{ WohnungVersion     : "versioniert durch"
    Wohnung        ||--o{ WohnungEigentuemer : "gehört (zeitlich)"
    Kontakt        ||--o{ WohnungEigentuemer : "ist Eigentümer"
    Wohnung        ||--o{ Vertrag            : "Grundlage von"
    Wohnung        ||--o{ Zaehler            : "ausgestattet mit"
    Wohnung        }o--o{ Umlage             : "zugeordnet zu"
    Vertrag        ||--|{ VertragVersion     : "versioniert durch"
    Vertrag        ||--o{ Mietminderung      : "hat Minderung"
    Vertrag        }o--o{ Kontakt            : "Mieter in"
    Vertrag        ||--o{ Abrechnungsresultat : "hat Ergebnis"
    Umlagetyp      ||--|{ Umlage             : "klassifiziert"
    Umlage         ||--|{ UmlageVersion      : "versioniert durch"
    Umlage         }o--|{ Zaehler            : "gemessen durch"
    Umlage         ||--o| HKVO              : "Heizkosten-Konfig"
    Zaehler        ||--o{ Zaehlerstand       : "abgelesen als"
    Kontakt        |o--o| Buchungskonto      : "Verbindlichkeitskonto"
    Abrechnungsresultat ||--|| Buchungssatz  : "festgehalten als"
```

> **HKVO:** Eine `HKVO` referenziert eine `Heizkosten`-Umlage (Pflicht) und optional eine `Betriebsstrom`-Umlage. Eine Umlage kann über `HeizkostenHKVOs` bzw. `BetriebsstromHKVOs` in beiden Rollen auftreten.

---

## Buchführungs-Entitäten

### Buchungskonto

Ein Konto im Kontenrahmen. Fachentitäten besitzen ihre Konten (z. B. `Wohnung.AufwandsKonto`).

| Feld             | Typ              | Pflicht | Beschreibung                          |
|------------------|------------------|---------|---------------------------------------|
| `BuchungskontoId`| int              | PK      | Primärschlüssel                       |
| `Kontonummer`    | string           | ja      | Kontonummer im Kontenrahmen           |
| `Bezeichnung`    | string           | ja      | Klartextbezeichnung                   |
| `Kontotyp`       | BuchungskontoTyp | ja      | `Aktiv`, `Passiv`, `Aufwand`, `Ertrag`|
| `Notiz`          | string           | nein    |                                       |

### Buchungssatz

Ein vollständiger Buchungssatz nach GoB / § 239 HGB. Korrekturen nur per Stornobuchung.

| Feld            | Typ          | Pflicht | Beschreibung                                                |
|-----------------|--------------|---------|------------------------------------------------------------|
| `BuchungssatzId`| Guid         | PK      | Primärschlüssel                                            |
| `Buchungsdatum` | DateOnly     | ja      | Datum der Buchung                                          |
| `Beschreibung`  | string       | ja      | Buchungstext                                              |
| `Buchungsnummer`| int          | (auto)  | Lückenlose fortlaufende Nummer je Buchungsjahr (DB-Sequence)|
| `Buchungsjahr`  | int          | ja      | Wirtschaftsjahr (Default = Jahr des Buchungsdatums)        |
| `Belegpfad`     | string       | nein    | S3-Pfad zum Originalbeleg                                  |
| `StornoVon`     | Buchungssatz | nein    | Gesetzt, wenn dieser Satz ein Storno ist                  |
| `StornoNach`    | Buchungssatz | nein    | Rücknavigation: gesetzt, wenn dieser Satz storniert wurde |
| `Transaktion`   | Transaktion  | nein    | Verweis auf den importierten Kontoauszugseintrag          |
| `Notiz`         | string       | nein    |                                                            |

### Buchungszeile

Eine Soll- oder Haben-Zeile eines Buchungssatzes.

| Feld            | Typ          | Pflicht | Beschreibung                            |
|-----------------|--------------|---------|-----------------------------------------|
| `BuchungszeileId`| Guid        | PK      | Primärschlüssel                         |
| `Buchungssatz`  | Buchungssatz | ja      | Zugehöriger Satz                        |
| `Buchungskonto` | Buchungskonto| ja      | Bebuchtes Konto                         |
| `SollHaben`     | SollHaben    | ja      | `Soll` oder `Haben`                     |
| `Betrag`        | decimal      | ja      | Betrag in Euro                          |

### OffenerPostenAusgleich

Verbindet eine offene Forderung (Soll-Zeile) mit ihrer ausgleichenden Zahlung (Haben-Zeile) auf demselben Buchungskonto (OPOS-Prinzip).

| Feld          | Typ           | Pflicht | Beschreibung                       |
|---------------|---------------|---------|------------------------------------|
| `…Id`         | Guid          | PK      | Primärschlüssel                    |
| `SollZeile`   | Buchungszeile | ja      | Die offene Forderung               |
| `HabenZeile`  | Buchungszeile | ja      | Die ausgleichende Zahlung          |

*Invariante:* `SollZeile.BuchungskontoId == HabenZeile.BuchungskontoId`. Offen ist eine Forderung, solange Σ(Ausgleiche) < `SollZeile.Betrag`.

### Transaktion

Ein importierter oder manuell erfasster Kontoauszugseintrag.

| Feld               | Typ        | Pflicht | Beschreibung                       |
|--------------------|------------|---------|------------------------------------|
| `TransaktionId`    | Guid       | PK      | Primärschlüssel                    |
| `Zahler`           | Bankkonto  | nein    | Zahlendes Bankkonto                |
| `Zahlungsempfaenger`| Bankkonto | nein    | Empfangendes Bankkonto             |
| `Zahlungsdatum`    | DateOnly   | ja      | Datum der Zahlung                  |
| `Betrag`           | decimal    | ja      | Betrag in Euro                     |
| `Verwendungszweck` | string     | ja      | Verwendungszweck                   |
| `Buchungssaetze`   | List       | —       | Zugeordnete Buchungssätze          |

### Bankkonto

| Feld            | Typ           | Pflicht | Beschreibung                       |
|-----------------|---------------|---------|------------------------------------|
| `BankkontoId`   | int           | PK      | Primärschlüssel                    |
| `Bank`          | string        | nein    | Name der Bank                      |
| `Iban`          | string        | nein    | IBAN                               |
| `Besitzer`      | List\<Kontakt\>| —      | Kontoinhaber                       |
| `BuchungsKonto` | Buchungskonto | ja      | Zugehöriges Buchungskonto          |

> Bei der Erfassung ist mindestens **IBAN oder Bank** erforderlich.

---

## Stamm- und Fachentitäten

### Adresse

| Feld           | Typ    | Pflicht | Beschreibung |
|----------------|--------|---------|--------------|
| `AdresseId`    | int    | PK      | Primärschlüssel |
| `Strasse`      | string | ja      | Straßenname |
| `Hausnummer`   | string | ja      | Hausnummer |
| `Postleitzahl` | string | ja      | Postleitzahl |
| `Stadt`        | string | ja      | Stadt |
| `Notiz`        | string | nein    | |

Berechnetes Feld: `Anschrift` → `"Strasse Hausnummer, PLZ Stadt"`.

### Kontakt

Natürliche oder juristische Person (Vermieter, Mieter, Ansprechpartner, Handwerker …).

| Feld                   | Typ           | Pflicht | Beschreibung |
|------------------------|---------------|---------|--------------|
| `KontaktId`            | int           | PK      | Primärschlüssel |
| `Name`                 | string        | ja      | Nachname oder Firmenname |
| `Rechtsform`           | Rechtsform    | ja      | siehe unten |
| `Vorname`              | string        | nein    | nur natürliche Personen |
| `Anrede`               | Anrede        | nein    | `Herr`, `Frau`, `Keine` |
| `Telefon`/`Mobil`/`Fax`/`Email` | string | nein  | Kontaktdaten |
| `Adresse`              | Adresse       | nein    | Postanschrift |
| `VerbindlichkeitsKonto`| Buchungskonto | nein    | Konto für Verbindlichkeiten ggü. diesem Kontakt (z. B. Handwerker); wird bei Bedarf automatisch angelegt |
| `Notiz`                | string        | nein    | |

Berechnetes Feld: `Bezeichnung` → `"Vorname Name"` bzw. `"Name"`.

Beziehungen: `Mietvertraege`, `VerwaltetVertraege`, `EigentuemerIn` (WohnungEigentuemer), `Garagen`, `Accounts`, `AlsMitglied`/`AlsJuristischePerson` (KontaktMitgliedschaft).

**Rechtsformen:** `natuerlich`, `gmbh`, `gbr`, `ag`, `ev`, `kg`, `ohg`, `ug`, `stiftung`, `verein`, `genossenschaft`, `sonstige`.

### KontaktMitgliedschaft

Verknüpft einen Kontakt (`Mitglied`) mit einer juristischen Person (`JuristischePerson`) — z. B. Gesellschafter ↔ GmbH.

### Wohnung

Die Wohneinheit. Veränderliche Größen liegen in `WohnungVersion`, die Eigentümer in `WohnungEigentuemer`.

| Feld               | Typ           | Pflicht | Beschreibung |
|--------------------|---------------|---------|--------------|
| `WohnungId`        | int           | PK      | Primärschlüssel |
| `Bezeichnung`      | string        | ja      | z. B. „OG links“ |
| `MietErtragskonto` | Buchungskonto | ja      | Ertragskonto für Mieten dieser Wohnung |
| `AufwandsKonto`    | Buchungskonto | ja      | Aufwandskonto (Erhaltungsaufwendungen, Leerstand …) |
| `Adresse`          | Adresse       | nein    | Lage der Wohnung |
| `Notiz`            | string        | nein    | |

Beziehungen: `Versionen`, `Eigentuemer`, `Vertraege`, `Zaehler`, `Umlagen`, `Verwalter`.

### WohnungVersion

Zeitlich gültige Flächen-/Anteilsangaben einer Wohnung.

| Feld                  | Typ      | Pflicht | Beschreibung |
|-----------------------|----------|---------|--------------|
| `WohnungVersionId`    | int      | PK      | Primärschlüssel |
| `Wohnung`             | Wohnung  | ja      | Zugehörige Wohnung |
| `Beginn`              | DateOnly | ja      | Gültig ab |
| `Wohnflaeche`         | decimal  | ja      | Wohnfläche in m² |
| `Nutzflaeche`         | decimal  | ja      | Nutzfläche in m² |
| `Miteigentumsanteile` | decimal  | ja      | MEA-Anteil (WEG) |
| `Nutzeinheit`         | int      | ja      | Anzahl Nutzeinheiten |

### WohnungEigentuemer

Zeitlich begrenzte Eigentümer-Zuordnung (Wohnung ↔ Kontakt) mit `Beginn`/`Ende`.

### Vertrag

Ein Mietverhältnis für eine Wohnung. Hält mehrere fachliche Buchungskonten.

| Feld                 | Typ           | Pflicht | Beschreibung |
|----------------------|---------------|---------|--------------|
| `VertragId`          | int           | PK      | Primärschlüssel |
| `Wohnung`            | Wohnung       | ja      | Zugehörige Wohnung |
| `Ansprechpartner`    | Kontakt       | nein    | Ansprechpartner |
| `Ende`               | DateOnly      | nein    | Vertragsende (null = laufend) |
| `MietBuchungskonto`  | Buchungskonto | ja      | Forderungskonto Kaltmiete |
| `NkBuchungskonto`    | Buchungskonto | ja      | Forderungskonto Nebenkostenvorauszahlung |
| `BkAbrechnungsKonto` | Buchungskonto | ja      | Konto für das Abrechnungsergebnis |
| `ZahlungsKonto`      | Buchungskonto | ja      | Zahlungseingangskonto |
| `MietminderungsKonto`| Buchungskonto | ja      | Konto für Mietminderungen |
| `KautionBetrag`      | decimal       | nein    | Kautionshöhe |
| `KautionEingangsdatum` / `KautionRueckgabedatum` | DateOnly | nein | Kautionsdaten |
| `KautionArt`         | string        | nein    | Art der Kaution |
| `Notiz`              | string        | nein    | |

Beziehungen: `Versionen`, `Mietminderungen`, `GarageVertraege`, `Mieter` (List\<Kontakt\>), `Abrechnungsresultate`.

### VertragVersion

| Feld              | Typ      | Pflicht | Beschreibung |
|-------------------|----------|---------|--------------|
| `VertragVersionId`| int      | PK      | Primärschlüssel |
| `Vertrag`         | Vertrag  | ja      | Zugehöriger Vertrag |
| `Beginn`          | DateOnly | ja      | Gültig ab |
| `Grundmiete`      | double   | ja      | Kaltmiete in Euro |
| `Personenzahl`    | int      | ja      | Personen im Haushalt |

### Mietminderung

| Feld              | Typ      | Pflicht | Beschreibung |
|-------------------|----------|---------|--------------|
| `MietminderungId` | int      | PK      | Primärschlüssel |
| `Vertrag`         | Vertrag  | ja      | Zugehöriger Vertrag |
| `Beginn`          | DateOnly | ja      | Beginn der Minderung |
| `Ende`            | DateOnly | nein    | Ende (null = unbegrenzt) |
| `Minderung`       | double   | ja      | Quote als Dezimalzahl (0.1 = 10 %) |

### Umlagetyp

Art einer Betriebskostenposition (z. B. „Heizkosten“, „Wasser“). Felder: `UmlagetypId`, `Bezeichnung`, `Notiz`.

### Umlage

Verbindet einen Umlagetyp mit einer Gruppe von Wohnungen. Der Verteilungsschlüssel liegt zeitlich in `UmlageVersion`.

| Feld                  | Typ           | Pflicht | Beschreibung |
|-----------------------|---------------|---------|--------------|
| `UmlageId`            | int           | PK      | Primärschlüssel |
| `Typ`                 | Umlagetyp     | ja      | Art der Kosten |
| `NkVerrechnungsKonto` | Buchungskonto | ja      | Verrechnungskonto der Nebenkosten |
| `ZahlungsKonto`       | Buchungskonto | ja      | Zahlungskonto |
| `Ende`                | DateOnly      | nein    | Enddatum der Umlage |
| `Beschreibung`/`Notiz`| string        | nein    | |

Beziehungen: `Versionen` (UmlageVersion), `Wohnungen` (n:m), `Zaehler`, `HeizkostenHKVOs`, `BetriebsstromHKVOs`.

### UmlageVersion

| Feld             | Typ              | Pflicht | Beschreibung |
|------------------|------------------|---------|--------------|
| `UmlageVersionId`| int              | PK      | Primärschlüssel |
| `Umlage`         | Umlage           | ja      | Zugehörige Umlage |
| `Beginn`         | DateOnly         | ja      | Gültig ab |
| `Schluessel`     | Umlageschluessel | ja      | Verteilungsschlüssel |

**Umlageschlüssel:** `NachWohnflaeche` (n. WF), `NachNutzflaeche` (n. NF), `NachNutzeinheit` (n. NE), `NachPersonenzahl` (n. Pers.), `NachVerbrauch` (n. Verb.), `NachMiteigentumsanteil` (n. MEA).

### HKVO

Konfiguration nach Heizkostenverordnung für eine warme Umlage.

| Feld            | Typ        | Pflicht | Beschreibung |
|-----------------|------------|---------|--------------|
| `HKVOId`        | int        | PK      | Primärschlüssel |
| `HKVO_P7`       | double     | ja      | Verbrauchsanteil Heizwärme (§ 7) |
| `HKVO_P8`       | double     | ja      | Verbrauchsanteil Warmwasser (§ 8) |
| `HKVO_P9`       | HKVO_P9A2  | ja      | Berechnungssatz § 9 Abs. 2 (`Satz_1`, `Satz_2`, `Satz_4`) |
| `Strompauschale`| double     | ja      | Anteil Betriebsstrom an den Heizkosten |
| `Heizkosten`    | Umlage     | ja      | Heizkosten-Umlage |
| `Betriebsstrom` | Umlage     | nein    | Betriebsstrom-Umlage |

### Zaehler

| Feld         | Typ        | Pflicht | Beschreibung |
|--------------|------------|---------|--------------|
| `ZaehlerId`  | int        | PK      | Primärschlüssel |
| `Kennnummer` | string     | ja      | Zählernummer |
| `Typ`        | Zaehlertyp | ja      | `Warmwasser`, `Kaltwasser`, `Strom`, `Gas` |
| `Wohnung`    | Wohnung    | nein    | null = Allgemeinzähler |
| `Adresse`    | Adresse    | nein    | Standort |
| `Ende`       | DateOnly   | nein    | Außerbetriebnahme |

Beziehungen: `Staende` (Zaehlerstand), `Umlagen`.

### Zaehlerstand

`ZaehlerstandId`, `Zaehler`, `Datum` (DateOnly), `Stand` (double), `Notiz`.

### Garage / GarageVertrag / GarageVertragVersion

- **Garage**: `GarageId`, `Kennung`, `Adresse`, `GarageVertraege`.
- **GarageVertrag**: `GarageVertragId`, `Garage`, `Mieter` (Kontakt), `Versionen`.
- **GarageVertragVersion**: `Beginn` (DateOnly), `Betrag` (decimal) — zeitlich gültige Miete des Stellplatzes.

### Abrechnungsresultat

Das gespeicherte Ergebnis einer Betriebskostenabrechnung. Die Beträge werden aus dem verknüpften `Buchungssatz` abgeleitet, nicht mehr redundant gespeichert.

| Feld                    | Typ          | Pflicht | Beschreibung |
|-------------------------|--------------|---------|--------------|
| `AbrechnungsresultatId` | Guid         | PK      | Primärschlüssel |
| `Vertrag`               | Vertrag      | ja      | Zugehöriger Vertrag |
| `Buchungssatz`          | Buchungssatz | ja      | Buchung des Abrechnungsergebnisses |
| `Abgesendet`            | bool         | nein    | Ob das Dokument versandt wurde |
| `Notiz`                 | string       | nein    | |

### Verwalter

Zugriffs-/Rollenzuordnung eines `UserAccount` zu einer Wohnung.

| Feld         | Typ           | Pflicht | Beschreibung |
|--------------|---------------|---------|--------------|
| `VerwalterId`| int           | PK      | Primärschlüssel |
| `UserAccount`| UserAccount   | ja      | Benutzer |
| `Wohnung`    | Wohnung       | nein    | Betroffene Wohnung |
| `Rolle`      | VerwalterRolle| ja      | `Eigentuemer`, `Vollmacht`, `Ableser` |

---

## Authentifizierung (`Auth/`)

- **UserAccount**: `Id` (Guid), `Username`, `IsAdmin`, optional verknüpfter `Kontakt`, `Verwalter`-Zuordnungen.
- **Pbkdf2PasswordCredential**: Passwort-Hash (PBKDF2) eines Benutzers.
- **UserResetCredential**: Token zum Zurücksetzen des Passworts.

---

## Gemeinsame Felder aller Entitäten

| Feld           | Typ      | Beschreibung |
|----------------|----------|--------------|
| `CreatedAt`    | DateTime | Erstellungszeitpunkt (UTC, read-only) |
| `LastModified` | DateTime | Letzter Änderungszeitpunkt (UTC) |
