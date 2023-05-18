﻿using Deeplex.Saverwalter.BetriebskostenabrechnungService;

namespace Deeplex.Saverwalter.PrintService
{
    public interface IPrint<T>
    {
        public T body { get; }

        public void Table(int[] widths, int[] justification, bool[] bold, bool[] underlined, string[][] cols);
        public void Text(string s);
        public void PageBreak();
        public void Break();
        public void EqHeizkostenV9_2(IAbrechnungseinheit gruppe);
        public void Heading(string str);
        public void SubHeading(string str);
        public void Paragraph(params PrintRun[] runs);
    }
}
