namespace Deeplex.Saverwalter.Model
{
    // Used to determine Betriebskostengruppen.
    public sealed class SortedSetIntEqualityComparer : EqualityComparer<SortedSet<int>>
    {
        public override bool Equals(SortedSet<int>? x, SortedSet<int>? y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }
            if (y is null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return x.SetEquals(y);
        }
        public override int GetHashCode(SortedSet<int> obj)
        {
            return obj.Sum(i => i.GetHashCode());
        }
    }
}
