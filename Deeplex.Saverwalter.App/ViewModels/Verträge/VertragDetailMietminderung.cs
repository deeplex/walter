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
    public sealed class VertragDetailMietMinderung : BindableBase
    {
        private MietMinderung Entity { get; }
        public void selfDestruct()
        {
            App.Walter.Remove(Entity);
            App.SaveWalter();
        }

        private void update<U>(string property, U value)
        {
            if (Entity == null) return;
            var type = Entity.GetType();
            var prop = type.GetProperty(property);
            var val = prop.GetValue(Entity, null);
            if (!value.Equals(val))
            {
                prop.SetValue(Entity, value);
                RaisePropertyChanged(property);
            };
        }

        public DateTimeOffset Beginn
        {
            get => Entity.Beginn;
            set => update(nameof(Entity.Beginn), value.UtcDateTime.AsUtcKind());

        }

        public DateTimeOffset? Ende
        {
            get => Entity.Ende;
            set => update(nameof(Entity.Ende), value?.UtcDateTime.AsUtcKind());
        }

        public double Minderung
        {
            get => Entity.Minderung;
            set => update(nameof(Entity.Minderung), value);

        }

        public string Notiz
        {
            get => Entity.Notiz;
            set => update(nameof(Entity.Notiz), value);
        }

        public VertragDetailMietMinderung(Guid vertragId)
            : this(new MietMinderung
            {
                VertragId = vertragId,
                Beginn = DateTime.UtcNow.Date,
            })
        {
        }


        public VertragDetailMietMinderung(MietMinderung m)
        {
            Entity = m;
            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.MietMinderungAnhaenge, m), _ => true);
            PropertyChanged += OnUpdate;
        }

        public AsyncRelayCommand AttachFile;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Beginn):
                case nameof(Ende):
                case nameof(Notiz):
                case nameof(Minderung):
                    break;
                default:
                    return;
            }

            if (Beginn == null || Minderung == 0) return;

            if (Entity.MietMinderungId != 0)
            {
                App.Walter.MietMinderungen.Update(Entity);
            }
            else
            {
                App.Walter.MietMinderungen.Add(Entity);
            }
            App.SaveWalter();
        }
    }

}
