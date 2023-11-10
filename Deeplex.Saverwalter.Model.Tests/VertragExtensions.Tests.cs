using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class VertragExtensionsTests
    {
        [Fact]
        public void BeginnAndEndTest1()
        {
            var wohnung = A.Fake<Wohnung>();
            var vertrag = new Vertrag()
            {
                Wohnung = wohnung,
            };

            var date1 = new DateOnly(2000, 1, 1);
            var date2 = new DateOnly(2001, 1, 1);
            var version1 = new VertragVersion(date1, 100, 1)
            {
                Vertrag = vertrag
            };
            var version2 = new VertragVersion(date2, 100, 1)
            {
                Vertrag = vertrag
            };

            vertrag.Versionen.Add(version1);
            vertrag.Versionen.Add(version2);


            vertrag.Beginn().Should().Be(date1);
            version1.Ende().Should().Be(date2.AddDays(-1));
            version2.Ende().Should().BeNull();
        }

        [Fact]
        public void BeginnAndEndTest2()
        {
            var wohnung = A.Fake<Wohnung>();

            var date0 = new DateOnly(2002, 5, 31);

            var vertrag = new Vertrag()
            {
                Wohnung = wohnung,
                Ende = date0
            };

            var date1 = new DateOnly(2000, 1, 1);
            var date2 = new DateOnly(2001, 1, 1);
            var version1 = new VertragVersion(date1, 100, 1)
            {
                Vertrag = vertrag
            };
            var version2 = new VertragVersion(date2, 100, 1)
            {
                Vertrag = vertrag
            };

            vertrag.Versionen.Add(version1);
            vertrag.Versionen.Add(version2);

            vertrag.Beginn().Should().Be(date1);
            version1.Ende().Should().Be(date2.AddDays(-1));
            version2.Ende().Should().Be(date0);
        }
    }
}
