using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class JuristischePersonViewModel
    {
        public int Id;
        public ObservableProperty<string> Name = new ObservableProperty<string>();

        public JuristischePersonViewModel(JuristischePerson j)
        {
            Id = j.JuristischePersonId;
            Name.Value = j.Bezeichnung;
        }

        public static JuristischePerson GetJuristischePerson(JuristischePersonViewModel j)
        {
            return App.Walter.JuristischePersonen.Find(j.Id);
        }
    }
}
