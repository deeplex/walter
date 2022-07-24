using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Services
{
    public interface INotificationService
    {
        Task<bool> Confirmation();
        void ShowAlert(string text);
        bool outOfSync { get; set; }

        public void Navigation<T>(T Element);
    }
}
