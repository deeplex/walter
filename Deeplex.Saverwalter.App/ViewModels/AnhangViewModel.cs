using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.EntityFrameworkCore;
using wuxc = Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AnhangViewModel : BindableBase
    {
        public AnhangViewModel()
        {

        }

        public class AnhangKontakt : TreeViewNode
        {
            public AnhangKontakt(IPerson p)
            {
                Content = p.Bezeichnung;

                if (p is NatuerlichePerson n)
                {
                    foreach (var na in App.Walter.NatuerlichePersonAnhaenge
                        .Where(a => a.Target.NatuerlichePersonId == n.NatuerlichePersonId))
                    {
                        Children.Add(new AnhangDatei(na.Anhang));
                    }
                }
                else if (p is JuristischePerson j)
                {
                    foreach (var ja in App.Walter.JuristischePersonAnhaenge
                        .Where(a => a.Target.JuristischePersonId == j.JuristischePersonId))
                    {
                        Children.Add(new AnhangDatei(ja.Anhang));
                    }
                }
            }
        }

        public static void AnhangRoot(TreeView ExplorerTree)
        {
            var Kontakte = new TreeViewNode { Content = "Kontakte" };
            var Mietobjekte = new TreeViewNode { Content = "Mietobjete" };
            var Vertraege = new TreeViewNode { Content = "Verträge" };

            foreach (var j in App.Walter.JuristischePersonen)
            {
                Kontakte.Children.Add(new AnhangKontakt(j));
            }
            foreach (var n in App.Walter.NatuerlichePersonen)
            {
                Kontakte.Children.Add(new AnhangKontakt(n));
            }
            foreach (var v in App.Walter.Vertraege
                .Include(i => i.Wohnung).ThenInclude(w => w.Adresse))
            {
                Vertraege.Children.Add(new AnhangVertrag(v));
            }
            foreach (var a in App.Walter.Adressen
                .Include(i => i.Wohnungen).ThenInclude(w => w.Zaehler).ThenInclude(z => z.Staende))
            {
                Mietobjekte.Children.Add(new AnhangAdresse(a));
            }

            ExplorerTree.RootNodes.Add(Kontakte);
            ExplorerTree.RootNodes.Add(Mietobjekte);
            ExplorerTree.RootNodes.Add(Vertraege);
        }

        public sealed class AnhangAdresse : TreeViewNode
        {
            public AnhangAdresse(Adresse a)
            {
                Content = AdresseViewModel.Anschrift(a);

                foreach (var w in a.Wohnungen)
                {
                    Children.Add(new AnhangWohnung(w));
                }

                foreach (var aa in App.Walter.AdresseAnhaenge
                    .Where(a2 => a2.Target.AdresseId == a.AdresseId))
                {
                    Children.Add(new AnhangDatei(aa.Anhang));
                }
            }
        }

        public sealed class AnhangVertrag : TreeViewNode
        {
            public AnhangVertrag(Vertrag v)
            {
                var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
                var cs = bs.Select(b => App.Walter.FindPerson(b.PersonId))
                    .Select(p => p is NatuerlichePerson n ? n.Nachname : p.Bezeichnung);
                var mieter = string.Join(", ", cs);

                Content = mieter + " – " + v.Wohnung.Adresse.Strasse + " " + v.Wohnung.Adresse.Hausnummer;

                foreach (var va in App.Walter.VertragAnhaenge
                    .Where(a => a.Target == v.VertragId))
                {
                    Children.Add(new AnhangDatei(va.Anhang));
                }

            }
        }

        public sealed class AnhangWohnung : TreeViewNode
        {
            public AnhangWohnung(Wohnung w)
            {
                Content = w.Bezeichnung;

                var zaehler = new TreeViewNode { Content = "Zähler" };
                Children.Add(zaehler);
                foreach (var z in w.Zaehler)
                {
                    zaehler.Children.Add(new AnhangZaehler(z));
                }

                var betriebskostenrechnungen = new TreeViewNode { Content = "Betriebskostenrechnungen" };
                Children.Add(betriebskostenrechnungen);
                foreach (var g in w.Betriebskostenrechnungsgruppen)
                {
                    betriebskostenrechnungen.Children.Add(new AnhangBetriebskostenrechnung(g.Rechnung));
                }

                foreach (var wa in App.Walter.WohnungAnhaenge
                    .Where(a => a.Target.WohnungId == w.WohnungId))
                {
                    Children.Add(new AnhangDatei(wa.Anhang));
                }

            }
        }

        public sealed class AnhangBetriebskostenrechnung : TreeViewNode
        {
            public AnhangBetriebskostenrechnung(Betriebskostenrechnung r)
            {
                Content = r.BetreffendesJahr.ToString() + " " + r.Typ.ToDescriptionString();

                foreach (var ra in App.Walter.BetriebskostenrechnungAnhaenge
                    .Where(a => a.Target.BetriebskostenrechnungId == r.BetriebskostenrechnungId))
                {
                    Children.Add(new AnhangDatei(ra.Anhang));
                }
            }

        }

        public sealed class AnhangZaehler : TreeViewNode
        {
            public AnhangZaehler(Zaehler z)
            {
                Content = z.Kennnummer;

                var zaehlerstaende = new TreeViewNode { Content = "Zählerstände" };
                Children.Add(zaehlerstaende);
                foreach (var s in z.Staende)
                {
                    zaehlerstaende.Children.Add(new AnhangZaehlerstand(s));
                }

                foreach (var za in App.Walter.ZaehlerAnhaenge
                    .Where(a => a.Target.ZaehlerId == z.ZaehlerId))
                {
                    Children.Add(new AnhangDatei(za.Anhang));
                }
            }
        }

        public sealed class AnhangZaehlerstand : TreeViewNode
        {
            public AnhangZaehlerstand(Zaehlerstand z)
            {
                Content = z.Datum.ToString("dd.MM.yyyy");

                foreach (var za in App.Walter.ZaehlerstandAnhaenge
                    .Where(a => a.Target.ZaehlerstandId == z.ZaehlerstandId))
                {
                    Children.Add(new AnhangDatei(za.Anhang));
                }
            }
        }

        public sealed class AnhangMiete : TreeViewNode
        {
            public AnhangMiete(Miete m)
            {
                Content = m.BetreffenderMonat.ToString("MMM yyyy");

                foreach (var ma in App.Walter.MieteAnhaenge
                    .Where(a => a.Target.MieteId == m.MieteId))
                {
                    Children.Add(new AnhangDatei(ma.Anhang));
                }
            }
        }

        public sealed class AnhangMietminderung : TreeViewNode
        {
            public AnhangMietminderung(MietMinderung m)
            {
                Content = m.Beginn.ToString("dd.MM.yyyy") + " – " +
                    (m.Ende != null ? m.Ende.Value.ToString("dd.MM.yyyy") : "Offen");

                foreach (var ma in App.Walter.MietMinderungAnhaenge
                    .Where(a => a.Target.MietMinderungId == m.MietMinderungId))
                {
                    Children.Add(new AnhangDatei(ma.Anhang));
                }
            }
        }

        public sealed class AnhangDatei : TreeViewNode
        {
            public Anhang Entity { get; }

            public AnhangDatei(Anhang a)
            {
                Entity = a;
                Content = a.FileName;
            }
            public string DateiName => Entity.FileName;
        }
    }
}
