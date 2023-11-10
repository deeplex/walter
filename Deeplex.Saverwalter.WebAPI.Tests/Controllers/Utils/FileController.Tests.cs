using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class FileControllerTests
    {
        public class FakeHttpClient : HttpClient
        {
            public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new StringContent("Fake Answer");
                response.Content.Headers.ContentType = null;
                return Task.FromResult(response);
            }
        }

        [Fact]
        public async void GetFiles()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<FileController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?param=value");
            context.Request.Method = HttpMethod.Get.Method;

            var httpClient = new FakeHttpClient();
            var controller = new FileController(logger, httpClient);
            controller.ControllerContext.HttpContext = context;

            var result = await controller.GetFiles();
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).FileContents.Length.Should().Be(11); // "Fake Answer"
        }

        [Fact]
        public async void GetFile()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<FileController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            //context.Request.QueryString = new QueryString("?param=value");
            context.Request.Body = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            context.Request.Method = HttpMethod.Put.Method;

            var httpClient = new FakeHttpClient();
            var controller = new FileController(logger, httpClient);
            controller.ControllerContext.HttpContext = context;

            var result = await controller.GetFile("whatever.txt");
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).FileContents.Length.Should().Be(11); // "Fake Answer"
        }

        [Fact]
        public async void PostFile()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<FileController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            //context.Request.QueryString = new QueryString("?param=value");
            context.Request.Body = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            context.Request.Method = HttpMethod.Put.Method;

            var httpClient = new FakeHttpClient();
            var controller = new FileController(logger, httpClient);
            controller.ControllerContext.HttpContext = context;

            var result = await controller.PostFile("whatever.txt");
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).FileContents.Length.Should().Be(11); // "Fake Answer"
        }

        [Fact]
        public async void DeleteFile()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<FileController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            //context.Request.QueryString = new QueryString("?param=value");
            context.Request.Body = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            context.Request.Method = HttpMethod.Put.Method;

            var httpClient = new FakeHttpClient();
            var controller = new FileController(logger, httpClient);
            controller.ControllerContext.HttpContext = context;

            var result = await controller.DeleteFile("whatever.txt");
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).FileContents.Length.Should().Be(11); // "Fake Answer"
        }

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
            Environment.SetEnvironmentVariable("S3_PROVIDER", null);
            var controller = new FileController(logger, new HttpClient());

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?param=value");
            context.Request.Method = HttpMethod.Get.Method;
            controller.ControllerContext.HttpContext = context;

            var result = await controller.GetFiles();

            result.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = result as StatusCodeResult;
            statusCodeResult!.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        }
    }
}