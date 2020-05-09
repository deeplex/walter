using Deeplex.Saverwalter.App.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragGridControl : UserControl
    {
        public int KontaktId
        {
            get => (int)GetValue(KontaktIdProperty);
            set { SetValue(KontaktIdProperty, value); }
        }
        public static readonly DependencyProperty KontaktIdProperty =
           DependencyProperty.Register("KontaktId", typeof(int), typeof(VertragGridControl), new PropertyMetadata(0));

        List<VertragGridControlModel> ViewModel { get; set; }

        public VertragGridControl()
        {
            Loaded += (args, sender) =>
            {
                if (KontaktId > 0)
                {
                    var vertraege = App.Walter.Vertraege.Include(v => v.Wohnung) // TODO This may be suboptimal...
                        .Where(v => v.Mieter.Where(m => m.KontaktId == KontaktId).Count() > 0).ToList();
                    ViewModel = vertraege.GroupBy(v => v.VertragId).Select(v => new VertragGridControlModel(v)).ToList();
                }
                InitializeComponent();
            };
        }
    }
}
