using System;

namespace Deeplex.Saverwalter.Model
{
    public class UnitAttribute : Attribute
    {
        public string Unit;

        public UnitAttribute(string unit)
        {
            Unit = unit;
        }
    }
}
