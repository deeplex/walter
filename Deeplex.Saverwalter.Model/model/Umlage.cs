using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Umlage
    {
        public int UmlageId { get; set; }
        [Required]
        public virtual Umlagetyp Typ { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public Umlageschluessel Schluessel { get; set; }
        public string? Beschreibung { get; set; }
        public string? Notiz { get; set; }
        public virtual HKVO? HKVO { get; set; }

        public virtual List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public virtual List<Betriebskostenrechnung> Betriebskostenrechnungen { get; private set; } = new List<Betriebskostenrechnung>();
        public virtual List<Zaehler> Zaehler { get; set; } = new List<Zaehler>();
        // Stromrechnungen keep track of the HKVOs that point to them
        public virtual List<HKVO> HKVOs { get; set; } = new List<HKVO>();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Umlage(Umlageschluessel schluessel)
        {
            Schluessel = schluessel;
        }
    }

    public enum Umlageschluessel
    {
        [Description("n. WF")]
        NachWohnflaeche,
        [Description("n. NE")]
        NachNutzeinheit,
        [Description("n. Pers.")]
        NachPersonenzahl,
        [Description("n. Verb.")]
        NachVerbrauch,
        [Description("n. NF")]
        NachNutzflaeche,
    }
}
