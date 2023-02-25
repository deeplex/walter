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
        public INotificationService NotificationService { get; }
    }

    public sealed class WalterDbService : IWalterDbService
    {
        #pragma warning disable IDE1006 // Naming Styles
        public string root { get; set; }
        #pragma warning restore IDE1006 // Naming Styles

        public IFileService FileService { get; }
        public INotificationService NotificationService { get; }

        public SaverwalterContext ctx { get; set; }

        public WalterDbService(INotificationService ns, IFileService fs)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql("Server=" + fs.databaseURL + ";Port=" + fs.databasePort + ";Database=postgres;Username=" + fs.databaseUser + ";Password=" + fs.databasePass);
            //optionsBuilder.UseSqlite("Data Source=" + fs.databaseRoot + ".db");
            ctx = new SaverwalterContext(optionsBuilder.Options);

            FileService = fs;
            NotificationService = ns;
        }

        public void SaveWalter()
        {
            try
            {
                ctx.SaveChanges();
                NotificationService.ShowAlert("Gespeichert");
            }
            catch (Exception e)
            {
                NotificationService.ShowAlert(e.Message);
            }
        }
    }
}
