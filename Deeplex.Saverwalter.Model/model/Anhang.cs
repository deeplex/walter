using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public class Anhang
    {
        public Guid AnhangId { get; set; }
        public string FileName { get; set; } = null!;
        public string? ContentType { get; set; }
        public DateTime CreationTime { get; set; }

        public virtual List<Adresse> Adressen { get; set; } = new List<Adresse>();
        public virtual List<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = new List<Betriebskostenrechnung>();
        public virtual List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; set; } = new List<Erhaltungsaufwendung>();
        public virtual List<Garage> Garagen { get; set; } = new List<Garage>();
        public virtual List<JuristischePerson> JuristischePersonen { get; set; } = new List<JuristischePerson>();
        public virtual List<Konto> Konten { get; set; } = new List<Konto>();
        public virtual List<Miete> Mieten { get; set; } = new List<Miete>();
        public virtual List<Mietminderung> Mietminderungen { get; set; } = new List<Mietminderung>();
        public virtual List<NatuerlichePerson> NatuerlichePersonen { get; set; } = new List<NatuerlichePerson>();
        public virtual List<Vertrag> Vertraege { get; set; } = new List<Vertrag>();
        public virtual List<VertragVersion> VertragVersionen { get; set; } = new List<VertragVersion>();
        public virtual List<VertragsBetriebskostenrechnung> VertragsBetriebskostenrechnungen { get; set; } = new List<VertragsBetriebskostenrechnung>();
        public virtual List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public virtual List<Zaehler> Zaehler { get; set; } = new List<Zaehler>();
        public virtual List<Zaehlerstand> Zaehlerstaende { get; set; } = new List<Zaehlerstand>();
        public virtual List<Umlage> Umlagen { get; set; } = new List<Umlage>();

        public Anhang()
        {
            AnhangId = Guid.NewGuid();
        }
    }

    public interface IAnhang
    {
        public List<Anhang> Anhaenge { get; set; }
    }
}