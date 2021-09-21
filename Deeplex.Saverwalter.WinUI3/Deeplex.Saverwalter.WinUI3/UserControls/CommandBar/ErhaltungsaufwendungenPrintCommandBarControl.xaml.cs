using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungenPrintCommandBarControl : UserControl
    {
        public ErhaltungsaufwendungenPrintCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Erhaltungsaufwendung"; // TODO Bezeichnung...
        }

        private string setTitle(ErhaltungsaufwendungenPrintViewModel vm)
            => vm.Wohnungen.Value
                .Select(w => App.ViewModel.ctx.Wohnungen.Find(w.Id))
                .ToList()
                .GetWohnungenBezeichnung(App.ViewModel);

        public ErhaltungsaufwendungenPrintViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungenPrintViewModel)GetValue(ViewModelProperty); }
            set
            {
                App.Window.CommandBar.Title = setTitle(value);
                SetValue(ViewModelProperty, value);
            }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(List<ErhaltungsaufwendungenListViewModel>),
            typeof(ErhaltungsaufwendungenPrintCommandBarControl),
            new PropertyMetadata(null));
    }
}
