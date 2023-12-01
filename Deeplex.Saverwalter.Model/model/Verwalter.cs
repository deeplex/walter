using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Verwalter
    {
        public int VerwalterId { get; set; }
        [Required]
        public virtual Kontakt Kontakt { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public VerwalterRolle Rolle { get; set; }
        public string? Notiz { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Verwalter(VerwalterRolle rolle)
        {
            Rolle = rolle;
        }
    }

    public enum VerwalterRolle
    {
        Keine,
        Vollmacht
    }
}
