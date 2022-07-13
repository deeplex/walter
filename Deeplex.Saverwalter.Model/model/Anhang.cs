using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Anhang
    {
        public Guid AnhangId { get; set; }
        public string FileName { get; set; } = null!;
        public byte[] Sha256Hash { get; set; } = null!;
        public string? ContentType { get; set; }
        public DateTime CreationTime { get; set; }

        public List<Adresse> Adressen { get; set; } = new List<Adresse>();
        public List<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = new List<Betriebskostenrechnung>();
        public List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; set; } = new List<Erhaltungsaufwendung>();
        public List<Garage> Garagen { get; set; } = new List<Garage>();
        public List<JuristischePerson> JuristischePersonen { get; set; } = new List<JuristischePerson>();
        public List<Konto> Konten { get; set; } = new List<Konto>();
        public List<Miete> Mieten { get; set; } = new List<Miete>();
        public List<MietMinderung> Mietminderungen { get; set; } = new List<MietMinderung>();
        public List<NatuerlichePerson> NatuerlichePersonen { get; set; } = new List<NatuerlichePerson>();
        public List<Vertrag> Vertraege { get; set; } = new List<Vertrag>();
        public List<VertragsBetriebskostenrechnung> VertragsBetriebskostenrechnungen { get; set; } = new List<VertragsBetriebskostenrechnung>();
        public List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public List<Zaehler> Zaehler { get; set; } = new List<Zaehler>();
        public List<Zaehlerstand> Zaehlerstaende { get; set; } = new List<Zaehlerstand>();

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