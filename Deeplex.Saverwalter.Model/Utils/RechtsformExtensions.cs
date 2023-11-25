﻿using System.ComponentModel;

namespace Deeplex.Saverwalter.Model
{
    public static class RechtsformExtensions
    {
        public static string ToDescriptionString(this Rechtsform val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())!
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}