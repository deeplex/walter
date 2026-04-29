import { beforeEach, describe, expect, it, vi } from 'vitest';

const downloadFileBlobMock = vi.fn();

vi.mock('$walter/services/files', () => ({
    download_file_blob: downloadFileBlobMock
}));

describe('WalterPreview helper', () => {
    beforeEach(() => {
        downloadFileBlobMock.mockReset();
    });

    it('downloads when blob exists', async () => {
        const { download } = await import('./WalterPreview');
        const blob = new Blob(['abc']);

        download({ fileName: 'demo.txt', blob } as never);

        expect(downloadFileBlobMock).toHaveBeenCalledWith(blob, 'demo.txt');
    });

    it('does nothing when no blob exists', async () => {
        const { download } = await import('./WalterPreview');

        download({ fileName: 'demo.txt', blob: undefined } as never);

        expect(downloadFileBlobMock).not.toHaveBeenCalled();
    });
});
