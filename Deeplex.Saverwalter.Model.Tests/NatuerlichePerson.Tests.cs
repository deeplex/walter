// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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

        [Theory]
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
            result.Entity.Bezeichnung.Should().BeEquivalentTo("Mustername");
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

        [Theory]
        [InlineData("Mustername")]
        public void ShouldNotAddNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new Kontakt(nachname, Rechtsform.natuerlich);
            entity.Name = null!;

            // Act
            DatabaseContext.Kontakte.Add(entity);
            var result = DatabaseContext.Kontakte.FirstOrDefault(e => e.KontaktId == entity.KontaktId);

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

        [Theory]
        [InlineData("Mustername")]
        public void ShouldUpdateNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new Kontakt(nachname, Rechtsform.natuerlich);
            DatabaseContext.Add(entity);
            DatabaseContext.SaveChanges();
            var testEntity = DatabaseContext.Kontakte.FirstOrDefault(e => e.KontaktId == entity.KontaktId)!;
            testEntity.Email = "Updated Test Email";

            // Act
            var result = DatabaseContext.Kontakte.Update(entity);
            DatabaseContext.SaveChanges();

            // Assert
            result.Entity.Should().BeEquivalentTo(testEntity, options => options.ExcludingMissingMembers());
            result.Entity.KontaktId.Should().Be(entity.KontaktId);
            result.Entity.Email.Should().Be(testEntity.Email);
            DatabaseContext.Set<Kontakt>().Should().Contain(entity);
        }

        [Theory(Skip = "Find out why this does not work.")]
        [InlineData("Mustername")]
        public void ShouldNotUpdateNatuerlichePerson(string nachname)
        {
            // Arrange
            var entity = new Kontakt(nachname, Rechtsform.natuerlich);
            DatabaseContext.Kontakte.Add(entity);
            DatabaseContext.SaveChanges();
            var testEntity = DatabaseContext.Kontakte.FirstOrDefault(e => e.KontaktId == entity.KontaktId)!;
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
