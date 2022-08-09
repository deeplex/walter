using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungDetailViewModel : DetailViewModel<Betriebskostenrechnung>, IDetailViewModel
    {
        public override string ToString() => Entity.Umlage.Typ.ToDescriptionString() + " - " + Entity.GetWohnungenBezeichnung();

        public Betriebskostenrechnung Entity { get; private set; }
        public int Id => Entity.BetriebskostenrechnungId;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new();
        public ObservableProperty<bool> ZeigeVorlagen = new();
        public ObservableProperty<WohnungListViewModelEntry> BetriebskostenrechnungsWohnung = new();

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageSchluesselUtil> Schluessel_List = Enums.UmlageSchluessel;
        public ObservableProperty<List<UmlageListViewModelEntry>> Umlagen_List { get; } = new();
        public List<ZaehlerListViewModelEntry> AllgemeinZaehler_List;

        public SavableProperty<ZaehlerListViewModelEntry> AllgemeinZaehler { get; private set; }

        public List<BetriebskostentypUtil> Typen_List { get; private set; }
        public BetriebskostentypUtil Typ
        {
            get => Typen_List.FirstOrDefault(t => t.Typ == Umlage.Value?.Typ);
            set
            {
                Umlagen_List.Value = updateUmlagenList(value.Typ);
                if (value != Typ)
                {
                    Umlage.Value = Umlagen_List.Value.First();
                }
                RaisePropertyChangedAuto();
            }
        }

        public SavableProperty<double> Betrag { get; private set; }
        public SavableProperty<int> BetreffendesJahr { get; private set; }
        public SavableProperty<DateTimeOffset> Datum { get; private set; }
        public SavableProperty<string> Notiz { get; private set; }
        public SavableProperty<UmlageListViewModelEntry> Umlage { get; private set; }

        public DateTimeOffset? BetreffendesJahrDatum // TODO Remove
        {
            get => new DateTime(BetreffendesJahr.Value, 1, 1);
            set
            {
                BetreffendesJahr.Value = value?.Year ?? DateTime.Now.Year - 1;
            }
        }

        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen = new();

        public IWalterDbService Db;
        public INotificationService NotifcationService;

        public void UpdateWohnungen(ImmutableList<WohnungListViewModelEntry> list)
        {
            var flagged = Wohnungen.Value.Count != list.Count;
            Wohnungen.Value = list
                .Select(e => new WohnungListViewModelEntry(e.Entity, Db))
                .ToImmutableList();
            if (flagged) Update();
        }
        public void SaveWohnungen()
        {
            if (Entity == null) return;

            // Add missing Wohnungen
            Entity.Umlage.Wohnungen
                .AddRange(Wohnungen.Value.Where(w => !Entity.Umlage.Wohnungen.Contains(w.Entity))
                .Select(w => w.Entity));
            // Remove old Wohnungen
            Entity.Umlage.Wohnungen.RemoveAll(w => !Wohnungen.Value.Exists(v => v.Entity == w));
        }

        private List<UmlageListViewModelEntry> updateUmlagenList(Betriebskostentyp e)
        {
            return Db.ctx.Umlagen
                .Include(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .ToList()
                .Where(u => u.Typ == e)
                //.ToList()
                .Select(e => new UmlageListViewModelEntry(e))
                .ToList();
        }

        public BetriebskostenrechnungDetailViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;

            Typen_List = Enums.Betriebskostentyp.Where(e => WalterDbService.ctx.Betriebskostenrechnungen.Include(b => b.Umlage).ToList().Exists(br => br.Umlage.Typ == e.Typ)).ToList();


            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotifcationService.Confirmation())
                {
                    Db.ctx.Betriebskostenrechnungen.Remove(Entity);
                    Db.SaveWalter();
                }
            });

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public override void SetEntity(Betriebskostenrechnung r)
        {
            Entity = r;

            var datum = r.Datum == default ? DateTime.Now : r.Datum;

            Betrag = new(this, r.Betrag);
            Datum = new(this, datum.AsUtcKind());
            Notiz = new(this, r.Notiz);
            BetreffendesJahr = new(this, datum.Year);

            if (r.Umlage != null)
            {
                Wohnungen.Value = r.Umlage.Wohnungen.Select(g => new WohnungListViewModelEntry(g, Db)).ToImmutableList();
            }
            else
            {
                Wohnungen.Value = new List<WohnungListViewModelEntry>().ToImmutableList();
            }
            if (BetriebskostenrechnungsWohnung.Value == null)
            {
                BetriebskostenrechnungsWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }

            AllgemeinZaehler_List = Db.ctx.ZaehlerSet
                .Select(a => new ZaehlerListViewModelEntry(a))
                .ToList();
            AllgemeinZaehler = new(this, AllgemeinZaehler_List.FirstOrDefault(e => e.Id == r.Umlage?.Zaehler?.ZaehlerId));

            Umlagen_List.Value = updateUmlagenList(r.Umlage.Typ);
            Umlage = new(this, Umlagen_List.Value.FirstOrDefault(e => e.Id == r.Umlage.UmlageId));
        }

        public void Update()
        {
            if (Entity.BetriebskostenrechnungId != 0)
            {
                Db.ctx.Betriebskostenrechnungen.Update(Entity);
            }
            else
            {
                Db.ctx.Betriebskostenrechnungen.Add(Entity);
            }
            SaveWohnungen();
            Db.SaveWalter();
            NotifcationService.outOfSync = false;
        }

        public bool checkNullable<T>(object a, T b)
        {
            if (a == null && b.Equals(default(T)))
            {
                return false;
            }
            else
            {
                return a.Equals(b);
            }

        }

        public override void checkForChanges()
        {
            if (Umlage.Value == null)
            {
                NotifcationService.outOfSync = true;
                return;
            }

            NotifcationService.outOfSync =
                Entity.Umlage.UmlageId != Umlage.Value.Entity.UmlageId ||
                Entity.Betrag != Betrag.Value ||
                Entity.Datum.AsUtcKind() != Datum.Value ||
                Entity.Notiz != Notiz.Value ||
                Entity.BetreffendesJahr != BetreffendesJahr.Value;
        }

        private void save()
        {
            Entity.Betrag = Betrag.Value;
            Entity.Datum = Datum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;
            Entity.BetreffendesJahr = BetreffendesJahr.Value;
            Entity.Umlage = Umlage.Value.Entity;
            
            Update();
        }
    }
}
