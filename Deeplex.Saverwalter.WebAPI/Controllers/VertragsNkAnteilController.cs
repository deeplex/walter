// Copyright (c) 2023-2026 Kai Lawrence
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

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vertrags-nk-anteile")]
    public class VertragsNkAnteilController(VertragsNkAnteilDbService dbService) : ControllerBase
    {
        public class VertragsNkAnteilEntry
        {
            public Guid Id { get; set; }
            public decimal Betrag { get; set; }
            public DateOnly Datum { get; set; }
            public int BetreffendesJahr { get; set; }
            public string? Notiz { get; set; }
            public SelectionEntry? Vertrag { get; set; }
            public SelectionEntry? Umlage { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();
        }

        [HttpGet]
        public Task<List<VertragsNkAnteilEntry>> Get(
            [FromQuery] int? vertragId,
            [FromQuery] int? umlageId)
            => dbService.GetList(User!, vertragId, umlageId);

        [HttpPost]
        public Task<ActionResult<VertragsNkAnteilEntry>> Post([FromBody] VertragsNkAnteilEntry entry)
            => dbService.Post(User!, entry);

        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(Guid id)
            => dbService.Delete(User!, id);
    }
}
