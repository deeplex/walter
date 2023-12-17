using System.Net.Http.Headers;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    public abstract class FileControllerBase<T, U> : ControllerBase
    {
        private readonly ILogger<FileControllerBase<T, U>> _logger;
        private readonly string? _baseUrl = Environment.GetEnvironmentVariable("S3_PROVIDER");
        private readonly HttpClient _httpClient;

        public FileControllerBase(ILogger<FileControllerBase<T, U>> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        private async Task<IActionResult> RedirectToFileServer(string path)
        {
            var requestMethod = HttpContext.Request.Method;

            HttpContent content = null!;
            var request = new HttpRequestMessage(new HttpMethod(requestMethod), path) { Content = content };

            if (requestMethod == HttpMethod.Put.Method)
            {
                content = new StreamContent(Request.Body);

                var contentLength = Request.ContentLength ?? -1;
                if (contentLength >= 0)
                {
                    content.Headers.ContentLength = contentLength;
                }

                request.Content = content;
                new FileExtensionContentTypeProvider()
                    .TryGetContentType(Path.GetExtension(path), out var contentType);
                if (contentType != null)
                {
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }

            var response = await _httpClient.SendAsync(request, CancellationToken.None);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            var responseType = response.Content.Headers.ContentType?.MediaType ?? "plain/text";

            return File(responseContent, responseType);
        }

        protected abstract WalterDbServiceBase<T, U> DbService { get; }

        private ActionResult<string> GetS3Path(string? filename = null)
        {
            if (_baseUrl is null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var path = ControllerContext.HttpContext.Request.Path.Value;

            if (string.IsNullOrEmpty(path))
            {
                return new BadRequestResult();
            }

            var splits = path.Split("/");

            if (splits.Length < 4)
            {
                return new BadRequestResult();
            }

            string resourceUrl;

            if (filename is string name)
            {
                if (splits[5] == "trash")
                {
                    resourceUrl = _baseUrl + "/" + splits[2] + "/" + splits[3] + "/" + splits[5] + "/" + name;
                }
                else
                {
                    resourceUrl = _baseUrl + "/" + splits[2] + "/" + splits[3] + "/" + name;
                }
            }
            else
            {
                resourceUrl = _baseUrl + "?prefix=" + splits[2] + "/" + splits[3];
            }

            return resourceUrl;
        }

        private async Task<IActionResult> ProcessFileRequest(int id, OperationAuthorizationRequirement operation, string? filename = null)
        {
            var entityResult = await DbService.GetEntity(User, id, operation);
            if (entityResult is ActionResult entityActionResult)
            {
                return entityActionResult;
            }

            var s3Path = GetS3Path(filename);
            if (s3Path.Result is ActionResult s3PathActionResult)
            {
                return s3PathActionResult;
            }
            else if (s3Path.Value is string fullPath)
            {
                return await RedirectToFileServer(fullPath);
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpGet("{id}/files")]
        public async Task<IActionResult> GetFiles(int id) => await ProcessFileRequest(id, Operations.Read);

        [HttpGet("{id}/files/{filename}")]
        public async Task<IActionResult> ReadFile(int id, string filename) => await ProcessFileRequest(id, Operations.Read, filename);

        [HttpDelete("{id}/files/{filename}")]
        [HttpPut("{id}/files/{filename}")]
        [HttpPut("{id}/files/trash/{filename}")]
        public async Task<IActionResult> WriteFile(int id, string filename) => await ProcessFileRequest(id, Operations.Update, filename);
    }
}
