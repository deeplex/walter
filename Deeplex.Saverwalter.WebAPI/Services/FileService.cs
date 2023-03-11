using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class FileService : IFileService
    {
        public string databaseRoot { get; set; }
        public string databasePort { get; set; }
        public string databaseURL { get; set; }
        public string databaseUser { get; set; }
        public string databasePass { get; set; }

        public FileService(INotificationService ns)
        {
            // TODO. This was set after the initialization. But the data is necessary at App-Start. So I have to check for the database beforehand
            
            databaseRoot = "C:/Users/me/OneDrive/Desktop/walter/walter"; // Settings.Values["root"] as string;
            databasePort = "5432";
            databaseURL = "192.168.178.61";
            databaseUser = "postgres";
            databasePass = "admin";
        }

        //public async Task loadDataBase(INotificationService ns)
        //{
        //    if (databaseRoot == null || !File.Exists(databaseRoot + ".db"))
        //    {
        //        var path = await ns.Confirmation(
        //        "Noch keine Datenbank ausgewählt",
        //        "Datenbank suchen, oder leere Datenbank erstellen?",
        //        "Existierende Datenbank auswählen", "Erstelle neue leere Datenbank") ?
        //            await pickFile(".db") :
        //            await saveFile("walter", new string[] { ".db" });
        //        var databaseRoot = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
        //    }
        //}

        public Task<List<string>> pickFiles()
        {
            throw new NotImplementedException();
        }
    }
}
