using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenDetailViewModel : BindableBase
    {
        public Erhaltungsaufwendung Entity { get; }
        public int Id => Entity.ErhaltungsaufwendungId;

        public async Task selfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Db.ctx.Erhaltungsaufwendungen.Remove(Entity);
                Db.SaveWalter();
            }
        }

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Personen
            = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();
        private KontaktListViewModelEntry mAussteller;
        public KontaktListViewModelEntry Aussteller
        {
            get => mAussteller;
            set
            {
                mAussteller = Personen.Value
                    .SingleOrDefault(e => e.Entity.PersonId == value?.Entity.PersonId);
                var old = Entity.AusstellerId;
                Entity.AusstellerId = value?.Entity.PersonId ?? Guid.Empty;
                RaisePropertyChangedAuto(old, value?.Entity.PersonId);
            }
        }

        public ObservableProperty<string> QuickPerson
            = new ObservableProperty<string>();

        public List<WohnungListViewModelEntry> Wohnungen { get; }
        private WohnungListViewModelEntry mWohnung;
        public WohnungListViewModelEntry Wohnung
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

        private IWalterDbService Db;
        private IAppImplementation Impl;

        public ErhaltungsaufwendungenDetailViewModel(IAppImplementation impl, IWalterDbService db) : this(new Erhaltungsaufwendung(), impl, db) { }
        public ErhaltungsaufwendungenDetailViewModel(Erhaltungsaufwendung e, IAppImplementation impl, IWalterDbService db)
        {
            Entity = e;
            Db = db;
            Impl = impl;

            Wohnungen = Db.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModelEntry(w, db)).ToList();
            Wohnung = Wohnungen.Find(f => f.Id == e.Wohnung?.WohnungId);

            Personen.Value = Db.ctx.NatuerlichePersonen
                .Where(w => w.isHandwerker)
                .Select(k => new KontaktListViewModelEntry(k))
                .ToList()
                .Concat(Db.ctx.JuristischePersonen
                    .Where(w => w.isHandwerker)
                    .Select(k => new KontaktListViewModelEntry(k))
                    .ToList())
                    .ToImmutableList();
            Aussteller = Personen.Value.SingleOrDefault(s => s.Entity.PersonId == e.AusstellerId);

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
                Db.ctx.Erhaltungsaufwendungen.Update(Entity);
            }
            else
            {
                Db.ctx.Erhaltungsaufwendungen.Add(Entity);
            }
            Db.SaveWalter();
        }
    }
}
