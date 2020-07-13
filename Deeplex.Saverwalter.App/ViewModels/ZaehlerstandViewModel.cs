using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class ZaehlerstandViewModel : BindableBase
    {
        public ZaehlerstandViewModel self => this;

        public Zaehlerstand Entity { get; }
        public int Id => Entity.ZaehlerstandId;

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

        public double Stand
        {
            get => Entity.Stand;
            set => update(nameof(Entity.Stand), value);
        }
        public DateTimeOffset Datum
        {
            get => Entity.Datum.AsUtcKind();
            set => update(nameof(Entity.Datum), value.UtcDateTime.AsUtcKind());

        }

        public string Notiz
        {
            get => Entity.Notiz;
            set => update(nameof(Entity.Notiz), value);

        }

        public ZaehlerstandViewModel(Zaehlerstand z, ZaehlerViewModel parent)
        {
            Entity = z;
            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.ZaehlerstandAnhaenge, z), _ => true);

            SelfDestruct = new RelayCommand(_ =>
            {
                App.Walter.Remove(Entity);
                parent.Zaehlerstaende.Value = parent.Zaehlerstaende.Value.Remove(this);

                App.SaveWalter();
            }, _ => true);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand SelfDestruct { get; }
    }
}
