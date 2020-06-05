using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
using System;

namespace Deeplex.Saverwalter.CLI

{
    sealed class Program
    {
        private static void Main()
        {
            //Dummies.CreateDummy();
            var b = new Betriebskostenabrechnung(12, 2018, new DateTime(2018, 1, 1), new DateTime(2018, 12, 31));
            b.SaveAsDocx("walter.docx");
        }
    }
}
