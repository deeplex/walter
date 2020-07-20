using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using wuxc = Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class AnhangTreeViewNode : TreeViewNode
    {
        public AsyncRelayCommand AttachFile { get; set; }
        public AnhangTreeViewNode() { }
    }

    public interface IAnhangTreeViewNode
    {
        object Target { get; }
    }
    public interface IAnhangTreeViewNode<T> : IAnhangTreeViewNode
    {
        new T Target { get; }
    }

    public sealed class AnhangViewModel : BindableBase
    {
        private TreeView Tree { get; }

        private void expand(TreeViewNode p)
        {
            p.IsExpanded = true;
            if (p.Parent != null)
            {
                expand(p.Parent);
            }
        }
        public void raiseChange<U>(U target, Anhang file)
        {
            void dig(TreeViewNode n)
            {
                n.Children.ToList().ForEach(dig);

                if (n is IAnhangTreeViewNode ax && ax.Target.Equals(target))
                {
                    var a = new AnhangDatei(file);
                    n.Children.Add(a);
                    expand(n);
                    Tree.SelectedItem = a;
                }
            }

            Tree.RootNodes.ToList().ForEach(dig);
        }
        
        // TODO merge raiseChange and navigate...
        public void navigate<U>(U target)
        {
            void dig(TreeViewNode n)
            {
                n.Children.ToList().ForEach(dig);

                if(n is IAnhangTreeViewNode ax && ax.Target.Equals(target))
                {
                    expand(n);
                    Tree.SelectedItem = n;
                }
            }

            Tree.RootNodes.ToList().ForEach(dig);
        }

        public ObservableProperty<bool> navigationSynced
            = new ObservableProperty<bool>();
        public bool inSelection => Tree.SelectionMode == TreeViewSelectionMode.Multiple;
        public bool notInSelection => Tree.SelectionMode != TreeViewSelectionMode.Multiple;

        public AnhangViewModel(TreeView ExplorerTree)
        {
            Tree = ExplorerTree;

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

            SelectionMultilple = new RelayCommand(_ =>
                {
                    Tree.SelectionMode = TreeViewSelectionMode.Multiple;
                    RaiseSelectionPropertyChangedAuto();
                }, _ => true);
            SaveFiles = new AsyncRelayCommand(async _ =>
                {
                    if (!Tree.SelectedNodes.Any())
                    {
                        Tree.SelectionMode = TreeViewSelectionMode.Single;
                        RaiseSelectionPropertyChangedAuto();
                        return;
                    }

                    var root = await Files.SelectDirectory();

                    if (root == null) return;

                    Tree.SelectionMode = TreeViewSelectionMode.Single;
                    RaiseSelectionPropertyChangedAuto();

                    var directory = await root.CreateFolderAsync("walter");

                    var local = ApplicationData.Current.LocalFolder;
                    foreach (var node in Tree.RootNodes)
                    {
                        SaveFilesLocal(node, directory, node.Content.ToString());
                    }

                }, _ => true);
        }

        private async void SaveFilesLocal(TreeViewNode n, StorageFolder root, params string[] paths)
        {
            foreach (var child in n.Children)
            {
                if (child.HasChildren)
                {
                    SaveFilesLocal(child, root, paths.Append(child.Content.ToString()).ToArray());
                }
                else if (child is AnhangDatei anhang && Tree.SelectedNodes.Contains(anhang))
                {
                    var dir = root;
                    foreach (var path in paths)
                    {
                        dir = await dir.CreateFolderAsync(path, CreationCollisionOption.GenerateUniqueName);
                    }
                    var file = await dir.CreateFileAsync(anhang.DateiName);
                    await FileIO.WriteBytesAsync(file, anhang.Entity.Content);
                }
            }
        }

        private void RaiseSelectionPropertyChangedAuto()
        {
            RaisePropertyChangedAuto(nameof(inSelection));
            RaisePropertyChangedAuto(nameof(notInSelection));
        }

        public RelayCommand SelectionMultilple { get; }
        public AsyncRelayCommand SaveFiles { get; }

        public class AnhangKontakt : AnhangTreeViewNode, IAnhangTreeViewNode<IPerson>
        {
            public IPerson Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangKontakt(IPerson p)
            {
                Target = p;
                Content = p.Bezeichnung;

                if (p is NatuerlichePerson n)
                {
                    AttachFile = new AsyncRelayCommand(async _ =>
                        await Files.SaveFilesToWalter(App.Walter.NatuerlichePersonAnhaenge, n), _ => true);

                    foreach (var na in App.Walter.NatuerlichePersonAnhaenge
                        .Include(a => a.Anhang)
                        .Where(a => a.Target.NatuerlichePersonId == n.NatuerlichePersonId))
                    {
                        Children.Add(new AnhangDatei(na.Anhang));
                    }
                }
                else if (p is JuristischePerson j)
                {
                    AttachFile = new AsyncRelayCommand(async _ =>
                        await Files.SaveFilesToWalter(App.Walter.JuristischePersonAnhaenge, j), _ => true);
                    foreach (var ja in App.Walter.JuristischePersonAnhaenge
                        .Include(a => a.Anhang)
                        .Where(a => a.Target.JuristischePersonId == j.JuristischePersonId))
                    {
                        Children.Add(new AnhangDatei(ja.Anhang));
                    }
                }
            }
        }

        public sealed class AnhangAdresse : AnhangTreeViewNode, IAnhangTreeViewNode<Adresse>
        {
            public Adresse Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangAdresse(Adresse a)
            {
                Target = a;

                Content = AdresseViewModel.Anschrift(a);

                foreach (var w in a.Wohnungen)
                {
                    Children.Add(new AnhangWohnung(w));
                }

                foreach (var aa in App.Walter.AdresseAnhaenge
                    .Include(a2 => a2.Anhang)
                    .Where(a2 => a2.Target.AdresseId == a.AdresseId))
                {
                    Children.Add(new AnhangDatei(aa.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.AdresseAnhaenge, a), _ => true);
            }
        }

        public sealed class AnhangVertrag : AnhangTreeViewNode, IAnhangTreeViewNode<Vertrag>
        {
            public Vertrag Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangVertrag(Vertrag v)
            {
                Target = v;

                var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
                var cs = bs.Select(b => App.Walter.FindPerson(b.PersonId))
                    .Select(p => p is NatuerlichePerson n ? n.Nachname : p.Bezeichnung);
                var mieter = string.Join(", ", cs);

                Content = mieter + " – " + v.Wohnung.Adresse.Strasse + " " + v.Wohnung.Adresse.Hausnummer;

                foreach (var va in App.Walter.VertragAnhaenge
                    .Include(a => a.Anhang)
                    .Where(a => a.Target == v.VertragId))
                {
                    Children.Add(new AnhangDatei(va.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.VertragAnhaenge, v.VertragId), _ => true);
            }
        }

        public sealed class AnhangWohnung : AnhangTreeViewNode, IAnhangTreeViewNode<Wohnung>
        {
            public Wohnung Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangWohnung(Wohnung w)
            {
                Target = w;

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
                    .Include(a => a.Anhang)
                    .Where(a => a.Target.WohnungId == w.WohnungId))
                {
                    Children.Add(new AnhangDatei(wa.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.WohnungAnhaenge, w), _ => true);
            }
        }

        public sealed class AnhangBetriebskostenrechnung : AnhangTreeViewNode, IAnhangTreeViewNode<Betriebskostenrechnung>
        {
            public Betriebskostenrechnung Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangBetriebskostenrechnung(Betriebskostenrechnung r)
            {
                Target = r;
                Content = r.BetreffendesJahr.ToString() + " " + r.Typ.ToDescriptionString();

                foreach (var ra in App.Walter.BetriebskostenrechnungAnhaenge
                    .Include(a => a.Anhang)
                    .Where(a => a.Target.BetriebskostenrechnungId == r.BetriebskostenrechnungId))
                {
                    Children.Add(new AnhangDatei(ra.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.BetriebskostenrechnungAnhaenge, r), _ => true);
            }
        }

        public sealed class AnhangZaehler : AnhangTreeViewNode, IAnhangTreeViewNode<Zaehler>
        {
            public Zaehler Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangZaehler(Zaehler z)
            {
                Target = z;

                Content = z.Kennnummer;

                var zaehlerstaende = new TreeViewNode { Content = "Zählerstände" };
                Children.Add(zaehlerstaende);
                foreach (var s in z.Staende)
                {
                    zaehlerstaende.Children.Add(new AnhangZaehlerstand(s));
                }

                foreach (var za in App.Walter.ZaehlerAnhaenge
                    .Include(a => a.Anhang)
                    .Where(a => a.Target.ZaehlerId == z.ZaehlerId))
                {
                    Children.Add(new AnhangDatei(za.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, z), _ => true);
            }
        }

        public sealed class AnhangZaehlerstand : AnhangTreeViewNode, IAnhangTreeViewNode<Zaehlerstand>
        {
            public Zaehlerstand Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangZaehlerstand(Zaehlerstand z)
            {
                Target = z;

                Content = z.Datum.ToString("dd.MM.yyyy");

                foreach (var za in App.Walter.ZaehlerstandAnhaenge
                    .Include(a => a.Anhang)
                    .Where(a => a.Target.ZaehlerstandId == z.ZaehlerstandId))
                {
                    Children.Add(new AnhangDatei(za.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.ZaehlerstandAnhaenge, z), _ => true);
            }
        }

        public sealed class AnhangMiete : AnhangTreeViewNode, IAnhangTreeViewNode<Miete>
        {
            public Miete Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangMiete(Miete m)
            {
                Target = m;

                Content = m.BetreffenderMonat.ToString("MMM yyyy");

                foreach (var ma in App.Walter.MieteAnhaenge
                    .Include(a => a.Anhang)
                    .Where(a => a.Target.MieteId == m.MieteId))
                {
                    Children.Add(new AnhangDatei(ma.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.MieteAnhaenge, m), _ => true);
            }
        }

        public sealed class AnhangMietminderung : AnhangTreeViewNode, IAnhangTreeViewNode<MietMinderung>
        {
            public MietMinderung Target { get; }
            object IAnhangTreeViewNode.Target => Target;

            public AnhangMietminderung(MietMinderung m)
            {
                Target = m;

                Content = m.Beginn.ToString("dd.MM.yyyy") + " – " +
                    (m.Ende != null ? m.Ende.Value.ToString("dd.MM.yyyy") : "Offen");

                foreach (var ma in App.Walter.MietMinderungAnhaenge
                    .Include(a => a.Anhang)
                    .Where(a => a.Target.MietMinderungId == m.MietMinderungId))
                {
                    Children.Add(new AnhangDatei(ma.Anhang));
                }

                AttachFile = new AsyncRelayCommand(async _ =>
                    await Files.SaveFilesToWalter(App.Walter.MietMinderungAnhaenge, m), _ => true);
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

            SaveFile = new AsyncRelayCommand(async _
                => await Files.ExtractTo(a), _ => true);

            DeleteFile = new RelayCommand(_ =>
            {
                App.Walter.Anhaenge.Remove(Entity);
                App.SaveWalter();
                Parent.Children.Remove(this);
            }, _ => true);
        }

        public AsyncRelayCommand SaveFile;
        public RelayCommand DeleteFile;
        public string DateiName => Entity.FileName;
        public string Symbol
        {
            get
            {
                var ext = Path.GetExtension(DateiName);
                switch (ext)
                {
                    case "jpg":
                    case "jpeg":
                    case "png":
                        return wuxc.Symbol.Camera.ToString();
                    default:
                        return wuxc.Symbol.Document.ToString();
                }
            }
        }
    }
}
