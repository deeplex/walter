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

import { WalterVertragEntry } from '$walter/lib';
import { walter_file_post } from '$walter/services/files';

const headers = {
    'Content-Type': 'application/octet-stream'
};

export function post_files(id: number, jahr: number, fetchImpl: typeof fetch) {
    const apiURL = `/api/betriebskostenabrechnung/${id}/${jahr}`;
    fetch(apiURL, {
        method: 'GET',
        headers
    })
        .then((e) => e.blob())
        .then((e) =>
            walter_file_post(
                new File([e], `Abrechnung ${jahr}.docx`),
                `${WalterVertragEntry.ApiURL}/${id}/files`,
                fetchImpl
            )
        );
}
