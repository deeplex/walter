namespace Deeplex.Saverwalter.Model
{
    // JOIN_TABLE
    public class Mieter
    {
        public int MieterId { get; set; }
        public Guid PersonId { get; set; }
        public virtual Vertrag Vertrag { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078

        public Mieter(Guid personId)
        {
            PersonId = personId;
        }
    }
}
