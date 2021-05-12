using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.ErhaltungsaufwendungListe;
using Deeplex.Saverwalter.Print;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.Storage;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public abstract class PersonViewModel : BindableBase
    {
        public IPerson Entity { get; set; }

        public ObservableProperty<int> ErhaltungsaufwendungJahr
            = new ObservableProperty<int>(DateTime.Now.Year - 1);
        protected bool mInklusiveZusatz;
        public ObservableProperty<ImmutableList<WohnungListEntry>> Wohnungen
            = new ObservableProperty<ImmutableList<WohnungListEntry>>();

        public PersonViewModel()
        {
            Print_Erhaltungsaufwendungen = new RelayCommand(_ =>
            {
                var s = ErhaltungsaufwendungJahr.Value.ToString() + " - " + Entity.Bezeichnung;
                if (mInklusiveZusatz)
                {
                    s += " + Zusatz";
                }
                var path = ApplicationData.Current.LocalFolder.Path + @"\" + s;

                var worked = Wohnungen.Value
                    .Select(w => new ErhaltungsaufwendungWohnung(App.Walter, w.Id, ErhaltungsaufwendungJahr.Value))
                    .ToImmutableList()
                    .SaveAsDocx(path + ".docx");
                var text = worked ? "Datei gespeichert als: " + s : "Datei konnte nicht gespeichert werden.";
                App.ViewModel.ShowAlert(text, 5000);

            }, _ => Wohnungen.Value.Count > 0);
        }

        public RelayCommand Print_Erhaltungsaufwendungen;

        public Guid PersonId
        {
            get => Entity.PersonId;
            set
            {
                var old = Entity.PersonId;
                Entity.PersonId = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                var old = Entity.Notiz;
                Entity.Notiz = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public bool isVermieter
        {
            get => Entity.isVermieter;
            set
            {
                var old = Entity.isVermieter;
                Entity.isVermieter = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public bool isMieter
        {
            get => Entity.isMieter;
            set
            {
                var old = Entity.isMieter;
                Entity.isMieter = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public bool isHandwerker
        {
            get => Entity.isHandwerker;
            set
            {
                var old = Entity.isHandwerker;
                Entity.isHandwerker = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public int AdresseId => Entity.AdresseId ?? 0;

        public string Email
        {
            get => Entity.Email;
            set
            {
                var old = Entity.Email;
                Entity.Email = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Telefon
        {
            get => Entity.Telefon;
            set
            {
                var old = Entity.Telefon;
                Entity.Telefon = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Mobil
        {
            get => Entity.Mobil;
            set
            {
                var old = Entity.Mobil;
                Entity.Mobil = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Fax
        {
            get => Entity.Fax;
            set
            {
                var old = Entity.Fax;
                Entity.Fax = value;
                RaisePropertyChangedAuto(old, value);
            }
        }
    }
}
