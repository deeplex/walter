namespace Deeplex.Saverwalter.Model
{
    public static class KalteBetriebskostenExtensions
    {
        public static DateTime AsMin(this DateTime t)
            => (t.Ticks == 0 ? DateTime.Now : t).AsUtcKind();

        public static DateTime AsUtcKind(this DateTime dt) => new(dt.Ticks, DateTimeKind.Utc);

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
            return attributes.Length > 0 ? attributes[0].UnitString : "";
        }

        public static Zaehlereinheit ToUnit(this Zaehlertyp typ)
        {
            var field = typ
                .GetType()
                .GetField(typ.ToString());

            if (field == null)
            {
                throw new ArgumentException("Zählertyp has no Unit");
            }


            UnitAttribute[] attributes = (UnitAttribute[])field.GetCustomAttributes(typeof(UnitAttribute), false);
            return attributes.Length > 0 ? attributes[0].Unit : Zaehlereinheit.Dimensionslos;

        }


        public static string ToUnitString(this Zaehlereinheit unit)
        {
            var field = unit
                .GetType()
                .GetField(unit.ToString());

            if (field == null)
            {
                throw new ArgumentException("Zählertyp has no UnitString");
            }

            UnitStringAttribute[] attributes = (UnitStringAttribute[])field.GetCustomAttributes(typeof(UnitStringAttribute), false);
            return attributes.Length > 0 ? attributes[0].UnitString : string.Empty;
        }
    }
}
