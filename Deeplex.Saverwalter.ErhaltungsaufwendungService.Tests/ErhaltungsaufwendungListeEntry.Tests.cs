using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Xunit;
using FluentAssertions;

namespace Deeplex.Saverwalter.ErhaltungsaufwendungService.Tests
{
    public class ErhaltungsaufwendungListeEntryTests
    {
        [Fact]
        public void ErhaltungsaufwendungListeEntry()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var entity = new Erhaltungsaufwendung(1000, "Test", vertrag.Wohnung.BesitzerId, new DateOnly(2021, 1, 1))
            {
                Wohnung = vertrag.Wohnung
            };

            var entry = new ErhaltungsaufwendungListeEntry(entity, ctx);

            entry.Wohnung.Should().Be(vertrag.Wohnung);
            entry.Betrag.Should().Be(1000);
            entry.Datum.Should().Be(new DateOnly(2021, 1, 1));
            entry.Bezeichnung.Should().Be("Test");
        }
    }
}