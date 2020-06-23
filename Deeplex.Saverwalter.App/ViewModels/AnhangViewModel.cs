using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AnhangViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<AnhangDatei>> Dateien = new ObservableProperty<ImmutableList<AnhangDatei>>();
        public ObservableProperty<string>FilterText = new ObservableProperty<string>();

        public ObservableProperty<ImmutableList<AnhangKontakt>> AnhangKontaktList = new ObservableProperty<ImmutableList<AnhangKontakt>>();
        public ObservableProperty<ImmutableList<AnhangVertrag>> AnhangVertragList = new ObservableProperty<ImmutableList<AnhangVertrag>>();
        public ObservableProperty<ImmutableList<AnhangAdresse>> AnhangAdresseList = new ObservableProperty<ImmutableList<AnhangAdresse>>();

        public AnhangViewModel()
        {
            Dateien.Value = App.Walter.Anhaenge.Select(a => new AnhangDatei(a)).ToImmutableList();
            FilterText.Value = "Hier könnte ein sinnvoller Filtertext stehen...";

            AnhangKontaktList.Value = App.Walter.JuristischePersonen.ToImmutableList()
                .Select(j => new AnhangKontakt(j))
                .Concat(App.Walter.NatuerlichePersonen.Select(n => new AnhangKontakt(n)))
                .ToImmutableList();

            AnhangVertragList.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Select(v => new AnhangVertrag(v)).ToImmutableList();

            AnhangAdresseList.Value = App.Walter.Adressen.Select(a => new AnhangAdresse(a)).ToImmutableList();
        }
    }

    public sealed class AnhangDatei : BindableBase
    {
        public Anhang Entity { get; }

        public AnhangDatei(Anhang a)
        {
            Entity = a;
        }

        public string DateiName => Entity.FileName;
    }
    public sealed class AnhangKontakt
    {
        public string Bezeichnung { get; }
        public ObservableProperty<ImmutableList<AnhangVertrag>> Vertraege = new ObservableProperty<ImmutableList<AnhangVertrag>>();

        public AnhangKontakt(IPerson p)
        {
            Bezeichnung = p.Bezeichnung;

            var ms = App.Walter.MieterSet.ToList();

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .ToList()
                .Where(v => v.Wohnung.BesitzerId == p.PersonId || ms.Exists(m => m.VertragId == v.VertragId && m.PersonId == p.PersonId))
                .Select(v => new AnhangVertrag(v))
                .ToImmutableList();
        }
    }

    public sealed class AnhangVertrag
    {
        public string Bezeichnung { get; }

        public AnhangVertrag(Vertrag v)
        {
            var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
            var cs = bs.Select(b => App.Walter.FindPerson(b.PersonId).Bezeichnung);
            var mieter = string.Join(", ", cs);

            Bezeichnung = mieter + " – " + v.Wohnung.Adresse.Strasse + " " + v.Wohnung.Adresse.Hausnummer + " – " + v.Wohnung.Bezeichnung;
        }
    }

    public sealed class AnhangAdresse
    {
        public string Bezeichnung { get; }

        public ObservableProperty<ImmutableList<AnhangWohnung>> Wohnungen = new ObservableProperty<ImmutableList<AnhangWohnung>>();

        public AnhangAdresse(Adresse a)
        {
            Bezeichnung = AdresseViewModel.Anschrift(a);
            Wohnungen.Value = a.Wohnungen.Select(w => new AnhangWohnung(w)).ToImmutableList();
        }
    }

    public sealed class AnhangWohnung
    {
        public string Bezeichnung { get; }
        public ObservableProperty<ImmutableList<AnhangZaehler>> Zaehler
            = new ObservableProperty<ImmutableList<AnhangZaehler>>();
        public ObservableProperty<ImmutableList<AnhangBetriebskostenRechnung>> BetriebskostenRechnungen
            = new ObservableProperty<ImmutableList<AnhangBetriebskostenRechnung>>();

        public AnhangWohnung(Wohnung w)
        {
            Bezeichnung = w.Bezeichnung;
            Zaehler.Value = w.Zaehler.Select(z => new AnhangZaehler(z)).ToImmutableList();
        }
    }

    public sealed class AnhangZaehler
    {
        public string Bezeichnung { get; }
        public ObservableProperty<ImmutableList<AnhangZaehlerstand>> Zaehlerstaende
            = new ObservableProperty<ImmutableList<AnhangZaehlerstand>>();
        public AnhangZaehler(Zaehler z)
        {
            Bezeichnung = z.Kennnummer;
            Zaehlerstaende.Value = z.Staende.Select(zs => new AnhangZaehlerstand(zs)).ToImmutableList();
        }
    }

    public sealed class AnhangZaehlerstand
    {
        public string Bezeichnung { get; }
        public AnhangZaehlerstand(Zaehlerstand z)
        {
            Bezeichnung = z.Datum.ToString("dd.MM.yyyy");
        }
    }

    public sealed class AnhangBetriebskostenRechnung
    {
        public string Bezeichnung { get; }

        public AnhangBetriebskostenRechnung(Betriebskostenrechnung r)
        {
            Bezeichnung = r.Typ.ToDescriptionString() + " " + r.BetreffendesJahr.ToString();
        }
    }
}
