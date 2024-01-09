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

import { walter_get } from '$walter/services/requests';

export abstract class WalterApiHandler {
    protected static ApiURL: string;

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public static fromJson(_json: unknown) {
        throw new Error('WalterApiHandler.fromJson not implemented.');
    }

    public static async GetAll<T extends WalterApiHandler>(
        fetchImpl: typeof fetch
    ): Promise<T[]> {
        const response = await walter_get(this.ApiURL, fetchImpl);

        if (!Array.isArray(response)) {
            throw new Error('Expected response to be an array.');
        }

        return response.map<T>((json) => this.fromJson(json) as unknown as T);
    }

    public static async GetOne<T extends WalterApiHandler>(
        id: string,
        fetchImpl: typeof fetch
    ): Promise<T> {
        const url = `${this.ApiURL}/${id}`;
        const response = await walter_get(url, fetchImpl);

        return this.fromJson(response) as unknown as T;
    }
}
