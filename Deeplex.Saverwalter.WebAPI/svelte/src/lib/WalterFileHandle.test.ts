import { beforeEach, describe, expect, it, vi } from 'vitest';

const walterGetFilesMock = vi.fn();
const walterGetFamilyMock = vi.fn();

vi.mock('$walter/services/files', () => ({
    walter_get_files: walterGetFilesMock,
    walter_get_family: walterGetFamilyMock
}));

describe('WalterFileHandle', () => {
    beforeEach(() => {
        walterGetFilesMock.mockReset();
        walterGetFamilyMock.mockReset();
    });

    it('loads the files of a plain handle on construction', async () => {
        walterGetFilesMock.mockResolvedValue([
            { fileName: 'existing.pdf' },
            { fileName: 'other.pdf' }
        ]);
        const { WalterFileHandle } = await import('./WalterFileHandle');
        const fetchImpl = vi.fn() as unknown as typeof fetch;

        const handle = new WalterFileHandle('Docs', '/api/files', fetchImpl);

        expect(walterGetFilesMock).toHaveBeenCalledWith(
            '/api/files',
            fetchImpl
        );
        expect(walterGetFamilyMock).not.toHaveBeenCalled();
        expect(await handle.files).toEqual([
            { fileName: 'existing.pdf' },
            { fileName: 'other.pdf' }
        ]);
    });

    it('reloads the files from the server', async () => {
        walterGetFilesMock.mockResolvedValueOnce([{ fileName: 'old.pdf' }]);
        const { WalterFileHandle } = await import('./WalterFileHandle');
        const handle = new WalterFileHandle(
            'Docs',
            '/api/files',
            vi.fn() as unknown as typeof fetch
        );
        walterGetFilesMock.mockResolvedValueOnce([
            { fileName: 'old.pdf', year: 2024 }
        ]);

        const reloaded = await handle.reload();

        expect(reloaded).toEqual([{ fileName: 'old.pdf', year: 2024 }]);
        expect(await handle.files).toEqual(reloaded);
    });

    it('loads the family of a family handle and remembers the default year', async () => {
        walterGetFamilyMock.mockResolvedValue({
            defaultYear: 2021,
            files: [{ fileName: 'a.jpg', key: '/api/zaehler/1/files/a.jpg' }]
        });
        const { WalterFileHandle } = await import('./WalterFileHandle');
        const fetchImpl = vi.fn() as unknown as typeof fetch;

        const handle = new WalterFileHandle(
            'Zähler',
            '/api/zaehler/1/files',
            fetchImpl,
            true
        );
        const files = await handle.files;

        expect(walterGetFamilyMock).toHaveBeenCalledWith(
            '/api/zaehler/1/files',
            fetchImpl
        );
        expect(walterGetFilesMock).not.toHaveBeenCalled();
        expect(files).toHaveLength(1);
        expect(handle.defaultYear).toBe(2021);
    });

    it('removes only the file of the given owner when names are equal', async () => {
        walterGetFamilyMock.mockResolvedValue({
            defaultYear: 2024,
            files: [
                {
                    fileName: 'stand.jpg',
                    key: '/api/zaehlerstaende/5/files/stand.jpg'
                },
                {
                    fileName: 'stand.jpg',
                    key: '/api/zaehlerstaende/6/files/stand.jpg'
                }
            ]
        });
        const { WalterFileHandle } = await import('./WalterFileHandle');
        const handle = new WalterFileHandle(
            'Zähler',
            '/api/zaehler/1/files',
            vi.fn() as unknown as typeof fetch,
            true
        );
        const [first] = await handle.files;

        const remaining = await handle.removeFile(first);

        expect(remaining.map((f) => f.key)).toEqual([
            '/api/zaehlerstaende/6/files/stand.jpg'
        ]);
    });

    it('removes files by name', async () => {
        walterGetFilesMock.mockResolvedValue([
            { fileName: 'keep.pdf' },
            { fileName: 'remove.pdf' }
        ]);
        const { WalterFileHandle } = await import('./WalterFileHandle');
        const handle = new WalterFileHandle(
            'Docs',
            '/api/files',
            vi.fn() as unknown as typeof fetch
        );

        const remaining = await handle.removeFile({
            fileName: 'remove.pdf'
        } as never);

        expect(remaining).toEqual([{ fileName: 'keep.pdf' }]);
    });
});
