using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageSchluesselUtil
    {
        public UmlageSchluessel Schluessel { get; }
        public string Beschreibung { get; }
        public UmlageSchluesselUtil(UmlageSchluessel u)
        {
            Schluessel = u;
            Beschreibung = u.ToDescriptionString();
        }
    }
}
