using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class KontaktListControllerTests
    {
        [Fact]
        public void Get()
        {
            var ctx = TestUtils.GetContext();
            var logger = A.Fake<ILogger<KontaktListController>>();
            var controller = new KontaktListController(logger, ctx);

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}