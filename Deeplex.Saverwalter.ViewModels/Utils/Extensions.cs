using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public static class Extensions
    {
        public static string GetWohnungenBezeichnung(this Betriebskostenrechnung r, AppViewModel avm)
        {
            var Wohnungen = r.Gruppen.Select(g => g.Wohnung).ToList();
            return Wohnungen.GetWohnungenBezeichnung(avm);
        }

        public static string GetWohnungenBezeichnung(this List<Wohnung> Wohnungen, AppViewModel avm)
            => string.Join(" — ", avm.ctx.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Where(w => Wohnungen.Contains(w))
                .ToList()
                .GroupBy(w => w.Adresse)
                .ToDictionary(g => g.Key, g => g.ToList())
                .Select(adr =>
                {
                    var a = adr.Key;
                    var ret = a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;
                    if (adr.Value.Count() != a.Wohnungen.Count)
                    {
                        ret += ": " + string.Join(", ", adr.Value.Select(w => w.Bezeichnung));
                    }
                    else
                    {
                        ret += " (gesamt)";
                    }
                    return ret;
                }));
    }
}
