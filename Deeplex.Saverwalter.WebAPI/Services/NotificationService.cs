using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class NotificationService : BindableBase, INotificationService
    {
        public bool outOfSync { get; set; }
        public void ShowAlert(string text) => ShowAlert(text, text.Length > 20 ? 0 : 3000);
        public void ShowAlert(string text, int ms)
        {
            throw new NotImplementedException();
        }

        
        public void Navigation<T>(T Element) where T : new()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Confirmation()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Confirmation(string title, string description, string yes, string no)
        {
            throw new NotImplementedException();
        }
    }
}
