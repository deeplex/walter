using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels.Rechnungen
{
    public sealed class ErhaltungsaufwendungenDetailViewModel : BindableBase
    {
        public Erhaltungsaufwendung Entity { get; }
        public int Id => Entity.ErhaltungsaufwendungId;

        public void selfDestruct()
        {
            App.Walter.Erhaltungsaufwendungen.Remove(Entity);
            App.SaveWalter();
        }

        public List<KontaktListEntry> Personen { get; }
        private KontaktListEntry mAussteller;
        public KontaktListEntry Aussteller
        {
            get => mAussteller;
            set
            {
                mAussteller = Personen
                    .SingleOrDefault(e => e.Guid == value?.Guid);
                var old = Entity.AusstellerId;
                Entity.AusstellerId = value?.Guid ?? Guid.Empty;
                RaisePropertyChangedAuto(old, value?.Guid);
            }
        }

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
                var old = Entity.Betrag;
                Entity.Betrag = value;
                RaisePropertyChangedAuto(old, value);
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

        public ErhaltungsaufwendungenDetailViewModel() : this(new Erhaltungsaufwendung()) { }
        public ErhaltungsaufwendungenDetailViewModel(Erhaltungsaufwendung e)
        {
            Entity = e;

            Wohnungen = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListEntry(w)).ToList();
            Wohnung = Wohnungen.Find(f => f.Id == e.Wohnung?.WohnungId);

            Personen = App.Walter.NatuerlichePersonen
                .Where(w => w.isHandwerker)
                .Select(k => new KontaktListEntry(k))
                .ToList()
                .Concat(App.Walter.JuristischePersonen
                    .Where(w => w.isHandwerker)
                    .Select(k => new KontaktListEntry(k))
                    .ToList())
                    .ToList();
            Aussteller = Personen.SingleOrDefault(s => s.Guid == e.AusstellerId);

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
                    break;
                default:
                    return;
            }

            if (Bezeichnung == "" || Bezeichnung == null || Entity.Datum == null)
            {
                return;
            }

            if (Entity.ErhaltungsaufwendungId != 0)
            {
                App.Walter.Erhaltungsaufwendungen.Update(Entity);
            }
            else
            {
                App.Walter.Erhaltungsaufwendungen.Add(Entity);
            }
            App.SaveWalter();
        }
    }
}
