using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Deeplex.Saverwalter.ModelTests.model
{
    public class NatuerlichePersonTests
    {
        private SaverwalterContext DatabaseContext;

        public NatuerlichePersonTests()
        {
            var options = new DbContextOptionsBuilder<SaverwalterContext>()
                .UseInMemoryDatabase(databaseName: "test_database")
            .Options;

            DatabaseContext = new SaverwalterContext(options);
        }

        public static IEnumerable<object[]> ShouldWorkData =>
            new List<object[]>
            {
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich) },
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich) { Vorname = "Test Vorname" } },
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich)
                {
                    Vorname = "Test Vorname",
                    Telefon = "Test Telefon",
                    Email = "Test Email@testmail.test",
                    Fax = "Test Fax",
                    Mobil = "Test Mobil",
                    Notiz = "Test Notiz",
                } },
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich)
                {
                    Anrede = Anrede.Frau,
                } },
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich)
                {
                    Anrede = Anrede.Herr,
                } },
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich)
                {
                    Anrede = Anrede.Keine,
                } },
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich)
                {
                    Name = "Test Nachname",
                    JuristischePersonen = new List<Kontakt>()
                    {
                        new Kontakt("Test Nachname", Rechtsform.gmbh)
                        {
                            Name = "Test JuristischePerson"
                        }
                    }
                } }
            };

        [Theory(Skip = "InMemoryDatabaseNotRunning")]
        [InlineData("Mustername")]
        public void ShouldAddNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new Kontakt(nachname, Rechtsform.natuerlich);

            // Act
            var result = DatabaseContext.Kontakte.Add(entity);
            DatabaseContext.SaveChanges();

            // Assert
            result.Should().BeEquivalentTo(entity, options => options.ExcludingMissingMembers());
            result.Entity.KontaktId.Should().BeGreaterThan(0);
            result.Entity.Bezeichnung.Should().BeEquivalentTo(string.Join(" ", entity.Vorname ?? "", entity.Name));
            DatabaseContext.Set<Kontakt>().Should().Contain(entity);
        }

        public static IEnumerable<object[]> ShouldNotWorkData
            => new List<object[]>
            {
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich)
                {
                Name = null!
                } },
                new object[] { new Kontakt("Test Nachname", Rechtsform.natuerlich)
                {
                    Name = null!,
                    Vorname = "Test Vorname",
                    Telefon = "Test Telefon",
                    Email = "Test Email@testmail.test",
                    Fax = "Test Fax",
                    Mobil = "Test Mobil",
                    Notiz = "Test Notiz",
                } }
            };

        [Theory(Skip = "InMemoryDatabaseNotRunning")]
        [InlineData("Mustername")]
        public void ShouldNotAddNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new Kontakt(nachname, Rechtsform.natuerlich);

            // Act
            DatabaseContext.Add(entity);
            var result = DatabaseContext.Set<Kontakt>().FirstOrDefault(e => e.KontaktId == entity.KontaktId);

            // Assert
            try
            {
                DatabaseContext.SaveChanges();
                Assert.Fail("Expected DbUpdateException was not thrown.");
            }
            catch (DbUpdateException ex)
            {
                ex.Should().NotBeNull();
            }

            result.Should().BeNull();
            DatabaseContext.Set<Kontakt>().Should().NotContain(entity);
        }

        [Theory(Skip = "InMemoryDatabaseNotRunning")]
        [InlineData("Mustername")]
        public void ShouldUpdateNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new Kontakt(nachname, Rechtsform.natuerlich);
            DatabaseContext.Add(entity);
            DatabaseContext.SaveChanges();
            var testEntity = DatabaseContext.Set<Kontakt>().FirstOrDefault(e => e.KontaktId == entity.KontaktId)!;
            testEntity.Email = "Updated Test Email";

            // Act
            var result = DatabaseContext.Update(entity);
            DatabaseContext.SaveChanges();

            // Assert
            result.Entity.Should().BeEquivalentTo(testEntity, options => options.ExcludingMissingMembers());
            result.Entity.KontaktId.Should().Be(entity.KontaktId);
            result.Entity.Email.Should().Be(testEntity.Email);
            DatabaseContext.Set<Kontakt>().Should().Contain(entity);
        }

        [Theory(Skip = "InMemoryDatabaseNotRunning")]
        [InlineData("Mustername")]
        public void ShouldNotUpdateNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new Kontakt(nachname, Rechtsform.natuerlich);
            DatabaseContext.Add(entity);
            DatabaseContext.SaveChanges();
            var testEntity = DatabaseContext.Set<Kontakt>().FirstOrDefault(e => e.KontaktId == entity.KontaktId)!;
            testEntity.Email = "Updated Test Email";
            testEntity.Name = null!;
            DatabaseContext.Kontakte.Update(testEntity);

            // Act
            try
            {
                DatabaseContext.SaveChanges();
                Assert.Fail("Expected DbUpdateException was not thrown.");
            }
            catch (DbUpdateException ex)
            {
                ex.Should().NotBeNull();
            }
            var result = DatabaseContext.Set<Kontakt>().FirstOrDefault(e => e.KontaktId == entity.KontaktId)!;

            // Assert

            result.Should().NotBeNull();
            DatabaseContext.Set<Kontakt>().Should().Contain(result);
            result.Should().NotBeEquivalentTo(testEntity, options => options.ExcludingMissingMembers());
            result.Email.Should().NotBe(testEntity.Email);
            DatabaseContext.Set<Kontakt>().Should().Contain(result);
        }
    }
}
