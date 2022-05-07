using System;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Anhang
    {
        public Guid AnhangId { get; set; }
        public string FileName { get; set; } = null!;
        public byte[] Sha256Hash { get; set; } = null!;
        public string? ContentType { get; set; }
        public DateTime CreationTime { get; set; }

        public Anhang()
        {
            AnhangId = Guid.NewGuid();
        }
    }

    public abstract class AnhangRef : IAnhang
    {
        public Anhang Anhang { get; set; } = null!;
        public Guid AnhangId { get; set; }
    }

    public interface IAnhang
    {
        public Guid AnhangId { get; set; }
        public Anhang Anhang { get; set; }
    }
    public interface IAnhang<T> : IAnhang
    {
        public T Target { get; set; }
    }

    public sealed class NatuerlichePersonAnhang : AnhangRef, IAnhang<NatuerlichePerson>
    {
        public int NatuerlichePersonAnhangId { get; set; }
        public NatuerlichePerson Target { get; set; } = null!;
    }

    public sealed class JuristischePersonAnhang : AnhangRef, IAnhang<JuristischePerson>
    {
        public int JuristischePersonAnhangId { get; set; }
        public JuristischePerson Target { get; set; } = null!;
    }

    public sealed class WohnungAnhang : AnhangRef, IAnhang<Wohnung>
    {
        public int WohnungAnhangId { get; set; }
        public Wohnung Target { get; set; } = null!;
    }

    public sealed class GarageAnhang : AnhangRef, IAnhang<Garage>
    {
        public int GarageAnhangId { get; set; }
        public Garage Target { get; set; } = null!;
    }

    public sealed class AdresseAnhang : AnhangRef, IAnhang<Adresse>
    {
        public int AdresseAnhangId { get; set; }
        public Adresse Target { get; set; } = null!;
    }

    public sealed class VertragAnhang : AnhangRef, IAnhang<Guid>
    {
        public int VertragAnhangId { get; set; }
        public Guid Target { get; set; }
        //public VertragAnhangTyp Typ { get; set; }
    }
    public enum VertragAnhangTyp
    {
        Sonstiges,
        Mietvertrag,
    }

    public sealed class MietMinderungAnhang : AnhangRef, IAnhang<MietMinderung>
    {
        public int MietMinderungAnhangId { get; set; }
        public MietMinderung Target { get; set; } = null!;
    }

    public sealed class KontoAnhang : AnhangRef, IAnhang<Konto>
    {
        public int KontoAnhangId { get; set; }
        public Konto Target { get; set; } = null!;
    }

    public sealed class BetriebskostenrechnungAnhang : AnhangRef, IAnhang<Betriebskostenrechnung>
    {
        public int BetriebskostenrechnungAnhangId { get; set; }
        public Betriebskostenrechnung Target { get; set; } = null!;
    }

    public sealed class VertragsBetriebskostenrechnungAnhang : AnhangRef, IAnhang<VertragsBetriebskostenrechnung>
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        public VertragsBetriebskostenrechnung Target { get; set; } = null!;
    }

    public sealed class ErhaltungsaufwendungAnhang : AnhangRef, IAnhang<Erhaltungsaufwendung>
    {
        public int ErhaltungsaufwendungAnhangId { get; set; }
        public Erhaltungsaufwendung Target { get; set; } = null!;
    }

    public sealed class ZaehlerAnhang : AnhangRef, IAnhang<Zaehler>
    {
        public int ZaehlerAnhangId { get; set; }
        public Zaehler Target { get; set; } = null!;
    }

    public sealed class ZaehlerstandAnhang : AnhangRef, IAnhang<Zaehlerstand>
    {
        public int ZaehlerstandAnhangId { get; set; }
        public Zaehlerstand Target { get; set; } = null!;
    }

    public sealed class MieteAnhang : AnhangRef, IAnhang<Miete>
    {
        public int MieteAnhangId { get; set; }
        public Miete Target { get; set; } = null!;
    }
}