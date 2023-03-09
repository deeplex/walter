using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class BetriebskostenabrechnungSerivce
    {
        public IActionResult Get(int id, int Jahr, SaverwalterContext ctx)
        {
            try
            {
                var vertrag = ctx.Vertraege.Find(id);
                var beginn = new DateTime(Jahr, 1, 1);
                var ende = new DateTime(Jahr, 12, 31);
                var Betriebskostenabrechnung = new Betriebskostenabrechnung(ctx, vertrag, Jahr, beginn, ende);

                var stream = new MemoryStream();

                StreamWriter writer = new StreamWriter(stream);

                Betriebskostenabrechnung.SaveAsDocx(stream);
                stream.Position = 0;

                return new OkObjectResult(stream);

            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IWalterDbService DbService { get; }

        public BetriebskostenabrechnungSerivce(IWalterDbService dbService)
        {
            DbService = dbService;
        }
    }
}
