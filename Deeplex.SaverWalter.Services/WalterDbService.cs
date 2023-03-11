using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
namespace Deeplex.Saverwalter.Services
{
    public interface IWalterDbService
    {
        #pragma warning disable IDE1006 // Naming Styles
        SaverwalterContext ctx { get; set; }
        #pragma warning restore IDE1006 // Naming Styles
        public void SaveWalter();
    }

    public sealed class WalterDbService : IWalterDbService
    {
        #pragma warning disable IDE1006 // Naming Styles
        public string root { get; set; }
        #pragma warning restore IDE1006 // Naming Styles

        public IFileService FileService { get; }

        public SaverwalterContext ctx { get; set; }

        public WalterDbService(IFileService fs)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql("Server=" + fs.databaseURL + ";Port=" + fs.databasePort + ";Database=postgres;Username=" + fs.databaseUser + ";Password=" + fs.databasePass);
            //optionsBuilder.UseSqlite("Data Source=" + fs.databaseRoot + ".db");
            ctx = new SaverwalterContext(optionsBuilder.Options);

            FileService = fs;
        }

        public void SaveWalter()
        {
            ctx.SaveChanges();
        }
    }
}
