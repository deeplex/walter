using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Helper
{
    public class AdresseEntry
    {
        private Adresse Entity { get; }
        public string? Strasse { get; set; }
        public string? Hausnummer { get; set; }
        public string? Postleitzahl { get; set; }
        public string? Stadt { get; set; }
        public string? Anschrift { get; set; }

        public AdresseEntry() { }
        public AdresseEntry(Adresse entity)
        {
            Entity = entity;
            Strasse = Entity.Strasse;
            Hausnummer = Entity.Hausnummer;
            Postleitzahl = Entity.Postleitzahl;
            Stadt = Entity.Stadt;
            Anschrift = Entity.Anschrift;
        }
    }
}
