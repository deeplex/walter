using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Table ErmittlungWarmanteil(Betriebskostenabrechnung b, Rechnungsgruppe gruppe)
        {
            var table = new Table(
                new TableProperties(
                    new TableBorders(new InsideHorizontalBorder() { Val = BorderValues.Thick, Color = "888888" })),
                new TableRow(
                    ContentHead("1200", "Kostenanteil"),
                    ContentHead("650", "Schlüssel"),
                    ContentHead("450", "Betrag"),
                    ContentHead("700", "Auft. §9(2)", JustificationValues.Center), // TODO Make this variable
                    ContentHead("700", "Auft. §7, 8", JustificationValues.Center),
                    ContentHead("650", "Ihr Anteil", JustificationValues.Right),
                    ContentHead("650", "Ihre Kosten", JustificationValues.Right)));

            foreach (var hk in gruppe.Heizkosten)
            {
                table.Append(
                    new TableRow(
                        ContentCell("Heizung"),
                        ContentCell(UmlageSchluessel.NachNutzflaeche.ToDescriptionString()),
                        ContentCell(Euro(hk.PauschalBetrag)),
                        ContentCell(Prozent(1 - hk.Para9_2), JustificationValues.Center),
                        ContentCell(Prozent(1 - hk.Para7), JustificationValues.Center),
                        ContentCell(Prozent(hk.NFZeitanteil), JustificationValues.Right),
                        ContentCell(Euro(hk.WaermeAnteilNF), JustificationValues.Right)),
                    new TableRow(
                        ContentCell("Heizung"),
                        ContentCell(UmlageSchluessel.NachVerbrauch.ToDescriptionString()),
                        ContentCell(Euro(hk.PauschalBetrag)),
                        ContentCell(Prozent(1 - hk.Para9_2), JustificationValues.Center),
                        ContentCell(Prozent(hk.Para7), JustificationValues.Center),
                        ContentCell(Prozent(hk.HeizkostenVerbrauchAnteil), JustificationValues.Right),
                        ContentCell(Euro(hk.WaermeAnteilVerb), JustificationValues.Right)),
                    new TableRow(
                        ContentCell("Warmwasser"),
                        ContentCell(UmlageSchluessel.NachNutzflaeche.ToDescriptionString()),
                        ContentCell(Euro(hk.PauschalBetrag)),
                        ContentCell(Prozent(hk.Para9_2), JustificationValues.Center),
                        ContentCell(Prozent(hk.Para8), JustificationValues.Center),
                        ContentCell(Prozent(hk.NFZeitanteil), JustificationValues.Right),
                        ContentCell(Euro(hk.WarmwasserAnteilNF), JustificationValues.Right)),
                    new TableRow(
                        ContentCell("Warmwasser"),
                        ContentCell(UmlageSchluessel.NachVerbrauch.ToDescriptionString()),
                        ContentCell(Euro(hk.PauschalBetrag)),
                        ContentCell(Prozent(hk.Para9_2), JustificationValues.Center),
                        ContentCell(Prozent(hk.Para8), JustificationValues.Center),
                        ContentCell(Prozent(hk.WarmwasserVerbrauchAnteil), JustificationValues.Right),
                        ContentCell(Euro(hk.WarmwasserAnteilVerb), JustificationValues.Right)));
            }
            table.Append(new TableRow(
                ContentCell(""), ContentCell(""),
                ContentCell(""), ContentCell(""), ContentCell(""),
                ContentHead("Summe: ", JustificationValues.Center),
                ContentHead(Euro(gruppe.BetragWarm), JustificationValues.Right)));

            return table;
        }

    }
}
