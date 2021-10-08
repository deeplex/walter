using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print.ThirdPage
{
    public static partial class ThirdPage
    {
        private static Paragraph Abrechnungsgruppe(Betriebskostenabrechnung b, Rechnungsgruppe g)
        {
            // TODO move content...
            var p = new Paragraph(Font());
            p.Append(new Run(Font(), new Text(g.Bezeichnung)));
            return p;
        }
    }
}
