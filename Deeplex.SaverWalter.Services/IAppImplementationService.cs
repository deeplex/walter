using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.SaverWalter.Services
{
    // TODO add attribute or sth to give instructions how to implement (sphinx documentation)
    public interface IAppImplementationService
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
