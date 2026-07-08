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

using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class UmlageDbService : WalterDbServiceBase<UmlageEntry, int, Umlage>
    {
        public UmlageDbService(SaverwalterContext dbService, IAuthorizationService authorizationService) : base(dbService, authorizationService)
        {
        }

        public Task<PagedResult<UmlageEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            UmlagePermissionHandler.GetQueryable(Ctx, user)
                .AsSplitQuery()
                .Include(u => u.Typ)
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben))
                    .ThenInclude(z => z.Buchungssatz)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Adresse)
                .Include(u => u.Wohnungen)
                    .ThenInclude(w => w.Verwalter)
                .PagedAsync(query,
                    searchPredicate: t => e =>
                        e.Typ.Bezeichnung.ToLower().Contains(t) ||
                        e.Wohnungen.Any(w =>
                            w.Bezeichnung.ToLower().Contains(t) ||
                            (w.Adresse != null && (
                                w.Adresse.Strasse.ToLower().Contains(t) ||
                                w.Adresse.Stadt.ToLower().Contains(t)))),
                    applySort: (q, sortBy, dir) => sortBy switch
                    {
                        "wohnungenBezeichnung" => q.SortBy(e => e.Wohnungen.Count, dir),
                        _ => q.SortBy(e => e.Typ.Bezeichnung, dir)
                    },
                    toEntry: async e => new UmlageEntryBase(e, await Utils.GetPermissions(user, e, Auth)));

        public override async Task<ActionResult<Umlage>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Umlagen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<UmlageEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new UmlageEntry(entity, permissions);

                entry.Wohnungen = await Task.WhenAll(entity.Wohnungen
                    .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Zaehler = await Task.WhenAll(entity.Zaehler
                    .Select(async e => new ZaehlerEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                var nkKontoId = entity.NkVerrechnungsKonto.BuchungskontoId;
                var bkSaetze = await Ctx.Buchungssaetze
                    .AsSplitQuery()
                    .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                    .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsHabenZeile).ThenInclude(opa => opa.SollZeile)
                    .Where(s => s.Buchungszeilen.Any(z =>
                        z.SollHaben == SollHaben.Haben &&
                        z.Buchungskonto.BuchungskontoId == nkKontoId))
                    .ToListAsync();
                entry.Betriebskostenrechnungen = bkSaetze
                    .OrderBy(s => s.Buchungsjahr)
                    .ThenBy(s => s.Buchungsdatum)
                    .Select(s => new BetriebskostenrechnungEntryBase(s, entity, permissions))
                    .ToList();

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Umlagen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<UmlageEntry>> Post(ClaimsPrincipal user, UmlageEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnungen = entry.Wohnungen!
                    .SelectMany(wohnung => Ctx.Wohnungen.Where(w => w.WohnungId == wohnung.Id));
                var authRx = await Auth.AuthorizeAsync(user, wohnungen, [Operations.SubCreate]);
                if (!authRx.Succeeded)
                {
                    return new ForbidResult();
                }

                return Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private UmlageEntry Add(UmlageEntry entry)
        {
            if (entry.Typ == null)
            {
                throw new ArgumentException("entry has no Typ.");
            }

            if (entry.Schluessel == null)
            {
                throw new ArgumentException("entry has no Schluessel.");
            }
            var schluessel = (Umlageschluessel)entry.Schluessel.Id;
            var typ = Ctx.Umlagetypen.First(typ => typ.UmlagetypId == entry.Typ.Id);
            var entity = new Umlage { Typ = typ };
            var firstVersion = entry.Versionen.FirstOrDefault();
            var beginn = firstVersion?.Beginn ?? DateOnly.FromDateTime(DateTime.Today);
            var version = new UmlageVersion(beginn, schluessel)
            {
                Umlage = entity
            };
            entity.Versionen.Add(version);

            SetOptionalValues(entity, entry);
            Ctx.Umlagen.Add(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<UmlageEntry>> Put(ClaimsPrincipal user, int id, UmlageEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                if (entry.Typ == null)
                {
                    throw new ArgumentException("entry has no Typ.");
                }

                entity.Typ = (await Ctx.Umlagetypen.FindAsync(entry.Typ.Id))!;
                SetOptionalValues(entity, entry);
                Ctx.Umlagen.Update(entity);
                Ctx.SaveChanges();

                return new UmlageEntry(entity, entry.Permissions);
            });
        }

        private void SetOptionalValues(Umlage entity, UmlageEntry entry)
        {
            if (entity.UmlageId != entry.Id)
            {
                throw new Exception();
            }

            entity.Beschreibung = entry.Beschreibung;

            if (entry.SelectedWohnungen is IEnumerable<SelectionEntry> l)
            {
                // Add missing Wohnungen
                entity.Wohnungen.AddRange(l
                    .Where(w => !entity.Wohnungen.Exists(e => w.Id == e.WohnungId))
                    .SelectMany(w => Ctx.Wohnungen.Where(u => u.WohnungId == w.Id)));
                // Remove old Wohnungen
                entity.Wohnungen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.WohnungId));
            }

            if (entry.SelectedZaehler is IEnumerable<SelectionEntry> zaehler)
            {
                // Add missing zaehler
                entity.Zaehler.AddRange(zaehler
                    .Where(z => !entity.Zaehler.Exists(e => z.Id == e.ZaehlerId))
                    .SelectMany(w => Ctx.ZaehlerSet.Where(u => u.ZaehlerId == w.Id)!));
                // Remove old zaehler
                entity.Zaehler.RemoveAll(w => !zaehler.ToList().Exists(e => e.Id == w.ZaehlerId));
            }

            if (entry.Schluessel != null &&
                (Umlageschluessel)entry.Schluessel.Id == Umlageschluessel.NachVerbrauch &&
                entry.HKVO is HKVOEntryBase hkvo)
            {
                var currentHkvo = entity.HeizkostenHKVOs.OrderByDescending(h => h.Beginn).FirstOrDefault();
                if (hkvo.HKVO_P7 < 50 || hkvo.HKVO_P7 > 70)
                    throw new ArgumentException($"HKVO §7: Verbrauchsanteil {hkvo.HKVO_P7}% liegt außerhalb des gesetzlichen Bereichs (50–70%).");
                if (hkvo.HKVO_P8 < 50 || hkvo.HKVO_P8 > 70)
                    throw new ArgumentException($"HKVO §8: Verbrauchsanteil {hkvo.HKVO_P8}% liegt außerhalb des gesetzlichen Bereichs (50–70%).");

                var isNewVersion = currentHkvo == null || currentHkvo.Beginn != hkvo.Beginn;

                if (isNewVersion)
                {
                    var newHKVO = new HKVO(
                        hkvo.Beginn,
                        (decimal)hkvo.HKVO_P7 / 100,
                        (decimal)hkvo.HKVO_P8 / 100,
                        (HKVO_P9A2)hkvo.HKVO_P9.Id,
                        (decimal)hkvo.Strompauschale / 100)
                    {
                        Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == hkvo.Stromrechnung.Id),
                    };
                    newHKVO.AllgemeinWaermeZaehler.AddRange(LadeWaermezaehler(hkvo.AllgemeinWaerme));
                    entity.HeizkostenHKVOs.Add(newHKVO);
                    Ctx.HKVO.Add(newHKVO);
                }
                else
                {
                    currentHkvo!.HKVO_P7 = (decimal)hkvo.HKVO_P7 / 100;
                    currentHkvo.HKVO_P8 = (decimal)hkvo.HKVO_P8 / 100;
                    currentHkvo.HKVO_P9 = (HKVO_P9A2)hkvo.HKVO_P9.Id;
                    currentHkvo.Strompauschale = (decimal)hkvo.Strompauschale / 100;
                    currentHkvo.Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == hkvo.Stromrechnung.Id);
                    currentHkvo.AllgemeinWaermeZaehler.Clear();
                    currentHkvo.AllgemeinWaermeZaehler.AddRange(LadeWaermezaehler(hkvo.AllgemeinWaerme));
                    Ctx.HKVO.Update(currentHkvo);
                }
            }
            else
            {
                entity.HeizkostenHKVOs.Clear();
            }

            entity.Ende = entry.Ende;
            entity.Notiz = entry.Notiz;
        }

        /// <summary>Lädt die ausgewählten Allgemein-Wärmezähler (für Q in §9(2)).</summary>
        private List<Zaehler> LadeWaermezaehler(IEnumerable<SelectionEntry> auswahl)
        {
            var ids = auswahl.Select(a => a.Id).ToList();
            return ids.Count == 0 ? [] : Ctx.ZaehlerSet.Where(z => ids.Contains(z.ZaehlerId)).ToList();
        }
    }
}
