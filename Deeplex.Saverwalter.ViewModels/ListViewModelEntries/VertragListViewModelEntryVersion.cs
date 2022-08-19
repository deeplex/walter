using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragListViewModelEntryVersion : BindableBase
    {
        public int Id => Entity.VertragVersionId;
        public int Personenzahl => Entity.Personenzahl;

        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public string BeginnString => Beginn.ToString("dd.MM.yyyy");
        public string EndeString => Ende is DateTime e ? e.ToString("dd.MM.yyyy") : "Offen";

        
        public bool hasEnde => Ende != null;

        public VertragVersion Entity { get; }
        private IWalterDbService WalterDbService;

        public VertragListViewModelEntryVersion(VertragVersion v, IWalterDbService db)
        {
            WalterDbService = db;
            Entity = v;

            Beginn = v.Beginn.AsUtcKind();
            Ende = v.Ende()?.AsUtcKind();
        }
    }
}
