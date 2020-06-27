using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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

        public Microsoft.Toolkit.Uwp.UI.Controls.WrapPanel BreadCrumbs { get; set; }

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

        private MenuFlyoutItem MakeItem(string text, object o)
        {
            void ClickFilter(object sender, RoutedEventArgs e) => ApplyFilter(sender);

            var item = new MenuFlyoutItem() { Text = text, Tag = o };
            item.Click += ClickFilter;
            return item;
        }

        private MenuFlyoutSubItem MakeSubItem(string text, object o)
        {
            void TappedFilter(object sender, TappedRoutedEventArgs e) => ApplyFilter(sender);

            var item = new MenuFlyoutSubItem() { Text = text, Tag = o };
            item.Tapped += TappedFilter;
            return item;
        }

        private MenuFlyoutItem AnhangKontakt(IPerson p)
            => MakeItem(p.Bezeichnung, p);

        private MenuFlyoutSubItem AnhangVertrag(Vertrag v)
        {
            var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
            var cs = bs.Select(b => App.Walter.FindPerson(b.PersonId))
                .Select(p => p is NatuerlichePerson n ? n.Nachname : p.Bezeichnung);
            var mieter = string.Join(", ", cs);

            var sub = MakeSubItem(mieter + " – " + v.Wohnung.Adresse.Strasse + " " +
                    v.Wohnung.Adresse.Hausnummer, v);

            var mieten = MakeSubItem("Mieten", null);

            App.Walter.Mieten
                .Where(m => m.VertragId == v.VertragId)
                .ToList()
                .ForEach(m => mieten.Items.Add(AnhangMiete(m)));

            var mietminderungen = MakeSubItem("Mietminderung", null);
            App.Walter.MietMinderungen
                .Where(m => m.VertragId == v.VertragId)
                .ToList()
                .ForEach(m => mietminderungen.Items.Add(AnhangMietMinderung(m)));

            sub.Items.Add(mieten);
            sub.Items.Add(mietminderungen);

            return sub;
        }

        private MenuFlyoutItem AnhangMiete(Miete m)
            => MakeItem(m.BetreffenderMonat.ToString("MMM yyyy"), m);

        private MenuFlyoutItem AnhangMietMinderung(MietMinderung m)
            => MakeItem(m.Beginn.ToString("dd.MM.yyyy") + " – " +
                    (m.Ende != null ? m.Ende.Value.ToString("dd.MM.yyyy") : "Offen"), m);

        private MenuFlyoutSubItem AnhangAdresse(Adresse a)
        {
            var sub = MakeSubItem(AdresseViewModel.Anschrift(a), a);
            a.Wohnungen.ToList().ForEach(w => sub.Items.Add(AnhangWohnung(w)));
            return sub;
        }

        private MenuFlyoutSubItem AnhangWohnung(Wohnung w)
        {
            var sub = MakeSubItem(w.Bezeichnung, w);
            var zaehler = MakeSubItem("Zähler", null);
            w.Zaehler.ToList().ForEach(z => zaehler.Items.Add(AnhangZaehler(z)));

            var betrRechnungen = MakeSubItem("Betriebskostenrechnungen", null);
            w.Betriebskostenrechnungsgruppen.ToList()
                .ForEach(r => betrRechnungen.Items
                    .Add(AnhangBetriebskostenRechnung(r.Rechnung)));

            sub.Items.Add(zaehler);
            sub.Items.Add(betrRechnungen);

            return sub;
        }

        private MenuFlyoutSubItem AnhangZaehler(Zaehler z)
        {
            var sub = MakeSubItem(z.Kennnummer, z);
            z.Staende.ToList().ForEach(zs => sub.Items.Add(AnhangZaehlerstand(zs)));

            return sub;
        }

        private MenuFlyoutItem AnhangZaehlerstand(Zaehlerstand zs)
            => MakeItem(zs.Datum.ToString("dd.MM.yyyy"), zs);

        private MenuFlyoutItem AnhangBetriebskostenRechnung(Betriebskostenrechnung r)
            => MakeItem(r.Typ.ToDescriptionString() + " " + r.BetreffendesJahr.ToString(), r);

        private void ApplyFilter(object sender)
        {
            ImmutableList<AnhangDatei> GetFilteredList<T>(T u)
            {
                ImmutableList<AnhangDatei> r<U>(IQueryable<IAnhang<U>> l, U target)
                {
                    var text = (sender is MenuFlyoutItem t ? t : null)?.Text ?? (sender as MenuFlyoutSubItem).Text;

                    BreadCrumbs.Children.Clear();
                    BreadCrumbs.Children.Add(new Button { Content = text });

                    return l.ToList().Where(member => member.Target.Equals(target))
                        .Select(member => new AnhangDatei(member.Anhang)).ToImmutableList();
                }

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
