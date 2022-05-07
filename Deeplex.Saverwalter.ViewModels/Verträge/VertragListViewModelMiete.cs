using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragListViewModelMiete : BindableBase
    {
        public int Id;
        public ObservableProperty<DateTimeOffset> Datum = new ObservableProperty<DateTimeOffset>();
        public ObservableProperty<DateTimeOffset> BetreffenderMonat = new ObservableProperty<DateTimeOffset>();
        public double Kalt;
        public string KaltString
        {
            get => Kalt > 0 ? string.Format("{0:F2}", Kalt) : "";
            set
            {
                if (double.TryParse(value, out double result))
                {
                    SetProperty(ref Kalt, result);
                }
                else
                {
                    SetProperty(ref Kalt, 0.0);
                }
                RaisePropertyChanged(nameof(Kalt));
            }
        }
        public double Betrag;
        public string BetragString
        {
            get => Betrag > 0 ? string.Format("{0:F2}", Betrag) : "";
            set
            {
                if (double.TryParse(value, out double result))
                {
                    SetProperty(ref Betrag, result);
                }
                else
                {
                    SetProperty(ref Betrag, 0.0);
                }
                RaisePropertyChanged(nameof(Betrag));
            }
        }

        public ObservableProperty<string> Notiz = new ObservableProperty<string>();

        public VertragListViewModelMiete()
        {
            Datum.Value = DateTime.UtcNow.Date;
            BetreffenderMonat.Value = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AsUtcKind();
            Betrag = 0;
            Notiz.Value = "";
        }

        public VertragListViewModelMiete(Miete m)
        {
            Id = m.MieteId;
            Datum.Value = m.Zahlungsdatum.AsUtcKind();
            BetreffenderMonat.Value = m.BetreffenderMonat.AsUtcKind();
            Betrag = m.Betrag ?? 0;
            Notiz.Value = m.Notiz ?? "";
        }
    }
}
