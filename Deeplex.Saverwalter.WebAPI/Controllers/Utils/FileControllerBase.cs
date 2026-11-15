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

using System.Runtime.InteropServices;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MigraDoc.DocumentObjectModel;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    public abstract class FileControllerBase<T, TId, U> : ControllerBase
    {
        private readonly ILogger<FileControllerBase<T, TId, U>> _logger;
        private readonly string? _baseUrl = Environment.GetEnvironmentVariable("S3_PROVIDER");
        private readonly HttpClient _httpClient;

        public FileControllerBase(ILogger<FileControllerBase<T, TId, U>> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        protected abstract WalterDbServiceBase<T, TId, U> DbService { get; }

        /// <summary>Die Familie des Objekts (siehe <see cref="FileFamilies"/>); ohne Angabe nur seine eigenen Dateien.</summary>
        protected virtual FileFamily? FileFamilyOf(U entity) => null;

        /// <summary>Das Jahr, in dem Dateien des Objekts voreingestellt liegen, z. B. das Datum eines Zählerstands; sonst <see cref="FileLayout.NoYear"/>.</summary>
        protected virtual int? FileYearOf(U entity) => null;

        public sealed class FileFamilyResult
        {
            /// <summary>Das Jahr, das ein Upload ohne Angabe erhält (0: ohne Jahr).</summary>
            public int DefaultYear { get; set; }
            public List<WalterFile> Files { get; set; } = [];
        }

        /// <summary>Dienst und Objekt (<c>/api/{entity}/{id}/files…</c>) dieser Anfrage; sonst die Fehlerantwort.</summary>
        private IActionResult? TryTarget(out EntityFileService service, out string entity, out string id)
        {
            service = null!;
            entity = id = "";
            if (string.IsNullOrEmpty(_baseUrl)) return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

            var splits = (Request.Path.Value ?? "").Split('/');
            if (splits.Length < 4) return new BadRequestResult();

            service = new EntityFileService(_httpClient, _baseUrl);
            entity = splits[2];
            id = splits[3];
            return null;
        }

        private async Task<ActionResult?> Authorize(TId id, OperationAuthorizationRequirement operation)
        {
            var entityResult = await DbService.GetEntity(User, id, operation);
            return entityResult.Result;
        }

        [HttpGet("{id}/files")]
        public async Task<ActionResult<List<WalterFile>>> GetFiles(TId id)
        {
            if (await Authorize(id, Operations.Read) is ActionResult authResult)
            {
                return authResult;
            }
            if (TryTarget(out var service, out var entity, out var objectId) != null)
            {
                return Ok(new List<WalterFile>());
            }
            return Ok(await service.ListAsync(entity, objectId, Request.Path.Value!));
        }

        /// <summary>Die Dateien der Familie des Objekts und das voreingestellte Jahr für neue Dateien.</summary>
        [HttpGet("{id}/files/family")]
        public async Task<ActionResult<FileFamilyResult>> GetFileFamily(TId id)
        {
            var loaded = await DbService.GetEntity(User, id, Operations.Read);
            if (loaded.Result is ActionResult authResult)
            {
                return authResult;
            }
            var entity = loaded.Value!;
            var defaultYear = FileYearOf(entity) ?? FileLayout.NoYear;
            if (TryTarget(out var service, out var route, out var objectId) != null)
            {
                return Ok(new FileFamilyResult { DefaultYear = defaultYear });
            }

            var family = FileFamilyOf(entity) ?? new FileFamily([]);
            return Ok(new FileFamilyResult
            {
                DefaultYear = defaultYear,
                Files = await service.ListFamilyAsync(family, route, objectId)
            });
        }

        [HttpGet("{id}/files/{filename}")]
        public async Task<IActionResult> ReadFile(TId id, string filename)
        {
            if (await Authorize(id, Operations.Read) is ActionResult authResult)
            {
                return authResult;
            }
            if (TryTarget(out var service, out var entity, out var objectId) is IActionResult error)
            {
                return error;
            }
            return await service.ReadOrDeleteAsync(Request, entity, objectId, filename);
        }

        /// <param name="year">Beim Hochladen das Jahr der Datei (0: ohne Jahr); ohne Angabe das der vorhandenen Datei, sonst das des Objekts, sonst ohne Jahr.</param>
        [HttpDelete("{id}/files/{filename}")]
        [HttpPut("{id}/files/{filename}")]
        public async Task<IActionResult> WriteFile(TId id, string filename, [FromQuery] int? year = null)
        {
            var loaded = await DbService.GetEntity(User, id, Operations.Update);
            if (loaded.Result is ActionResult authResult)
            {
                return authResult;
            }
            if (TryTarget(out var service, out var entity, out var objectId) is IActionResult error)
            {
                return error;
            }
            if (year is not FileLayout.NoYear and (< 1900 or > 2200))
            {
                return BadRequest($"Jahr {year} ist nicht möglich.");
            }

            return Request.Method == HttpMethod.Put.Method
                ? await service.UploadAsync(Request, entity, objectId, filename, year, FileYearOf(loaded.Value!))
                : await service.ReadOrDeleteAsync(Request, entity, objectId, filename);
        }

        [HttpPut("{id}/files/{old_filename}/{new_filename}")]
        public async Task<IActionResult> RenameFile(TId id, string old_filename, string new_filename)
        {
            if (await Authorize(id, Operations.Update) is ActionResult authResult)
            {
                return authResult;
            }
            if (TryTarget(out var service, out var entity, out var objectId) is IActionResult error)
            {
                return error;
            }
            return await service.RenameAsync(Request, entity, objectId, old_filename, new_filename);
        }

        /// <summary>Verschiebt die Datei in den Ordner des Jahres; Jahr 0 nimmt ihr das Jahr.</summary>
        [HttpPut("{id}/files/{filename}/year/{year:int}")]
        public async Task<IActionResult> ChangeFileYear(TId id, string filename, int year)
        {
            if (await Authorize(id, Operations.Update) is ActionResult authResult)
            {
                return authResult;
            }
            if (TryTarget(out var service, out var entity, out var objectId) is IActionResult error)
            {
                return error;
            }
            if (year is not FileLayout.NoYear and (< 1900 or > 2200))
            {
                return BadRequest($"Jahr {year} ist nicht möglich.");
            }
            return await service.ChangeYearAsync(entity, objectId, filename, year);
        }
    }
}
