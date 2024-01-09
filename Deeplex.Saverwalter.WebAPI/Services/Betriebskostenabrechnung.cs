// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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

        public ActionResult<BetriebskostenabrechnungEntry> Get(int vertrag_id, int Jahr)
        {

            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var abrechnung = CreateAbrechnung(vertrag, Jahr);
                var controller = new BetriebskostenabrechnungEntry(abrechnung);

                return controller;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public ActionResult<MemoryStream> GetWordDocument(int vertrag_id, int Jahr)
        {
            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var stream = new MemoryStream();
                var abrechnung = CreateAbrechnung(vertrag, Jahr);
                abrechnung.SaveAsDocx(stream);
                stream.Position = 0;

                return stream;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public ActionResult<MemoryStream> GetPdfDocument(int vertrag_id, int Jahr)
        {
            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var stream = new MemoryStream();
                var abrechnung = CreateAbrechnung(vertrag, Jahr);
                abrechnung.SaveAsPdf(stream);
                stream.Position = 0;

                return stream;
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
