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

import { WalterToastContent, type WalterFileHandle } from '$walter/lib';
import {
    fileURL,
    walter_file_delete,
    walter_file_get,
    walter_file_post
} from '$walter/services/files';
import type { WalterFile } from '$walter/types';

/** Daten, die beim Ziehen einer Datei mitgegeben werden. */
export const dragFileType = 'application/x-walter-file';
export const dragSourceType = 'application/x-walter-source';

/**
 * Die Ablage, aus der gerade eine Datei gezogen wird. Beim Darüberziehen sind die mitgegebenen
 * Daten nicht lesbar, nur ihre Typen; so lässt sich die eigene Ablage als Ziel ausschließen.
 */
export const dragging: { source?: string } = {};

/** Die Ablagen mit Jahresordnern sind die der Objekte; der Ablagestapel hat keine. */
export function hasYears(handle: WalterFileHandle): boolean {
    return handle.fileURL !== fileURL.stack;
}

/**
 * Verschiebt eine Datei von einer Ablage in eine andere: erst in die Zieldatei schreiben, dann
 * die Quelle löschen. Eine Datei mit Jahr behält es, wenn das Ziel Jahresordner hat.
 * Beide Ablagen werden danach neu geladen.
 * @returns ob die Datei verschoben wurde (kopiert, aber nicht gelöscht gilt als nicht verschoben)
 */
export async function move_file_between_handles(
    file: WalterFile,
    source: WalterFileHandle,
    target: WalterFileHandle,
    fetchImpl: typeof fetch
): Promise<boolean> {
    const copyToast = new WalterToastContent(
        undefined,
        'Verschieben fehlgeschlagen',
        undefined,
        () =>
            `Die Datei ${file.fileName} konnte nicht nach ${target.name} verschoben werden.`
    );
    const moveToast = new WalterToastContent(
        'Verschieben erfolgreich',
        'Verschieben fehlgeschlagen',
        () => `Die Datei ${file.fileName} liegt jetzt in ${target.name}.`,
        () =>
            `Die Datei ${file.fileName} wurde nach ${target.name} kopiert, konnte aber in ${source.name} nicht gelöscht werden.`
    );

    const blob = (await walter_file_get(file.key)) as Blob;
    const posted = await walter_file_post(
        new File([blob], file.fileName),
        target.fileURL,
        fetchImpl,
        copyToast,
        hasYears(target) && file.year ? file.year : undefined
    );
    if (posted.status !== 200) return false;

    const deleted = await walter_file_delete(file, moveToast);
    await Promise.all([source.reload(), target.reload()]);
    return deleted.status === 200;
}
