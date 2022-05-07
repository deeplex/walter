using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class ErhaltungsaufwendungListeEntryTests
    {
        [Fact(Skip = "Fake SaverwalterContext")]
        public void ErhaltungsaufwendungListeEntryTest()
        {
            var mock = new Erhaltungsaufwendung();
            var fake = A.Fake<SaverwalterContext>();
            var stub = new ErhaltungsaufwendungListeEntry(mock, fake);
            stub.Should().NotBeNull();
        }
    }
}