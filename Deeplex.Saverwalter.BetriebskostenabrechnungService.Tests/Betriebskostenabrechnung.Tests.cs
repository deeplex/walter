using Deeplex.Saverwalter.Model;

using FluentAssertions;
using Xunit;
using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Microsoft.EntityFrameworkCore;

public class BetriebskostenabrechnungTests
{
    private SaverwalterContext GetContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
        optionsBuilder.UseInMemoryDatabase("TestDb");
        var ctx = new SaverwalterContext(optionsBuilder.Options);

        return ctx;
    }
    private Vertrag FillVertragWithSomeData(SaverwalterContext ctx, double grundmiete)
    {
        var vermieter = new NatuerlichePerson("TestKopf")
        {
            Anrede = Anrede.Frau,
            Vorname = "TestVorname",
        };

        ctx.NatuerlichePersonen.Add(vermieter);

        var wohnung = new Wohnung("TestWohnung", 100, 100, 1)
        {
            Adresse = new Adresse("TestStraße", "TestHausnummer", "TestPLZ", "TestOrt"),
            BesitzerId = vermieter.PersonId
        };

        var vertrag = new Vertrag()
        {
            AnsprechpartnerId = vermieter.PersonId,
            Wohnung = wohnung
        };
        var version = new VertragVersion(
            new DateOnly(2020, 1, 1),
            grundmiete,
            1
        )
        {
            Vertrag = vertrag
        };
        ctx.VertragVersionen.Add(version);

        return vertrag;
    }

    private List<Miete> Add12Mieten(Vertrag vertrag, double grundmiete)
    {
        var mieten = new List<Miete>();
        for (int month = 1; month <= 12; month++)
        {
            mieten.Add(new Miete(
                new DateOnly(2021, month, 1),
                new DateOnly(2021, month, 1),
                grundmiete)
            {
                Vertrag = vertrag,
            });
        }

        return mieten;
    }

    private List<Miete> Add6Mieten(Vertrag vertrag, double grundmiete)
    {
        var mieten = new List<Miete>();
        for (int month = 1; month <= 6; month++)
        {
            mieten.Add(new Miete(
                new DateOnly(2021, month, 1),
                new DateOnly(2021, month, 1),
                grundmiete)
            {
                Vertrag = vertrag,
            });
        }

        return mieten;
    }

    private List<Umlage> Add6Umlagen(List<Wohnung> wohnungen)
    {
        var umlagen = new List<Umlage>();
        var date = new DateOnly(2021, 1, 1);

        var grundsteuer = new Umlage(Betriebskostentyp.Grundsteuer, Umlageschluessel.NachWohnflaeche)
        {
            Wohnungen = wohnungen
        };
        var grundsteuer_rechnung = new Betriebskostenrechnung(1000, date, 2021) { Umlage = grundsteuer };
        grundsteuer.Betriebskostenrechnungen.Add(grundsteuer_rechnung);
        umlagen.Add(grundsteuer);

        var dachrinnenreinigung = new Umlage(Betriebskostentyp.Dachrinnenreinigung, Umlageschluessel.NachNutzeinheit)
        {
            Wohnungen = wohnungen
        };
        var dachrinnenreinigung_rechnung = new Betriebskostenrechnung(500, date, 2021) { Umlage = dachrinnenreinigung };
        dachrinnenreinigung.Betriebskostenrechnungen.Add(dachrinnenreinigung_rechnung);
        umlagen.Add(dachrinnenreinigung);

        var gartenpflege = new Umlage(Betriebskostentyp.Gartenpflege, Umlageschluessel.NachNutzeinheit);
        gartenpflege.Wohnungen.Add(wohnungen.First());
        var gartenpflege_rechnung = new Betriebskostenrechnung(650, date, 2021) { Umlage = gartenpflege };
        gartenpflege.Betriebskostenrechnungen.Add(gartenpflege_rechnung);
        umlagen.Add(gartenpflege);

        var allgemeinstrom = new Umlage(Betriebskostentyp.AllgemeinstromHausbeleuchtung, Umlageschluessel.NachWohnflaeche)
        {
            Wohnungen = wohnungen
        };
        var allgemeinstrom_rechnung = new Betriebskostenrechnung(200, date, 2021) { Umlage = allgemeinstrom };
        allgemeinstrom.Betriebskostenrechnungen.Add(allgemeinstrom_rechnung);
        umlagen.Add(allgemeinstrom);

        var muellbeseitigung = new Umlage(Betriebskostentyp.Muellbeseitigung, Umlageschluessel.NachPersonenzahl)
        {
            Wohnungen = wohnungen
        };
        var muellbeseitigung_rechnung = new Betriebskostenrechnung(1000, date, 2021) { Umlage = muellbeseitigung };
        muellbeseitigung.Betriebskostenrechnungen.Add(muellbeseitigung_rechnung);
        umlagen.Add(muellbeseitigung);

        var heizkosten = new Umlage(Betriebskostentyp.Heizkosten, Umlageschluessel.NachVerbrauch)
        {
            Wohnungen = wohnungen
        };
        var heizkosten_rechnung = new Betriebskostenrechnung(2000, date, 2021) { Umlage = heizkosten };
        heizkosten.Betriebskostenrechnungen.Add(heizkosten_rechnung);

        var allgemeinZaehler = new Zaehler("Allgemein_Heizung", Zaehlertyp.Gas);
        var allgemeinZaehlerstand_beginn = new Zaehlerstand(new DateOnly(2021, 1, 1), 0)
        {
            Zaehler = allgemeinZaehler

        };
        var allgemeinZaehlerstand_ende = new Zaehlerstand(new DateOnly(2021, 12, 31), 25000)
        {
            Zaehler = allgemeinZaehler
        };
        allgemeinZaehler.Staende.Add(allgemeinZaehlerstand_beginn);
        allgemeinZaehler.Staende.Add(allgemeinZaehlerstand_ende);
        heizkosten.Zaehler.Add(allgemeinZaehler);
        foreach (var wohnung in wohnungen)
        {
            var zaehler = new Zaehler("Heizung", Zaehlertyp.Gas)
            {
                Wohnung = wohnung
            };
            var zaehlerstand_beginn = new Zaehlerstand(new DateOnly(2021, 1, 1), 0)
            {
                Zaehler = zaehler

            };
            var zaehlerstand_ende = new Zaehlerstand(new DateOnly(2021, 12, 31), 10000)
            {
                Zaehler = zaehler
            };
            zaehler.Staende.Add(zaehlerstand_beginn);
            zaehler.Staende.Add(zaehlerstand_ende);
            heizkosten.Zaehler.Add(zaehler);
        }

        foreach (var wohnung in wohnungen)
        {
            var zaehler = new Zaehler("Heizung", Zaehlertyp.Warmwasser)
            {
                Wohnung = wohnung
            };
            var zaehlerstand_beginn = new Zaehlerstand(new DateOnly(2021, 1, 1), 0)
            {
                Zaehler = zaehler

            };
            var zaehlerstand_ende = new Zaehlerstand(new DateOnly(2021, 12, 31), 30)
            {
                Zaehler = zaehler
            };
            zaehler.Staende.Add(zaehlerstand_beginn);
            zaehler.Staende.Add(zaehlerstand_ende);
            heizkosten.Zaehler.Add(zaehler);
        }
        umlagen.Add(heizkosten);

        return umlagen;
    }


    [Fact]
    public void EverythingZeroTest()
    {
        // Arrange
        var ctx = GetContext();
        var vertrag = FillVertragWithSomeData(ctx, 0);

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );


        // Assert
        testObject.Result.Should().Be(0);
    }

    [Fact]
    public void MieteGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);

        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(0);
    }

    [Fact]
    public void MieteNichtGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(-12000);
    }

    [Fact]
    public void MieteZuVielGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);

        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete + 100));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(1200);
    }

    [Fact]
    public void VertragVorzeitigZuendeKeineMiete()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        vertrag.Ende = new DateOnly(2021, 6, 30);

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(-6000);
    }

    [Fact]
    public void VertragVorzeitigZuendeMieteGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        vertrag.Ende = new DateOnly(2021, 6, 30);

        ctx.Mieten.AddRange(Add6Mieten(vertrag, grundmiete));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(0);
    }

    [Fact]
    public void VertragVorzeitigZuendeZuvielMieteGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        vertrag.Ende = new DateOnly(2021, 6, 30);

        ctx.Mieten.AddRange(Add6Mieten(vertrag, grundmiete + 100));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(600);
    }

    [Fact]
    public void VertragAenderungKeineMieteGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        ctx.VertragVersionen.Add(new VertragVersion(
            new DateOnly(2021, 7, 1),
            grundmiete * 2,
            2)
        {
            Vertrag = vertrag
        });

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(-18000);
    }

    [Fact]
    public void VertragAenderungMieteGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        ctx.VertragVersionen.Add(new VertragVersion(
            new DateOnly(2021, 7, 1),
            grundmiete * 2,
            2)
        {
            Vertrag = vertrag
        });

        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete * 1.5));
        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(0);
    }

    [Fact]
    public void VertragAenderungZuVielMieteGezahlt()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        ctx.VertragVersionen.Add(new VertragVersion(
            new DateOnly(2021, 7, 1),
            grundmiete * 2,
            2)
        {
            Vertrag = vertrag
        });

        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete * 2));
        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(6000);
    }

    [Fact]
    public void EverythingZeroTestWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var vertrag = FillVertragWithSomeData(ctx, 0);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(-3000);
    }

    [Fact]
    public void MieteGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);

        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete + 250));
        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(0);
    }

    [Fact]
    public void MieteNichtGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(-15000);
    }

    [Fact]
    public void MieteZuVielGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete + 350));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().Be(1200);
    }

    [Fact]
    public void VertragVorzeitigZuendeKeineMieteWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        vertrag.Ende = new DateOnly(2021, 6, 30);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().BeApproximately(-7227.33, 0.01);
    }

    [Fact]
    public void VertragVorzeitigZuendeMieteGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        vertrag.Ende = new DateOnly(2021, 6, 30);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.Mieten.AddRange(Add6Mieten(vertrag, grundmiete + 250));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().BeApproximately(272.67, 0.01);
    }

    [Fact]
    public void VertragVorzeitigZuendeZuvielMieteGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);

        vertrag.Ende = new DateOnly(2021, 6, 30);

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.Mieten.AddRange(Add6Mieten(vertrag, grundmiete + 350));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().BeApproximately(872.67, 0.01);
    }

    [Fact]
    public void VertragAenderungKeineMieteGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);
        ctx.VertragVersionen.Add(new VertragVersion(
            new DateOnly(2021, 7, 1),
            grundmiete * 2,
            2)
        {
            Vertrag = vertrag
        });

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));

        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().BeApproximately(-21084.018, 0.01);
    }

    [Fact]
    public void VertragAenderungMieteGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);
        ctx.VertragVersionen.Add(new VertragVersion(
            new DateOnly(2021, 7, 1),
            grundmiete * 2,
            2)
        {
            Vertrag = vertrag
        });

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete * 1.5 + 250));
        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().BeApproximately(-84.02, 0.01);
    }

    [Fact]
    public void VertragAenderungZuVielMieteGezahltWithUmlagen()
    {
        // Arrange
        var ctx = GetContext();
        var grundmiete = 1000;
        var vertrag = FillVertragWithSomeData(ctx, grundmiete);
        var vertrag2 = FillVertragWithSomeData(ctx, 0);
        ctx.VertragVersionen.Add(new VertragVersion(
            new DateOnly(2021, 7, 1),
            grundmiete * 2,
            2)
        {
            Vertrag = vertrag
        });

        ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
        ctx.Mieten.AddRange(Add12Mieten(vertrag, grundmiete * 2 + 250));
        ctx.SaveChanges();

        // Act
        var testObject = new Betriebskostenabrechnung(
            ctx,
            vertrag,
            2021,
            new DateOnly(2021, 1, 1),
            new DateOnly(2021, 12, 31)
        );

        // Assert
        testObject.Result.Should().BeApproximately(5915.98, 0.01);
    }
}