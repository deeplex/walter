using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Deeplex.Saverwalter.Model.Betriebskostenabrechnung;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Paragraph EqHeizkostenV9_2(Betriebskostenabrechnung b, Rechnungsgruppe gruppe)
        {
            RunProperties rp() => new RunProperties(new RunFonts() { Ascii = "Cambria Math", HighAnsi = "Cambria Math" });
            DocumentFormat.OpenXml.Math.Run t(string str) => new DocumentFormat.OpenXml.Math.Run(rp(), new DocumentFormat.OpenXml.Math.Text(str));
            Run r(string str) => new Run(Font(), new Text(str) { Space = SpaceProcessingModeValues.Preserve });

            DocumentFormat.OpenXml.Math.OfficeMath om(string str) => new DocumentFormat.OpenXml.Math.OfficeMath(t(str));
            DocumentFormat.OpenXml.Math.OfficeMath omtw() => new DocumentFormat.OpenXml.Math.OfficeMath(tw());

            DocumentFormat.OpenXml.Math.ParagraphProperties justifyLeft
                () => new DocumentFormat.OpenXml.Math.ParagraphProperties(
                    new DocumentFormat.OpenXml.Math.Justification()
                    { Val = DocumentFormat.OpenXml.Math.JustificationValues.Left });

            DocumentFormat.OpenXml.Math.Base tw()
                => new DocumentFormat.OpenXml.Math.Base(
                        new DocumentFormat.OpenXml.Math.Subscript(
                        new DocumentFormat.OpenXml.Math.SubscriptProperties(
                        new DocumentFormat.OpenXml.Math.ControlProperties(rp())),
                        new DocumentFormat.OpenXml.Math.Base(t("t")),
                        new DocumentFormat.OpenXml.Math.SubArgument(t("w"))));

            DocumentFormat.OpenXml.Math.Fraction frac(string num, string den)
                => new DocumentFormat.OpenXml.Math.Fraction(new DocumentFormat.OpenXml.Math.FractionProperties(
                    new DocumentFormat.OpenXml.Math.ControlProperties(rp())),
                    new DocumentFormat.OpenXml.Math.Numerator(t(num)),
                    new DocumentFormat.OpenXml.Math.Denominator(t(den)));

            DocumentFormat.OpenXml.Math.Fraction units() => frac("kWh", "m³ x K");

            var p = new Paragraph(Font(), new Run(Font(), new Break(), new Text("Davon der Warmwasseranteil nach HeizkostenV §9(2):"), new Break(), new Break()));

            foreach (var hk in gruppe.Heizkosten)
            {
                p.Append(new DocumentFormat.OpenXml.Math.Paragraph(justifyLeft(),
                    new DocumentFormat.OpenXml.Math.OfficeMath(
                    t("2,5 ×"), frac("V", "Q"), t(" × ("), tw(), t("-10°C)"), units(), t(" ⟹ "),
                    t("2,5 ×"), frac(Unit(hk.V, "m³"), Unit(hk.Q, "kWh")), t(" × ("), t(Celsius((int)hk.tw)), t("-10°C)"), units(), t(" = "), t(Prozent(hk.Para9_2)))));
            }
            p.Append(new Break(), new Break(),
                new Run(Font(), r("Wobei "), om("V"), r(" die Menge des Warmwassers, die im Abrechnungszeitraum gemessen wurde, "),
                om("Q"), r(" die gemessene Wärmemenge am Allgemeinzähler und "), omtw(), r("die geschätzte mittlere Temperatur des Warmwassers darstellt.")));
            return p;
        }
    }
}
