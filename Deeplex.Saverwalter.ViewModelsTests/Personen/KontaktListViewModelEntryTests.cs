using Deeplex.Saverwalter.Model;
using FluentAssertions;
using System;
using Xunit;

namespace Deeplex.Saverwalter.ViewModels.Tests
{
    public class KontaktListViewModelEntryTests
    {
        [Fact()]
        public void ToStringTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void KontaktListViewModelEntryTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void KontaktListViewModelEntryFromNewNatuerlichePerson()
        {
            var name = "TestName";

            var stub = new KontaktListViewModelEntry(new NatuerlichePerson()
            {
                Nachname = name // Nachname is the only required field.
            });

            stub.ToString().Should().Be(stub.Entity.Bezeichnung);
            stub.Type.Should().BeOfType<NatuerlichePerson>();
            stub.Entity.PersonId.Should().Be(Guid.Empty);
            stub.Vorname.Should().Be("");
            stub.Name.Should().Be(null); // TODO has to have a value.
            stub.Anschrift.Should().Be("");
            stub.Email.Should().Be("");
            stub.Telefon.Should().Be("");
            stub.Mobil.Should().Be("");
        }

        [Fact()]
        public void KontaktListViewModelEntryFromJuristischePerson()
        {
            var name = "Test GmbH";

            var stub = new KontaktListViewModelEntry(new JuristischePerson()
            {
                Bezeichnung = name // Bezeichnung ist die einzig notwendige Angabe.
            });

            stub.ToString().Should().Be(stub.Entity.Bezeichnung);
            stub.Type.Should().BeOfType<NatuerlichePerson>();
            stub.Entity.PersonId.Should().Be(Guid.Empty);
            stub.Vorname.Should().Be("");
            stub.Name.Should().Be(name); // TODO has to have a value.
            stub.Anschrift.Should().Be("");
            stub.Email.Should().Be("");
            stub.Telefon.Should().Be("");
            stub.Mobil.Should().Be("");
        }
    }
}