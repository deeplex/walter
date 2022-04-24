using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Services
{
    public interface IFileService
    {
        Task<string> pickFile(params string[] ext);
        Task<List<string>> pickFiles(params string[] ext);
        Task<string> saveFile(string filename, string[] ext);
        void launchFile(Anhang a);
    }
}
