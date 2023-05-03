namespace Deeplex.Saverwalter.PrintService
{
    public class PrintRun
    {
        public string Text;
        public bool Bold = false;
        public bool Underlined = false;
        public bool NoBreak = false;
        public bool Tab = false;
        public PrintRun(string text)
        {
            Text = text;
        }
    }
}
