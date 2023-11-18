using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class HKVO
    {
        public int HKVOId { get; set; }

        [Required]
        public double HKVO_P7 { get; set; }
        [Required]
        public double HKVO_P8 { get; set; }
        [Required]
        public HKVO_P9A2 HKVO_P9 { get; set; }
        [Required]
        public double Strompauschale { get; set; }

        public int BetriebsstromId { get; set; }
        public virtual Umlage Betriebsstrom { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078

        public string? Notiz { get; set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public HKVO(double hKVO_P7, double hKVO_P8, HKVO_P9A2 hKVO_P9, double strompauschale)
        {
            HKVO_P7 = hKVO_P7;
            HKVO_P8 = hKVO_P8;
            HKVO_P9 = hKVO_P9;

            // TODO this could be calculated by a zaehler, too... Then it would be different for each year...
            Strompauschale = strompauschale;
        }
    }

    public enum HKVO_P9A2
    {
        Satz_1 = 1,
        Satz_2 = 2,
        Satz_4 = 4,
    }
}
