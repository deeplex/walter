using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AnhangViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<AnhangDatei>> Dateien = new ObservableProperty<ImmutableList<AnhangDatei>>();
        public ObservableProperty<string> FilterText = new ObservableProperty<string>();

        public Microsoft.UI.Xaml.Controls.MenuBarItem AnhangKontaktList = new Microsoft.UI.Xaml.Controls.MenuBarItem();
        public Microsoft.UI.Xaml.Controls.MenuBarItem AnhangVertragList = new Microsoft.UI.Xaml.Controls.MenuBarItem();
        public Microsoft.UI.Xaml.Controls.MenuBarItem AnhangAdresseList = new Microsoft.UI.Xaml.Controls.MenuBarItem();

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
            => new MenuFlyoutItem()
            {
                Text = p.Bezeichnung
            };

        private MenuFlyoutItem AnhangVertrag(Vertrag v)
        {
            var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
            var cs = bs.Select(b => App.Walter.FindPerson(b.PersonId).Bezeichnung);
            var mieter = string.Join(", ", cs);

            return new MenuFlyoutItem()
            {
                Text = mieter + " – " + v.Wohnung.Adresse.Strasse + " " + v.Wohnung.Adresse.Hausnummer + " – " + v.Wohnung.Bezeichnung,
            };
        }

        private MenuFlyoutSubItem AnhangAdresse(Adresse a)
        {
            var sub = new MenuFlyoutSubItem()
            {
                Text = AdresseViewModel.Anschrift(a),
            };
            a.Wohnungen.ToList().ForEach(w => sub.Items.Add(AnhangWohnung(w)));

            return sub;
        }

        private MenuFlyoutSubItem AnhangWohnung(Wohnung w)
        {
            var sub = new MenuFlyoutSubItem()
            {
                Text = w.Bezeichnung,
            };
            var zaehler = new MenuFlyoutSubItem()
            {
                Text = "Zähler",
            };
            w.Zaehler.ToList().ForEach(z => zaehler.Items.Add(AnhangZaehler(z)));

            var betrRechnungen = new MenuFlyoutSubItem()
            {
                Text = "Betriebskostenrechnungen",
            };
            // TODO group these by years.
            w.Betriebskostenrechnungsgruppen.ToList()
                .ForEach(r => betrRechnungen.Items
                    .Add(AnhangBetriebskostenRechnung(r.Rechnung)));

            sub.Items.Add(zaehler);
            sub.Items.Add(betrRechnungen);

            return sub;
        }

        private MenuFlyoutSubItem AnhangZaehler(Zaehler z)
        {
            var sub = new MenuFlyoutSubItem()
            {
                Text = z.Kennnummer,
            };
            z.Staende.ToList().ForEach(zs => sub.Items.Add(AnhangZaehlerstand(zs)));

            return sub;
        }

        private MenuFlyoutItem AnhangZaehlerstand(Zaehlerstand zs)
            => new MenuFlyoutItem()
            {
                Text = zs.Datum.ToString("dd.MM.yyyy"),
            };

        private MenuFlyoutItem AnhangBetriebskostenRechnung(Betriebskostenrechnung r)
            => new MenuFlyoutItem()
            {
                Text = r.Typ.ToDescriptionString() + " " + r.BetreffendesJahr.ToString(),
            };

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
}
