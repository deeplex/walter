using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Deeplex.Utils.ObjectModel;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KontaktViewModel
    {
        public int Id { get; }
        public ObservableProperty<string> Vorname { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Nachname { get; } = new ObservableProperty<string>();
        public ObservableProperty<Adresse> Adresse { get; } = new ObservableProperty<Adresse>();
        public ObservableProperty<string> Email { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Telefon { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Mobil { get; } = new ObservableProperty<string>();

        public string Anschrift => Adresse.Value is Adresse a ?
            a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt : "";
        
        public KontaktViewModel(Kontakt k)
        {
            Id = k.KontaktId;
            Vorname.Value = k.Vorname ?? "";
            Nachname.Value = k.Nachname ?? "";
            Adresse.Value = k.Adresse;
            Email.Value = k.Email ?? "";
            Telefon.Value = k.Telefon ?? "";
            Mobil.Value = k.Mobil ?? "";
        }

        public void BeginEdit()
        {
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            throw new NotImplementedException();
        }

        public void EndEdit()
        {
            throw new NotImplementedException();
        }
    }
}
