using System;
using System.Linq;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Dummies.CreateDummy();
            //using (var db = new SaverwalterContext())
            //{
            //    var adressen = db.Adressen
            //        .Where(b => b.Hausnummer == "7")
            //        .OrderBy(b => b.Hausnummer)
            //        .ToList();
            //}
            Console.WriteLine("Hello World!");
        }
    }
}
