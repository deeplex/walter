using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.FirstPage
{
    public static partial class FirstPage
    {
        private static Table AnschriftVermieter(Betriebskostenabrechnung b)
        {
            var AnsprechpartnerBezeichnung = b.Ansprechpartner is NatuerlichePerson a ? a.Vorname + " " + a.Nachname : ""; // TODO jur. Person

            var ap = b.Ansprechpartner;

            var left = new List<TableCell>();
            var right = new List<TableCell>();

            left.Add(ContentCell(b.Vermieter.Bezeichnung));
            if (b.Vermieter.Bezeichnung != AnsprechpartnerBezeichnung)
            {
                left.Add(ContentCell("℅ " + AnsprechpartnerBezeichnung));
            }
            left.Add(ContentCell(b.Ansprechpartner.Adresse!.Strasse + " " + b.Ansprechpartner.Adresse.Hausnummer));
            left.Add(ContentCell(b.Ansprechpartner.Adresse!.Postleitzahl + " " + b.Ansprechpartner.Adresse.Stadt));

            right.Add(ContentCell("", JustificationValues.Right));
            if (ap.Telefon != null && ap.Telefon != "")
            {
                right.Add(ContentCell("Tel.: " + ap.Telefon, JustificationValues.Right));
            }
            if (ap.Fax != null && ap.Fax != "")
            {
                right.Add(ContentCell("Fax: " + ap.Fax, JustificationValues.Right));
            }
            if (ap.Email != null && ap.Email != "")
            {
                right.Add(ContentCell("E-Mail: " + ap.Email, JustificationValues.Right));
            }

            // TODO Ansprechpartner HAS to have a Adresse...
            var table = new Table(new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct });

            for (var i = 0; i < Math.Max(left.Count, right.Count); i++)
            {
                var row = new TableRow();

                if (left.Count > i) row.Append(left[i]);
                if (right.Count > i) row.Append(right[i]);
                table.Append(row);
            }

            return table;
        }

    }
}
