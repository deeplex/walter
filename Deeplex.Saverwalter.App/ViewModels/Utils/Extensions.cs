using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels.Utils
{
    public static class BetriebskostenExtensions
    {
        public static string GetWohnungenBezeichnung(this Betriebskostenrechnung r, IAppImplementation impl, IList<Wohnung> l = null)
        {
            var Wohnungen = l != null ? l : r.Gruppen.Select(g => g.Wohnung).ToList();
            return string.Join(" — ", impl.ctx.Wohnungen
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
}
