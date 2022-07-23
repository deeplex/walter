using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageSchluesselUtil
    {
        public override string ToString() => Schluessel.ToDescriptionString();

        public UmlageSchluessel Schluessel { get; }
        public UmlageSchluesselUtil(UmlageSchluessel u)
        {
            Schluessel = u;
        }
    }
}
