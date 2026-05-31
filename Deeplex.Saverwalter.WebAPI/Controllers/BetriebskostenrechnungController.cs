// Copyright (c) 2023-2026 Kai Lawrence
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
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/betriebskostenrechnungen")]
    public class BetriebskostenrechnungController(
        BetriebskostenrechnungDbService dbService) : ControllerBase
    {
        public class BuchungszeileInfo
        {
            public string Konto { get; set; } = "";
            public string SollHaben { get; set; } = "";
            public decimal Betrag { get; set; }
        }

        public class BetriebskostenrechnungEntryBase
        {
            public Guid Id { get; set; }
            public decimal Betrag { get; set; }
            public decimal Verteilt { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateOnly Datum { get; set; }
            public SelectionEntry? Typ { get; set; }
            public SelectionEntry? Umlage { get; set; }
            public bool IsBalanced { get; set; }
            public bool IsBezahlt { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public BetriebskostenrechnungEntryBase() { }
            public BetriebskostenrechnungEntryBase(Buchungssatz satz, Deeplex.Saverwalter.Model.Umlage umlage, Permissions permissions)
            {
                Id = satz.BuchungssatzId;
                BetreffendesJahr = satz.Buchungsjahr;
                Datum = satz.Buchungsdatum;
                Betrag = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag);
                Verteilt = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag);
                Typ = new SelectionEntry(umlage.Typ.UmlagetypId, umlage.Typ.Bezeichnung);
                Umlage = new SelectionEntry(
                    umlage.UmlageId,
                    umlage.GetWohnungenBezeichnung(),
                    umlage.Typ.UmlagetypId.ToString());
                Permissions = permissions;
                IsBalanced = Math.Abs(Verteilt - Betrag) <= 0.005m;
                var habenZeile = satz.Buchungszeilen.FirstOrDefault(z => z.SollHaben == SollHaben.Haben);
                var bezahlt = habenZeile?.AlsHabenZeile.Sum(a => a.SollZeile.Betrag) ?? 0;
                IsBezahlt = Betrag > 0 && bezahlt >= Betrag - 0.005m;
            }
        }

        public class BetriebskostenrechnungEntry : BetriebskostenrechnungEntryBase
        {
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public List<BuchungszeileInfo> Buchungszeilen { get; set; } = [];
            public IEnumerable<BetriebskostenrechnungEntryBase> Betriebskostenrechnungen { get; set; } = [];
            public IEnumerable<WohnungEntryBase>? Wohnungen { get; set; } = [];

            public BetriebskostenrechnungEntry() : base() { }
            public BetriebskostenrechnungEntry(Buchungssatz satz, Deeplex.Saverwalter.Model.Umlage umlage, Permissions permissions)
                : base(satz, umlage, permissions)
            {
                Notiz = satz.Notiz;
                CreatedAt = satz.CreatedAt;
                LastModified = satz.LastModified;
                Buchungszeilen = satz.Buchungszeilen.Select(z => new BuchungszeileInfo
                {
                    Konto = z.Buchungskonto.Bezeichnung,
                    SollHaben = z.SollHaben == SollHaben.Soll ? "Soll" : "Haben",
                    Betrag = z.Betrag
                }).ToList();
            }
        }

        [HttpGet]
        public Task<PagedResult<BetriebskostenrechnungEntryBase>> Get([FromQuery] PagedQuery query)
            => dbService.GetList(User!, query);

        [HttpGet("{id}")]
        public Task<ActionResult<BetriebskostenrechnungEntry>> Get(Guid id)
            => dbService.Get(User!, id);

        [HttpPost]
        public Task<ActionResult<BetriebskostenrechnungEntry>> Post([FromBody] BetriebskostenrechnungEntry entry)
            => dbService.Post(User!, entry);

        [HttpPut("{id}")]
        public Task<ActionResult<BetriebskostenrechnungEntry>> Put(Guid id, [FromBody] BetriebskostenrechnungEntry entry)
            => dbService.Put(User!, id, entry);

        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(Guid id)
            => dbService.Delete(User!, id);
    }
}
