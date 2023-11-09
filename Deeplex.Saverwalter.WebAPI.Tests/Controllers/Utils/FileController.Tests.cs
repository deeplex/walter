using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Microsoft.AspNetCore.Http;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class FileControllerTests
    {

        [Fact]
        public async void GetFilesNoServer()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<FileController>>();
            var controller = new FileController(logger, new HttpClient());
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?param=value");
            context.Request.Method = HttpMethod.Get.Method;
            controller.ControllerContext.HttpContext = context;

            try
            {
                var result = await controller.GetFiles();
            }
            catch (HttpRequestException e)
            {
                e.Should().BeOfType<HttpRequestException>();
                e.Message.Should().Be("No such host is known. (this_is_a_valid_url:1000)");

                return;
            }

            // Should not be called.
            Assert.True(false);
        }

        [Fact]
        public async void GetFilesNoS3Provider()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<FileController>>();
            var controller = new FileController(logger, new HttpClient());

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?param=value");
            controller.ControllerContext.HttpContext = context;

            var result = await controller.GetFiles();

            result.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = result as StatusCodeResult;
            statusCodeResult!.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        }
    }
}