using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class NoteTests
    {
        [Fact]
        public void NoteTest()
        {
            var testMessage = "This is a test";
            var severity = Severity.Error;
            var stub = new Note(testMessage, severity);
            stub.Should().NotBeNull();
        }
    }
}