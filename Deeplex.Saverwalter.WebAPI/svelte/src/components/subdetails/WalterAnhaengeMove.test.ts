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

import { beforeEach, describe, expect, it, vi } from 'vitest';

const getMock = vi.fn();
const postMock = vi.fn();
const deleteMock = vi.fn();

vi.mock('$walter/services/files', () => ({
    fileURL: { stack: '/api/user/files' },
    walter_file_get: getMock,
    walter_file_post: postMock,
    walter_file_delete: deleteMock
}));

vi.mock('$walter/lib', () => ({
    WalterToastContent: class {}
}));

const handle = (name: string, fileURL: string) => ({
    name,
    fileURL,
    reload: vi.fn().mockResolvedValue([])
});

const file = (year = 0) =>
    ({ fileName: 'a.pdf', key: '/api/vertraege/3/files/a.pdf', year }) as never;

describe('move_file_between_handles', () => {
    beforeEach(() => {
        getMock.mockReset().mockResolvedValue(new Blob(['abc']));
        postMock.mockReset().mockResolvedValue({ status: 200 });
        deleteMock.mockReset().mockResolvedValue({ status: 200 });
    });

    it('writes the file to the target, deletes the source and reloads both', async () => {
        const { move_file_between_handles } =
            await import('./WalterAnhaengeMove');
        const source = handle('Vertrag', '/api/vertraege/3/files');
        const target = handle('Ablagestapel', '/api/user/files');

        const moved = await move_file_between_handles(
            file(2023),
            source as never,
            target as never,
            fetch
        );

        expect(moved).toBe(true);
        expect(postMock).toHaveBeenCalledWith(
            expect.objectContaining({ name: 'a.pdf' }),
            '/api/user/files',
            fetch,
            expect.anything(),
            undefined // der Ablagestapel hat keine Jahre
        );
        expect(deleteMock).toHaveBeenCalledWith(file(2023), expect.anything());
        expect(source.reload).toHaveBeenCalled();
        expect(target.reload).toHaveBeenCalled();
    });

    it('keeps the year when the target has year folders', async () => {
        const { move_file_between_handles } =
            await import('./WalterAnhaengeMove');

        await move_file_between_handles(
            file(2023),
            handle('Ablagestapel', '/api/user/files') as never,
            handle('Vertrag', '/api/vertraege/3/files') as never,
            fetch
        );

        expect(postMock).toHaveBeenCalledWith(
            expect.anything(),
            '/api/vertraege/3/files',
            fetch,
            expect.anything(),
            2023
        );
    });

    it('does not delete the source when writing the target failed', async () => {
        postMock.mockResolvedValue({ status: 500 });
        const { move_file_between_handles } =
            await import('./WalterAnhaengeMove');
        const source = handle('Vertrag', '/api/vertraege/3/files');

        const moved = await move_file_between_handles(
            file(),
            source as never,
            handle('Ablagestapel', '/api/user/files') as never,
            fetch
        );

        expect(moved).toBe(false);
        expect(deleteMock).not.toHaveBeenCalled();
        expect(source.reload).not.toHaveBeenCalled();
    });

    it('reports a copy without deleting the source as not moved', async () => {
        deleteMock.mockResolvedValue({ status: 500 });
        const { move_file_between_handles } =
            await import('./WalterAnhaengeMove');
        const target = handle('Ablagestapel', '/api/user/files');

        const moved = await move_file_between_handles(
            file(),
            handle('Vertrag', '/api/vertraege/3/files') as never,
            target as never,
            fetch
        );

        expect(moved).toBe(false);
        expect(target.reload).toHaveBeenCalled();
    });
});
