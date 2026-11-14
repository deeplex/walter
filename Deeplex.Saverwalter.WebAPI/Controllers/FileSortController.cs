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

using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    /// <summary>
    /// Sortiert die Dateien ohne Jahr in den Ordner ihres Upload-Jahres. Dateien ohne Jahr sind ein
    /// erlaubter Zustand; dieser Vorgang ist nur ein Hilfsmittel, wenn man sie gesammelt zuordnen
    /// will. Er ist wiederholbar und löscht eine Datei erst, wenn die neue geschrieben ist.
    /// </summary>
    [ApiController]
    [Route("api/files/sort")]
    [Authorize(Policy = "RequireAdmin")]
    public class FileSortController(HttpClient client) : ControllerBase
    {
        public class FileSortResult
        {
            public int Sorted { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult<FileSortResult>> Post()
        {
            var baseUrl = Environment.GetEnvironmentVariable("S3_PROVIDER");
            if (string.IsNullOrEmpty(baseUrl))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var sorted = await new EntityFileService(client, baseUrl).SortAsync();
            return Ok(new FileSortResult { Sorted = sorted });
        }
    }
}
