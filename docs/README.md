# Dokumentation — Walter

Walter ist eine Webanwendung zur Verwaltung von Wohnungen, Mietverhältnissen und
Betriebskostenabrechnungen (Nebenkostenabrechnungen). Alle Geldbewegungen werden über
eine doppelte Buchführung (Buchungssätze auf Buchungskonten) abgebildet.

## Inhalt

### Allgemein

| Dokument | Beschreibung |
|----------|-------------|
| [Architektur](architektur.md) | Technologie-Stack, Projektstruktur, Schichten, Buchführungs-Kern, Umgebungsvariablen |
| [Datenmodell](datenmodell.md) | ER-Diagramm und alle Datenbankentitäten mit Feldern, Typen und Beziehungen |
| [Betriebskostenabrechnung](betriebskostenabrechnung.md) | Schritt-für-Schritt-Erklärung der Abrechnungsberechnung, HKVO, API-Endpunkte |
| [Tests](testing.md) | Test-Ebenen, Entwicklungs-Nutzer/Rollen, Berechtigungsmatrix, Ausführung |

### Nach Zielgruppe

| Dokument | Zielgruppe | Beschreibung |
|----------|-----------|-------------|
| [Leitfaden für Buchhalter](buchhalter.md) | Buchhalter, Verwalter | Grundprinzip der Buchführung, Jahresablauf, Datenpflege, Checkliste, häufige Fragen |
| [Bedienungsleitfaden](bedienung.md) | Alle Anwender | Die Weboberfläche Seite für Seite: Navigation, Startseite, Transaktionen, Abrechnungslauf, Jahresabschluss |
