using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostentypUtil
    {
        public override string ToString()
        {
            // TODO i18n out of viewmodel...
            return Typ.ToDescriptionString() + ((int)Typ % 2 == 0 ? " (kalt)" : " (warm)");
        }

        public Betriebskostentyp Typ { get; }
        public int index { get; }
        public BetriebskostentypUtil(Betriebskostentyp t)
        {
            Typ = t;
            index = (int)t;
        }
    }
}
