namespace Deeplex.Saverwalter.Model
{
    public static class VertragExtensions
    {
        public static DateOnly Beginn(this Vertrag v) => v.Versionen.OrderBy(e => e.Beginn).FirstOrDefault()?.Beginn ?? default;
        public static DateOnly? Ende(this VertragVersion v)
            => v.Vertrag.Versionen.OrderBy(e => e.Beginn).FirstOrDefault(e => e.Beginn > v.Beginn)?.Beginn.AddDays(-1) ??
                v.Vertrag.Ende;
    }
}
