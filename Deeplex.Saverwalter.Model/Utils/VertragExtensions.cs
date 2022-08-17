using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    public static class VertragExtensions
    {
        public static DateTime Beginn(this Vertrag v) => v.Versionen.OrderBy(e => e.Beginn).First().Beginn;
        public static DateTime? Ende(this Vertrag v) => v.Versionen.OrderBy(e => e.Beginn).Last().Ende;
    }
}
