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
import { walter_post } from '$walter/services/requests';
import { addToast } from '$walter/store';

export async function handle_save(
    url: string,
    body: unknown,
    toastTitle: string
) {
    const PostToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        () => `${toastTitle} erfolgreich gespeichert.`,
        (a: unknown) => `Konnte ${a} nicht speichern.`
    );

    const response = await walter_post(url, body);
    const parsed = await response.json();
    addToast(PostToast, response.status === 200, parsed);

    return parsed;
}
