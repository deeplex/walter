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
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/wohnungversionen")]
    public class WohnungVersionController : FileControllerBase<WohnungVersionEntry, int, WohnungVersion>
    {
        private readonly ILogger<WohnungVersionController> _logger;
        protected override WohnungVersionDbService DbService { get; }

        public WohnungVersionController(ILogger<WohnungVersionController> logger, WohnungVersionDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpPost]
        public Task<ActionResult<WohnungVersionEntry>> Post([FromBody] WohnungVersionEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<WohnungVersionEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<WohnungVersionEntry>> Put(int id, WohnungVersionEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
