using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.Services
{
    public interface AutoSuggestService
    {
        ImmutableList<AutoSuggestEntry> AllAutoSuggestEntries { get; set; }
        ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries { get; }
        void updateAutoSuggestEntries(string filter);
    }
}
