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
import { walter_goto } from '$walter/services/utils';
import { addToast, changeTracker } from '$walter/store';

export async function handle_save(apiURL: string, entry: unknown) {
    const SaveToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        (a: unknown) => a as string,
        (a: unknown) =>
            `Speichern fehlgeschlagen.
            Folgende Einträge sind erforderlich:
            ${Object.keys((a as { errors: string }).errors)
                .map((e) => e.split('.').pop())
                .join(', \n')}`
    );

    const response = await walter_post(apiURL, entry);
    const parsed = await response.json();
    addToast(SaveToast, response.status === 200, parsed);

    if (parsed.id) {
        changeTracker.set(0);
        walter_goto(`${apiURL}/${parsed.id}`.replace('api/', ''));
    }
}
