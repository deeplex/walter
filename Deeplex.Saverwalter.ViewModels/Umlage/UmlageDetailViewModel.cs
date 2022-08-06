using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageDetailViewModel : BindableBase, IDetailViewModel
    {
        public override string ToString() => Entity.Typ.ToDescriptionString() + " - " + Entity.Wohnungen.GetWohnungenBezeichnung();

        public Umlage Entity { get; }
        public int Id => Entity.UmlageId;

        public ObservableProperty<WohnungListViewModelEntry> UmlageWohnung = new();

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageSchluesselUtil> Schluessel_List = Enums.UmlageSchluessel;
        public List<BetriebskostentypUtil> Typen_List = Enums.Betriebskostentyp;

        public SavableProperty<string> Notiz { get; }
        public SavableProperty<BetriebskostentypUtil> Typ { get; }
        public SavableProperty<UmlageSchluesselUtil> Schluessel { get; }
        public SavableProperty<string> Beschreibung { get; }
        //public SavableProperty<HKVO_P9A2?> HKVO_P9 { get; }

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
            Entity.Wohnungen
                .AddRange(Wohnungen.Value.Where(w => !Entity.Wohnungen.Contains(w.Entity))
                .Select(w => w.Entity));
            // Remove old Wohnungen
            Entity.Wohnungen.RemoveAll(w => !Wohnungen.Value.Exists(v => v.Entity == w));
        }

        public UmlageDetailViewModel(Umlage r, INotificationService ns, IWalterDbService db)
        {
            Entity = r;
            Db = db;
            NotifcationService = ns;

            Notiz = new(this, r.Notiz);
            Typ = new(this, new(r.Typ));
            Beschreibung = new(this, r.Beschreibung);
            //HKVO_P9 = new(this, r.HKVO_P9);
            Schluessel = new(this, new(r.Schluessel));
            Schluessel.Value = Schluessel_List.FirstOrDefault(e => e.Schluessel == r.Schluessel);
            Typ.Value = Typen_List.FirstOrDefault(e => e.Typ == r.Typ);

            Wohnungen.Value = r.Wohnungen.Select(g => new WohnungListViewModelEntry(g, Db)).ToImmutableList();
            if (UmlageWohnung.Value == null)
            {
                UmlageWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotifcationService.Confirmation())
                {
                    Db.ctx.Umlagen.Remove(Entity);
                    Db.SaveWalter();
                }
            });

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public UmlageDetailViewModel(Umlage r, int w, List<Wohnung> l, INotificationService ns, IWalterDbService db) : this(r, w, ns, db)
        {
            Wohnungen.Value = l.Select(e => new WohnungListViewModelEntry(e, db)).ToImmutableList();
        }

        public UmlageDetailViewModel(Umlage r, int w, INotificationService ns, IWalterDbService avm) : this(r, ns, avm)
        {
            UmlageWohnung.Value = Wohnungen.Value.Find(e => e.Id == w);
        }

        public UmlageDetailViewModel(INotificationService ns, IWalterDbService db) : this(new Umlage(), ns, db) { }

        public RelayCommand Save { get; }
        public AsyncRelayCommand Delete { get; }

        public void Update()
        {
            if (Entity.UmlageId != 0)
            {
                Db.ctx.Umlagen.Update(Entity);
            }
            else
            {
                Db.ctx.Umlagen.Add(Entity);
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

        public void checkForChanges()
        {
            NotifcationService.outOfSync =
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
