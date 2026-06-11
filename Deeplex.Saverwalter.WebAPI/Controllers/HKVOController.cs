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
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/hkvo")]
    public class HKVOController : ControllerBase
    {
        private readonly HKVODbService _dbService;

        public HKVOController(HKVODbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}")]
        public Task<ActionResult<HKVOEntryBase>> Get(int id) => _dbService.Get(User!, id);

        [HttpPost]
        public Task<ActionResult<HKVOEntryBase>> Post(HKVOEntryBase entry) => _dbService.Post(User!, entry);

        [HttpPut("{id}")]
        public Task<ActionResult<HKVOEntryBase>> Put(int id, HKVOEntryBase entry) => _dbService.Put(User!, id, entry);

        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => _dbService.Delete(User!, id);
    }
}
