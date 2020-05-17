using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
                .Include(v => v.Mieter).ThenInclude(m => m.Kontakt)
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

        public VertragListVertrag(IGrouping<Guid, Vertrag> v)
            : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
            BeginnString = Versionen.First().BeginnString;
            Beginn = Versionen.First().Beginn;
        }
    }

    public class VertragVersionListViewModel
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

        public VertragVersionListViewModel(Vertrag v)
        {
            Id = v.rowid;
            VertragId = v.VertragId;
            Version = v.Version;
            Personenzahl = v.Personenzahl;
            Anschrift = AdresseViewModel.Anschrift(v.Wohnung); // TODO only true if wohnung and not adressen
            Wohnung = v.Wohnung is Wohnung w ? w.Bezeichnung : "";
            AuflistungMieter = string.Join(", ", v.Mieter.Select(m =>
                (m.Kontakt.Vorname is string n ? n + " " : "") + m.Kontakt.Nachname)); // Such grace...

            Beginn = v.Beginn;
            BeginnString = v.Beginn.ToString("dd.MM.yyyy"); ;
            hasEnde = v.Ende is DateTime;
            EndeString = v.Ende is DateTime e ? e.ToString("dd.MM.yyyy") : "";
        }
    }
}
