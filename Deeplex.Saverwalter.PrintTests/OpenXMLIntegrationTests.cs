using Microsoft.VisualStudio.TestTools.UnitTesting;
using Deeplex.Saverwalter.Print;
using Xunit;
using FluentAssertions;
using DocumentFormat.OpenXml.Wordprocessing;
using FakeItEasy;
using Deeplex.Saverwalter.Model;

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
                m.Left.Should().Be(1418);
                m.Right.Should().Be(567);
                m.Top.Should().Be(958);
                m.Bottom.Should().Be(958);
            }
            pageSize.ChildElements[1].Should().NotBeNull();
            if (pageSize is PageSize s)
            {
                s.Code.Should().Be(9);
                s.Width.Should().Be(11906);
                s.Height.Should().Be(16838);
            }
        }

        [Fact()]
        public void SaveAsDocxTest()
        {
            var fake = A.Fake<IErhaltungsaufwendungWohnung>();
            var mockPath = "test";

            fake.SaveAsDocx(mockPath);
            //Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void SaveAsDocxTest1()
        {
            //Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void SaveAsDocxTest2()
        {
            //Assert.True(false, "This test needs an implementation");
        }
    }
}