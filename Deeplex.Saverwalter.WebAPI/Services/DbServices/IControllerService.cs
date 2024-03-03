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
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public interface ICRUDServiceGuid<T>
    {
        public Task<ActionResult<T>> Get(ClaimsPrincipal user, Guid id);
        public Task<ActionResult<T>> Put(ClaimsPrincipal user, Guid id, T entry);
        public Task<ActionResult<T>> Post(ClaimsPrincipal user, T entry);
        public Task<ActionResult> Delete(ClaimsPrincipal user, Guid id);
    }
}
