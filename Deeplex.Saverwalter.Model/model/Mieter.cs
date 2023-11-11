using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    // JOIN_TABLE
    public class Mieter
    {
        public int MieterId { get; set; }
        [Required]
        public Guid PersonId { get; set; }
        [Required]
        public virtual Vertrag Vertrag { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Mieter(Guid personId)
        {
            PersonId = personId;
        }
    }
}
