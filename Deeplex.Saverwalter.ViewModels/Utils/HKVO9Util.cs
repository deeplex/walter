using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class HKVO9Util
    {
        public override string ToString() => Absatz;

        public HKVO_P9A2 Enum { get; }
        public int index { get; }
        public string Absatz { get; }
        public HKVO9Util(HKVO_P9A2 h)
        {
            Enum = h;
            index = (int)h;
            // TODO i18n out of viewmodel...
            Absatz = "Absatz " + index.ToString();
        }
    }
}
