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

import { walter_get_family, walter_get_files } from '$walter/services/files';
import type { WalterFile } from '$walter/types';

export class WalterFileHandle {
    files: Promise<WalterFile[]>;
    /** Das Jahr, das ein Upload in diese Ablage voreingestellt erhält (Familie), sonst unbekannt. */
    defaultYear?: number;

    /** `family`: die Dateien der ganzen Familie des Objekts laden, nicht nur seine eigenen. */
    constructor(
        public name: string,
        public fileURL: string,
        public fetchImpl: typeof fetch,
        public family = false
    ) {
        this.files = this.load();
    }

    private load(): Promise<WalterFile[]> {
        if (!this.family) {
            return walter_get_files(this.fileURL, this.fetchImpl);
        }
        return walter_get_family(this.fileURL, this.fetchImpl).then(
            (result) => {
                this.defaultYear = result.defaultYear;
                return result.files;
            }
        );
    }

    /** Lädt die Dateien neu vom Server (z. B. nach einem Upload oder Jahreswechsel). */
    reload(): Promise<WalterFile[]> {
        this.files = this.load();
        return this.files;
    }

    async removeFile(file: WalterFile) {
        const files = await this.files;
        this.files = Promise.resolve(
            files.filter(
                (e) => !(e.fileName === file.fileName && e.key === file.key)
            )
        );

        return this.files;
    }
}
