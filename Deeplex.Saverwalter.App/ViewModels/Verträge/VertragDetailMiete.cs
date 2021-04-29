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
    public sealed class VertragDetailMiete : BindableBase
    {
        public void selfDestruct()
        {
            App.Walter.Remove(Entity);
            App.SaveWalter();
        }
        private Miete Entity { get; }

        public DateTimeOffset Zahlungsdatum
        {
            get => Entity.Zahlungsdatum.AsUtcKind();
            set
            {
                var old = Entity.Zahlungsdatum;
                Entity.Zahlungsdatum = value.UtcDateTime;
                RaisePropertyChangedAuto(old, value.UtcDateTime);
            }
        }

        public DateTimeOffset BetreffenderMonat
        {
            get => Entity.BetreffenderMonat.AsUtcKind();
            set
            {
                var old = Entity.BetreffenderMonat;
                Entity.Zahlungsdatum = new DateTime(value.Year, value.Month, 1).AsUtcKind();
                RaisePropertyChangedAuto(old, Entity.Zahlungsdatum);
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

        public double Betrag
        {
            get => Entity.Betrag ?? 0;
            set
            {
                var old = Entity.Betrag;
                Entity.Betrag = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public VertragDetailMiete(Guid vertragId)
            : this(new Miete
            {
                VertragId = vertragId,
                Zahlungsdatum = DateTime.UtcNow.Date,
            })
        {
        }

        public VertragDetailMiete(Miete m)
        {
            Entity = m;
            PropertyChanged += OnUpdate;
            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.MieteAnhaenge, m), _ => true);
        }

        public AsyncRelayCommand AttachFile;

        private bool savable =>
            Entity.Betrag != null &&
            Entity.BetreffenderMonat != null &&
            Entity.Zahlungsdatum != null &&
            Entity.VertragId != null;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Betrag):
                case nameof(Zahlungsdatum):
                case nameof(Notiz):
                case nameof(BetreffenderMonat):
                    break;
                default:
                    return;
            }

            if (!savable) return;

            if (Entity.MieteId != 0)
            {
                App.Walter.Mieten.Update(Entity);
            }
            else
            {
                App.Walter.Mieten.Add(Entity);
            }
            App.SaveWalter();
        }
    }

}
