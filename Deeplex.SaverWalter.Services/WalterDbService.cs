using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Services
{
    public interface IWalterDbService
    {
        SaverwalterContext ctx { get; set; }
        public void SaveWalter();
        string root { get; set; }
    }

    public sealed class WalterDbService : IWalterDbService
    {
        public string root { get; set; }

        public SaverwalterContext ctx { get; set; }


        public void SaveWalter()
        {
            ctx.SaveChanges();
            // TODO this should have an own Service
            //Impl.ShowAlert("Gespeichert");
        }
    }
}
