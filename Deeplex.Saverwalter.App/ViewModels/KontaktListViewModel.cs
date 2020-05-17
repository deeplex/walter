using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KontaktListViewModel
    {
        public List<KontaktListKontakt> Kontakte = new List<KontaktListKontakt>();
        public ObservableProperty<KontaktListKontakt> SelectedKontakt
            = new ObservableProperty<KontaktListKontakt>();

        public KontaktListViewModel()
        {
            Kontakte = App.Walter.Kontakte
                .Include(k => k.Adresse)
                .Select(k => new KontaktListKontakt(k)).ToList();
        }
    }

    public class KontaktListKontakt
    {
        public int Id { get; }
        public string Vorname { get; }
        public string Nachname { get; }
        public string Anschrift { get; }
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }

        public KontaktListKontakt(Kontakt k)
        {
            Id = k.KontaktId;
            Vorname = k.Vorname ?? "";
            Nachname = k.Nachname ?? "";
            Email = k.Email ?? "";
            Telefon = k.Telefon ?? "";
            Mobil = k.Mobil ?? "";
            Anschrift = k.Adresse is Adresse a ?
                a.Strasse + " " + a.Hausnummer + ", " +
                a.Postleitzahl + " " + a.Stadt : "";
        }
    }
}
