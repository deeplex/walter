using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels.Rechnungen
{
    public sealed class ErhaltungsaufwendungenDetailViewModel : BindableBase
    {
        public Erhaltungsaufwendung Entity { get; }
        public int Id => Entity.ErhaltungsaufwendungId;

        public void selfDestruct()
        {
            Impl.ctx.Erhaltungsaufwendungen.Remove(Entity);
            Impl.SaveWalter();
        }

        public ObservableProperty<ImmutableList<KontaktListEntry>> Personen
            = new ObservableProperty<ImmutableList<KontaktListEntry>>();
        private KontaktListEntry mAussteller;
        public KontaktListEntry Aussteller
        {
            get => mAussteller;
            set
            {
                mAussteller = Personen.Value
                    .SingleOrDefault(e => e.Guid == value?.Guid);
                var old = Entity.AusstellerId;
                Entity.AusstellerId = value?.Guid ?? Guid.Empty;
                RaisePropertyChangedAuto(old, value?.Guid);
            }
        }

        public ObservableProperty<string> QuickPerson
            = new ObservableProperty<string>();

        public List<WohnungListEntry> Wohnungen { get; }
        private WohnungListEntry mWohnung;
        public WohnungListEntry Wohnung
        {
            get => mWohnung;
            set
            {
                mWohnung = Wohnungen.SingleOrDefault(e => e.Id == value?.Id);
                var old = Entity.Wohnung;
                Entity.Wohnung = value?.Entity;
                mWohnung = value;
                RaisePropertyChangedAuto(old, value?.Entity);
            }
        }


        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                var old = Entity.Bezeichnung;
                Entity.Bezeichnung = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public double Betrag
        {
            get => Entity.Betrag;
            set
            {
                var val = Double.IsNaN(value) ? 0 : value;
                var old = Entity.Betrag;
                Entity.Betrag = val;
                RaisePropertyChangedAuto(old, val);
            }
        }

        public DateTimeOffset Datum
        {
            get => Entity.Datum.AsMin();
            set
            {
                var old = Entity.Datum;
                Entity.Datum = value.Date.AsMin();
                RaisePropertyChangedAuto(old, value.Date.AsMin());
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

        private IAppImplementation Impl;

        public ErhaltungsaufwendungenDetailViewModel(IAppImplementation impl) : this(new Erhaltungsaufwendung(), impl) { }
        public ErhaltungsaufwendungenDetailViewModel(Erhaltungsaufwendung e, IAppImplementation impl)
        {
            Entity = e;
            Impl = impl;

            Wohnungen = Impl.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListEntry(w, Impl)).ToList();
            Wohnung = Wohnungen.Find(f => f.Id == e.Wohnung?.WohnungId);

            Personen.Value = Impl.ctx.NatuerlichePersonen
                .Where(w => w.isHandwerker)
                .Select(k => new KontaktListEntry(k))
                .ToList()
                .Concat(Impl.ctx.JuristischePersonen
                    .Where(w => w.isHandwerker)
                    .Select(k => new KontaktListEntry(k))
                    .ToList())
                    .ToImmutableList();
            Aussteller = Personen.Value.SingleOrDefault(s => s.Guid == e.AusstellerId);

            PropertyChanged += OnUpdate;
            AttachFile = new AsyncRelayCommand(async _ =>
               /* TODO */await Task.FromResult<object>(null), _ => false);
        }

        public AsyncRelayCommand AttachFile;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Bezeichnung):
                case nameof(Datum):
                case nameof(Wohnung):
                case nameof(Aussteller):
                case nameof(Betrag):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if (Bezeichnung == "" || Bezeichnung == null || Datum == null || Wohnung == null || Aussteller == null)
            {
                return;
            }

            if (Entity.ErhaltungsaufwendungId != 0)
            {
                Impl.ctx.Erhaltungsaufwendungen.Update(Entity);
            }
            else
            {
                Impl.ctx.Erhaltungsaufwendungen.Add(Entity);
            }
            Impl.SaveWalter();
        }
    }
}
