using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.UserControls;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{
    public sealed partial class BetriebskostenrechnungPrintViewPage : Page
    {
        public BetriebskostenrechnungPrintViewModel ViewModel { get; set; }

        public BetriebskostenrechnungPrintViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Vertrag v)
            {
                ViewModel = new BetriebskostenrechnungPrintViewModel(v, App.WalterService, App.Impl);
            }

            App.Window.CommandBar.MainContent = new BetriebskostenrechnungPrintCommandBarControl { ViewModel = ViewModel };
            var self = this;
            TPrint<Page>.Print(ViewModel.Betriebskostenabrechnung, new BetriebskostenPrintImpl(self));

            base.OnNavigatedTo(e);
        }

        private sealed class BetriebskostenPrintImpl : IPrint<Page>
        {
            private TextBlock text(string s) => new TextBlock()
            {
                Text = s,
                TextWrapping = TextWrapping.WrapWholeWords,
            };

            private double panelWidth
            {
                get => Parent.Panel.Width - Parent.Panel.Padding.Right - Parent.Panel.Padding.Left;
            }

            private BetriebskostenrechnungPrintViewPage Parent { get; }
            public BetriebskostenPrintImpl(BetriebskostenrechnungPrintViewPage parent)
            {
                Parent = parent;
            }

            public Page body => Parent;

            public void Break()
            {
                Parent.Panel.Children.Add(new Grid() { Height = 20 });
            }

            public void EqHeizkostenV9_2(Rechnungsgruppe gruppe)
            {
                // TODO
            }

            public void Paragraph(params PrintRun[] runs)
            {
                var rtb = new RichTextBlock();
                var para = new Paragraph();
                for (var i = 0; i < runs.Length; ++i)
                {
                    var run = runs[i];
                    var r = new Run()
                    {
                        Text = run.Text,
                        FontWeight = run.Bold ? FontWeights.Bold : FontWeights.Normal,
                        TextDecorations = run.Underlined ?
                            Windows.UI.Text.TextDecorations.Underline :
                            Windows.UI.Text.TextDecorations.None
                    };
                    para.Inlines.Add(r);
                    if (run.Tab)
                    {
                        // TODO tab does not really work..
                        var number = 8 - r.Text.Length % 8;
                        r.Text += string.Concat(Enumerable.Repeat(" ", number));
                    }
                    if (!run.NoBreak)
                    {
                        if (i != runs.Length - 1)
                        {
                            para.Inlines.Add(new LineBreak());
                        }
                    }
                }
                rtb.Blocks.Add(para);

                Parent.Panel.Children.Add(rtb);
            }

            public void Heading(string str)
            {
                Parent.Panel.Children.Add(new TextBlock
                {
                    Margin = new Thickness(0, 5, 0, 5),
                    Text = str,
                    FontWeight = FontWeights.Bold,
                    FontStyle = Windows.UI.Text.FontStyle.Italic,
                    MaxWidth = panelWidth
                });
            }

            public void Introtext(Betriebskostenabrechnung b)
            {
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock()
                {
                    Text = b.Title(),
                    FontWeight = FontWeights.Bold,
                });
                stack.Children.Add(text(b.Mieterliste()));
                stack.Children.Add(text(b.Mietobjekt()));
                var h1 = new StackPanel() { Orientation = Orientation.Horizontal };
                h1.Children.Add(new TextBlock() { Text = "Abrechnungszeitraum: ", Width = 150 });
                h1.Children.Add(text(b.Abrechnungszeitraum()));
                var h2 = new StackPanel() { Orientation = Orientation.Horizontal };
                h2.Children.Add(new TextBlock() { Text = "Nutzungszeitraum: ", Width = 150 });
                h2.Children.Add(text(b.Nutzungszeitraum()));
                stack.Children.Add(h1);
                stack.Children.Add(h2);
                stack.Children.Add(new Grid() { Height = 10 });

                stack.Children.Add(text(b.Gruss()));
                stack.Children.Add(text(b.ResultTxt()));
                stack.Children.Add(text(Print.Utils.Euro(Math.Abs(b.Result))));
                stack.Children.Add(text(b.RefundDemand()));
                stack.Children.Add(new Grid() { Height = 10 });
                stack.Children.Add(text(b.GenerischerText()));

                var Anpassung = -b.Result / 12;

                if (Anpassung > 0)
                {
                    stack.Children.Add(new Grid() { Height = 10 });
                    stack.Children.Add(new TextBlock()
                    {
                        Text = "Wir empfehlen Ihnen die monatliche Mietzahlung, um einen Betrag von " +
                        Print.Utils.Euro(Anpassung) + " auf " +
                        Print.Utils.Euro(b.Gezahlt / 12 + Anpassung) + " anzupassen."
                    });
                }

                Parent.Panel.Children.Add(stack);
            }

            public void PageBreak()
            {
                Parent.Panel.Children.Add(new MenuFlyoutSeparator()
                {
                    Margin = new Thickness(20)
                });
            }

            public void SubHeading(string str)
            {
                Parent.Panel.Children.Add(new TextBlock
                {
                    Text = str,
                    FontWeight = FontWeights.Bold,
                });
            }

            public void Table(int[] widths, int[] justification, bool[] bold, bool[] underlined, string[][] cols)
            {
                var grid = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = widths.Sum() * panelWidth / 100
                };
                foreach (var width in widths)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = width * panelWidth / 100 });
                }

                var jst = justification.Select(w => w == 0 ?
                    TextAlignment.Left : w == 1 ?
                    TextAlignment.Center :
                    TextAlignment.Right).ToList();

                var max = cols.Max(w => w.Length);
                for (var i = 0; i < max; ++i)
                {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(18) });
                }

                for (var i = 0; i < cols.Length; ++i)
                {
                    for (var j = 0; j < cols[i].Length; ++j)
                    {
                        var container = new Grid();

                        if (underlined[j])
                        {
                            container.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush()
                            {
                                Color = Microsoft.UI.Colors.Black,
                            };
                            container.BorderThickness = new Thickness(0, 0, 0, 1);
                        }

                        var tb = new TextBlock()
                        {
                            TextWrapping = TextWrapping.WrapWholeWords,
                            Text = cols[i][j],
                            TextAlignment = jst[i],
                            FontWeight = bold[j] ? FontWeights.Bold : FontWeights.Normal,
                        };

                        container.Children.Add(tb);
                        grid.Children.Add(container);

                        Grid.SetRow(container, j);
                        Grid.SetColumn(container, i);
                    }
                }

                Parent.Panel.Children.Add(grid);
            }

            public void Text(string s)
            {
                Parent.Panel.Children.Add(new TextBlock { Text = s, TextWrapping = TextWrapping.WrapWholeWords });
            }
        }
    }
}
