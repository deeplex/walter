using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class ErhaltungsaufwendungWohnungTests
    {
        [Fact(Skip = "Fake SaverwalterContext")]
        public void ErhaltungsaufwendungWohnungTest()
        {
            var fake = A.Fake<SaverwalterContext>();
            var stub = new ErhaltungsaufwendungWohnung(fake, 0, 0);
            stub.Should().NotBeNull();
        }
    }
}