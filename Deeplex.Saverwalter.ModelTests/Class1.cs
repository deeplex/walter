using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace Deeplex.Saverwalter.ModelTests
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
                new object[] { new NatuerlichePerson { Nachname = "Test Nachname" } },
                new object[] { new NatuerlichePerson { Vorname = "Test Vorname", Nachname = "Test Nachname"} },
                new object[] { new NatuerlichePerson
                {
                    Vorname = "Test Vorname",
                    Nachname = "Test Nachname",
                    Telefon = "Test Telefon",
                    Email = "Test Email@testmail.test",
                    Fax = "Test Fax",
                    Mobil = "Test Mobil",
                    Notiz = "Test Notiz",
                } },
                new object[] { new NatuerlichePerson
                {
                    Nachname = "Test Nachname",
                    Anrede = Anrede.Frau,
                } },
                new object[] { new NatuerlichePerson
                {
                    Nachname = "Test Nachname",
                    Anrede = Anrede.Herr,
                } },
                new object[] { new NatuerlichePerson
                {
                    Nachname = "Test Nachname",
                    Anrede = Anrede.Keine,
                } },
                new object[] { new NatuerlichePerson
                {
                    Nachname = "Test Nachname",
                    JuristischePersonen = new List<JuristischePerson>()
                    {
                        new JuristischePerson
                        {
                            Bezeichnung = "Test JuristischePerson"
                        }
                    }
                } }
            };

        [Theory]
        [MemberData(nameof(ShouldWorkData))]
        public void ShouldAddNatuerlichePerson(NatuerlichePerson entity)
        {
            // Arrange

            // Act
            var result = DatabaseContext.Add(entity);
            DatabaseContext.SaveChanges();

            // Assert
            result.Should().BeEquivalentTo(entity, options => options.ExcludingMissingMembers());
            result.Entity.NatuerlichePersonId.Should().BeGreaterThan(0);
            result.Entity.PersonId.Should().NotBeEmpty();
            result.Entity.Bezeichnung.Should().BeEquivalentTo(string.Join(" ", entity.Vorname ?? "", entity.Nachname));
            DatabaseContext.Set<NatuerlichePerson>().Should().Contain(entity);
        }

        public static IEnumerable<object[]> ShouldNotWorkData =>
            new List<object[]>
            {
                new object[] { new NatuerlichePerson { } },
                new object[] { new NatuerlichePerson { Vorname = "Test Vorname" } },
                new object[] { new NatuerlichePerson
                {
                    Vorname = "Test Vorname",
                    Telefon = "Test Telefon",
                    Email = "Test Email@testmail.test",
                    Fax = "Test Fax",
                    Mobil = "Test Mobil",
                    Notiz = "Test Notiz",
                } },
                new object[] { new NatuerlichePerson
                {
                    Anrede = Anrede.Frau,
                } },
                new object[] { new NatuerlichePerson
                {
                    JuristischePersonen = new List<JuristischePerson>()
                    {
                        new JuristischePerson
                        {
                            Bezeichnung = "Test JuristischePerson"
                        }
                    }
                } }
            };

        [Theory]
        [MemberData(nameof(ShouldNotWorkData))]
        public void ShouldNotAddNatuerlichePerson(NatuerlichePerson entity)
        {
            // Arrange

            //Act
            var result = DatabaseContext.Add(entity);

            Action saveAction = () => DatabaseContext.SaveChanges();
            saveAction.Should().Throw<DbUpdateException>();
            entity.NatuerlichePersonId.Should().Be(0);

            DatabaseContext.Set<NatuerlichePerson>().Should().NotContain(entity);
        }

    }
}
