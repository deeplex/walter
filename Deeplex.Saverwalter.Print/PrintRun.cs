using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deeplex.Saverwalter.Model.Utils;
using static Deeplex.Saverwalter.Print.Utils;

namespace Deeplex.Saverwalter.Print
{
    public class PrintRun
    {
        public string Text;
        public bool Bold;
        public bool Underlined;
        public bool NoBreak;
        public bool Tab;
        public PrintRun(string text, bool bold = false, bool underlined = false, bool noBreak = false, bool tab = false)
        {
            Text = text;
            Bold = bold;
            Underlined = underlined;
            NoBreak = noBreak;
            Tab = tab;
        }
    }
}
