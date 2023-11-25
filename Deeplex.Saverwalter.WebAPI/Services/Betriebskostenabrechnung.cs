using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.PrintService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Utils.BetriebskostenabrechnungController;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class BetriebskostenabrechnungHandler
    {
        private static Betriebskostenabrechnung CreateAbrechnung(Vertrag vertrag, int Jahr)
        {
            var beginn = new DateOnly(Jahr, 1, 1);
            var ende = new DateOnly(Jahr, 12, 31);

            return new Betriebskostenabrechnung(vertrag, Jahr, beginn, ende);
        }

        public IActionResult Get(int vertrag_id, int Jahr)
        {

            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var abrechnung = CreateAbrechnung(vertrag, Jahr);
                var controller = new BetriebskostenabrechnungEntry(abrechnung);

                return new OkObjectResult(controller);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult GetWordDocument(int vertrag_id, int Jahr)
        {
            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var stream = new MemoryStream();
                var abrechnung = CreateAbrechnung(vertrag, Jahr);
                abrechnung.SaveAsDocx(stream);
                stream.Position = 0;

                return new OkObjectResult(stream);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult GetPdfDocument(int vertrag_id, int Jahr)
        {
            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var stream = new MemoryStream();
                var abrechnung = CreateAbrechnung(vertrag, Jahr);
                abrechnung.SaveAsPdf(stream);
                stream.Position = 0;

                return new OkObjectResult(stream);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        public SaverwalterContext Ctx { get; }

        public BetriebskostenabrechnungHandler(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }
    }
}
