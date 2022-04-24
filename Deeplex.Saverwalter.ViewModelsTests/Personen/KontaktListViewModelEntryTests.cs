using Deeplex.Saverwalter.Model;
using FakeItEasy;
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

            var pers = new NatuerlichePerson()
            {
                Nachname = name
            };

            var stub = new KontaktListViewModelEntry(pers);

            stub.Type.Should().BeOfType<NatuerlichePerson>();
            stub.Vorname.Should().Be(pers.Vorname);
            stub.Name.Should().Be(name);

            PersonViewModelTests.PersonViewModelTest(stub, pers);
        }

        [Fact()]
        public void KontaktListViewModelEntryFromJuristischePerson()
        {
            var name = "TestName GmbH";

            var pers = new JuristischePerson()
            {
                Bezeichnung = name
            };

            var stub = new KontaktListViewModelEntry(pers);

            stub.Vorname.Should().BeNull();
            stub.Type.Should().BeOfType<JuristischePerson>();
            stub.Name.Should().Be(name);

            PersonViewModelTests.PersonViewModelTest(stub, pers);
        }
    }
}