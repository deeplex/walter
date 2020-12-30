using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class AddresseControl : UserControl
    {
        public AdresseViewModel ViewModel { get; set; }

        public Type test;

        public AddresseControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(IdProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel(App.Walter.Adressen.Find(Id));
            });

            RegisterPropertyChangedCallback(AdresseProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel<IAdresse>(Adresse);
            });
        }

        public IAdresse Adresse
        {
            get { return (IAdresse)GetValue(AdresseProperty); }
            set { SetValue(AdresseProperty, value); }
        }

        public static readonly DependencyProperty AdresseProperty
            = DependencyProperty.Register(
                "value",
                typeof(IAdresse),
                typeof(AddresseControl),
                new PropertyMetadata(null));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty
            = DependencyProperty.Register(
                  "Id",
                  typeof(int),
                  typeof(AddresseControl),
                  new PropertyMetadata(0));
    }
}
