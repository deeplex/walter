import { beforeEach, describe, expect, it, vi } from 'vitest';

const getAccessTokenMock = vi.fn();
const walterGotoMock = vi.fn().mockResolvedValue(undefined);
const changeTrackerSetMock = vi.fn();

vi.mock('$walter/services/auth', () => ({
    getAccessToken: getAccessTokenMock
}));

vi.mock('$walter/services/utils', () => ({
    walter_goto: walterGotoMock
}));

vi.mock('$walter/store', () => ({
    addToast: vi.fn(),
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
});
