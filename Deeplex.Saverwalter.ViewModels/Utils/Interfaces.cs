using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IFilterViewModel
    {
        ObservableProperty<string> Filter { get; set; }
    }

    // TODO add attribute or sth to give instructions how to implement
    public interface IAppImplementation
    {
        Task<bool> Confirmation();
        Task<bool> Confirmation(string title, string content, string primary, string secondary);
        void ShowAlert(string text);
        void launchFile(Anhang a);
        Task<string> saveFile(string filename, params string[] ext);
        Task<string> pickFile(params string[] ext);
        Task<List<string>> pickFiles(params string[] ext);
    }
}
