using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class VertragListViewModel : IFilterViewModel
    {
        public ObservableProperty<ImmutableList<VertragListVertrag>> Vertraege = new ObservableProperty<ImmutableList<VertragListVertrag>>();
        public ObservableProperty<VertragListVertrag> SelectedVertrag
            = new ObservableProperty<VertragListVertrag>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<VertragListVertrag> AllRelevant { get; set; }

        public VertragListViewModel()
        {
            AllRelevant = Vertraege.Value = App.Walter.Vertraege
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .ToList()
                .GroupBy(v => v.VertragId)
                .Select(v => new VertragListVertrag(v))
                .OrderBy(v => v.Beginn).Reverse()
                .ToImmutableList();
            Vertraege.Value = AllRelevant;
        }
    }

    public sealed class VertragListVertrag : VertragVersionListViewModel
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
                var last = mieten.Any() ? mieten.Last() : null;
                return last != null ? last.Datum.Value.ToString("dd.MM.yyyy") +
                    " - Betrag: " + string.Format("{0:F2}€", last.Betrag) : "";
            }
        }
        public bool HasLastMiete => LastMiete != "";

        public VertragListVertrag(IGrouping<Guid, Vertrag> v)
            : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
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
                    BetreffenderMonat = AddMieteValue.Value.BetreffenderMonat.Value.UtcDateTime,
                    Betrag = AddMieteValue.Value.Betrag,
                    VertragId = Versionen.Last().VertragId,
                });
                App.SaveWalter();
                AddMieteValue.Value = new VertragListMiete();
                RaisePropertyChanged(nameof(LastMiete));
                RaisePropertyChanged(nameof(HasLastMiete));
            }, _ => true);

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.VertragAnhaenge, v.Key), _ => true);
        }

        public RelayCommand AddMiete { get; }
        public AsyncRelayCommand AttachFile;

    }

    public class VertragVersionListViewModel : BindableBase
    {
        public int Id { get; }
        public Guid VertragId { get; }
        public int Version { get; }
        public int Personenzahl { get; }
        public string Anschrift { get; }
        public string AnschriftMitWohnung => Anschrift + ", " + Wohnung.Bezeichnung;
        public Wohnung Wohnung { get; }
        public ImmutableList<Guid> Mieter { get; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public string BeginnString => Beginn.ToString("dd.MM.yyyy");
        public string EndeString => Ende is DateTime e ? e.ToString("dd.MM.yyyy") : "Offen";
        public string AuflistungMieter { get; }
        public bool hasEnde => Ende != null;
        public string Besitzer { get; }

        public VertragVersionListViewModel(Vertrag v)
        {
            Id = v.rowid;
            VertragId = v.VertragId;
            Version = v.Version;
            Personenzahl = v.Personenzahl;
            Anschrift = AdresseViewModel.Anschrift(v.Wohnung);
            Besitzer = App.Walter.FindPerson(v.Wohnung.BesitzerId)?.Bezeichnung;

            Mieter = App.Walter.MieterSet
                .Where(w => w.VertragId == v.VertragId)
                .Select(m => m.PersonId).ToImmutableList();

            Wohnung = v.Wohnung;



            AuflistungMieter = string.Join(", ", App.Walter.MieterSet
                .Where(m => m.VertragId == v.VertragId).ToList()
                .Select(a => App.Walter.FindPerson(a.PersonId).Bezeichnung));

            Beginn = v.Beginn.AsUtcKind();
            Ende = v.Ende?.AsUtcKind();
        }
    }

    public sealed class VertragListMiete : BindableBase
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

        public VertragListMiete()
        {
            Datum.Value = DateTime.UtcNow.Date;
            BetreffenderMonat.Value = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AsUtcKind();
            Betrag = 0;
            Notiz.Value = "";
        }

        public VertragListMiete(Miete m)
        {
            Id = m.MieteId;
            Datum.Value = m.Zahlungsdatum.AsUtcKind();
            BetreffenderMonat.Value = m.BetreffenderMonat.AsUtcKind();
            Betrag = m.Betrag ?? 0;
            Notiz.Value = m.Notiz ?? "";
        }
    }
}
