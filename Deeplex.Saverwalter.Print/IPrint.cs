using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print
{
    public interface IPrint<T>
    {
        public T body { get; }

        public void Table(int[] widths, int[] justification, bool[] bold, bool[] underlined, string[][] cols);
        public void Text(string s);
        public void PageBreak();
        public void Break();
        public void EqHeizkostenV9_2(Rechnungsgruppe gruppe);
        public void Heading(string str);
        public void SubHeading(string str);
        public void Paragraph(params PrintRun[] runs);
    }
}
