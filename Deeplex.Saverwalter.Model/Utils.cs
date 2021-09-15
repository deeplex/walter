using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

    public static class AnhangExtensions
    {
        public static string getPath(this Anhang a, string root)
        {
            return System.IO.Path.Combine(root, a.AnhangId + System.IO.Path.GetExtension(a.FileName));
        }
    }

    public static class KalteBetriebskostenExtensions
    {
        public static DateTime AsMin(this DateTime t)
            => t.Ticks == 0 ? DateTime.Now.AsUtcKind() : t;

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

    public static class UmlageSchluesselExtensions
    {
        public static string ToDescriptionString(this UmlageSchluessel val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }

    // Used to determine Betriebskostengruppen.
    public sealed class SortedSetIntEqualityComparer : EqualityComparer<SortedSet<int>>
    {
        public override bool Equals(SortedSet<int> x, SortedSet<int> y)
        {
            return x.SetEquals(y);
        }
        public override int GetHashCode(SortedSet<int> obj)
        {
            return obj.Sum(i => i.GetHashCode());
        }
    }
}
