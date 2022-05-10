using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public interface IPerson : IAdresse
    {
        public Guid PersonId { get; set; }
        public string Bezeichnung { get; }
        public bool isVermieter { get; set; }
        public bool isMieter { get; set; }
        public bool isHandwerker { get; set; }
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public int? AdresseId { get; set; }
        public string? Notiz { get; set; }
        public List<JuristischePerson> JuristischePersonen { get; set; }
    }

}
