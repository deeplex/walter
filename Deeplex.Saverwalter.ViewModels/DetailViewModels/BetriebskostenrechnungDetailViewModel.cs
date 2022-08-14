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

        public int Id => Entity.BetriebskostenrechnungId;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new();
        public ObservableProperty<bool> ZeigeVorlagen = new();
        public ObservableProperty<WohnungListViewModelEntry> BetriebskostenrechnungsWohnung = new();

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageschluesselUtil> Schluessel_List = Enums.Umlageschluessel;
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

        private List<UmlageListViewModelEntry> updateUmlagenList(Betriebskostentyp e)
        {
            return WalterDbService.ctx.Umlagen
                .Include(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .ToList()
                .Where(u => u.Typ == e)
                //.ToList()
                .Select(e => new UmlageListViewModelEntry(e))
                .ToList();
        }

        public BetriebskostenrechnungDetailViewModel(IWalterDbService db, INotificationService ns) : base(ns, db)
        {
            Typen_List = Enums.Betriebskostentyp
                .Where(e => WalterDbService.ctx.Betriebskostenrechnungen
                    .Include(b => b.Umlage)
                    .ToList()
                    .Exists(br => br.Umlage.Typ == e.Typ))
                .ToList();

            Save = new RelayCommand(_ =>
            {
                Entity.Betrag = Betrag.Value;
                Entity.Datum = Datum.Value.UtcDateTime;
                Entity.Notiz = Notiz.Value;
                Entity.BetreffendesJahr = BetreffendesJahr.Value;
                Entity.Umlage = Umlage.Value.Entity;

                save();
            }, _ => true);
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
                Wohnungen.Value = r.Umlage.Wohnungen.Select(g => new WohnungListViewModelEntry(g, WalterDbService)).ToImmutableList();
            }
            else
            {
                Wohnungen.Value = new List<WohnungListViewModelEntry>().ToImmutableList();
            }
            if (BetriebskostenrechnungsWohnung.Value == null)
            {
                BetriebskostenrechnungsWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }

            AllgemeinZaehler_List = WalterDbService.ctx.ZaehlerSet
                .Select(a => new ZaehlerListViewModelEntry(a))
                .ToList();
            AllgemeinZaehler = new(this, AllgemeinZaehler_List.FirstOrDefault(e => e.Id == r.Umlage?.Zaehler?.ZaehlerId));

            Umlagen_List.Value = updateUmlagenList(r.Umlage.Typ);
            Umlage = new(this, Umlagen_List.Value.FirstOrDefault(e => e.Id == r.Umlage.UmlageId));
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
                NotificationService.outOfSync = true;
                return;
            }

            NotificationService.outOfSync =
                Entity.Umlage.UmlageId != Umlage.Value.Entity.UmlageId ||
                Entity.Betrag != Betrag.Value ||
                Entity.Datum.AsUtcKind() != Datum.Value ||
                Entity.Notiz != Notiz.Value ||
                Entity.BetreffendesJahr != BetreffendesJahr.Value;
        }
    }
}
