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

        public AnhangAdresse(Adresse a)
        {
            Bezeichnung = AdresseViewModel.Anschrift(a);
        }
    }
}
