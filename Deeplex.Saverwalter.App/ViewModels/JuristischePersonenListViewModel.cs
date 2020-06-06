using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class JuristischePersonenListViewModel
    {
        public ObservableProperty<ImmutableList<JuristischePersonenPerson>> Personen
            = new ObservableProperty<ImmutableList<JuristischePersonenPerson>>();

        public ObservableProperty<string> AddPersonName = new ObservableProperty<string>();
        public RelayCommand AddPerson { get; }

        public JuristischePersonenListViewModel()
        {

            Personen.Value = App.Walter.JuristischePersonen
                    .Select(j => new JuristischePersonenPerson(j))
                    .ToImmutableList();

            AddPerson = new RelayCommand(_ =>
            {
                var j = new JuristischePerson { Bezeichnung = AddPersonName.Value };
                App.Walter.JuristischePersonen.Add(j);
                App.Walter.SaveChanges();
                Personen.Value = Personen.Value.Add(new JuristischePersonenPerson(j));
            }, _ => true);
        }
    }

    public sealed class JuristischePersonenPerson
    {
        public int Id;
        public ObservableProperty<string> Name = new ObservableProperty<string>();

        public JuristischePersonenPerson(JuristischePerson j)
        {
            Id = j.JuristischePersonId;
            Name.Value = j.Bezeichnung;
        }
    }
}
