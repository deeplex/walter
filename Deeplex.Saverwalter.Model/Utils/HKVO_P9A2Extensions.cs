using System.ComponentModel;

namespace Deeplex.Saverwalter.Model
{
    public static class HKVOP9_A2Extensions
    {
        public static string ToDescriptionString(this HKVO_P9A2 val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())!
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
