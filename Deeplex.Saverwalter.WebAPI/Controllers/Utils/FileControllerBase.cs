using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

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

        private async Task<ActionResult?> Authorize(int id, OperationAuthorizationRequirement operation)
        {
            var entityResult = await DbService.GetEntity(User, id, operation);
            return entityResult.Result;
        }

        [HttpGet("{id}/files")]
        public async Task<IActionResult> GetFiles(int id)
        {
            if (await Authorize(id, Operations.Read) is ActionResult authResult)
            {
                return authResult;
            }
            return await FileHandling.ProcessFileRequest(_baseUrl, Request, _httpClient);
        }
        [HttpGet("{id}/files/{filename}")]
        public async Task<IActionResult> ReadFile(int id, string filename)
        {
            if (await Authorize(id, Operations.Read) is ActionResult authResult)
            {
                return authResult;
            }
            return await FileHandling.ProcessFileRequest(_baseUrl, Request, _httpClient, filename);
        }

        [HttpDelete("{id}/files/{filename}")]
        [HttpPut("{id}/files/{filename}")]
        public async Task<IActionResult> WriteFile(int id, string filename)
        {
            if (await Authorize(id, Operations.Read) is ActionResult authResult)
            {
                return authResult;
            }
            return await FileHandling.ProcessFileRequest(_baseUrl, Request, _httpClient, filename);
        }
    }
}
