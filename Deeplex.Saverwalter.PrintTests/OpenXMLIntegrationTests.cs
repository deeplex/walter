using Deeplex.Saverwalter.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Print.Tests
{
    public class OpenXMLIntegrationTests
    {
        [Fact()]
        public void DinA4Test()
        {
            var stub = OpenXMLIntegration.DinA4();

            var sectionProperties = stub.ChildElements[0];
            sectionProperties.Should().NotBeNull();
            var pageMargin = sectionProperties.ChildElements[0];
            var pageSize = sectionProperties.ChildElements[1];
            pageMargin.Should().NotBeNull();
            if (pageMargin is PageMargin m)
            {
                m.Left.Value.Should().Be(1418);
                m.Right.Value.Should().Be(567);
                m.Top.Value.Should().Be(958);
                m.Bottom.Value.Should().Be(958);
            }
            pageSize.Should().NotBeNull();
            if (pageSize is PageSize s)
            {
                s.Code.Value.Should().Be(9);
                s.Width.Value.Should().Be(11906);
                s.Height.Value.Should().Be(16838);
            }
        }

        // https://stackoverflow.com/questions/9064566/is-it-possible-to-mock-fake-an-extension-method
        [Fact(Skip = "Can't fake extension methods see comment.")]
        public void SaveAsDocxTest()
        {
            var fake = A.Fake<IErhaltungsaufwendungWohnung>();
            var mockPath = "test";

            fake.SaveAsDocx(mockPath);
            A.CallTo(() => fake.SaveAsDocx(mockPath)).MustHaveHappened();
        }

        [Fact(Skip = "Can't fake extension methods see comment.")]
        public void SaveAsDocxTest1()
        {
            //Assert.True(false, "This test needs an implementation");
        }

        [Fact(Skip = "Can't fake extension methods see comment.")]
        public void SaveAsDocxTest2()
        {
            //Assert.True(false, "This test needs an implementation");
        }
    }
}