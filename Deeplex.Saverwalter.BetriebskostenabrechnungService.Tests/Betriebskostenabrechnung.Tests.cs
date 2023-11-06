using Deeplex.Saverwalter.Model;

using FluentAssertions;
using Xunit;
using FakeItEasy;
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
        ctx.Mieten.AddRange(mieten);

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

}