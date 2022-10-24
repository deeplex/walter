using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageDetailViewModel : DetailViewModel<Umlage>, IDetailViewModel
    {
        public override string ToString() => Entity.Typ.ToDescriptionString() + " - " + Entity.Wohnungen.GetWohnungenBezeichnung();

        public ObservableProperty<WohnungListViewModelEntry> UmlageWohnung = new();

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageschluesselUtil> Schluessel_List = Enums.Umlageschluessel;
        public List<BetriebskostentypUtil> Typen_List = Enums.Betriebskostentyp;
        public List<ZaehlerListViewModelEntry> Zaehler_List;

        public SavableProperty<string> Notiz { get; private set; }
        public SavableProperty<BetriebskostentypUtil> Typ { get; private set; }
        public SavableProperty<UmlageschluesselUtil> Schluessel { get; private set; }
        public SavableProperty<string> Beschreibung { get; private set; }
        
        public SavableProperty<double> HKVO_P7 { get; private set; }
        public SavableProperty<double> HKVO_P8 { get; private set; }
        public SavableProperty<HKVO9Util> HKVO_P9 { get; private set; }
        public SavableProperty<ZaehlerListViewModelEntry> Zaehler { get; private set; }

        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen = new();

        public void UpdateWohnungen(ImmutableList<WohnungListViewModelEntry> list)
        {
            var flagged = Wohnungen.Value.Count != list.Count;
            Wohnungen.Value = list
                .Select(e => new WohnungListViewModelEntry(e.Entity, WalterDbService))
                .ToImmutableList();
            if (flagged) Update();
        }
        public void SaveWohnungen()
        {
            if (Entity == null) return;

            // Add missing Wohnungen
            Entity.Wohnungen
                .AddRange(Wohnungen.Value.Where(w => !Entity.Wohnungen.Contains(w.Entity))
                .Select(w => w.Entity));
            // Remove old Wohnungen
            Entity.Wohnungen.RemoveAll(w => !Wohnungen.Value.Exists(v => v.Entity == w));
        }

        public override void SetEntity(Umlage r)
        {
            Entity = r;

            Notiz = new(this, r.Notiz);
            Typ = new(this, Typen_List.FirstOrDefault(e => e.Typ == r.Typ));
            Beschreibung = new(this, r.Beschreibung);
            Schluessel = new(this, Schluessel_List.FirstOrDefault(e => e.Schluessel == r.Schluessel));

            Zaehler_List = WalterDbService.ctx.ZaehlerSet
                   .Select(y => new ZaehlerListViewModelEntry(y))
                   .ToList();
            Zaehler = new(this, Zaehler_List.FirstOrDefault(e => r.Zaehler == e.Entity));

            HKVO_P7 = new(this, r.HKVO?.HKVO_P7 ?? 0.5);
            HKVO_P8 = new(this, r.HKVO?.HKVO_P8 ?? 0.5);
            HKVO_P9 = new(this,
                HKVO_P9_List.FirstOrDefault(e => e.Enum == r.HKVO?.HKVO_P9) ??
                HKVO_P9_List.FirstOrDefault(e => e.Enum == HKVO_P9A2.Satz_2));

            Wohnungen.Value = r.Wohnungen.Select(g => new WohnungListViewModelEntry(g, WalterDbService)).ToImmutableList();

            if (UmlageWohnung.Value == null)
            {
                UmlageWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }
        }

        public UmlageDetailViewModel(INotificationService ns, IWalterDbService db): base(ns, db)
        {
            Save = new RelayCommand(_ =>
            {
                Entity.Notiz = Notiz.Value;
                Entity.Typ = Typ.Value.Typ;
                Entity.Schluessel = Schluessel.Value.Schluessel;
                Entity.Zaehler = Zaehler.Value.Entity;
                Entity.Beschreibung = Beschreibung.Value;

                if (Entity.HKVO == null)
                {
                    Entity.HKVO = new HKVO();
                }

                Entity.HKVO.HKVO_P7 = HKVO_P7.Value / 100;
                Entity.HKVO.HKVO_P8 = HKVO_P8.Value / 100;
                Entity.HKVO.HKVO_P9 = HKVO_P9.Value.Enum;

                Update();
            }, _ => true);
        }

        public void Update()
        {
            SaveWohnungen();
            save();
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
            NotificationService.outOfSync =
                Entity.Notiz != Notiz.Value ||
                Entity.Typ != Typ.Value.Typ ||
                Entity.Schluessel != Schluessel.Value.Schluessel ||
                Entity.Zaehler != Zaehler.Value?.Entity ||
                Entity.Beschreibung != Beschreibung.Value ||
                Entity.HKVO?.HKVO_P7 * 100 != HKVO_P7.Value ||
                Entity.HKVO?.HKVO_P8 * 100 != HKVO_P8.Value ||
                Entity.HKVO?.HKVO_P9 != HKVO_P9.Value.Enum;
        }
    }
}
