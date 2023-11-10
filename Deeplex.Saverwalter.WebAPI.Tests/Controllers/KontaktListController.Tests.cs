using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

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