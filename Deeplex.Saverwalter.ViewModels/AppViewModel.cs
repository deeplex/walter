using Deeplex.Saverwalter.Model;
using Deeplex.SaverWalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
        public sealed class AppViewModel : BindableBase
    {
        public ObservableProperty<string> Titel { get; set; } = new();

        public ImmutableList<AutoSuggestEntry> AllAutoSuggestEntries { get; set; }
        public ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries { get; } = new();

        public IAppImplementationService Impl { get; }

        public AppViewModel(IAppImplementationService impl)
        {
            Impl = impl;
            Titel.Value = "Walter";
        }
        
        public void updateAutoSuggestEntries(string filter)
        {
            if (AllAutoSuggestEntries != null)
            {
                AutoSuggestEntries.Value = AllAutoSuggestEntries.Where(w => w.ToString().ToLower().Contains(filter.ToLower())).ToImmutableList();
            }
        }
    }
}
