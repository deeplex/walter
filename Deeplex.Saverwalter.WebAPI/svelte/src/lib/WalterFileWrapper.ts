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

import { fileURL } from '$walter/services/files';
import { WalterFileHandle } from './WalterFileHandle';

export class WalterFileWrapper {
    handles: WalterFileHandle[] = [];

    constructor(public fetchImpl: typeof fetch) {}

    // Always register for the next to last position; the last is always the stack.
    register(name: string, fileURL: string) {
        const handle = new WalterFileHandle(name, fileURL, this.fetchImpl);
        this.handles.splice(this.handles.length - 1, 0, handle);
    }

    registerStack() {
        this.register('Ablagestapel', fileURL.stack);
    }
}
