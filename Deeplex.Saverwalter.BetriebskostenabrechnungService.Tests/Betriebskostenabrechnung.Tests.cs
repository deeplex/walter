using Deeplex.Saverwalter.Model;

using FluentAssertions;
using Xunit;
using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Microsoft.EntityFrameworkCore;
using Deeplex.Saverwalter.ModelTests;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests
{
    public class BetriebskostenabrechnungTests
    {
        [Fact]
        public void EverythingZeroTest()
        {
            // Arrange
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 0);

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);

            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);

            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete + 100));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            vertrag.Ende = new DateOnly(2021, 6, 30);

            ctx.Mieten.AddRange(TestUtils.Add6Mieten(vertrag, grundmiete));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            vertrag.Ende = new DateOnly(2021, 6, 30);

            ctx.Mieten.AddRange(TestUtils.Add6Mieten(vertrag, grundmiete + 100));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            ctx.VertragVersionen.Add(new VertragVersion(
                new DateOnly(2021, 7, 1),
                grundmiete * 2,
                2)
            {
                Vertrag = vertrag
            });

            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete * 1.5));
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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            ctx.VertragVersionen.Add(new VertragVersion(
                new DateOnly(2021, 7, 1),
                grundmiete * 2,
                2)
            {
                Vertrag = vertrag
            });

            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete * 2));
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
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 0);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);

            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete + 250));
            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete + 350));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            vertrag.Ende = new DateOnly(2021, 6, 30);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            vertrag.Ende = new DateOnly(2021, 6, 30);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
            ctx.Mieten.AddRange(TestUtils.Add6Mieten(vertrag, grundmiete + 250));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);

            vertrag.Ende = new DateOnly(2021, 6, 30);

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
            ctx.Mieten.AddRange(TestUtils.Add6Mieten(vertrag, grundmiete + 350));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);
            ctx.VertragVersionen.Add(new VertragVersion(
                new DateOnly(2021, 7, 1),
                grundmiete * 2,
                2)
            {
                Vertrag = vertrag
            });

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));

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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);
            ctx.VertragVersionen.Add(new VertragVersion(
                new DateOnly(2021, 7, 1),
                grundmiete * 2,
                2)
            {
                Vertrag = vertrag
            });

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete * 1.5 + 250));
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
            var ctx = TestUtils.GetContext();
            var grundmiete = 1000;
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, grundmiete);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 0);
            ctx.VertragVersionen.Add(new VertragVersion(
                new DateOnly(2021, 7, 1),
                grundmiete * 2,
                2)
            {
                Vertrag = vertrag
            });

            ctx.Umlagen.AddRange(TestUtils.Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));
            ctx.Mieten.AddRange(TestUtils.Add12Mieten(vertrag, grundmiete * 2 + 250));
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
}