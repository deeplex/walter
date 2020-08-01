using Deeplex.Saverwalter.App.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class VertragListControl : UserControl
    {
        public VertragListViewModel ViewModel { get; set; }

        public VertragListControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(WohnungIdProperty, (WohnungIdDepObject, WohnungIdProp) =>
            {
                ViewModel = new VertragListViewModel(App.Walter.Wohnungen.Find(WohnungId));
            });

            RegisterPropertyChangedCallback(PersonIdProperty, (PersonIddepObject, PersonIdProp) =>
            {
                ViewModel = new VertragListViewModel(App.Walter.FindPerson(PersonId));
            });
        }

        public Guid PersonId
        {
            get { return (Guid)GetValue(PersonIdProperty); }
            set { SetValue(PersonIdProperty, value); }
        }

        public static readonly DependencyProperty PersonIdProperty
            = DependencyProperty.Register(
                "PersonId",
                typeof(Guid),
                typeof(VertragListControl),
                new PropertyMetadata(Guid.Empty));

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Property1.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(VertragListControl),
                  new PropertyMetadata(0));
    }
}
