using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragDetailVersion : BindableBase
    {
        private Vertrag Entity { get; }
        public int Id => Entity.rowid;
        public int Version => Entity.Version;
        public double KaltMiete
        {
            get => Entity.KaltMiete;
            set
            {
                var old = Entity.KaltMiete;
                Entity.KaltMiete = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public int Personenzahl
        {
            get => Entity.Personenzahl;
            set
            {
                var old = Entity.Personenzahl;
                Entity.Personenzahl = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        private VertragDetailWohnung mWohnung;
        public VertragDetailWohnung Wohnung
        {
            get => mWohnung;
            set
            {
                if (value == null) return;
                mWohnung = value;
                Entity.WohnungId = mWohnung.Id;
                var old = mWohnung.Id;
                if (RaisePropertyChangedAuto(old, value.Id))
                {
                    RaisePropertyChanged(nameof(Vermieter));
                }
            }
        }
        public DateTimeOffset Beginn
        {
            get => Entity.Beginn.AsUtcKind();
            set
            {
                var old = Entity.Beginn;
                Entity.Beginn = value.UtcDateTime;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public DateTimeOffset? Ende
        {
            get => Entity.Ende?.AsUtcKind();
            set
            {
                var old = Entity.Ende;
                Entity.Ende = value?.UtcDateTime;
                RaisePropertyChangedAuto(old, value?.UtcDateTime);
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

        public string Vermieter => Wohnung.Besitzer;
        private VertragDetailKontakt mAnsprechpartner;
        public VertragDetailKontakt Ansprechpartner
        {
            get => mAnsprechpartner;
            set
            {
                mAnsprechpartner = value;
                Entity.AnsprechpartnerId = mAnsprechpartner.PersonId;
                RaisePropertyChangedAuto();
            }
        }

        public VertragDetailVersion(int id) : this(App.Walter.Vertraege.Find(id)) { }
        public VertragDetailVersion(Vertrag v)
        {
            Entity = v;

            if (v.AnsprechpartnerId != Guid.Empty && v.AnsprechpartnerId != null)
            {
                Ansprechpartner = new VertragDetailKontakt(v.AnsprechpartnerId.Value);
            }

            if (v.Wohnung != null)
            {
                Wohnung = new VertragDetailWohnung(v.Wohnung);
            }
            PropertyChanged += OnUpdate;
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Wohnung):
                case nameof(Beginn):
                case nameof(Ende):
                case nameof(Notiz):
                case nameof(Personenzahl):
                case nameof(KaltMiete):
                // case nameof(VersionsNotiz):
                case nameof(Ansprechpartner):
                    break;
                default:
                    return;
            }

            if (Entity.VertragId == null ||
                Entity.Beginn == null ||
                (Entity.Wohnung == null && Entity.WohnungId == 0) ||
                Entity.AnsprechpartnerId == null)
            {
                return;
            }

            if (Entity.rowid != 0)
            {
                App.Walter.Vertraege.Update(Entity);
            }
            else
            {
                App.Walter.Vertraege.Add(Entity);
            }
            App.SaveWalter();
        }
    }

}
