using System.Text;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MigraDoc.DocumentObjectModel;

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

        protected abstract WalterDbServiceBase<T, U> DbService { get; }

        private static ActionResult<string> GetS3Path(string? baseUrl, string? requestPath, string? filename = null)
        {
            if (baseUrl.IsValueNullOrEmpty())
            {
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }

            if (string.IsNullOrEmpty(requestPath))
            {
                return new BadRequestResult();
            }

            var splits = requestPath.Split("/");

            if (splits.Length < 4)
            {
                return new BadRequestResult();
            }

            string resourceUrl;

            if (filename is string name)
            {
                if (splits.Length > 5 && splits[5] == "trash")
                {
                    resourceUrl = baseUrl + "/" + splits[2] + "/" + splits[3] + "/" + splits[5] + "/" + name;
                }
                else
                {
                    resourceUrl = baseUrl + "/" + splits[2] + "/" + splits[3] + "/" + name;
                }
            }
            else
            {
                resourceUrl = baseUrl + "?prefix=" + splits[2] + "/" + splits[3];
            }

            return resourceUrl;
        }



        private static async Task<IActionResult> ProcessFileRequest(string? baseUrl, HttpRequest request, HttpClient client, string? filename = null)
        {
            var s3Path = GetS3Path(baseUrl, request.Path.Value, filename);
            if (s3Path.Result is ActionResult s3PathActionResult)
            {
                return s3PathActionResult;
            }
            else if (s3Path.Value is string fullPath)
            {
                return await FileHandling.RedirectToFileServer(request, client, fullPath);
            }
            else
            {
                return new BadRequestResult();
            }
        }

        private async Task<ActionResult?> Authorize(int id, OperationAuthorizationRequirement operation)
        {
            var entityResult = await DbService.GetEntity(User, id, operation);
            return entityResult.Result;
        }

        [HttpGet("{id}/files")]
        public async Task<ActionResult<List<WalterFile>>> GetFiles(int id)
        {
            if (await Authorize(id, Operations.Read) is ActionResult authResult)
            {
                return authResult;
            }
            var result = await ProcessFileRequest(_baseUrl, Request, _httpClient);
            if (result is FileContentResult fileContentResult)
            {
                var files = FileHandling.ParseS3Stream(Encoding.UTF8.GetString(fileContentResult.FileContents), HttpContext.Request.Path.Value!);
                return Ok(files);
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpGet("{id}/files/{filename}")]
        public async Task<IActionResult> ReadFile(int id, string filename)
        {
            if (await Authorize(id, Operations.Read) is ActionResult authResult)
            {
                return authResult;
            }
            return await ProcessFileRequest(_baseUrl, Request, _httpClient, filename);
        }

        [HttpDelete("{id}/files/{filename}")]
        [HttpPut("{id}/files/{filename}")]
        public async Task<IActionResult> WriteFile(int id, string filename)
        {
            if (await Authorize(id, Operations.Update) is ActionResult authResult)
            {
                return authResult;
            }
            return await ProcessFileRequest(_baseUrl, Request, _httpClient, filename);
        }

        [HttpPut("{id}/files/{old_filename}/{new_filename}")]
        public async Task<IActionResult> RenameFile(int id, string old_filename, string new_filename)
        {
            if (await Authorize(id, Operations.Update) is ActionResult authResult)
            {
                return authResult;
            }

            var deletePath = GetS3Path(_baseUrl, Request.Path.Value, old_filename);
            if (deletePath.Result is ActionResult deletePathResult)
            {
                return deletePathResult;
            }

            Request.Path = Request.Path.Value!.Replace(old_filename, new_filename);
            var response = await ProcessFileRequest(_baseUrl, Request, _httpClient, new_filename);

            if (response is FileContentResult)
            {
                // Put trash before the filename
                var deleteRequest = new HttpRequestMessage(new HttpMethod(HttpMethod.Delete.Method), deletePath.Value) { Content = null };
                await _httpClient.SendAsync(deleteRequest, CancellationToken.None);
            }

            return response;
        }
    }
}
