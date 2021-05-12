﻿using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.SecondPage
{
    public static partial class SecondPage
    {
        private static Table ExplainUmlageschluessel(Betriebskostenabrechnung b)
        {
            var dir = b.Gruppen.Any(g => g.Rechnungen.Any(r => r.Gruppen.Count == 1));
            var nWF = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachWohnflaeche));
            var nNF = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachNutzflaeche));
            var nNE = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachNutzeinheit));
            var nPZ = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl));
            var nVb = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => r.Schluessel == UmlageSchluessel.NachVerbrauch));
            // There is a Umlage nach Nutzfläche in the Heizkostenberechnung:
            if (!nNF)
            {
                nNF = b.Gruppen.Any(g => g.Rechnungen.Where(r => r.Gruppen.Count > 1).Any(r => (int)r.Typ % 2 == 1));
            }

            var t = new Table(
                // This cell defines the width of the left cell.
                new TableRow(
                    ContentHead("1250", "Umlageschlüssel"),
                    ContentHead("Bedeutung")));

            if (dir == true)
            {
                t.Append(new TableRow(
                    ContentCell("Direkt"),
                    ContentCell("Direkte Zuordnung")));
            }
            if (nWF == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. WF."),
                    ContentCell("nach Wohnfläche in m²")));
            }
            if (nNF == true)
            {
                t.Append(new TableRow( // This row has SpacingBeetweenLine.
                    ContentCell("n. NF."),
                    ContentCell("nach Nutzfläche in m²")));
            }
            if (nNE == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. NE."),
                    ContentCell("nach Anzahl der Wohn-/Nutzeinheiten")));
            }
            if (nPZ == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. Pers."),
                    ContentCell("nach Personenzahl/Anzahl der Bewohner")));
            }
            if (nVb == true)
            {
                t.Append(new TableRow( // This row has SpacingBeetweenLine.
                    ContentCell("n. Verb."),
                    ContentCell("nach Verbrauch (in m³ oder in kWh)")));
            }

            t.Append(new TableRow(ContentCell(""), ContentCell("")),
                new TableRow(
                    new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text("Umlageweg")))),
                    new TableCell(new Paragraph(Font(), NoSpace(), new Run(Font(), Bold(), new Text("Beschreibung"))))));

            if (dir == true)
            {
                t.Append(new TableRow(
                    ContentCell("Direkt"),
                    ContentCell("Kostenanteil = Kosten werden Einheit direkt zugeordnet.")));
            }
            if (nWF == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. WF."),
                    ContentCell("Kostenanteil = Kosten je Quadratmeter Wohnfläche mal Anteil Fläche je Wohnung.")));
            }
            if (nNF == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. WF."),
                    ContentCell("Kostenanteil = Kosten je Quadratmeter Nutzfläche mal Anteil Fläche je Wohnung.")));
            }
            if (nNE == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. NE."),
                    ContentCell("Kostenanteil = Kosten je Wohn-/Nutzeinheit.")));
            }
            if (nPZ == true)
            {
                t.Append(new TableRow(
                    ContentCell("n. Pers."),
                    ContentCell("Kostenanteil = Kosten je Hausbewohner mal Anzahl Bewohner je Wohnung.")));
            }
            if (nVb)
            {
                t.Append(new TableRow( // This row has SpacingBeetweenLine.
                    ContentCellEnd("n. Verb."),
                    ContentCellEnd("Kostenanteil = Kosten je Verbrauchseinheit mal individuelle Verbrauchsmenge in Kubikmetern oder Kilowattstunden.")));
            }

            t.Append(new TableRow(ContentCell(""), ContentCell("")),
                    ContentCellEnd("Anmerkung: "),
                    ContentCellEnd("Bei einer Nutzungsdauer, die kürzer als der Abrechnungszeitraum ist, werden Ihre Einheiten als Rechnungsfaktor mit Hilfe des Promille - Verfahrens ermittelt; Kosten je Einheit mal Ihre Einheiten = (zeitanteiliger) Kostenanteil"));

            return t;
        }

    }
}