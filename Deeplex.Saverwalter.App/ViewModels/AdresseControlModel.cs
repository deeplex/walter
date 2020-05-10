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
    public class AdresseControlModel
    {
        public int Id { get; }
        public ObservableProperty<string> Strasse = new ObservableProperty<string>();
        public ObservableProperty<string> Hausnummer = new ObservableProperty<string>();
        public ObservableProperty<string> Postleitzahl = new ObservableProperty<string>();
        public ObservableProperty<string> Stadt = new ObservableProperty<string>();

        public AdresseControlModel(int id) : this(App.Walter.Adressen.Find(id)) { }

        public AdresseControlModel(Adresse a)
        {
            Id = a.AdresseId;
            Strasse.Value = a.Strasse;
            Hausnummer.Value = a.Hausnummer;
            Postleitzahl.Value = a.Postleitzahl;
            Stadt.Value = a.Stadt;
        }
    }
}