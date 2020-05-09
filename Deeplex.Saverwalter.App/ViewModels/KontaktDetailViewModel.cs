﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Deeplex.Utils.ObjectModel;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KontaktDetailViewModel : BindableBase
    {
        public int Id { get; }
        public ObservableProperty<string> Vorname { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Nachname { get; } = new ObservableProperty<string>();
        public ObservableProperty<int> Adresse { get; } = new ObservableProperty<int>();
        public ObservableProperty<string> Email { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Telefon { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Mobil { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Fax { get; } = new ObservableProperty<string>();

        public string Name => Vorname.Value + " " + Nachname.Value;
        
        public KontaktDetailViewModel(int id)
            : this(App.Walter.Kontakte.Find(id))
        {
        }

        private KontaktDetailViewModel(Kontakt k)
        {
            Id = k.KontaktId;
            Vorname.Value = k.Vorname ?? "";
            Nachname.Value = k.Nachname ?? "";
            Adresse.Value = k.AdresseId ?? 0;
            Email.Value = k.Email ?? "";
            Fax.Value = k.Fax ?? "";
            Telefon.Value = k.Telefon ?? "";
            Mobil.Value = k.Mobil ?? "";

            BeginEdit = new RelayCommand(_ => IsInEdit.Value = true, _ => !IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => BeginEdit.RaiseCanExecuteChanged(ev);

            SaveEdit = new RelayCommand(_ =>
            {
                IsInEdit.Value = false;

                k.Vorname = Vorname.Value;
                k.Nachname = Nachname.Value;
                k.Email = Email.Value;
                k.Telefon = Telefon.Value;
                k.Mobil = Mobil.Value;
                k.Fax = Fax.Value;

                App.Walter.Kontakte.Update(k);
                App.Walter.SaveChanges();

            }, _ => IsInEdit.Value);
            IsInEdit.PropertyChanged += (_, ev) => SaveEdit.RaiseCanExecuteChanged(ev);

            IsInEdit.PropertyChanged += (_, ev) => RaisePropertyChanged(nameof(IsNotInEdit));
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        public RelayCommand BeginEdit { get; }
        public RelayCommand SaveEdit { get; }
    }
}
