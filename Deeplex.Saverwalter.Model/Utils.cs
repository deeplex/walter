using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    public static class KalteBetriebskostenExtensions
    {
        public static string ToDescriptionString(this KalteBetriebskosten val)
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
}
