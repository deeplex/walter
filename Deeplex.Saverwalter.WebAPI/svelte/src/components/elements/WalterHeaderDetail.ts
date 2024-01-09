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
import { walter_delete, walter_put } from '$walter/services/requests';
import { convertDateCanadian } from '$walter/services/utils';
import { changeTracker, openModal } from '$walter/store';

export async function handle_save(
    apiURL: string,
    entry: { lastModified: Date } & unknown,
    toastTitle: string
) {
    const PutToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        () => `${toastTitle} erfolgreich gespeichert.`,
        (a: unknown) =>
            `Folgende Einträge sind erforderlich:
            ${Object.keys((a as { errors: string }).errors)
                .map((e) => e.split('.').pop())
                .join(', \n')}`
    );

    const result = await walter_put(apiURL, entry, PutToast);
    if (result.ok) {
        entry.lastModified = new Date();
        changeTracker.set(-1);
        changeTracker.set(0);
    }
}

export function handle_delete(title: string, apiURL: string) {
    const content = `Bist du sicher, dass du ${title} löschen möchtest?
    Dieser Vorgang kann nicht rückgängig gemacht werden.`;

    const DeleteToast = new WalterToastContent(
        'Löschen erfolgreich',
        'Löschen fehlgeschlagen',
        () => `${title} erfolgreich gelöscht.`,
        () => ''
    );

    openModal({
        modalHeading: 'Löschen',
        content,
        danger: true,
        primaryButtonText: 'Löschen',
        submit: async () => {
            const result = await walter_delete(apiURL, DeleteToast);
            if (result.ok) {
                changeTracker.set(-1);
                changeTracker.set(0);
                history.back();
            }
        }
    });
}
