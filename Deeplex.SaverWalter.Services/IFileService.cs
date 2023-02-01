using Deeplex.Saverwalter.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Services
{
    public interface IFileService
    {
        string databaseRoot { get; set; }
        string databaseURL { get; set; }
        string databasePort { get; set; }
        string databaseUser { get; set; }
        string databasePass { get; set; }
        Task<string> pickFile(params string[] ext);
        Task<List<string>> pickFiles(params string[] ext);
        Task<string> saveFile(string filename, string[] ext);
        void launchFile(Anhang a);
    }
}
