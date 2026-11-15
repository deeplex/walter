// Copyright (c) 2023-2025 Kai Lawrence
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

import type { WalterFileHandle } from '$walter/lib';
import { walter_file_post } from '$walter/services/files';
import { openModal } from '$walter/store';
import type { WalterFile } from '$walter/lib/WalterFile';

// Der Server legt das Jahr fest (auch beim Überschreiben); daher neu laden.
async function post_file(
    handle: WalterFileHandle,
    file: File,
    fetchImpl: typeof fetch,
    year?: number
): Promise<WalterFile[]> {
    return walter_file_post(
        file,
        handle.fileURL,
        fetchImpl,
        undefined,
        year
    ).then(() => handle.reload());
}

/** `year`: das Jahr, in das die neuen Dateien kommen; ohne Angabe wählt der Server es. */
export async function upload_new_files(
    handle: WalterFileHandle,
    newFiles: File[],
    fetchImpl: typeof fetch,
    year?: number
) {
    let files = await handle.files;

    for (const file of newFiles) {
        {
            // Nur die Dateien des eigenen Objekts: gleichnamige der Familie bleiben unberührt.
            if (
                files
                    .filter((e) => !e.source)
                    .map((e) => e.fileName)
                    .includes(file.name)
            ) {
                const content = `Eine Datei mit dem Namen ${file.name} existiert bereits in dieser Ablage. Bist du sicher, dass diese Datei hochgeladen werden soll?`;
                openModal({
                    modalHeading: `Datei existiert bereits`,
                    content,
                    primaryButtonText: 'Überschreiben',
                    submit: () => post_file(handle, file, fetchImpl, year)
                });
            } else {
                files = await post_file(handle, file, fetchImpl, year);
            }
        }
    }

    return files;
}
