using Xunit;
using FakeItEasy;
using FluentAssertions;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.ViewModels.Tests
{
    public class PersonViewModelTests
    {
        // Called from KontaktListViewModelEntryTests
        public static void PersonViewModelTest(KontaktListViewModelEntry stub, IPerson fake)
        {
            stub.ToString().Should().Be(fake.Bezeichnung);
            stub.Entity.PersonId.Should().Be(fake.PersonId);
            stub.Anschrift.Should().Be(AdresseViewModel.Anschrift(fake));
            stub.Email.Should().Be(fake.Email);
            stub.Telefon.Should().Be(fake.Telefon);
            stub.Mobil.Should().Be(fake.Mobil);
        }
    }
}