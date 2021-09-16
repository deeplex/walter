﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IFilterViewModel
    {
        ObservableProperty<string> Filter { get; set; }
    }

    public interface IAppImplementation
    {
        string root { get; set; }
        SaverwalterContext ctx { get; set; }
        void SaveWalter();
        Task<bool> Confirmation(string title, string content, string primary, string secondary);
        Task<bool> Confirmation();
        void OpenAnhangPane();
        void ShowAlert(string text);
        ObservableProperty<string> Titel { get; set; }
        void launchFile(Anhang a);
        Task<List<string>> pickFiles();
        Task deleteFile(Anhang a);
    }
}
