using System;
using System.ComponentModel;

namespace Deeplex.Saverwalter.Model
{
    public static class KalteBetriebskostenExtensions
    {
        public static DateTime AsMin(this DateTime t)
            => (t.Ticks == 0 ? DateTime.Now : t).AsUtcKind();

        public static DateTime AsUtcKind(this DateTime dt)
            => new DateTime(dt.Ticks, DateTimeKind.Utc);

        public static string ToDescriptionString(this Betriebskostentyp val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }

        public static string ToUnitString(this Zaehlertyp val)
        {
            UnitAttribute[] attributes = (UnitAttribute[])val
                .GetType()
                .GetField(val.ToString())
                .GetCustomAttributes(typeof(UnitAttribute), false);
            return attributes.Length > 0 ? attributes[0].Unit : string.Empty;
        }
    }
}
