using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
namespace Deeplex.Saverwalter.Services
{
    public interface IWalterDbService
    {
        SaverwalterContext ctx { get; set; }
        public void SaveWalter();
        public INotificationService NotificationService { get; }
    }

    public sealed class WalterDbService : IWalterDbService
    {
        public string root { get; set; }

        public IFileService FileService { get; }
        public INotificationService NotificationService { get; }

        public SaverwalterContext ctx { get; set; }

        public WalterDbService(INotificationService ns, IFileService fs)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseSqlite("Data Source=" + fs.databaseRoot + ".db");
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
