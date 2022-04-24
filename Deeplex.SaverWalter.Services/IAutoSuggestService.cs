using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;

namespace Deeplex.SaverWalter.Services
{
    public interface IAutoSuggestService
    {
        ImmutableList<AutoSuggestEntry> AllAutoSuggestEntries { get; set; }
        ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries { get; }
        void updateAutoSuggestEntries(string filter);
    }
}
