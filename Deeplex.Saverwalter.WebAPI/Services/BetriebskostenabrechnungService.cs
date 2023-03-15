﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.Print;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Utils.BetriebskostenabrechnungController;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class BetriebskostenabrechnungSerivce
    {
        private Betriebskostenabrechnung createAbrechnung(int id, int Jahr, SaverwalterContext ctx)
        {
            var vertrag = ctx.Vertraege.Find(id);
            var beginn = new DateTime(Jahr, 1, 1);
            var ende = new DateTime(Jahr, 12, 31);

            return new Betriebskostenabrechnung(ctx, vertrag, Jahr, beginn, ende);
        }

        public IActionResult Get(int id, int Jahr, IWalterDbService dbService)
        {
            try
            {
                var controller = new BetriebskostenabrechnungEntry(
                    createAbrechnung(id, Jahr, dbService.ctx),
                    dbService);

                return new OkObjectResult(controller);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        // TODO not used!
        public IActionResult GetDocument(int id, int Jahr, SaverwalterContext ctx)
        {
            try
            {
                var stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                createAbrechnung(id, Jahr, ctx).SaveAsDocx(stream);
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
