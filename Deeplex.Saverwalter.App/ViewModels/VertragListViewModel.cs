using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragListViewModel
    {
        public List<VertragListVertrag> Vertraege = new List<VertragListVertrag>();
        public ObservableProperty<VertragListVertrag> SelectedVertrag
            = new ObservableProperty<VertragListVertrag>();

        public VertragListViewModel()
        {
            Vertraege = App.Walter.Vertraege
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                .ToList()
                .GroupBy(v => v.VertragId)
                .Select(v => new VertragListVertrag(v))
                .OrderBy(v => v.Beginn).Reverse()
                .ToList();
        }
    }

    public class VertragListVertrag : VertragVersionListViewModel
    {
        public List<VertragVersionListViewModel> Versionen { get; }
            = new List<VertragVersionListViewModel>();
        public ObservableProperty<VertragListMiete> AddMieteValue
            = new ObservableProperty<VertragListMiete>();

        public ImmutableList<VertragListMiete> Mieten { get; set; }
        public string LastMiete
        {
            get
            {
                var mieten = Mieten.OrderBy(m => m.Datum.Value);
                var last = mieten.Count() > 0 ? mieten.Last() : null;
                return last != null ? last.Datum.Value.ToString("dd.MM.yyyy") +
                    " - Betrag: " + string.Format("{0:F2}€", last.Betrag) : "";
            }
        }
        public bool HasLastMiete => LastMiete != "";

        public VertragListVertrag(IGrouping<Guid, Vertrag> v)
            : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
            BeginnString = Versionen.First().BeginnString;
            Beginn = Versionen.First().Beginn;

            Mieten = App.Walter.Mieten
                .Where(m => m.VertragId == v.First().VertragId)
                .Select(m => new VertragListMiete(m))
                .ToImmutableList();

            AddMieteValue.Value = new VertragListMiete();
            AddMiete = new RelayCommand(_ =>
            {
                Mieten = Mieten.Add(AddMieteValue.Value);
                App.Walter.Mieten.Add(new Miete
                {
                    Zahlungsdatum = AddMieteValue.Value.Datum.Value.UtcDateTime,
                    Betrag = AddMieteValue.Value.Betrag,
                    VertragId = Versionen.Last().VertragId,
                });
                App.Walter.SaveChanges();
                AddMieteValue.Value = new VertragListMiete();
                RaisePropertyChanged(nameof(LastMiete));
                RaisePropertyChanged(nameof(HasLastMiete));
            }, _ => true);
        }
        public RelayCommand AddMiete { get; }
    }

    public class VertragVersionListViewModel : BindableBase
    {
        public int Id { get; }
        public Guid VertragId { get; }
        public int Version { get; }
        public int Personenzahl { get; }
        public string Anschrift { get; }
        public string Wohnung { get; }
        public DateTime Beginn { get; set; }
        public string BeginnString { get; set; }
        public string EndeString { get; }
        public string AuflistungMieter { get; }
        public bool hasEnde { get; }
        public string Besitzer { get; }

        public VertragVersionListViewModel(Vertrag v)
        {
            Id = v.rowid;
            VertragId = v.VertragId;
            Version = v.Version;
            Personenzahl = v.Personenzahl;
            Anschrift = AdresseViewModel.Anschrift(v.Wohnung);
            Besitzer = v.Wohnung.Besitzer?.Bezeichnung ?? ""; // TODO Vertrag w/o Besitzer is a dummy (?)
            Wohnung = v.Wohnung is Wohnung w ? w.Bezeichnung : "";
            AuflistungMieter = string.Join(", ",
                App.Walter.MieterSet
                    .Where(m => m.VertragId == v.VertragId)
                    .Include(m => m.Kontakt)
                    .ToList()
                    .Select(m => (m.Kontakt.Vorname is string n ? n + " " : "") + m.Kontakt.Nachname));
            // Such grace...

            Beginn = v.Beginn.AsUtcKind();
            BeginnString = v.Beginn.ToString("dd.MM.yyyy");
            hasEnde = v.Ende is DateTime;
            EndeString = v.Ende is DateTime e ? e.ToString("dd.MM.yyyy") : "Offen";

        }
    }

    public class VertragListMiete : BindableBase
    {
        public int Id;
        public ObservableProperty<DateTimeOffset> Datum = new ObservableProperty<DateTimeOffset>();
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

        public VertragListMiete()
        {
            Datum.Value = DateTime.UtcNow.Date;
            Betrag = 0;
            Notiz.Value = "";
        }

        public VertragListMiete(Miete m)
        {
            Id = m.MieteId;
            Datum.Value = m.Zahlungsdatum.AsUtcKind();
            Betrag = m.Betrag ?? 0;
            Notiz.Value = m.Notiz ?? "";
        }
    }
}
