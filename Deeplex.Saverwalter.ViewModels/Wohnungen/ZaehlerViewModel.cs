using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerViewModel : BindableBase
    {
        public ZaehlerViewModel self => this;

        public Zaehler Entity { get; }

        public int Id => Entity.ZaehlerId;


        public string Kennnummer => Entity.Kennnummer;

        public string Typ => Entity.Typ.ToString();

        public ObservableProperty<ImmutableList<ZaehlerstandViewModel>> Zaehlerstaende = new();
        public DateTimeOffset AddZaehlerstandDatum => DateTime.UtcNow.Date.AsUtcKind();
        public double AddZaehlerstandStand => Zaehlerstaende.Value.FirstOrDefault()?.Stand ?? 0;
        public void LoadList()
        {
            Zaehlerstaende.Value = Db.ctx.Zaehlerstaende.ToList()
                .Where(zs => Entity == zs.Zaehler)
                .OrderBy(zs => zs.Datum).Reverse()
                .Select(zs => new ZaehlerstandViewModel(zs, this)).ToImmutableList();
        }

        public IWalterDbService Db;

        public ZaehlerViewModel(Zaehler z, IWalterDbService db)
        {
            Entity = z;
            Db = db;

            LoadList();

            //TODO AttachFile = new AsyncRelayCommand(async _ =>
            //    await Utils.Files.SaveFilesToWalter(Avm.ctx.ZaehlerAnhaenge, z), _ => true);
        }

        public AsyncRelayCommand AttachFile;
    }
}
