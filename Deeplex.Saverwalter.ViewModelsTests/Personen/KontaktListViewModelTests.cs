using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using FakeItEasy;
using FakeItEasy.Sdk;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace Deeplex.Saverwalter.ViewModels.Tests
{
    public class KontaktListViewModelTests
    {
        private static DbSet<T> CreateDbFake<T>(int count) where T: class
        {
            return (DbSet<T>)A.CollectionOfFake<T>(count);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 2)]
        [InlineData(2, 0)]
        [InlineData(2, 2)]
        public void KontaktListViewModelTest(int countOfNatuerlichePersonen, int countOfJuristischePersonen)
        {
            // TODO can't fake sealed classes.
            var avm = A.Fake<IWalterDbService>();

            avm.ctx.NatuerlichePersonen = CreateDbFake<NatuerlichePerson>(countOfNatuerlichePersonen);
            avm.ctx.JuristischePersonen = CreateDbFake<JuristischePerson>(countOfJuristischePersonen);

            var stub = new KontaktListViewModel(avm);

            // Check initial values
            stub.SelectedKontakt.Should().BeNull();
            stub.hasSelectedKontakt.Should().BeFalse();
            stub.Filter.Value.Should().Be("");
            stub.Vermieter.Value.Should().BeTrue();
            stub.Mieter.Value.Should().BeFalse();
            stub.Handwerker.Value.Should().BeFalse();
            stub.AllRelevant.Should().NotBeNull().And
                .HaveCount(countOfNatuerlichePersonen + countOfJuristischePersonen);
            stub.Kontakte.Value.Should().NotBeNull().And
                .HaveCount(stub.AllRelevant.Count);
        }
    }
}