using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.PrintService;
using Deeplex.Saverwalter.WalterDbService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Utils.BetriebskostenabrechnungController;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class BetriebskostenabrechnungHandler
    {
        private IBetriebskostenabrechnung createAbrechnung(int vertragId, int Jahr, SaverwalterContext ctx)
        {
            var vertrag = ctx.Vertraege.Find(vertragId)!;
            var beginn = new DateOnly(Jahr, 1, 1);
            var ende = new DateOnly(Jahr, 12, 31);

            return new Betriebskostenabrechnung(ctx, vertrag, Jahr, beginn, ende);
        }

        public IActionResult Get(int id, int Jahr, WalterDb dbService)
        {
            try
            {
                var abrechnung = createAbrechnung(id, Jahr, dbService.ctx);
                var controller = new BetriebskostenabrechnungEntry(abrechnung, dbService);

                return new OkObjectResult(controller);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult GetWordDocument(int id, int Jahr, SaverwalterContext ctx)
        {

            try
            {
                var stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                var abrechnung = createAbrechnung(id, Jahr, ctx);
                abrechnung.SaveAsDocx(stream);
                stream.Position = 0;

                return new OkObjectResult(stream);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult GetPdfDocument(int id, int Jahr, SaverwalterContext ctx)
        {
            try
            {
                var stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                var abrechnung = createAbrechnung(id, Jahr, ctx);
                abrechnung.SaveAsPdf(stream);
                stream.Position = 0;

                return new OkObjectResult(stream);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        public WalterDb DbService { get; }

        public BetriebskostenabrechnungHandler(WalterDb dbService)
        {
            DbService = dbService;
        }
    }
}
