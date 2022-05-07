using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
namespace Deeplex.Saverwalter.Services
{
    public interface IWalterDbService
    {
        SaverwalterContext ctx { get; set; }
        public void SaveWalter();
        string root { get; set; }
        public INotificationService NotificationService { get; }
    }

    public sealed class WalterDbService : IWalterDbService
    {
        public string root { get; set; }
        public INotificationService NotificationService { get; }

        public SaverwalterContext ctx { get; set; }

        public WalterDbService(INotificationService ns)
        {
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
