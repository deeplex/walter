using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Services
{
    public interface INotificationService
    {
        Task<bool> Confirmation();
        Task<bool> Confirmation(string title, string description, string yes, string no);
        void ShowAlert(string text);
        bool outOfSync { get; set; }

        public void Navigation<T>(T Element) where T : new();
    }
}
