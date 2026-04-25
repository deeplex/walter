import { beforeEach, describe, expect, it, vi } from 'vitest';

const walterGetFilesMock = vi.fn();

vi.mock('$walter/services/files', () => ({
    walter_get_files: walterGetFilesMock
}));

describe('WalterFileHandle', () => {
    beforeEach(() => {
        walterGetFilesMock.mockReset();
    });

    it('loads files on construction and appends only new files', async () => {
        walterGetFilesMock.mockResolvedValue([
            { fileName: 'existing.pdf' },
            { fileName: 'other.pdf' }
        ]);
        const { WalterFileHandle } = await import('./WalterFileHandle');
        const fetchImpl = vi.fn() as unknown as typeof fetch;
        const handle = new WalterFileHandle('Docs', '/api/files', fetchImpl);

        const duplicate = await handle.addFile({
            fileName: 'existing.pdf'
        } as never);
        const appended = await handle.addFile({ fileName: 'new.pdf' } as never);

        expect(walterGetFilesMock).toHaveBeenCalledWith('/api/files', fetchImpl);
        expect(duplicate).toEqual([
            { fileName: 'existing.pdf' },
            { fileName: 'other.pdf' }
        ]);
        expect(appended).toEqual([
            { fileName: 'existing.pdf' },
            { fileName: 'other.pdf' },
            { fileName: 'new.pdf' }
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