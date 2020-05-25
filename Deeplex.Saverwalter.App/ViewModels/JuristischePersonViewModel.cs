using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class JuristischePersonViewModel
    {
        public int Id;
        public ObservableProperty<string> Name = new ObservableProperty<string>();

        public JuristischePersonViewModel() { }

        public override string ToString() => Name.Value;

        public JuristischePersonViewModel(JuristischePerson j)
        {
            Id = j.JuristischePersonId;
            Name.Value = j.Bezeichnung;
        }

        public static JuristischePerson GetJuristischePerson(JuristischePersonViewModel j)
        {
            if (j.Id != 0)
            {
                return App.Walter.JuristischePersonen.Find(j.Id);
            }
            else
            {
                return App.Walter.JuristischePersonen.First(p => p.Bezeichnung == j.Name.Value);
            }
        }
    }
}
