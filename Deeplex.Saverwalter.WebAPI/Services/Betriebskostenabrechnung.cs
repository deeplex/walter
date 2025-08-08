// Copyright (c) 2023-2025 Kai Lawrence
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
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Utils.BetriebskostenabrechnungController;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class BetriebskostenabrechnungHandler
    {
        private static Betriebskostenabrechnung CreateAbrechnung(Vertrag vertrag, int jahr)
        {
            var beginn = new DateOnly(jahr, 1, 1);
            var ende = new DateOnly(jahr, 12, 31);

            return new Betriebskostenabrechnung(vertrag, jahr, beginn, ende);
        }

        private async Task SaveAbrechnungsresultat(Betriebskostenabrechnung abrechnung)
        {
            var resultate = Ctx.Abrechnungsresultate.Where(e =>
                e.Vertrag == abrechnung.Vertrag &&
                e.Jahr == abrechnung.Zeitraum.Jahr);
            Ctx.Abrechnungsresultate.RemoveRange(resultate);

            var resultat = new Abrechnungsresultat
            {
                Vertrag = abrechnung.Vertrag,
                Jahr = abrechnung.Zeitraum.Jahr,
                Kaltmiete = abrechnung.KaltMiete,
                Vorauszahlung = abrechnung.GezahlteMiete,
                Rechnungsbetrag = abrechnung.BetragNebenkosten,
                Minderung = abrechnung.Mietminderung,
                IstBeglichen = false,
            };

            Ctx.Abrechnungsresultate.Add(resultat);
            await Ctx.SaveChangesAsync();
        }

        public async Task<ActionResult<BetriebskostenabrechnungEntry>> Get(int vertrag_id, int jahr)
        {

            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var abrechnung = CreateAbrechnung(vertrag, jahr);
                var resultat = await Ctx.Abrechnungsresultate
                    .FirstOrDefaultAsync(e => e.Vertrag == vertrag && e.Jahr == jahr);
                var controller = new BetriebskostenabrechnungEntry(abrechnung, resultat);

                return controller;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<ActionResult<MemoryStream>> GetWordDocument(int vertrag_id, int jahr)
        {
            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var stream = new MemoryStream();
                var abrechnung = CreateAbrechnung(vertrag, jahr);
                await SaveAbrechnungsresultat(abrechnung);
                abrechnung.SaveAsDocx(stream);
                stream.Position = 0;

                return stream;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<ActionResult<MemoryStream>> GetPdfDocument(int vertrag_id, int Jahr)
        {
            try
            {
                var vertrag = Ctx.Vertraege.Find(vertrag_id)!;
                var stream = new MemoryStream();
                var abrechnung = CreateAbrechnung(vertrag, Jahr);
                await SaveAbrechnungsresultat(abrechnung);
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
