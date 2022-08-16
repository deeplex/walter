using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class UmlageschluesselUtil
    {
        public override string ToString() => Schluessel.ToDescriptionString();

        public Umlageschluessel Schluessel { get; }
        public UmlageschluesselUtil(Umlageschluessel u)
        {
            Schluessel = u;
        }
    }
}
