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

import { walter_file_get } from '$walter/services/files';
import { walter_goto } from '$walter/services/utils';
import { WalterFile } from '$walter/lib/WalterFile';

export async function get_file_and_update_url(file: WalterFile) {
    const searchParams = new URLSearchParams({ file: `${file.fileName}` });
    walter_goto(`?${searchParams.toString()}`, { noScroll: true });

    return walter_file_get(file.key).then((e: unknown) => {
        const blob = e as Blob;
        return new WalterFile(
            file.fileName,
            file.key,
            file.lastModified,
            blob.size,
            new File([blob], file.fileName, { type: blob.type }),
            blob.type
        );
    });
}
