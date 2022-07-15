using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Services
{
    public interface INotificationService
    {
        Task<bool> Confirmation();
        void ShowAlert(string text);
        bool outOfSync { get; set; }
    }
}
