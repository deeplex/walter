using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
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
        SaverwalterContext ctx { get; set; }
        void SaveWalter();
        Task<bool> Confirmation();
        void OpenAnhang();
        void ShowAlert(string text, int time);
        ObservableProperty<string> Titel { get; set; }

        //private object SavedIndicator { get; set; }
        //private object SavedIndicatorText { get; set; }
        //private object ConfirmationDialog { get; set; }
        //private object AnhangPane { get; set; }
        //private object AnhangSymbol { get; set; }
    }
}
