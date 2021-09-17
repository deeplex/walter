using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class PersonViewModel : BindableBase
    {
        public IPerson Entity { get; set; }

        public ObservableProperty<int> ErhaltungsaufwendungJahr
            = new ObservableProperty<int>(DateTime.Now.Year - 1);
        protected bool mInklusiveZusatz;
        public ObservableProperty<ImmutableList<WohnungListEntry>> Wohnungen
            = new ObservableProperty<ImmutableList<WohnungListEntry>>();

        protected AppViewModel Avm;
        protected IAppImplementation Impl;

        public PersonViewModel(IAppImplementation impl, AppViewModel avm)
        {
            Avm = avm;
            Impl = impl;
            Print_Erhaltungsaufwendungen = new AsyncRelayCommand(async _ =>
            {
                await Utils.Files.PrintErhaltungsaufwendungen(
                    Wohnungen.Value.Select(w => w.Entity).ToList(),
                    mInklusiveZusatz,
                    ErhaltungsaufwendungJahr.Value,
                    Avm,
                    Impl);
            }, _ => Wohnungen.Value.Count > 0);
        }

        public AsyncRelayCommand Print_Erhaltungsaufwendungen;

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
