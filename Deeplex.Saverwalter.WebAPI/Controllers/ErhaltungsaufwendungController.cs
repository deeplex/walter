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
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/erhaltungsaufwendungen")]
    public class ErhaltungsaufwendungController(ErhaltungsaufwendungDbService dbService) : ControllerBase
    {
        public class BuchungszeileInfo
        {
            public string Konto { get; set; } = "";
            public string SollHaben { get; set; } = "";
            public decimal Betrag { get; set; }
        }

        public class ErhaltungsaufwendungEntryBase
        {
            public Guid Id { get; set; }
            public decimal Betrag { get; set; }
            public DateOnly Datum { get; set; }
            public string Bezeichnung { get; set; } = "";
            public SelectionEntry Wohnung { get; set; } = null!;
            public SelectionEntry? Aussteller { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public ErhaltungsaufwendungEntryBase() { }
            public ErhaltungsaufwendungEntryBase(Buchungssatz satz, SelectionEntry wohnung, Kontakt? aussteller, Permissions permissions)
            {
                Id = satz.BuchungssatzId;
                Datum = satz.Buchungsdatum;
                Betrag = satz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag);
                Bezeichnung = satz.Beschreibung.StartsWith("Erhaltungsaufwendung: ")
                    ? satz.Beschreibung["Erhaltungsaufwendung: ".Length..]
                    : satz.Beschreibung;
                Wohnung = wohnung;
                Aussteller = aussteller != null
                    ? new SelectionEntry(aussteller.KontaktId, aussteller.Bezeichnung)
                    : null;
                Permissions = permissions;
            }
        }

        public class ErhaltungsaufwendungEntry : ErhaltungsaufwendungEntryBase
        {
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public string? Notiz { get; set; }
            public List<BuchungszeileInfo> Buchungszeilen { get; set; } = [];

            public ErhaltungsaufwendungEntry() { }
            public ErhaltungsaufwendungEntry(Buchungssatz satz, Deeplex.Saverwalter.Model.Wohnung wohnung, Kontakt? aussteller, Permissions permissions)
                : base(satz, BuildWohnungEntry(wohnung), aussteller, permissions)
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

            private static SelectionEntry BuildWohnungEntry(Deeplex.Saverwalter.Model.Wohnung wohnung)
            {
                var anschrift = wohnung.Adresse is Adresse a ? a.Anschrift : "Unbekannte Anschrift";
                return new SelectionEntry(wohnung.WohnungId, $"{anschrift} - {wohnung.Bezeichnung}");
            }
        }

        [HttpGet]
        public Task<PagedResult<ErhaltungsaufwendungEntryBase>> Get([FromQuery] PagedQuery query)
            => dbService.GetList(User!, query);

        [HttpGet("{id}")]
        public Task<ActionResult<ErhaltungsaufwendungEntry>> Get(Guid id)
            => dbService.Get(User!, id);

        [HttpPost]
        public Task<ActionResult<ErhaltungsaufwendungEntry>> Post([FromBody] ErhaltungsaufwendungEntry entry)
            => dbService.Post(User!, entry);

        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(Guid id)
            => dbService.Delete(User!, id);
    }
}
