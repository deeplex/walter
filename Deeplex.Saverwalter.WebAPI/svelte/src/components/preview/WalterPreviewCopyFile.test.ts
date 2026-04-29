import { beforeEach, describe, expect, it, vi } from 'vitest';

const walterFilePostMock = vi.fn();
const walterFileDeleteMock = vi.fn();

const selectionMock = {
    adressen: vi.fn().mockResolvedValue([]),
    betriebskostenrechnungen: vi.fn().mockResolvedValue([]),
    erhaltungsaufwendungen: vi.fn().mockResolvedValue([]),
    kontakte: vi.fn().mockResolvedValue([]),
    mieten: vi.fn().mockResolvedValue([]),
    mietminderungen: vi.fn().mockResolvedValue([]),
    umlagen: vi.fn().mockResolvedValue([]),
    umlagetypen: vi.fn().mockResolvedValue([]),
    vertraege: vi.fn().mockResolvedValue([]),
    wohnungen: vi.fn().mockResolvedValue([]),
    zaehler: vi.fn().mockResolvedValue([]),
    zaehlerstaende: vi.fn().mockResolvedValue([])
};

vi.mock('$walter/services/requests', () => ({
    walter_selection: selectionMock
}));

vi.mock('$walter/services/files', () => ({
    fileURL: {
        adresse: (id: string) => `/api/adressen/${id}/files`,
        betriebskostenrechnung: (id: string) =>
            `/api/betriebskostenrechnungen/${id}/files`,
        erhaltungsaufwendung: (id: string) =>
            `/api/erhaltungsaufwendungen/${id}/files`,
        kontakt: (id: string) => `/api/kontakte/${id}/files`,
        miete: (id: string) => `/api/mieten/${id}/files`,
        mietminderung: (id: string) => `/api/mietminderungen/${id}/files`,
        umlage: (id: string) => `/api/umlagen/${id}/files`,
        umlagetyp: (id: string) => `/api/umlagetypen/${id}/files`,
        vertrag: (id: string) => `/api/vertraege/${id}/files`,
        wohnung: (id: string) => `/api/wohnungen/${id}/files`,
        zaehler: (id: string) => `/api/zaehler/${id}/files`,
        zaehlerstand: (id: string) => `/api/zaehlerstaende/${id}/files`,
        stack: '/api/user/files'
    },
    walter_file_post: walterFilePostMock,
    walter_file_delete: walterFileDeleteMock
}));

vi.mock('$walter/lib', () => {
    class ToastMock {
        constructor(
            public successTitle?: string,
            public failureTitle?: string,
            public subtitleSuccess?: () => string,
            public subtitleFailure?: () => string
        ) {}
    }

    return {
        WalterToastContent: ToastMock,
        WalterAdresseEntry: { ApiURL: '/api/adressen' },
        WalterBetriebskostenrechnungEntry: {
            ApiURL: '/api/betriebskostenrechnungen'
        },
        WalterErhaltungsaufwendungEntry: {
            ApiURL: '/api/erhaltungsaufwendungen'
        },
        WalterKontaktEntry: { ApiURL: '/api/kontakte' },
        WalterMieteEntry: { ApiURL: '/api/mieten' },
        WalterMietminderungEntry: { ApiURL: '/api/mietminderungen' },
        WalterUmlageEntry: { ApiURL: '/api/umlagen' },
        WalterUmlagetypEntry: { ApiURL: '/api/umlagetypen' },
        WalterVertragEntry: { ApiURL: '/api/vertraege' },
        WalterWohnungEntry: { ApiURL: '/api/wohnungen' },
        WalterZaehlerEntry: { ApiURL: '/api/zaehler' },
        WalterZaehlerstandEntry: { ApiURL: '/api/zaehlerstaende' }
    };
});

vi.mock('..', () => ({
    WalterAdresse: { name: 'WalterAdresse' },
    WalterBetriebskostenrechnung: { name: 'WalterBetriebskostenrechnung' },
    WalterErhaltungsaufwendung: { name: 'WalterErhaltungsaufwendung' },
    WalterKontakt: { name: 'WalterKontakt' },
    WalterMiete: { name: 'WalterMiete' },
    WalterMietminderung: { name: 'WalterMietminderung' },
    WalterUmlage: { name: 'WalterUmlage' },
    WalterUmlagetyp: { name: 'WalterUmlagetyp' },
    WalterVertrag: { name: 'WalterVertrag' },
    WalterWohnung: { name: 'WalterWohnung' },
    WalterZaehler: { name: 'WalterZaehler' },
    WalterZaehlerstand: { name: 'WalterZaehlerstand' }
}));

