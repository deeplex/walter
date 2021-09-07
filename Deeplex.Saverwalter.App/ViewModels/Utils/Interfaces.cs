using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IFileCommands
    {
        Task<IList<Anhang>> PickFiles();
        Task<string> ExtractTo(Anhang a);
        Task<Anhang> ExtractFrom(string path);
        void SaveFilesToWalter<T, U>(DbSet<T> set, U target, IList<Anhang> files) where T : class, IAnhang<U>, new();
    }

    public interface IFilterViewModel
    {
        ObservableProperty<string> Filter { get; set; }
    }

    public interface IAppImplementation
    {
        SaverwalterContext ctx { get; set; }
        void SaveWalter();
        Task<bool> Confirmation(string title, string content, string primary, string secondary);
        Task<bool> Confirmation();
        void OpenAnhang();
        void ShowAlert(string text, int time);
        ObservableProperty<string> Titel { get; set; }
    }
}
