using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{

    public class BetriebskostenRechnungenListViewModel
    {
        public ObservableProperty<ImmutableDictionary<BetriebskostenRechnungenBetriebskostentyp, BetriebskostenRechnungenListTypenJahr>> Typen
            = new ObservableProperty<ImmutableDictionary<BetriebskostenRechnungenBetriebskostentyp, BetriebskostenRechnungenListTypenJahr>>();

        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;

        public List<BetriebskostenRechnungenBetriebskostentyp> Betriebskostentypen =
            Enum.GetValues(typeof(Betriebskostentyp))
                .Cast<Betriebskostentyp>()
                .ToList()
                .Select(e => new BetriebskostenRechnungenBetriebskostentyp(e))
                .ToList();

        public List<BetriebskostenRechnungenSchluessel> Betriebskostenschluessel =
            Enum.GetValues(typeof(UmlageSchluessel))
                .Cast<UmlageSchluessel>()
                .ToList()
                .Select(t => new BetriebskostenRechnungenSchluessel(t))
                .ToList();

        public TreeViewNode AddBetriebskostenTree;
        public BetriebskostenRechnungenListViewModel()
        {
            Typen.Value = App.Walter.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .ToList()
                .GroupBy(g => g.Typ)
                .ToImmutableDictionary(
                    g => new BetriebskostenRechnungenBetriebskostentyp(g.Key),
                    g => new BetriebskostenRechnungenListTypenJahr(g.ToList()));

            AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Select(w => new BetriebskostenRechungenListWohnungListWohnung(w))
                .GroupBy(w => w.AdresseId)
                .ToImmutableDictionary(
                    g => new BetriebskostenRechungenListWohnungListAdresse(g.Key),
                    g => g.ToImmutableList());
        }
    }

    public class BetriebskostenRechnungenBetriebskostentyp
    {
        public Betriebskostentyp Typ { get; }
        public string Beschreibung { get; }
        public BetriebskostenRechnungenBetriebskostentyp(Betriebskostentyp t)
        {
            Typ = t;
            Beschreibung = t.ToDescriptionString();
        }
    }

    public class BetriebskostenRechnungenSchluessel
    {
        public UmlageSchluessel Schluessel { get; }
        public string Beschreibung { get; }
        public BetriebskostenRechnungenSchluessel(UmlageSchluessel u)
        {
            Schluessel = u;
            Beschreibung = u.ToDescriptionString();
        }
    }

    public class BetriebskostenRechungenListWohnungListAdresse : TreeViewNode
    {
        public int Id { get; }
        public string Anschrift { get; }
        public BetriebskostenRechungenListWohnungListAdresse(int id)
        {
            Id = id;
            Anschrift = AdresseViewModel.Anschrift(id);
            Content = Anschrift;
        }
    }

    public class BetriebskostenRechungenListWohnungListWohnung : TreeViewNode
    {
        public int Id { get; }
        public int AdresseId { get; }
        public string Bezeichnung { get; }


        public BetriebskostenRechungenListWohnungListWohnung(Wohnung w)
        {
            Id = w.WohnungId;
            AdresseId = w.AdresseId;
            Bezeichnung = w.Bezeichnung;
            Content = Bezeichnung;
        }
    }

    public class BetriebskostenRechnungenListTypenJahr
    {
        public ImmutableDictionary<int, ImmutableList<BetriebskostenRechnungenRechnung>> Jahre { get; }

        public BetriebskostenRechnungenListTypenJahr(List<Betriebskostenrechnung> r)
        {
            Jahre = r.GroupBy(gg => gg.Datum.Year).ToImmutableDictionary(
                gg => gg.Key, gg => gg.ToList().Select(ggg => new BetriebskostenRechnungenRechnung(ggg)).ToImmutableList());
        }
    }

    public class BetriebskostenRechnungenRechnung
    {
        public string Betrag { get; }
        public string Datum { get; }
        public ImmutableDictionary<string, ImmutableList<string>> Gruppen { get; }
        public ImmutableList<string> Wohnungen { get; }

        public BetriebskostenRechnungenRechnung(Betriebskostenrechnung r)
        {
            Betrag = string.Format("{0:F2}€", r.Betrag);
            Datum = r.Datum.ToString("dd.MM.yyyy");
            Gruppen = App.Walter.Betriebskostenrechnungsgruppen
                .Where(g => g.Rechnung == r)
                .ToList()
                .GroupBy(g => g.Wohnung.Adresse)
                .ToImmutableDictionary(
                    g => AdresseViewModel.Anschrift(g.Key),
                    g => g.Select(gg => gg.Wohnung.Bezeichnung).ToImmutableList());

            Wohnungen = App.Walter.Betriebskostenrechnungsgruppen
                .Where(g => g.Rechnung == r)
                .Select(g => AdresseViewModel.Anschrift(g.Wohnung) + " — " + g.Wohnung.Bezeichnung)
                .ToImmutableList();
        }
    }
}
