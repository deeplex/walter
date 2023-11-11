namespace Deeplex.Saverwalter.Model
{
    public interface IPerson
    {
        public Guid PersonId { get; set; }
        public string Bezeichnung { get; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Notiz { get; set; }
        public Adresse? Adresse { get; set; }

        public DateTime CreatedAt { get; }
        public DateTime LastModified { get; set; }

        public List<JuristischePerson> JuristischePersonen { get; set; }
    }

}
