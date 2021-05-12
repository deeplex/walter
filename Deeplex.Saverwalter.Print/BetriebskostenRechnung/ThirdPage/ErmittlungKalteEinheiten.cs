using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Table ErmittlungKalteEinheiten(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            TableCell pContentCell(string str, JustificationValues value, BorderType border) =>
                new TableCell(new TableCellProperties(border),
                    new Paragraph(new ParagraphProperties(new Justification() { Val = value }),
                    Font(), NoSpace(), new Run(Font(), new Text(str))));

            BottomBorder bot() => new BottomBorder() { Val = BorderValues.Single, Size = 4 };
            TopBorder top() => new TopBorder() { Val = BorderValues.Single, Size = 4 };

            var table = new Table(new TableRow(
                ContentHead("2000", "Ermittlung Ihrer Einheiten"),
                ContentHead("1170", "Nutzungsintervall", JustificationValues.Center),
                ContentHead("1200", "Tage", JustificationValues.Center),
                ContentHead("630", "Ihr Anteil", JustificationValues.Center)));

            if (g.GesamtEinheiten == 1)
            {
                table.Append(
                    new TableRow(
                        pContentCell("Direkte Zuordnung", JustificationValues.Left, bot()),
                        pContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center, bot()),
                        pContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                        pContentCell(Prozent(g.WFZeitanteil), JustificationValues.Center, bot())));

                return table;
            }

            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachWohnflaeche))
            {
                table.Append(
                    new TableRow(ContentHead("bei Umlage nach Wohnfläche (n. WF)"), ContentHead(""), ContentHead(""), ContentHead("")),
                    new TableRow(
                        pContentCell(Quadrat(b.Wohnung.Wohnflaeche) + " / " + Quadrat(g.GesamtWohnflaeche), JustificationValues.Left, bot()),
                        pContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center, bot()),
                        pContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                        pContentCell(Prozent(g.WFZeitanteil), JustificationValues.Center, bot())));
            }
            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachNutzflaeche))
            {
                table.Append(
                    new TableRow(ContentHead("bei Umlage nach Nutzfläche (n. NF)"), ContentHead(""), ContentHead(""), ContentHead("")),
                    new TableRow(
                        pContentCell(Quadrat(b.Wohnung.Nutzflaeche) + " / " + Quadrat(g.GesamtNutzflaeche), JustificationValues.Left, bot()),
                        pContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center, bot()),
                        pContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                        pContentCell(Prozent(g.NFZeitanteil), JustificationValues.Center, bot())));
            }
            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachNutzeinheit))
            {
                table.Append(
                    new TableRow(ContentHead("bei Umlage nach Nutzeinheiten (n. NE)"), ContentHead(""), ContentHead(""), ContentHead("")),
                    new TableRow(
                        pContentCell(1.ToString() + " / " + g.GesamtEinheiten, JustificationValues.Left, bot()),
                        pContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Center, bot()),
                        pContentCell(b.Nutzungszeitspanne.ToString() + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                        pContentCell(Prozent(g.NEZeitanteil), JustificationValues.Center, bot())));
            }
            if (g.Rechnungen.Exists(r => r.Schluessel == UmlageSchluessel.NachPersonenzahl))
            {
                table.Append(new TableRow(
                    ContentHead("bei Umlage nach Personenzahl (n. Pers.)"), ContentHead(""), ContentHead(""), ContentHead("")));
                string PersonEn(int i) => i.ToString() + (i > 1 ? " Personen" : " Person");
                for (var i = 0; i < g.PersZeitanteil.Count; ++i)
                {
                    var Beginn = g.PersZeitanteil[i].Beginn;
                    var Ende = g.PersZeitanteil[i].Ende;
                    var GesamtPersonenzahl = g.GesamtPersonenIntervall.Last(gs => gs.Beginn.Date <= g.PersZeitanteil[i].Beginn.Date).Personenzahl;
                    var Personenzahl = g.PersonenIntervall.Last(p => p.Beginn.Date <= g.PersZeitanteil[i].Beginn).Personenzahl;
                    var timespan = ((Ende - Beginn).Days + 1).ToString();

                    if (i == g.PersZeitanteil.Count - 1)
                    {
                        table.Append(new TableRow(
                           pContentCell(PersonEn(Personenzahl) + " / " + PersonEn(GesamtPersonenzahl), JustificationValues.Left, bot()),
                           pContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center, bot()),
                           pContentCell(timespan + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center, bot()),
                           pContentCell(Prozent(g.PersZeitanteil[i].Anteil), JustificationValues.Center, bot())));
                    }
                    else
                    {
                        table.Append(new TableRow(
                            ContentCell(PersonEn(Personenzahl) + " / " + PersonEn(GesamtPersonenzahl)),
                            ContentCell(Datum(Beginn) + " - " + Datum(Ende), JustificationValues.Center),
                            ContentCell(timespan + " / " + b.Abrechnungszeitspanne.ToString(), JustificationValues.Center),
                            ContentCell(Prozent(g.PersZeitanteil[i].Anteil), JustificationValues.Center)));
                    }
                }
            }

            if (g.Verbrauch.Any())
            {
                table.Append(new TableRow(
                    ContentHead("bei Umlage nach Verbrauch (n. Verb.)"),
                    ContentHead(""), ContentHead("Zählernummer", JustificationValues.Center), ContentHead("")));
                foreach (var Verbrauch in g.Verbrauch.Where(v => (int)v.Key % 2 == 0)) // Kalte Betriebskosten are equal / warme are odd
                {
                    foreach (var Value in Verbrauch.Value)
                    {
                        var unit = Value.Typ.ToUnitString();
                        table.Append(new TableRow(
                            ContentCell(Unit(Value.Delta, unit) + " / " + Unit(Value.Delta / Value.Anteil, unit) + "\t(" + Value.Typ + ")"),
                            ContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende)),
                            ContentCell(Value.Kennnummer, JustificationValues.Center),
                            ContentCell(Verbrauch.Value.Count > 1 ? "" : Prozent(Value.Anteil), JustificationValues.Center)));
                    }
                    if (Verbrauch.Value.Count > 1)
                    {
                        var unit = Verbrauch.Value[0].Typ.ToUnitString();
                        table.Append(new TableRow(
                            pContentCell(Unit(Verbrauch.Value.Sum(v => v.Delta), unit) + " / " + Unit(Verbrauch.Value.Sum(v => v.Delta / v.Anteil), unit), JustificationValues.Left, top()),
                            pContentCell(Datum(b.Nutzungsbeginn) + " - " + Datum(b.Nutzungsende), JustificationValues.Left, top()),
                            pContentCell(Verbrauch.Key.ToDescriptionString(), JustificationValues.Center, top()),
                            pContentCell(Prozent(g.VerbrauchAnteil[Verbrauch.Key]), JustificationValues.Center, top())));
                    }
                }
            }

            return table;
        }
    }
}
