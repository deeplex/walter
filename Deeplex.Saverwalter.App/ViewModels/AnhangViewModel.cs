using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class AnhangViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<AnhangDatei>> Dateien = new ObservableProperty<ImmutableList<AnhangDatei>>();

        public AnhangViewModel()
        {
            Dateien.Value = App.Walter.Anhaenge.Select(a => new AnhangDatei(a)).ToImmutableList();
        }

    }

    public sealed class AnhangDatei : BindableBase
    {
        public Anhang Entity { get; }

        public AnhangDatei(Anhang a)
        {
            Entity = a;
        }

        public string DateiName => Entity.FileName;
    }
}
