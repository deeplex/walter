using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Deeplex.Saverwalter.WebAPI.Controllers.Services;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class MieterControllerTests
    {
        [Fact]
        public void GetWohnungenForBesitzer()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MieterController>>();
            var controller = new MieterController(logger, ctx);

            var result = controller.GetWohnungen(vertrag.Wohnung.BesitzerId);

            result.Should().BeOfType<List<WohnungEntryBase>>();
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(vertrag.Wohnung.WohnungId);
        }

        [Fact]
        public void GetVertraegeForBesitzer()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<MieterController>>();
            var controller = new MieterController(logger, ctx);

            var result = controller.GetVertraege(vertrag.Wohnung.BesitzerId);

            result.Should().BeOfType<List<VertragEntryBase>>();
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(vertrag.VertragId);
        }
    }
}