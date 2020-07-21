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

        public ImmutableList<Anrede> Anreden { get; }

        public void selfDestruct()
        {
            App.Walter.NatuerlichePersonen.Remove(Entity);
            App.SaveWalter();
        }

        private void update<U>(string property, U value)
        {
            if (Entity == null) return;
            var type = Entity.GetType();
            var prop = type.GetProperty(property);
            var val = prop.GetValue(Entity, null);
            if (!value.Equals(val))
            {
                prop.SetValue(Entity, value);
                RaisePropertyChanged(property);
            };
        }

        public Guid PersonId
        {
            get => Entity.PersonId;
            set => update(nameof(Entity.PersonId), value);

        }

        public Anrede Anrede
        {
            get => Entity.Anrede;
            set => update(nameof(Entity.Anrede), value);
        }

        public string Vorname
        {
            get => Entity.Vorname;
            set => update(nameof(Entity.Vorname), value);
        }
        public string Nachname
        {
            get => Entity.Nachname;
            set => update(nameof(Entity.Nachname), value);
        }
        public bool isVermieter
        {
            get => Entity.isVermieter;
            set => update(nameof(Entity.isVermieter), value);

        }
        public bool isMieter
        {
            get => Entity.isMieter;
            set => update(nameof(Entity.isMieter), value);

        }
        public bool isHandwerker
        {
            get => Entity.isHandwerker;
            set => update(nameof(Entity.isHandwerker), value);

        }
        public int AdresseId => Entity.AdresseId ?? 0;


        public string Email
        {
            get => Entity.Email;
            set => update(nameof(Entity.Email), value);
        }

        public string Telefon
        {
            get => Entity.Telefon;
            set => update(nameof(Entity.Telefon), value);
        }

        public string Mobil
        {
            get => Entity.Mobil;
            set => update(nameof(Entity.Mobil), value);
        }

        public string Fax
        {
            get => Entity.Fax;
            set => update(nameof(Entity.Fax), value);
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set => update(nameof(Entity.Notiz), value);
        }

        public ObservableProperty<List<NatuerlichePersonVertrag>> Vertraege
            = new ObservableProperty<List<NatuerlichePersonVertrag>>();

        public string Name => Vorname + " " + Nachname;

        public NatuerlichePersonViewModel(int id)
            : this(App.Walter.NatuerlichePersonen.Find(id)) { }

        public NatuerlichePersonViewModel() : this(new NatuerlichePerson()) { IsInEdit.Value = true; }
        public NatuerlichePersonViewModel(NatuerlichePerson k)
        {
            Entity = k;
            Id = k.NatuerlichePersonId;

            Anreden = Enum.GetValues(typeof(Anrede)).Cast<Anrede>().ToImmutableList();

            Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung).ToList()
                .Where(v => App.Walter.MieterSet.ToList().Exists(m => m.VertragId == v.VertragId))
                .Select(v => new NatuerlichePersonVertrag(v.VertragId))
                .ToList();

            PropertyChanged += OnUpdate;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.NatuerlichePersonAnhaenge, k), _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>(false);
        public bool IsNotInEdit => !IsInEdit.Value;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Anrede):
                case nameof(Vorname):
                case nameof(Nachname):
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
            App.SaveWalter();
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
