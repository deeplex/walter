using System.ComponentModel;

namespace Deeplex.Saverwalter.Model
{
    public static class KalteBetriebskostenExtensions
    {
        public static DateTime AsMin(this DateTime t)
            => (t.Ticks == 0 ? DateTime.Now : t).AsUtcKind();

        public static DateTime AsUtcKind(this DateTime dt) => new(dt.Ticks, DateTimeKind.Utc);

        public static string ToDescriptionString(this Betriebskostentyp typ)
        {
            var field = typ
                .GetType()
                .GetField(typ.ToString());

            if (field == null)
            {
                throw new ArgumentException("Betriebskostentyp has no DescriptionString");
            }

            DescriptionAttribute[] attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }

        public static string ToUnitString(this Zaehlertyp typ)
        {
            var field = typ
                .GetType()
                .GetField(typ.ToString());

            if (field == null)
            {
                throw new ArgumentException("Zählertyp has no UnitString");
            }

            UnitAttribute[] attributes = (UnitAttribute[])field.GetCustomAttributes(typeof(UnitAttribute), false);
            return attributes.Length > 0 ? attributes[0].Unit : string.Empty;
        }
    }
}
