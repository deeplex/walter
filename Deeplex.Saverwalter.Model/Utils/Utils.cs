namespace Deeplex.Saverwalter.Model
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UnitAttribute : Attribute
    {
        public Zaehlereinheit Unit;
        public string UnitString;

        public UnitAttribute(Zaehlereinheit unit)
        {
            Unit = unit;
            UnitString = unit.ToUnitString();
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UnitStringAttribute : Attribute
    {
        public string UnitString;

        public UnitStringAttribute(Zaehlereinheit unit)
        {
            UnitString = unit.ToUnitString();
        }

        public UnitStringAttribute(string unit)
        {
            UnitString = unit;
        }
    }
}
