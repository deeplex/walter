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
                var count = ctx.AdresseAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.BetriebskostenrechnungAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.ErhaltungsaufwendungAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.GarageAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.JuristischePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.KontoAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.MieteAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.MietMinderungAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.NatuerlichePersonAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.VertragAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.WohnungAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.ZaehlerAnhaenge.Count(j => j.Anhang == Entity);
                count += ctx.ZaehlerstandAnhaenge.Count(j => j.Anhang == Entity);

                return count;
            }
        }

        public string path => Entity.getPath(Container.Db.root);
        public double size => File.Exists(path) ? new FileInfo(path).Length : 0;

        public AnhangListViewModel Container { get; }
        private SaverwalterContext ctx => Container.Db.ctx;

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
                if (await Container.NotificationService.Confirmation())
                {
                    ctx.Anhaenge.Remove(Entity);
                    Container.Db.SaveWalter();

                    File.Delete(Entity.getPath(Container.Db.root));

                    var deleted = Container.Liste.Value.Find(e => e.Entity.AnhangId == Entity.AnhangId);
                    if (deleted != null)
                    {
                        Container.Liste.Value = Container.Liste.Value.Remove(deleted);
                    }
                }
            }
            catch (Exception e)
            {
                Container.NotificationService.ShowAlert(e.Message);
            }
        }

        public void OpenFile()
        {
            try
            {
                Container.fs.launchFile(Entity);
            }
            catch (Exception e)
            {
                Container.NotificationService.ShowAlert(e.Message);
            }

        }

        public string DateiName => Entity.FileName;
    }
}
