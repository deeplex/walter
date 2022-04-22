using Deeplex.Saverwalter.Model;
using System;
using System.IO;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class AnhangListViewModelEntry
    {
        public Anhang Entity { get; }
        public override string ToString() => Entity.FileName;
        public DateTime CreationTime => Entity.CreationTime;

        public int GetReferences
        {
            get
            {
                var count = Container.Avm.ctx.AdresseAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.BetriebskostenrechnungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.ErhaltungsaufwendungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.GarageAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.JuristischePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.KontoAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.MieteAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.MietMinderungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.NatuerlichePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.VertragAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.WohnungAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.ZaehlerAnhaenge.Count(j => j.Anhang == Entity);
                count += Container.Avm.ctx.ZaehlerstandAnhaenge.Count(j => j.Anhang == Entity);

                return count;
            }
        }

        public string path => Entity.getPath(Container.Avm.root);
        public double size => File.Exists(path) ? new FileInfo(path).Length : 0;

        public AnhangListViewModel Container { get; }

        public AnhangListViewModelEntry(IAnhang a, AnhangListViewModel vm) : this(a.Anhang, vm) { }
        public AnhangListViewModelEntry(Anhang a, AnhangListViewModel vm)
        {
            Container = vm;
            Entity = a;
        }

        public async void DeleteFile()
        {
            try
            {
                if (await Container.Impl.Confirmation())
                {
                    Container.Avm.ctx.Anhaenge.Remove(Entity);
                    Container.Avm.SaveWalter();

                    File.Delete(Entity.getPath(Container.Avm.root));

                    var deleted = Container.Liste.Value.Find(e => e.Entity.AnhangId == Entity.AnhangId);
                    if (deleted != null)
                    {
                        Container.Liste.Value = Container.Liste.Value.Remove(deleted);
                    }
                }
            }
            catch (Exception e)
            {
                Container.Impl.ShowAlert(e.Message);
            }
        }

        public void OpenFile()
        {
            try
            {
                Container.Impl.launchFile(Entity);
            }
            catch (Exception e)
            {
                Container.Impl.ShowAlert(e.Message);
            }

        }

        public string DateiName => Entity.FileName;
    }
}
