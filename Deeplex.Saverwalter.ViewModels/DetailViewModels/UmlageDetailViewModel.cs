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

        public Umlage Entity { get; private set; }
        public int Id => Entity.UmlageId;

        public ObservableProperty<WohnungListViewModelEntry> UmlageWohnung = new();

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageschluesselUtil> Schluessel_List = Enums.UmlageSchluessel;
        public List<BetriebskostentypUtil> Typen_List = Enums.Betriebskostentyp;

        public SavableProperty<string> Notiz { get; private set; }
        public SavableProperty<BetriebskostentypUtil> Typ { get; private set; }
        public SavableProperty<UmlageschluesselUtil> Schluessel { get; private set; }
        public SavableProperty<string> Beschreibung { get; private set; }
        //public SavableProperty<HKVO_P9A2?> HKVO_P9 { get; private set; }

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
            Typ = new(this, new(r.Typ));
            Beschreibung = new(this, r.Beschreibung);
            //HKVO_P9 = new(this, r.HKVO_P9);
            Schluessel = new(this, new(r.Schluessel));
            Schluessel.Value = Schluessel_List.FirstOrDefault(e => e.Schluessel == r.Schluessel);
            Typ.Value = Typen_List.FirstOrDefault(e => e.Typ == r.Typ);

            Wohnungen.Value = r.Wohnungen.Select(g => new WohnungListViewModelEntry(g, WalterDbService)).ToImmutableList();

            if (UmlageWohnung.Value == null)
            {
                UmlageWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }
        }

        public UmlageDetailViewModel(INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            NotificationService = ns;

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    WalterDbService.ctx.Umlagen.Remove(Entity);
                    WalterDbService.SaveWalter();
                }
            });

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public void Update()
        {
            if (Entity.UmlageId != 0)
            {
                WalterDbService.ctx.Umlagen.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.Umlagen.Add(Entity);
            }
            SaveWohnungen();
            WalterDbService.SaveWalter();
            checkForChanges();
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
                Entity.Beschreibung != Beschreibung.Value;
            //checkNullable(Entity.HKVO_P9, HKVO_P9.Value);
        }

        private void save()
        {
            Entity.Notiz = Notiz.Value;
            Entity.Typ = Typ.Value.Typ;
            Entity.Schluessel = Schluessel.Value.Schluessel;
            Entity.Beschreibung = Beschreibung.Value;
            //if (Entity.HKVO_P9 != null && HKVO_P9.Value != 0)
            //{
            //    Entity.HKVO_P9 = HKVO_P9.Value;
            //}

            Update();
        }
    }
}
