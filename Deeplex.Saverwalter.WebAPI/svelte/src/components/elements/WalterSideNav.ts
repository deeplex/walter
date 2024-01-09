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

import { WalterToastContent } from '$walter/lib';
import { walter_sign_out } from '$walter/services/auth';
import { fileURL, walter_get_files } from '$walter/services/files';
import { walter_goto } from '$walter/services/utils';
import { Checkmark, DocumentAttachment } from 'carbon-icons-svelte';

export async function checkStackTodo(fetchImpl: typeof fetch) {
    const files = await walter_get_files(fileURL.stack, fetchImpl);
    const todo = files.length > 0;

    if (todo) {
        return DocumentAttachment;
    } else {
        return Checkmark;
    }
}

export function logout() {
    const LogoutToast = new WalterToastContent(
        'Abmeldung erfolgreich',
        'Abmeldung fehlgeschlagen'
    );
    walter_sign_out(LogoutToast);
    walter_goto('/login');
}
