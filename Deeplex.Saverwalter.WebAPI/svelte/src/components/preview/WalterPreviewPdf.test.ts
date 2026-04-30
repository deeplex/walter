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

const downloadFileBlobMock = vi.fn();

vi.mock('$walter/services/files', () => ({
    download_file_blob: downloadFileBlobMock
}));

describe('WalterPreviewPdf helper', () => {
    beforeEach(() => {
        downloadFileBlobMock.mockReset();
    });

    it('calls download_file_blob with blob and fileName when blob is present', async () => {
        const { downloadPdf } = await import('./WalterPreviewPdf');
        const blob = new Blob(['%PDF-1.4 fake content'], {
            type: 'application/pdf'
        });

        downloadPdf({ fileName: 'abrechnung.pdf', blob } as never);

        expect(downloadFileBlobMock).toHaveBeenCalledOnce();
        expect(downloadFileBlobMock).toHaveBeenCalledWith(
            blob,
            'abrechnung.pdf'
        );
    });

    it('does nothing when no blob is present', async () => {
        const { downloadPdf } = await import('./WalterPreviewPdf');

        downloadPdf({ fileName: 'missing.pdf', blob: undefined } as never);

        expect(downloadFileBlobMock).not.toHaveBeenCalled();
    });
});
