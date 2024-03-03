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

import { walter_get_files } from '$walter/services/files';
import type { WalterFile } from '$walter/types';

export class WalterFileHandle {
    files: Promise<WalterFile[]>;

    constructor(
        public name: string,
        public fileURL: string,
        public fetchImpl: typeof fetch
    ) {
        this.files = walter_get_files(this.fileURL, this.fetchImpl);
    }

    // target might be either an index of a fileURL that has to match the handles fileURL
    async addFile(file: WalterFile): Promise<WalterFile[]> {
        const files = await this.files;

        if (!files) {
            return [];
        }
        // Do not overwrite if the file already exists
        if (files.some((e) => e.fileName === file.fileName)) {
            return files;
        }

        this.files = Promise.resolve([...files, file]);

        return this.files;
    }

    async removeFile(file: WalterFile) {
        const files = await this.files;
        this.files = Promise.resolve(
            files.filter((e) => e.fileName !== file.fileName)
        );

        return this.files;
    }
}
