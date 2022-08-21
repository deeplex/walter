using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    public static class VertragExtensions
    {
        public static DateTime Beginn(this Vertrag v) => v.Versionen.OrderBy(e => e.Beginn).FirstOrDefault()?.Beginn ?? default;
        public static DateTime? Ende(this VertragVersion v) => v.Vertrag.Versionen.OrderBy(e => e.Beginn).FirstOrDefault(e => e.Beginn > v.Beginn)?.Beginn.AddDays(-1);
    }
}
