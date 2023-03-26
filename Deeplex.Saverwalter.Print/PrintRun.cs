namespace Deeplex.Saverwalter.PrintService
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
