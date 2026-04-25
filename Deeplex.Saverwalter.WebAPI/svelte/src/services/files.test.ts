import { beforeEach, describe, expect, it, vi } from 'vitest';

const addToastMock = vi.fn();
const walterDeleteMock = vi.fn();
const walterFetchMock = vi.fn();
const walterGetMock = vi.fn();

class WalterFileMock {
    fileName: string;
    key: string;
    lastModified: string;
    size: number;
    blob: Blob | null;
    type: string;

    constructor(
        fileName: string,
        key: string,
        lastModified: string,
        size: number,
        blob: Blob | null,
        type: string
    ) {
        this.fileName = fileName;
        this.key = key;
        this.lastModified = lastModified;
        this.size = size;
        this.blob = blob;
        this.type = type;
    }
}

vi.mock('$walter/store', () => ({
    addToast: addToastMock
}));

vi.mock('./requests', () => ({
    walter_delete: walterDeleteMock,
    walter_fetch: walterFetchMock,
    walter_get: walterGetMock
}));

vi.mock('$walter/lib', () => ({
    WalterAbrechnungsresultatEntry: { ApiURL: '/api/abrechnungsresultate' },
    WalterAdresseEntry: { ApiURL: '/api/adressen' },
    WalterBetriebskostenrechnungEntry: { ApiURL: '/api/betriebskostenrechnungen' },
    WalterErhaltungsaufwendungEntry: { ApiURL: '/api/erhaltungsaufwendungen' },
    WalterKontaktEntry: { ApiURL: '/api/kontakte' },
    WalterMieteEntry: { ApiURL: '/api/mieten' },
    WalterMietminderungEntry: { ApiURL: '/api/mietminderungen' },
    WalterTransaktionEntry: { ApiURL: '/api/transaktionen' },
    WalterUmlageEntry: { ApiURL: '/api/umlagen' },
    WalterUmlagetypEntry: { ApiURL: '/api/umlagetypen' },
    WalterVertragEntry: { ApiURL: '/api/vertraege' },
    WalterVertragVersionEntry: { ApiURL: '/api/vertragversionen' },
    WalterWohnungEntry: { ApiURL: '/api/wohnungen' },
    WalterZaehlerEntry: { ApiURL: '/api/zaehler' },
    WalterZaehlerstandEntry: { ApiURL: '/api/zaehlerstaende' }
}));

vi.mock('$walter/lib/WalterFile', () => ({
    WalterFile: WalterFileMock
}));