describe('WalterPreviewCopyFile helpers', () => {
    beforeEach(() => {
        walterFilePostMock.mockReset();
        walterFileDeleteMock.mockReset();
        Object.values(selectionMock).forEach((mock) => mock.mockClear());
    });

    it('copyImpl validates required target selection', async () => {
        const { copyImpl } = await import('./WalterPreviewCopyFile');

        const resultNoTable = await copyImpl(
            { fileName: 'demo.pdf' } as never,
            fetch,
            undefined,
            undefined
        );
        const resultNoEntry = await copyImpl(
            { fileName: 'demo.pdf' } as never,
            fetch,
            {
                key: 'adressen',
                fileURL: (id: string) => `/api/adressen/${id}/files`
            } as never,
            undefined
        );

        expect(resultNoTable).toBeUndefined();
        expect(resultNoEntry).toBeUndefined();
    });

    it('copyImpl posts to stack and regular targets', async () => {
        walterFilePostMock.mockResolvedValue(new Response('', { status: 200 }));
        const { copyImpl, tables } = await import('./WalterPreviewCopyFile');
        const stackTable = tables.find((table) => table.key === 'stack')!;
        const adressenTable = tables.find((table) => table.key === 'adressen')!;
        const file = { fileName: 'demo.pdf', blob: new Blob(['abc']) } as never;

        const stackResult = await copyImpl(file, fetch, stackTable, undefined);
        const tableResult = await copyImpl(file, fetch, adressenTable, {
            id: 12,
            text: 'Adresse 12'
        } as never);

        expect(stackResult).toBe(true);
        expect(tableResult).toBe(true);
        expect(walterFilePostMock).toHaveBeenNthCalledWith(
            1,
            expect.any(File),
            '/api/user/files',
            fetch,
            expect.anything()
        );
        expect(walterFilePostMock).toHaveBeenNthCalledWith(
            2,
            expect.any(File),
            '/api/adressen/12/files',
            fetch,
            expect.anything()
        );
    });

    it('renameImpl uploads new file content using the same key', async () => {
        walterFilePostMock.mockResolvedValue(new Response('', { status: 200 }));
        const { renameImpl } = await import('./WalterPreviewCopyFile');

        const result = await renameImpl(
            {
                fileName: 'old.txt',
                key: '/api/files/old.txt',
                blob: new Blob(['old'])
            } as never,
            fetch,
            'new.txt'
        );

        expect(result.status).toBe(200);
        expect(walterFilePostMock).toHaveBeenCalledWith(
            expect.objectContaining({ name: 'new.txt' }),
            '/api/files/old.txt',
            fetch,
            expect.anything()
        );
    });

    it('moveImpl returns true on successful copy+delete and false otherwise', async () => {
        const { moveImpl } = await import('./WalterPreviewCopyFile');
        const selectedTable = {
            key: 'wohnungen',
            fileURL: (id: string) => `/api/wohnungen/${id}/files`
        } as never;
        const selectedEntry = { id: 4, text: 'Wohnung 4' } as never;
        const file = {
            fileName: 'move.pdf',
            blob: new Blob(['x']),
            key: '/api/files/move.pdf'
        } as never;

        walterFilePostMock.mockResolvedValueOnce(
            new Response('', { status: 200 })
        );
        walterFileDeleteMock.mockResolvedValueOnce(
            new Response('', { status: 200 })
        );

        const successResult = await moveImpl(
            file,
            fetch,
            selectedTable,
            selectedEntry
        );

        walterFilePostMock.mockResolvedValueOnce(
            new Response('', { status: 500 })
        );

        const failedCopyResult = await moveImpl(
            file,
            fetch,
            selectedTable,
            selectedEntry
        );

        walterFilePostMock.mockResolvedValueOnce(
            new Response('', { status: 200 })
        );
        walterFileDeleteMock.mockResolvedValueOnce(
            new Response('', { status: 500 })
        );

        const failedDeleteResult = await moveImpl(
            file,
            fetch,
            selectedTable,
            selectedEntry
        );

        expect(successResult).toBe(true);
        expect(failedCopyResult).toBe(false);
        expect(failedDeleteResult).toBe(false);
    });

    it('executes all table lambdas (fetch, fileURL, newPage)', async () => {
        const { tables } = await import('./WalterPreviewCopyFile');

        for (const table of tables) {
            await table.fetch(fetch);
            const url = table.fileURL('77');
            expect(typeof url).toBe('string');

            const page = table.newPage();
            if (table.key === 'stack') {
                expect(page).toBeUndefined();
            } else {
                expect(page).toBeDefined();
            }
        }

        expect(selectionMock.adressen).toHaveBeenCalled();
        expect(selectionMock.wohnungen).toHaveBeenCalled();
        expect(selectionMock.zaehlerstaende).toHaveBeenCalled();
    });
});
