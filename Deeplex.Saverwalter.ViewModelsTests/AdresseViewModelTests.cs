using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.ViewModels.Tests
{
    public class AdresseViewModelTests
    {

        // Generic class
        [Fact(Skip = "Mock database")]
        public void AdresseViewModelTest()
        {
            var fake1 = A.Fake<IAdresse>();
            var fake2 = A.Fake<IWalterDbService>();
            var stub = new AdresseViewModel<IAdresse>(fake1, fake2);
            stub.Should().NotBeNull();
        }
        // Non-Generic Class
        [Fact(Skip = "TODO")]
        public void updateAdressenTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "Mock database")]
        public void AdresseViewModelTest1()
        {
            var mock = new Adresse();
            var fake = A.Fake<IWalterDbService>();
            var stub = new AdresseViewModel(mock, fake);
            stub.Should().NotBeNull();
        }

        [Fact(Skip = "TODO => refactor?")]
        public void AnschriftTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO => refactor?")]
        public void AnschriftTest1()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO => refactor?")]
        public void AnschriftTest2()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "TODO => refactor?")]
        public void AnschriftTest3()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}