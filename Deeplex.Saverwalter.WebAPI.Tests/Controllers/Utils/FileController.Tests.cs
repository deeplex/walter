// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Security.Claims;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
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
            var logger = A.Fake<ILogger<AdresseController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?param=value");
            context.Request.Method = HttpMethod.Get.Method;

            var httpClient = new FakeHttpClient();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, httpClient);
            controller.ControllerContext.HttpContext = context;
            controller.ControllerContext.HttpContext.Request.Path = "/api/adressen/1/files";

            var result = await controller.GetFiles(1);
            result.Should().BeOfType<FileContentResult>();
            result.As<FileContentResult>().FileContents.Length.Should().Be(11); // "Fake Answer"
        }

        [Fact]
        public async void GetFile()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            //context.Request.QueryString = new QueryString("?param=value");
            context.Request.Body = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            context.Request.Method = HttpMethod.Put.Method;

            var httpClient = new FakeHttpClient();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, httpClient);
            controller.ControllerContext.HttpContext = context;
            controller.ControllerContext.HttpContext.Request.Path = "/api/adressen/1/files";

            var result = await controller.ReadFile(1, "whatever.txt");
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).FileContents.Length.Should().Be(11); // "Fake Answer"
        }

        [Fact]
        public async void PostFile()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            //context.Request.QueryString = new QueryString("?param=value");
            context.Request.Body = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            context.Request.Method = HttpMethod.Put.Method;

            var httpClient = new FakeHttpClient();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, httpClient);
            controller.ControllerContext.HttpContext = context;
            controller.ControllerContext.HttpContext.Request.Path = "/api/adressen/1/files";
            controller.ControllerContext.HttpContext.Request.Method = HttpMethod.Put.Method;

            var result = await controller.WriteFile(1, "whatever.txt");
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).FileContents.Length.Should().Be(11); // "Fake Answer"
        }

        [Fact]
        public async void DeleteFile()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            //context.Request.QueryString = new QueryString("?param=value");
            context.Request.Body = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            context.Request.Method = HttpMethod.Put.Method;

            var httpClient = new FakeHttpClient();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, httpClient);
            controller.ControllerContext.HttpContext = context;
            controller.ControllerContext.HttpContext.Request.Path = "/api/adressen/1/files";
            controller.ControllerContext.HttpContext.Request.Method = HttpMethod.Delete.Method;

            var result = await controller.WriteFile(1, "whatever.txt");
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).FileContents.Length.Should().Be(11); // "Fake Answer"
        }

        [Fact]
        public async void GetFilesNoServer()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();

            var httpClient = new FakeHttpClient();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, httpClient);

            Environment.SetEnvironmentVariable("S3_PROVIDER", "http://this_is_a_valid_url:1000");

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?param=value");
            context.Request.Method = HttpMethod.Get.Method;
            controller.ControllerContext.HttpContext = context;

            var result = await controller.GetFiles(1);

            result.As<StatusCodeResult>().StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        }

        [Fact]
        public async void GetFilesNoS3Provider()
        {
            var ctx = TestUtils.GetContext();
            TestUtils.GetVertragForAbrechnung(ctx);
            var logger = A.Fake<ILogger<AdresseController>>();
            Environment.SetEnvironmentVariable("S3_PROVIDER", null);

            var httpClient = new FakeHttpClient();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var dbService = new AdresseDbService(ctx, auth);
            var controller = new AdresseController(logger, dbService, httpClient);

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?param=value");
            context.Request.Method = HttpMethod.Get.Method;
            controller.ControllerContext.HttpContext = context;

            var result = await controller.GetFiles(1);

            result.Should().BeOfType<StatusCodeResult>();
            result.As<StatusCodeResult>().StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