describe('files service', () => {
    beforeEach(() => {
        vi.resetModules();
        addToastMock.mockReset();
        walterDeleteMock.mockReset();
        walterFetchMock.mockReset();
        walterGetMock.mockReset();
        vi.stubGlobal(
            'URL',
            Object.assign(URL, {
                createObjectURL: vi.fn().mockReturnValue('blob:test'),
                revokeObjectURL: vi.fn()
            })
        );
    });

    it('builds entity file URLs', async () => {
        const { fileURL } = await import('./files');

        expect(fileURL.abrechnungsresultat('1')).toBe(
            '/api/abrechnungsresultate/1/files'
        );
        expect(fileURL.wohnung('12')).toBe('/api/wohnungen/12/files');
        expect(fileURL.adresse('5')).toBe('/api/adressen/5/files');
        expect(fileURL.betriebskostenrechnung('8')).toBe(
            '/api/betriebskostenrechnungen/8/files'
        );
        expect(fileURL.erhaltungsaufwendung('7')).toBe(
            '/api/erhaltungsaufwendungen/7/files'
        );
        expect(fileURL.miete('4')).toBe('/api/mieten/4/files');
        expect(fileURL.mietminderung('6')).toBe(
            '/api/mietminderungen/6/files'
        );
        expect(fileURL.kontakt('3')).toBe('/api/kontakte/3/files');
        expect(fileURL.transaktion('13')).toBe(
            '/api/transaktionen/13/files'
        );
        expect(fileURL.umlage('14')).toBe('/api/umlagen/14/files');
        expect(fileURL.umlagetyp('15')).toBe('/api/umlagetypen/15/files');
        expect(fileURL.vertrag('9')).toBe('/api/vertraege/9/files');
        expect(fileURL.vertragversion('10')).toBe(
            '/api/vertragversionen/10/files'
        );
        expect(fileURL.zaehler('11')).toBe('/api/zaehler/11/files');
        expect(fileURL.zaehlerstand('16')).toBe(
            '/api/zaehlerstaende/16/files'
        );
        expect(fileURL.stack).toBe('/api/user/files');
    });

    it('uploads files through walter_fetch and reports toast state', async () => {
        walterFetchMock.mockResolvedValue(
            new Response('', { status: 200, headers: { 'Content-Type': 'application/json' } })
        );
        const { walter_file_post } = await import('./files');
        const file = new File(['hello'], 'greeting.txt', { type: 'text/plain' });
        const toast = {
            successTitle: 'ok',
            failureTitle: 'fail',
            subtitleSuccess: () => 'ok',
            subtitleFailure: () => 'fail'
        };

        const response = await walter_file_post(file, '/api/files', fetch, toast);

        expect(response.status).toBe(200);
        expect(walterFetchMock).toHaveBeenCalledWith(
            fetch,
            '/api/files/greeting.txt',
            expect.objectContaining({ method: 'PUT', body: file })
        );
        expect(addToastMock).toHaveBeenCalledWith(toast, true);
    });

    it('downloads file blobs using walter_fetch', async () => {
        const blob = new Blob(['abc'], { type: 'text/plain' });
        walterFetchMock.mockResolvedValue(
            new Response(blob, { status: 200 })
        );
        const { walter_file_get } = await import('./files');

        const result = await walter_file_get('/api/files/test');

        expect(walterFetchMock).toHaveBeenCalledWith(
            fetch,
            '/api/files/test',
            expect.objectContaining({ method: 'GET' })
        );
        expect(result).toBeInstanceOf(Blob);
    });

    it('deletes files via walter_delete using the file key', async () => {
        walterDeleteMock.mockResolvedValue(new Response('', { status: 200 }));
        const { walter_file_delete } = await import('./files');
        const toast = {
            successTitle: 'ok',
            failureTitle: 'fail',
            subtitleSuccess: () => 'ok',
            subtitleFailure: () => 'fail'
        };

        await walter_file_delete({ key: '/api/files/demo.txt' } as never, toast);

        expect(walterDeleteMock).toHaveBeenCalledWith('/api/files/demo.txt', toast);
    });

    it('finish_file_post reports failed uploads through the toast branch', async () => {
        const { finish_file_post } = await import('./files');
        const toast = {
            successTitle: 'ok',
            failureTitle: 'fail',
            subtitleSuccess: () => 'ok',
            subtitleFailure: () => 'fail'
        };

        const response = await finish_file_post(
            new Response('', { status: 500 }),
            toast
        );

        expect(response.status).toBe(500);
        expect(addToastMock).toHaveBeenCalledWith(toast, false);
    });

    it('downloads blobs through a temporary anchor element', async () => {
        const appendChildSpy = vi.spyOn(document.body, 'appendChild');
        const removeChildSpy = vi.spyOn(document.body, 'removeChild');
        const clickSpy = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {});
        const { download_file_blob } = await import('./files');

        download_file_blob(new Blob(['demo']), 'demo.txt');

        expect(URL.createObjectURL).toHaveBeenCalled();
        expect(clickSpy).toHaveBeenCalledOnce();
        expect(appendChildSpy).toHaveBeenCalledOnce();
        expect(removeChildSpy).toHaveBeenCalledOnce();
        expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test');
    });

    it('maps fetched file metadata into WalterFile instances', async () => {
        walterGetMock.mockResolvedValue([
            {
                fileName: 'report.pdf',
                key: '/api/files/report.pdf',
                lastModified: '2026-04-25T10:00:00Z',
                size: 42,
                blob: null,
                type: 'application/pdf'
            }
        ]);
        const { walter_get_files } = await import('./files');

        const files = await walter_get_files('/api/files', fetch);

        expect(walterGetMock).toHaveBeenCalledWith('/api/files', fetch);
        expect(files).toHaveLength(1);
        expect(files[0]).toBeInstanceOf(WalterFileMock);
        expect(files[0]).toMatchObject({
            fileName: 'report.pdf',
            key: '/api/files/report.pdf',
            type: 'application/pdf'
        });
    });
});