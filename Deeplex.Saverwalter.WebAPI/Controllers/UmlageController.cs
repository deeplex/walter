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

using System.ComponentModel.DataAnnotations;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/umlagen")]
    public class UmlageController : FileControllerBase<UmlageEntry, int, Umlage>
    {
        public class HKVOEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }

            [Range(50, 70, ErrorMessage = "HKVO §7: Verbrauchsanteil muss zwischen 50% und 70% liegen.")]
            public int HKVO_P7 { get; set; }
            [Range(50, 70, ErrorMessage = "HKVO §8: Verbrauchsanteil muss zwischen 50% und 70% liegen.")]
            public int HKVO_P8 { get; set; }
            public SelectionEntry HKVO_P9 { get; set; } = null!;
            public int Strompauschale { get; set; }
            public SelectionEntry Stromrechnung { get; set; } = null!;
            /// <summary>AllgemeinWärme-Zähler (Q für §9(2) Satz 2). Optional.</summary>
            public SelectionEntry? AllgemeinWaerme { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public HKVOEntryBase() { }
            public HKVOEntryBase(HKVO entity, Permissions permissions)
            {
                Id = entity.HKVOId;
                Beginn = entity.Beginn;

                HKVO_P7 = (int)(entity.HKVO_P7 * 100);
                HKVO_P8 = (int)(entity.HKVO_P8 * 100);
                HKVO_P9 = new((int)entity.HKVO_P9, entity.HKVO_P9.ToDescriptionString());
                Strompauschale = (int)(entity.Strompauschale * 100);
                Stromrechnung = new SelectionEntry(entity.Betriebsstrom.UmlageId, entity.Betriebsstrom.Typ.Bezeichnung);
                AllgemeinWaerme = entity.AllgemeinWaerme is { } z
                    ? new SelectionEntry(z.ZaehlerId, z.Kennnummer)
                    : null;

                Permissions = permissions;
            }
        }

        public class UmlageVersionEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public SelectionEntry Schluessel { get; set; } = null!;
            public Permissions Permissions { get; set; } = new Permissions();

            public UmlageVersionEntryBase() { }
            public UmlageVersionEntryBase(UmlageVersion entity, Permissions permissions)
            {
                Id = entity.UmlageVersionId;
                Beginn = entity.Beginn;
                Schluessel = new SelectionEntry((int)entity.Schluessel, entity.Schluessel.ToDescriptionString());
                Permissions = permissions;
            }
        }

        public class UmlageVersionEntry : UmlageVersionEntryBase
        {
            public string? Notiz { get; set; }
            public SelectionEntry? Umlage { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public UmlageVersionEntry() : base() { }
            public UmlageVersionEntry(UmlageVersion entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                Umlage = new(entity.Umlage.UmlageId, entity.Umlage.Typ.Bezeichnung);
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        public class UmlageEntryBase
        {
            public int Id { get; set; }
            public SelectionEntry? Typ { get; set; }
            public string? WohnungenBezeichnung { get; set; }
            public DateOnly? Ende { get; set; }
            public IEnumerable<int> BetriebskostenrechnungsJahre { get; set; } = [];

            // For Tabelle
            public IEnumerable<SelectionEntry>? SelectedWohnungen { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public UmlageEntryBase() { }
            public UmlageEntryBase(Umlage entity, Permissions permissions)
            {
                Id = entity.UmlageId;

                Typ = new SelectionEntry(entity.Typ.UmlagetypId, entity.Typ.Bezeichnung);
                WohnungenBezeichnung = entity.GetWohnungenBezeichnung() ?? "";
                Ende = entity.Ende;
                BetriebskostenrechnungsJahre = entity.NkVerrechnungsKonto?.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben)
                    .Select(z => z.Buchungssatz.Buchungsjahr)
                    .Distinct()
                    .ToList() ?? [];

                SelectedWohnungen = entity.Wohnungen.Select(e =>
                    new SelectionEntry(
                        e.WohnungId,
                        $"{e.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {e.Bezeichnung}"));

                Permissions = permissions;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            private Umlage Entity { get; } = null!;

            public string? Notiz { get; set; }
            public string? Beschreibung { get; set; }
            public SelectionEntry? Schluessel { get; set; }
            public HKVOEntryBase? HKVO { get; set; }
            public IEnumerable<SelectionEntry>? SelectedZaehler { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<WohnungEntryBase> Wohnungen { get; set; } = [];
            public IEnumerable<ZaehlerEntryBase> Zaehler { get; set; } = [];
            public IEnumerable<UmlageVersionEntryBase> Versionen { get; set; } = [];
            public IEnumerable<HKVOEntryBase> HKVOs { get; set; } = [];
            public IEnumerable<BetriebskostenrechnungEntryBase> Betriebskostenrechnungen { get; set; } = [];

            public UmlageEntry() : base() { }
            public UmlageEntry(Umlage entity, Permissions permissions) : base(entity, permissions)
            {
                Entity = entity;

                Notiz = entity.Notiz;
                Beschreibung = entity.Beschreibung;
                var currentVersion = entity.Versionen.OrderByDescending(v => v.Beginn).FirstOrDefault();
                if (currentVersion != null)
                {
                    Schluessel = new SelectionEntry((int)currentVersion.Schluessel, currentVersion.Schluessel.ToDescriptionString());
                }

                SelectedZaehler = entity.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer));

                var currentHkvo = entity.HeizkostenHKVOs.OrderByDescending(h => h.Beginn).FirstOrDefault();
                if (currentHkvo != null)
                {
                    HKVO = new HKVOEntryBase(currentHkvo, permissions);
                }

                Versionen = entity.Versionen.OrderBy(v => v.Beginn).Select(e => new UmlageVersionEntryBase(e, permissions)).ToList();
                HKVOs = entity.HeizkostenHKVOs.OrderBy(h => h.Beginn).Select(e => new HKVOEntryBase(e, permissions)).ToList();
                Betriebskostenrechnungen = [];

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<UmlageController> _logger;
        protected override UmlageDbService DbService { get; }

        public UmlageController(ILogger<UmlageController> logger, UmlageDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            _logger = logger;
            DbService = dbService;
        }


        [HttpGet]
        public Task<PagedResult<UmlageEntryBase>> Get([FromQuery] PagedQuery query) => DbService.GetList(User!, query);

        [HttpPost]
        public Task<ActionResult<UmlageEntry>> Post([FromBody] UmlageEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<UmlageEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<UmlageEntry>> Put(int id, UmlageEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
