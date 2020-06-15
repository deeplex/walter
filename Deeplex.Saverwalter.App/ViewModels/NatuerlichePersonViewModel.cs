﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class NatuerlichePersonViewModel : BindableBase
    {
        public NatuerlichePerson Entity { get; }
        public int Id { get; }

        public ImmutableList<AdresseViewModel> AlleAdressen { get; }
        public ImmutableList<Anrede> Anreden { get; }

        public void selfDestruct()
        {
            App.Walter.NatuerlichePersonen.Remove(Entity);
            App.Walter.SaveChanges();
        }

        public Anrede Anrede
        {
            get => Entity.Anrede;
            set
            {
                Entity.Anrede = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Vorname
        {
            get => Entity.Vorname;
            set
            {
                Entity.Vorname = value;
                RaisePropertyChangedAuto();
            }
        }
        public string Nachname
        {
            get => Entity.Nachname;
            set
            {
                Entity.Nachname = value;
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
        private AdresseViewModel mAdresse;
        public AdresseViewModel Adresse
        {
            get => mAdresse;
            set
            {
                Entity.Adresse = AdresseViewModel.GetAdresse(value);
                mAdresse = value;
                RaisePropertyChangedAuto();
            }
        }

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

        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                Entity.Notiz = value;
                RaisePropertyChangedAuto();
            }
        }

        public ObservableProperty<List<NatuerlichePersonVertrag>> Vertraege
            = new ObservableProperty<List<NatuerlichePersonVertrag>>();

        public string Name => Vorname + " " + Nachname;

        public NatuerlichePersonViewModel(int id)
            : this(App.Walter.NatuerlichePersonen.Find(id)) { }

        public NatuerlichePersonViewModel() : this(new NatuerlichePerson()) { IsInEdit.Value = true; }
        private NatuerlichePersonViewModel(NatuerlichePerson k)
        {
            Entity = k;
            Id = k.NatuerlichePersonId;

            AlleAdressen = App.Walter.Adressen
                .Select(a => new AdresseViewModel(a))
                .ToImmutableList();

            Anreden = Enum.GetValues(typeof(Anrede)).Cast<Anrede>().ToImmutableList();

            if (k.Adresse != null)
            {
                Adresse = new AdresseViewModel(k.Adresse);
            }

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung).ToList()
                .Where(v => App.Walter.MieterSet.ToList().Exists(m => m.VertragId == v.VertragId))
                .Select(v => new NatuerlichePersonVertrag(v.VertragId))
                .ToList();

            PropertyChanged += OnUpdate;
        }

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Anrede):
                case nameof(Vorname):
                case nameof(Nachname):
                case nameof(Adresse):
                case nameof(Email):
                case nameof(Telefon):
                case nameof(Mobil):
                case nameof(Fax):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if (Entity.Nachname == null)
            {
                return;
            }

            if (Entity.NatuerlichePersonId != 0)
            {
                App.Walter.NatuerlichePersonen.Update(Entity);
            }
            else
            {
                App.Walter.NatuerlichePersonen.Add(Entity);
            }
            App.Walter.SaveChanges();
        }
    }

    public sealed class NatuerlichePersonVertrag
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<string> Anschrift { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Wohnung { get; } = new ObservableProperty<string>();
        public ObservableProperty<DateTimeOffset> Beginn { get; } = new ObservableProperty<DateTimeOffset>();
        public ObservableProperty<DateTimeOffset?> Ende { get; } = new ObservableProperty<DateTimeOffset?>();
        public ObservableProperty<string> AuflistungMieter { get; } = new ObservableProperty<string>();
        public ObservableProperty<List<NatuerlichePersonVertrag>> Versionen { get; }
            = new ObservableProperty<List<NatuerlichePersonVertrag>>();

        public NatuerlichePersonVertrag(Guid id)
            : this(App.Walter.Vertraege.Where(v => v.VertragId == id)) { }

        private NatuerlichePersonVertrag(IEnumerable<Vertrag> v)
            : this(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new NatuerlichePersonVertrag(vs)).ToList();
            Beginn.Value = Versionen.Value.First().Beginn.Value;
            Ende.Value = Versionen.Value.Last().Ende.Value;
        }

        private NatuerlichePersonVertrag(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Anschrift.Value = v.Wohnung is Wohnung w ? AdresseViewModel.Anschrift(w) : "";
            Wohnung.Value = v.Wohnung is Wohnung ww ? ww.Bezeichnung : "";

            Beginn.Value = v.Beginn.AsUtcKind();
            Ende.Value = v.Ende?.AsUtcKind();

            var bs = App.Walter.MieterSet.Where(m => m.VertragId == v.VertragId).ToList();
            var cs = bs.Select(b => App.Walter.FindPerson(b.PersonId).Bezeichnung);
            AuflistungMieter.Value = string.Join(", ", cs);
        }
    }
}
