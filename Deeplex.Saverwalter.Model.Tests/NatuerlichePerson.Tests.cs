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
                new object[] { new NatuerlichePerson("Test Nachname") },
                new object[] { new NatuerlichePerson("Test Nachname") { Vorname = "Test Vorname" } },
                new object[] { new NatuerlichePerson("Test Nachname")
                {
                    Vorname = "Test Vorname",
                    Telefon = "Test Telefon",
                    Email = "Test Email@testmail.test",
                    Fax = "Test Fax",
                    Mobil = "Test Mobil",
                    Notiz = "Test Notiz",
                } },
                new object[] { new NatuerlichePerson("Test Nachname")
                {
                    Anrede = Anrede.Frau,
                } },
                new object[] { new NatuerlichePerson("Test Nachname")
                {
                    Anrede = Anrede.Herr,
                } },
                new object[] { new NatuerlichePerson("Test Nachname")
                {
                    Anrede = Anrede.Keine,
                } },
                new object[] { new NatuerlichePerson("Test Nachname")
                {
                    Nachname = "Test Nachname",
                    JuristischePersonen = new List<JuristischePerson>()
                    {
                        new JuristischePerson("Test Nachname")
                        {
                            Bezeichnung = "Test JuristischePerson"
                        }
                    }
                } }
            };

        [Theory(Skip = "InMemoryDatabaseNotRunning")]
        [InlineData("Mustername")]
        public void ShouldAddNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new NatuerlichePerson(nachname);

            // Act
            var result = DatabaseContext.NatuerlichePersonen.Add(entity);
            DatabaseContext.SaveChanges();

            // Assert
            result.Should().BeEquivalentTo(entity, options => options.ExcludingMissingMembers());
            result.Entity.NatuerlichePersonId.Should().BeGreaterThan(0);
            result.Entity.PersonId.Should().NotBeEmpty();
            result.Entity.Bezeichnung.Should().BeEquivalentTo(string.Join(" ", entity.Vorname ?? "", entity.Nachname));
            DatabaseContext.Set<NatuerlichePerson>().Should().Contain(entity);
        }

        public static IEnumerable<object[]> ShouldNotWorkData
            => new List<object[]>
            {
                new object[] { new NatuerlichePerson("Test Nachname")
                {
                Nachname = null!
                } },
                new object[] { new NatuerlichePerson("Test Nachname")
                {
                    Nachname = null!,
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
            var entity = new NatuerlichePerson(nachname);

            // Act
            DatabaseContext.Add(entity);
            var result = DatabaseContext.Set<NatuerlichePerson>().FirstOrDefault(e => e.NatuerlichePersonId == entity.NatuerlichePersonId);

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
            DatabaseContext.Set<NatuerlichePerson>().Should().NotContain(entity);
        }

        [Theory(Skip = "InMemoryDatabaseNotRunning")]
        [InlineData("Mustername")]
        public void ShouldUpdateNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new NatuerlichePerson(nachname);
            DatabaseContext.Add(entity);
            DatabaseContext.SaveChanges();
            var testEntity = DatabaseContext.Set<NatuerlichePerson>().FirstOrDefault(e => e.NatuerlichePersonId == entity.NatuerlichePersonId)!;
            testEntity.Email = "Updated Test Email";

            // Act
            var result = DatabaseContext.Update(entity);
            DatabaseContext.SaveChanges();

            // Assert
            result.Entity.Should().BeEquivalentTo(testEntity, options => options.ExcludingMissingMembers());
            result.Entity.NatuerlichePersonId.Should().Be(entity.NatuerlichePersonId);
            result.Entity.Email.Should().Be(testEntity.Email);
            DatabaseContext.Set<NatuerlichePerson>().Should().Contain(entity);
        }

        [Theory(Skip = "InMemoryDatabaseNotRunning")]
        [InlineData("Mustername")]
        public void ShouldNotUpdateNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new NatuerlichePerson(nachname);
            DatabaseContext.Add(entity);
            DatabaseContext.SaveChanges();
            var testEntity = DatabaseContext.Set<NatuerlichePerson>().FirstOrDefault(e => e.NatuerlichePersonId == entity.NatuerlichePersonId)!;
            testEntity.Email = "Updated Test Email";
            testEntity.Nachname = null!;
            DatabaseContext.NatuerlichePersonen.Update(testEntity);

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
            var result = DatabaseContext.Set<NatuerlichePerson>().FirstOrDefault(e => e.NatuerlichePersonId == entity.NatuerlichePersonId)!;

            // Assert

            result.Should().NotBeNull();
            DatabaseContext.Set<NatuerlichePerson>().Should().Contain(result);
            result.Should().NotBeEquivalentTo(testEntity, options => options.ExcludingMissingMembers());
            result.Email.Should().NotBe(testEntity.Email);
            DatabaseContext.Set<NatuerlichePerson>().Should().Contain(result);
        }
    }
}
