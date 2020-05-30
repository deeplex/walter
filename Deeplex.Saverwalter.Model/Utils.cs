using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    public static class KalteBetriebskostenExtensions
    {
        public static DateTime AsUtcKind(this DateTime dt)
        {
            return new DateTime(dt.Ticks, DateTimeKind.Utc);
        }

        public static string ToDescriptionString(this Betriebskostentyp val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
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
    public class SortedSetIntEqualityComparer : EqualityComparer<SortedSet<int>>
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
