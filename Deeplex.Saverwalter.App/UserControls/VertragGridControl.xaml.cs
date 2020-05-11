using Deeplex.Saverwalter.App.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragGridControl : UserControl
    {
        public int WohnungId
        {
            get => (int)GetValue(WohnungIdProperty);
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty =
            DependencyProperty.Register("WohnungId", typeof(int), typeof(VertragGridControl), new PropertyMetadata(0));

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
                var vs = App.Walter.Vertraege.Include(v => v.Mieter).ThenInclude(m => m.Kontakt).Include(v => v.Wohnung);
                var vs2 =
                    WohnungId is int wid && wid > 0 ?
                        vs.Where(v => v.WohnungId == wid) :
                    KontaktId is int kid && kid > 0 ?
                        vs.Where(v => v.Mieter.Where(m => m.KontaktId == KontaktId).Count() > 0) :
                    null;

                ViewModel = vs2.ToList().GroupBy(v => v.VertragId).Select(v => new VertragGridControlModel(v)).ToList();
                InitializeComponent();
            };
        }
    }
}
