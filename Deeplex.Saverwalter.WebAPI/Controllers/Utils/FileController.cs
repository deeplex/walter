using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Http.Headers;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private static string? baseUrl = Environment.GetEnvironmentVariable("S3_PROVIDER");
        private HttpClient httpClient;
        public FileController(ILogger<FileController> logger, HttpClient _httpClient)
        {
            _logger = logger;
            httpClient = _httpClient;
        }

        private string GetMimeTypeFromExtension(string extension)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(extension, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private async Task<IActionResult> RedirectToFileServer(string path)
        {
            if (baseUrl is null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var resourceUrl = baseUrl + path;
            var requestMethod = HttpContext.Request.Method;

            HttpContent content = null!;
            var request = new HttpRequestMessage(new HttpMethod(requestMethod), resourceUrl) { Content = content };

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
                    .TryGetContentType(Path.GetExtension(path), out string? contentType);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }

            var response = await httpClient.SendAsync(request, CancellationToken.None);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            var responseType = response.Content.Headers.ContentType?.MediaType ?? "plain/text";

            return File(responseContent, responseType);
        }

        [HttpGet]
        public Task<IActionResult> GetFiles() => RedirectToFileServer(Request.QueryString.ToString());
        [HttpGet]
        [Route("{**path}")]
        public Task<IActionResult> GetFile(string path) => RedirectToFileServer("/" + path + Request.QueryString);
        [HttpPut]
        [Route("{**path}")]
        public Task<IActionResult> PostFile(string path) => RedirectToFileServer("/" + path + Request.QueryString);
        [HttpDelete]
        [Route("{**path}")]
        public Task<IActionResult> DeleteFile(string path) => RedirectToFileServer("/" + path + Request.QueryString);
    }
}
