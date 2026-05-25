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

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/betriebskostenrechnungen")]
    public class BetriebskostenrechnungController : FileControllerBase<BetriebskostenrechnungEntry, int, Betriebskostenrechnung>
    {
        public class BuchungszeileInfo
        {
            public string Konto { get; set; } = "";
            public string SollHaben { get; set; } = "";
            public decimal Betrag { get; set; }
        }

        public class BetriebskostenrechnungEntryBase
        {
            public int Id { get; set; }
            public decimal Betrag { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateOnly Datum { get; set; }
            public SelectionEntry? Typ { get; set; }
            public SelectionEntry? Umlage { get; set; }
            public bool IsBalanced { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public BetriebskostenrechnungEntryBase() { }
            public BetriebskostenrechnungEntryBase(Betriebskostenrechnung entity, Permissions permissions)
            {
                Id = entity.BetriebskostenrechnungId;

                Betrag = entity.Betrag;
                BetreffendesJahr = entity.BetreffendesJahr;
                Datum = entity.Datum;
                Typ = new SelectionEntry(entity.Umlage.Typ.UmlagetypId, entity.Umlage.Typ.Bezeichnung);

                Umlage = new SelectionEntry(
                    entity.Umlage.UmlageId,
                    entity.Umlage.GetWohnungenBezeichnung(),
                    entity.Umlage.Typ.UmlagetypId.ToString());

                Permissions = permissions;

                if (entity.Buchungssatz is { } satz)
                {
                    var soll = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag);
                    var haben = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag);
                    IsBalanced = Math.Abs(soll - haben) <= 0.005m;
                }
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public List<BuchungszeileInfo> Buchungszeilen { get; set; } = [];

            private Betriebskostenrechnung? Entity { get; }

            public IEnumerable<BetriebskostenrechnungEntryBase> Betriebskostenrechnungen { get; set; } = [];
            public IEnumerable<WohnungEntryBase>? Wohnungen { get; set; } = [];

            public BetriebskostenrechnungEntry() : base() { }
            public BetriebskostenrechnungEntry(Betriebskostenrechnung entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;

                Entity = entity;

                if (entity.Buchungssatz is { } satz)
                {
                    Buchungszeilen = satz.Buchungszeilen.Select(z => new BuchungszeileInfo
                    {
                        Konto = z.Buchungskonto.Bezeichnung,
                        SollHaben = z.SollHaben == SollHaben.Soll ? "Soll" : "Haben",
                        Betrag = z.Betrag
                    }).ToList();
                }
            }
        }

        protected override BetriebskostenrechnungDbService DbService { get; }

        public BetriebskostenrechnungController(
            ILogger<BetriebskostenrechnungController> logger,
            BetriebskostenrechnungDbService dbService,
            HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
        }

        [HttpGet]
        public Task<PagedResult<BetriebskostenrechnungEntryBase>> Get([FromQuery] PagedQuery query)
            => DbService.GetList(User!, query);

        [HttpGet("{id}")]
        public Task<ActionResult<BetriebskostenrechnungEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
