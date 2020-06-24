using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AnhangViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<AnhangDatei>> Dateien = new ObservableProperty<ImmutableList<AnhangDatei>>();
        public ObservableProperty<string> FilterText = new ObservableProperty<string>();

        // TODO Add icons - People, Street, Library
        public Microsoft.UI.Xaml.Controls.MenuBarItem AnhangKontaktList
            = new Microsoft.UI.Xaml.Controls.MenuBarItem()
            {
                Title = "Kontakte",
            };
        public Microsoft.UI.Xaml.Controls.MenuBarItem AnhangAdresseList
            = new Microsoft.UI.Xaml.Controls.MenuBarItem()
            {
                Title = "Mietobjekte"
            };
        public Microsoft.UI.Xaml.Controls.MenuBarItem AnhangVertragList
            = new Microsoft.UI.Xaml.Controls.MenuBarItem()
            {
                Title = "Verträge",
            };

        public AnhangViewModel()
        {
            Dateien.Value = App.Walter.Anhaenge.Select(a => new AnhangDatei(a)).ToImmutableList();
            FilterText.Value = "Hier könnte ein sinnvoller Filtertext stehen...";

            App.Walter.JuristischePersonen.ToList().ForEach(j => AnhangKontaktList.Items.Add(AnhangKontakt(j)));
            App.Walter.NatuerlichePersonen.ToList().ForEach(n => AnhangKontaktList.Items.Add(AnhangKontakt(n)));

            App.Walter.Vertraege
                .Include(v => v.Wohnung)
                .ThenInclude(w => w.Adresse)
                .ToList()
                .ForEach(v => AnhangVertragList.Items.Add(AnhangVertrag(v)));

            App.Walter.Adressen
                .Include(a => a.Wohnungen)
                    .ThenInclude(w => w.Zaehler)
                    .ThenInclude(z => z.Staende)
                .Include(a => a.Wohnungen)
                    .ThenInclude(w => w.Betriebskostenrechnungsgruppen)
                    .ThenInclude(g => g.Rechnung)
                .ToList()
                .ForEach(a => AnhangAdresseList.Items.Add(AnhangAdresse(a)));
        }

        private MenuFlyoutItem AnhangKontakt(IPerson p)
        {
            var person = new MenuFlyoutItem()
            {
                Text = p.Bezeichnung,
                Tag = p,
            };

            person.ContextRequested += ApplyFilter;
            return person;
        }

        private MenuFlyoutSubItem AnhangVertrag(Vertrag v)
        {
            var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
            var cs = bs.Select(b => App.Walter.FindPerson(b.PersonId).Bezeichnung);
            var mieter = string.Join(", ", cs);

            var sub = new MenuFlyoutSubItem()
            {
                Text = mieter + " – " + v.Wohnung.Adresse.Strasse + " " +
                    v.Wohnung.Adresse.Hausnummer + " – " + v.Wohnung.Bezeichnung,
                Tag = v,
            };

            var mieten = new MenuFlyoutSubItem()
            {
                Text = "Mieten",
                // TODO Filter for all Mieten
            };
            App.Walter.Mieten
                .Where(m => m.VertragId == v.VertragId)
                .ToList()
                .ForEach(m => mieten.Items.Add(AnhangMiete(m)));

            var mietminderungen = new MenuFlyoutSubItem()
            {
                Text = "Mietminderungen",
                // TODO Filter for all Mietminderungen
            };
            App.Walter.MietMinderungen
                .Where(m => m.VertragId == v.VertragId)
                .ToList()
                .ForEach(m => mietminderungen.Items.Add(AnhangMietMinderung(m)));

            sub.Items.Add(mieten);
            sub.Items.Add(mietminderungen);

            sub.ContextRequested += ApplyFilter;
            return sub;
        }

        private MenuFlyoutItem AnhangMiete(Miete m)
        {
            var miete = new MenuFlyoutItem()
            {
                Text = m.BetreffenderMonat.ToString(),
                Tag = m,
            };
            miete.ContextRequested += ApplyFilter;
            return miete;
        }

        private MenuFlyoutItem AnhangMietMinderung(MietMinderung m)
        {
            var mietminderung = new MenuFlyoutItem()
            {
                Text = m.Beginn.ToString("dd.MM.yyyy") + " – " +
                    m.Ende != null ? m.Ende.Value.ToString("dd.MM.yyyy") : "Offen",
                Tag = m,
            };
            mietminderung.ContextRequested += ApplyFilter;
            return mietminderung;
        }

        private MenuFlyoutSubItem AnhangAdresse(Adresse a)
        {
            var sub = new MenuFlyoutSubItem()
            {
                Text = AdresseViewModel.Anschrift(a),
                Tag = a,
            };
            a.Wohnungen.ToList().ForEach(w => sub.Items.Add(AnhangWohnung(w)));

            sub.ContextRequested += ApplyFilter;
            return sub;
        }

        private MenuFlyoutSubItem AnhangWohnung(Wohnung w)
        {
            var sub = new MenuFlyoutSubItem()
            {
                Text = w.Bezeichnung,
                Tag = w,
            };
            var zaehler = new MenuFlyoutSubItem()
            {
                Text = "Zähler",
                // TODO filter for all zähler
            };
            w.Zaehler.ToList().ForEach(z => zaehler.Items.Add(AnhangZaehler(z)));

            var betrRechnungen = new MenuFlyoutSubItem()
            {
                Text = "Betriebskostenrechnungen",
                // TODO filter for all betriebskostenrechnungen
            };
            // TODO group these by years.
            w.Betriebskostenrechnungsgruppen.ToList()
                .ForEach(r => betrRechnungen.Items
                    .Add(AnhangBetriebskostenRechnung(r.Rechnung)));

            sub.Items.Add(zaehler);
            sub.Items.Add(betrRechnungen);

            sub.ContextRequested += ApplyFilter;
            return sub;
        }

        private MenuFlyoutSubItem AnhangZaehler(Zaehler z)
        {
            var sub = new MenuFlyoutSubItem()
            {
                Text = z.Kennnummer,
                Tag = z,
            };
            z.Staende.ToList().ForEach(zs => sub.Items.Add(AnhangZaehlerstand(zs)));

            sub.ContextRequested += ApplyFilter;
            return sub;
        }

        private MenuFlyoutItem AnhangZaehlerstand(Zaehlerstand zs)
        {
            var m = new MenuFlyoutItem()
            {
                Text = zs.Datum.ToString("dd.MM.yyyy"),
                Tag = zs,
            };

            m.ContextRequested += ApplyFilter;
            return m;
        }

        private MenuFlyoutItem AnhangBetriebskostenRechnung(Betriebskostenrechnung r)
        {
            var rechnung = new MenuFlyoutItem()
            {
                Text = r.Typ.ToDescriptionString() + " " + r.BetreffendesJahr.ToString(),
                Tag = r,
            };

            rechnung.ContextRequested += ApplyFilter;
            return rechnung;
        }

        private void ApplyFilter(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ImmutableList<AnhangDatei> GetFilteredList<T>(T u)
            {
                ImmutableList<AnhangDatei> r<U>(IQueryable<IAnhang<U>> l, U target)
                    => l.ToList().Where(member => member.Target.Equals(target))
                        .Select(member => new AnhangDatei(member.Anhang)).ToImmutableList();

                switch (u)
                {
                    case Adresse v: return r(App.Walter.AdresseAnhaenge, v);
                    case Betriebskostenrechnung v: return r(App.Walter.BetriebskostenrechnungAnhaenge, v);
                    case Garage v: return r(App.Walter.GarageAnhaenge, v);
                    case JuristischePerson v: return r(App.Walter.JuristischePersonAnhaenge, v);
                    case Konto v: return r(App.Walter.KontoAnhaenge, v);
                    case Miete v: return r(App.Walter.MieteAnhaenge, v);
                    case MietMinderung v: return r(App.Walter.MietMinderungAnhaenge, v);
                    case NatuerlichePerson v: return r(App.Walter.NatuerlichePersonAnhaenge, v);
                    case Vertrag v: return r(App.Walter.VertragAnhaenge, v.VertragId);
                    case Wohnung v: return r(App.Walter.WohnungAnhaenge, v);
                    case Zaehler v: return r(App.Walter.ZaehlerAnhaenge, v);
                    case Zaehlerstand v: return r(App.Walter.ZaehlerstandAnhaenge, v);
                    default: return App.Walter.Anhaenge.Select(a => new AnhangDatei(a)).ToImmutableList();
                };
            }

            var cp = (sender is MenuFlyoutItem s ? s : null)?.Tag ?? (sender as MenuFlyoutSubItem).Tag;
            Dateien.Value = GetFilteredList(cp);
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
}
