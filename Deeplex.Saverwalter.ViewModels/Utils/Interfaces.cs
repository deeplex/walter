using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IFilterViewModel
    {
        ObservableProperty<string> Filter { get; set; }
    }

    public interface IAppImplementation
    {
        Task<bool> Confirmation();
        Task<bool> Confirmation(string title, string content, string primary, string secondary);
        void ShowAlert(string text);
        void launchFile(Anhang a);
        Task<string> saveFile();
        Task<string> pickFile();
        Task<List<string>> pickFiles();
    }

    
}
