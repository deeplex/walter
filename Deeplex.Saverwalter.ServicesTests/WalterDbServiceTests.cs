using Deeplex.Saverwalter.Model;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Services.Tests
{
    public class WalterDbServiceTests
    {
        [Fact()]
        public void WalterDbServiceTest()
        {
            var fake = A.Fake<INotificationService>();
            var stub = new WalterDbService(fake);

            stub.Should().NotBeNull();
            stub.root.Should().BeNull();
            stub.ctx.Should().BeNull();

            stub.root = "test";

            stub.root.Should().Be("test");
        }

        [Fact(Skip = "TODO How to fake saving to db?")]
        public void SaveWalterTest()
        {
            var fake = A.Fake<INotificationService>();
            var stub = new WalterDbService(fake);
            stub.ctx = A.Fake<SaverwalterContext>();

            stub.SaveWalter();
        }
    }
}