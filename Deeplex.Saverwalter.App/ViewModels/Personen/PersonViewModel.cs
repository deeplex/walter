using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public abstract class PersonViewModel : BindableBase
    {
        protected IPerson Entity { get; set; }

        public Guid PersonId
        {
            get => Entity.PersonId;
            set
            {
                Entity.PersonId = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                Entity.Notiz = value;
                RaisePropertyChangedAuto();
            }
        }

        public bool isVermieter
        {
            get => Entity.isVermieter;
            set
            {
                Entity.isVermieter = value;
                RaisePropertyChangedAuto();
            }
        }

        public bool isMieter
        {
            get => Entity.isMieter;
            set
            {
                Entity.isMieter = value;
                RaisePropertyChangedAuto();
            }
        }

        public bool isHandwerker
        {
            get => Entity.isHandwerker;
            set
            {
                Entity.isHandwerker = value;
                RaisePropertyChangedAuto();
            }
        }

        public int AdresseId => Entity.AdresseId ?? 0;

        public string Email
        {
            get => Entity.Email;
            set
            {
                Entity.Email = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Telefon
        {
            get => Entity.Telefon;
            set
            {
                Entity.Telefon = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Mobil
        {
            get => Entity.Mobil;
            set
            {
                Entity.Mobil = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Fax
        {
            get => Entity.Fax;
            set
            {
                Entity.Fax = value;
                RaisePropertyChangedAuto();
            }
        }

        public AsyncRelayCommand AttachFile;

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;
    }
}
