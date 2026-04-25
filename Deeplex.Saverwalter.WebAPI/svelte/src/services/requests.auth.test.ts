import { beforeEach, describe, expect, it, vi } from 'vitest';

const getAccessTokenMock = vi.fn();
const walterGotoMock = vi.fn().mockResolvedValue(undefined);
const changeTrackerSetMock = vi.fn();
const addToastMock = vi.fn();

vi.mock('$walter/services/auth', () => ({
    getAccessToken: getAccessTokenMock
}));

vi.mock('$walter/services/utils', () => ({
    walter_goto: walterGotoMock
}));

vi.mock('$walter/store', () => ({
    addToast: addToastMock,
    changeTracker: {
        set: changeTrackerSetMock
    }
}));

describe('requests auth behavior', () => {
    beforeEach(() => {
        vi.resetModules();
        getAccessTokenMock.mockReset();
        walterGotoMock.mockReset();
        changeTrackerSetMock.mockReset();
        addToastMock.mockReset();
        vi.stubGlobal('fetch', vi.fn());
    });

    it('adds Authorization header when access token exists', async () => {
        getAccessTokenMock.mockReturnValue('abc123');
        const { walter_fetch } = await import('./requests');

        const fetchImpl = vi.fn().mockResolvedValue(
            new Response('{}', {
                status: 200,
                headers: { 'Content-Type': 'application/json' }
            })
        );

        await walter_fetch(fetchImpl, '/api/test', { method: 'GET' });

        expect(fetchImpl).toHaveBeenCalledOnce();
        const init = fetchImpl.mock.calls[0][1] as RequestInit;
        const headers = new Headers(init.headers);
        expect(headers.get('Authorization')).toBe('X-WalterToken abc123');
        expect(headers.get('Content-Type')).toBe('application/json');
    });

    it('redirects to login on 401 and throws', async () => {
        getAccessTokenMock.mockReturnValue(undefined);
        const { walter_fetch } = await import('./requests');
        const fetchImpl = vi.fn().mockResolvedValue(
            new Response('{}', {
                status: 401,
                headers: { 'Content-Type': 'application/json' }
            })
        );

        await expect(
            walter_fetch(fetchImpl, '/api/protected', { method: 'GET' })
        ).rejects.toThrow('Unauthorized access');

        expect(changeTrackerSetMock).toHaveBeenCalledWith(0);
        expect(walterGotoMock).toHaveBeenCalledWith('/login');
    });

    it('walter_get parses json payload', async () => {
        getAccessTokenMock.mockReturnValue('abc123');
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(JSON.stringify({ ok: true }), {
                status: 200,
                headers: { 'Content-Type': 'application/json' }
            })
        );
        const { walter_get } = await import('./requests');

        const result = await walter_get('/api/data', fetchMock);

        expect(result).toEqual({ ok: true });
        expect(fetchMock).toHaveBeenCalledWith(
            '/api/data',
            expect.objectContaining({ method: 'GET' })
        );
    });

    it('selection wrapper methods use the correct API paths', async () => {
        getAccessTokenMock.mockReturnValue(undefined);
        const fetchMock = vi.fn().mockImplementation(() =>
            Promise.resolve(
                new Response(JSON.stringify([]), {
                status: 200,
                headers: { 'Content-Type': 'application/json' }
                })
            )
        );
        const { walter_selection } = await import('./requests');

        const expectations: Array<[keyof typeof walter_selection, string]> = [
            ['adressen', '/api/selection/adressen'],
            ['anreden', '/api/selection/anreden'],
            ['betriebskostenrechnungen', '/api/selection/betriebskostenrechnungen'],
            ['erhaltungsaufwendungen', '/api/selection/erhaltungsaufwendungen'],
            ['hkvo_p9a2', '/api/selection/hkvo_p9a2'],
            ['juristischePersonen', '/api/selection/juristischepersonen'],
            ['mieten', '/api/selection/mieten'],
            ['mietminderungen', '/api/selection/mietminderungen'],
            ['rechtsformen', '/api/selection/rechtsformen'],
            ['umlagen', '/api/selection/umlagen'],
            ['umlagetypen', '/api/selection/umlagetypen'],
            ['vertraege', '/api/selection/vertraege'],
            ['zaehler', '/api/selection/zaehler'],
            ['zaehlerstaende', '/api/selection/zaehlerstaende'],
            ['wohnungen', '/api/selection/wohnungen'],
            ['kontakte', '/api/selection/kontakte'],
            ['umlageschluessel', '/api/selection/umlageschluessel'],
            ['umlagen_verbrauch', '/api/selection/umlagen_verbrauch'],
            ['umlagen_wohnungen', '/api/selection/umlagen_wohnungen'],
            ['zaehlertypen', '/api/selection/zaehlertypen'],
            ['verwalterrollen', '/api/selection/verwalterrollen'],
            ['userrole', '/api/selection/userrole']
        ];

        for (const [method, path] of expectations) {
            await walter_selection[method](fetchMock);
        }

        expect(fetchMock.mock.calls.map((call) => call[0])).toEqual(
            expectations.map(([, path]) => path)
        );
    });

    it('walter_put uses global fetch and reports toast payload', async () => {
        getAccessTokenMock.mockReturnValue('abc123');
        const fetchMock = vi.mocked(fetch).mockResolvedValue(
            new Response(JSON.stringify({ saved: true }), {
                status: 200,
                headers: { 'Content-Type': 'application/json' }
            })
        );
        const { walter_put } = await import('./requests');

        const toast = {
            successTitle: 'saved',
            failureTitle: 'failed',
            subtitleSuccess: () => 'ok',
            subtitleFailure: () => 'no'
        };
        const result = await walter_put('/api/resource', { id: 1 }, toast);

        expect(result.status).toBe(200);
        expect(fetchMock).toHaveBeenCalledWith(
            '/api/resource',
            expect.objectContaining({
                method: 'PUT',
                body: JSON.stringify({ id: 1 })
            })
        );
        expect(addToastMock).toHaveBeenCalledWith(toast, true, { saved: true });
    });

    it('walter_post uses global fetch with POST body', async () => {
        getAccessTokenMock.mockReturnValue(undefined);
        const fetchMock = vi.mocked(fetch).mockResolvedValue(
            new Response(JSON.stringify({ created: true }), {
                status: 200,
                headers: { 'Content-Type': 'application/json' }
            })
        );
        const { walter_post } = await import('./requests');

        const response = await walter_post('/api/resource', { id: 2 });

        expect(response.status).toBe(200);
        expect(fetchMock).toHaveBeenCalledWith(
            '/api/resource',
            expect.objectContaining({
                method: 'POST',
                body: JSON.stringify({ id: 2 })
            })
        );
    });

    it('walter_delete reports unsuccessful delete via toast', async () => {
        getAccessTokenMock.mockReturnValue(undefined);
        const fetchMock = vi.mocked(fetch).mockResolvedValue(
            new Response('', {
                status: 500,
                headers: { 'Content-Type': 'application/json' }
            })
        );
        const { walter_delete } = await import('./requests');

        const toast = {
            successTitle: 'deleted',
            failureTitle: 'failed',
            subtitleSuccess: () => 'ok',
            subtitleFailure: () => 'no'
        };
        const response = await walter_delete('/api/resource', toast);

        expect(response.status).toBe(500);
        expect(fetchMock).toHaveBeenCalledWith(
            '/api/resource',
            expect.objectContaining({ method: 'DELETE' })
        );
        expect(addToastMock).toHaveBeenCalledWith(toast, false);
    });
});
