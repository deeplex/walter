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
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/buchungskonten")]
    public class BuchungskontoController(SaverwalterContext ctx) : ControllerBase
    {
        public class BuchungskontoEntry
        {
            public int Id { get; set; }
            public string Kontonummer { get; set; } = "";
            public string Bezeichnung { get; set; } = "";
            public string Kontotyp { get; set; } = "";
            public string? Notiz { get; set; }
            public int AnzahlBuchungszeilen { get; set; }
            public decimal Saldo { get; set; }
            /// <summary>Funktion des Kontos bei seiner Besitzer-Entität, z.B. "Mietforderungen".</summary>
            public string? Funktion { get; set; }
            /// <summary>Ob das Konto sich ausgleichen soll — ein Saldo ist dann ein offener Posten.</summary>
            public bool Ausgleichbar { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();
        }

        public class MonatsSumme
        {
            public int Jahr { get; set; }
            public int Monat { get; set; }
            public decimal Soll { get; set; }
            public decimal Haben { get; set; }
        }

        public class BuchungskontoDetail : BuchungskontoEntry
        {
            public decimal SollSumme { get; set; }
            public decimal HabenSumme { get; set; }
            public List<MonatsSumme> MonatsSummen { get; set; } = [];
            public List<BuchungszeileInfo> LetzteZeilen { get; set; } = [];
            public List<KontoVerknuepfungEntry> Verknuepfungen { get; set; } = [];
        }

        /// <summary>
        /// Schlanke Konto-Referenz für die Detail-DTOs der Entitäten, denen Konten
        /// zugeordnet sind (Vertrag, Wohnung, Umlage, Kontakt, Garage, Garagenvertrag).
        /// </summary>
        public class BuchungskontoRefEntry
        {
            public int Id { get; set; }
            public string Kontonummer { get; set; } = "";
            public string Bezeichnung { get; set; } = "";
            public string Kontotyp { get; set; } = "";
            public string Funktion { get; set; } = "";
            public bool Ausgleichbar { get; set; }
            public decimal Saldo { get; set; }

            public static BuchungskontoRefEntry? From(Buchungskonto? konto, KontoFunktion funktion) =>
                konto is null ? null : new BuchungskontoRefEntry
                {
                    Id = konto.BuchungskontoId,
                    Kontonummer = konto.Kontonummer,
                    Bezeichnung = konto.Bezeichnung,
                    Kontotyp = konto.Kontotyp.ToString(),
                    Funktion = funktion.Name,
                    Ausgleichbar = funktion.Ausgleichbar,
                    Saldo = konto.Buchungszeilen
                        .Sum(z => z.SollHaben == Model.SollHaben.Soll ? z.Betrag : -z.Betrag)
                };

            /// <summary>Sammelt die Referenzen, lässt nicht gesetzte Konten aus.</summary>
            public static List<BuchungskontoRefEntry> Collect(
                params (Buchungskonto? Konto, KontoFunktion Funktion)[] konten) =>
                konten
                    .Select(k => From(k.Konto, k.Funktion))
                    .OfType<BuchungskontoRefEntry>()
                    .ToList();
        }

        public class BuchungszeileInfo
        {
            public Guid Id { get; set; }
            public DateOnly Datum { get; set; }
            public string Beschreibung { get; set; } = "";
            public string SollHaben { get; set; } = "";
            public decimal Betrag { get; set; }
        }

        public class BuchungskontoUpdateEntry
        {
            public string Bezeichnung { get; set; } = "";
            public string? Notiz { get; set; }
        }

        private static BuchungskontoEntry ToEntry(
            Buchungskonto k, bool canUpdate, KontoVerknuepfungEntry? verknuepfung) => new()
            {
                Id = k.BuchungskontoId,
                Kontonummer = k.Kontonummer,
                Bezeichnung = k.Bezeichnung,
                Kontotyp = k.Kontotyp.ToString(),
                Notiz = k.Notiz,
                AnzahlBuchungszeilen = k.Buchungszeilen.Count,
                Saldo = k.Buchungszeilen
                .Sum(z => z.SollHaben == Model.SollHaben.Soll ? z.Betrag : -z.Betrag),
                Funktion = verknuepfung?.Funktion,
                Ausgleichbar = verknuepfung?.Ausgleichbar ?? false,
                CreatedAt = k.CreatedAt,
                LastModified = k.LastModified,
                Permissions = new Permissions(true) { Update = canUpdate }
            };

        /// <summary>
        /// Konto-IDs mit Vollmacht-Zugriff des Nutzers — null bedeutet Admin (alle).
        /// </summary>
        private async Task<HashSet<int>?> VollmachtKontoIds()
        {
            if (User.IsInRole("Admin"))
            {
                return null;
            }

            var ids = await TransaktionPermissionHandler
                .ManagedBuchungskontoIds(ctx, User.GetUserId(), VerwalterRolle.Vollmacht)
                .ToListAsync();
            return ids.ToHashSet();
        }

        /// <summary>
        /// Buchungskonten, die der Nutzer in der gegebenen Rolle sehen darf — Admin
        /// alle, sonst nur Konten von Wohnungen (inkl. Verträge/Garagen/Umlagen), die
        /// er verwaltet. Das Buchungskonto ist der Anker der Buchungssätze, daher wird
        /// die Sichtbarkeit hier verankert.
        /// </summary>
        private IQueryable<Buchungskonto> ScopedKonten(VerwalterRolle rolle)
        {
            if (User.IsInRole("Admin"))
            {
                return ctx.Buchungskonten;
            }

            var kontoIds = TransaktionPermissionHandler
                .ManagedBuchungskontoIds(ctx, User.GetUserId(), rolle);
            return ctx.Buchungskonten.Where(k => kontoIds.Contains(k.BuchungskontoId));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BuchungskontoEntry>>> GetAll()
        {
            var konten = await ScopedKonten(VerwalterRolle.Keine)
                .Include(k => k.Buchungszeilen)
                .OrderBy(k => k.Kontonummer)
                .ToListAsync();

            var vollmachtIds = await VollmachtKontoIds();
            var verknuepfungen = await KontoVerknuepfungService.ForKontenAsync(
                ctx, konten.Select(k => k.BuchungskontoId).ToList());
            return Ok(konten.Select(k => ToEntry(k,
                vollmachtIds == null || vollmachtIds.Contains(k.BuchungskontoId),
                verknuepfungen.FirstOrDefault(v => v.KontoId == k.BuchungskontoId))));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BuchungskontoDetail>> Get(int id)
        {
            var konto = await ScopedKonten(VerwalterRolle.Keine)
                .Include(k => k.Buchungszeilen)
                    .ThenInclude(z => z.Buchungssatz)
                .FirstOrDefaultAsync(k => k.BuchungskontoId == id);

            if (konto is null)
            {
                // Existiert das Konto, ist aber außerhalb des Sichtbereichs? -> 403 statt 404
                return await ctx.Buchungskonten.AnyAsync(k => k.BuchungskontoId == id)
                    ? Forbid()
                    : NotFound();
            }

            var vollmachtIds = await VollmachtKontoIds();
            var verknuepfungen = await KontoVerknuepfungService
                .ForKontenAsync(ctx, [konto.BuchungskontoId]);
            var entry = ToEntry(konto,
                vollmachtIds == null || vollmachtIds.Contains(konto.BuchungskontoId),
                verknuepfungen.FirstOrDefault());
            var detail = new BuchungskontoDetail
            {
                Id = entry.Id,
                Kontonummer = entry.Kontonummer,
                Bezeichnung = entry.Bezeichnung,
                Kontotyp = entry.Kontotyp,
                Notiz = entry.Notiz,
                AnzahlBuchungszeilen = entry.AnzahlBuchungszeilen,
                Saldo = entry.Saldo,
                Funktion = entry.Funktion,
                Ausgleichbar = entry.Ausgleichbar,
                CreatedAt = entry.CreatedAt,
                LastModified = entry.LastModified,
                Permissions = entry.Permissions,
                SollSumme = konto.Buchungszeilen
                    .Where(z => z.SollHaben == Model.SollHaben.Soll)
                    .Sum(z => z.Betrag),
                HabenSumme = konto.Buchungszeilen
                    .Where(z => z.SollHaben == Model.SollHaben.Haben)
                    .Sum(z => z.Betrag),
                MonatsSummen = konto.Buchungszeilen
                    .GroupBy(z => (z.Buchungssatz.Buchungsdatum.Year, z.Buchungssatz.Buchungsdatum.Month))
                    .OrderBy(g => g.Key)
                    .Select(g => new MonatsSumme
                    {
                        Jahr = g.Key.Year,
                        Monat = g.Key.Month,
                        Soll = g.Where(z => z.SollHaben == Model.SollHaben.Soll).Sum(z => z.Betrag),
                        Haben = g.Where(z => z.SollHaben == Model.SollHaben.Haben).Sum(z => z.Betrag)
                    })
                    .ToList(),
                LetzteZeilen = konto.Buchungszeilen
                    .OrderByDescending(z => z.Buchungssatz.Buchungsdatum)
                    .Take(50)
                    .Select(z => new BuchungszeileInfo
                    {
                        Id = z.BuchungszeileId,
                        Datum = z.Buchungssatz.Buchungsdatum,
                        Beschreibung = z.Buchungssatz.Beschreibung,
                        SollHaben = z.SollHaben == Model.SollHaben.Soll ? "Soll" : "Haben",
                        Betrag = z.Betrag
                    })
                    .ToList(),
                Verknuepfungen = verknuepfungen
            };

            return Ok(detail);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BuchungskontoEntry>> Put(int id, [FromBody] BuchungskontoUpdateEntry update)
        {
            var konto = await ScopedKonten(VerwalterRolle.Vollmacht)
                .Include(k => k.Buchungszeilen)
                .FirstOrDefaultAsync(k => k.BuchungskontoId == id);

            if (konto is null)
            {
                return await ctx.Buchungskonten.AnyAsync(k => k.BuchungskontoId == id)
                    ? Forbid()
                    : NotFound();
            }

            konto.Bezeichnung = update.Bezeichnung;
            konto.Notiz = update.Notiz;
            await ctx.SaveChangesAsync();

            var verknuepfungen = await KontoVerknuepfungService
                .ForKontenAsync(ctx, [konto.BuchungskontoId]);
            return Ok(ToEntry(konto, canUpdate: true, verknuepfungen.FirstOrDefault()));
        }
    }
}
