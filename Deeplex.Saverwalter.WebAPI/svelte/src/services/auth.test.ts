import { get } from 'svelte/store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
const addToastMock = vi.fn();

vi.mock('$app/environment', () => ({
    browser: true
}));

vi.mock('$walter/store', () => ({
    addToast: addToastMock
}));

function createToken(expiresAt: string) {
    return Buffer.from(`${'A'.repeat(32)}${expiresAt}`, 'ascii').toString(
        'base64'
    );
}

describe('auth service', () => {
    beforeEach(() => {
        vi.restoreAllMocks();
        vi.resetModules();
        addToastMock.mockReset();
        vi.spyOn(console, 'error').mockImplementation(() => undefined);
        localStorage.clear();
    });

    it('sign-in stores auth state and access token', async () => {
        const { walter_sign_in, getAccessToken } = await import('./auth');
        const fetchImpl = vi.fn().mockResolvedValue(
            new Response(
                JSON.stringify({
                    userId: 'user-1',
                    token: 'session-token',
                    role: 1,
                    name: 'Alice'
                }),
                {
                    status: 200,
                    headers: { 'Content-Type': 'application/json' }
                }
            )
        );

        const result = await walter_sign_in(fetchImpl, 'alice', 'secret');

        expect(result?.userId).toBe('user-1');
        expect(getAccessToken()).toBe('session-token');
        expect(localStorage.getItem('auth-state')).toContain('user-1');
        expect(fetchImpl).toHaveBeenCalledOnce();
    });

    it('failed sign-in returns null and does not set token', async () => {
        const { walter_sign_in, getAccessToken } = await import('./auth');
        const fetchImpl = vi.fn().mockResolvedValue(
            new Response('', {
                status: 401
            })
        );

        const result = await walter_sign_in(fetchImpl, 'alice', 'wrong');

        expect(result).toBeNull();
        expect(getAccessToken()).toBeUndefined();
        expect(localStorage.getItem('auth-state')).toBeNull();
        expect(addToastMock).not.toHaveBeenCalled();
    });

    it('failed sign-in uses the failure toast branch when a toast is provided', async () => {
        const { walter_sign_in } = await import('./auth');
        const fetchImpl = vi.fn().mockResolvedValue(
            new Response('', {
                status: 401
            })
        );

        await walter_sign_in(fetchImpl, 'alice', 'wrong', {
            successTitle: 'ok',
            failureTitle: 'fail',
            subtitleSuccess: () => 'ok',
            subtitleFailure: () => 'fail'
        });

        expect(addToastMock).toHaveBeenCalledWith(
            expect.objectContaining({ failureTitle: 'fail' }),
            false
        );
    });

    it('loads persisted auth state from localStorage when the token is still valid', async () => {
        const token = createToken('20991231T235959Z');
        vi.spyOn(globalThis, 'atob').mockReturnValue(
            `${'A'.repeat(32)}20991231T235959Z`
        );
        localStorage.setItem(
            'auth-state',
            JSON.stringify({
                userId: 'persisted-user',
                token,
                role: 2,
                name: 'Persisted User'
            })
        );

        const { getAuthState, getAccessToken } = await import('./auth');

        expect(get(getAuthState())).toEqual({
            userId: 'persisted-user',
            token,
            role: 2,
            name: 'Persisted User'
        });
        expect(getAccessToken()).toBe(token);
    });

    it('rejects malformed persisted auth state', async () => {
        localStorage.setItem('auth-state', '{not-json');

        const { getAuthState, getAccessToken } = await import('./auth');

        expect(get(getAuthState())).toBeNull();
        expect(getAccessToken()).toBeUndefined();
    });

    it('rejects persisted auth state with an invalid shape', async () => {
        localStorage.setItem(
            'auth-state',
            JSON.stringify({ userId: 123, token: true, role: 'admin' })
        );

        const { getAuthState } = await import('./auth');

        expect(get(getAuthState())).toBeNull();
    });

    it('removes expired persisted auth state', async () => {
        const token = createToken('20000101T000000Z');
        vi.spyOn(globalThis, 'atob').mockReturnValue(
            `${'A'.repeat(32)}20000101T000000Z`
        );
        localStorage.setItem(
            'auth-state',
            JSON.stringify({
                userId: 'expired-user',
                token,
                role: 1,
                name: 'Expired User'
            })
        );

        const { getAuthState, getAccessToken } = await import('./auth');

        expect(get(getAuthState())).toBeNull();
        expect(getAccessToken()).toBeUndefined();
        expect(localStorage.getItem('auth-state')).toBeNull();
    });

    it('sign-out clears state and localStorage', async () => {
        const { walter_sign_in, walter_sign_out, getAccessToken } = await import('./auth');
        const fetchImpl = vi.fn().mockResolvedValue(
            new Response(
                JSON.stringify({
                    userId: 'user-1',
                    token: 'session-token',
                    role: 1,
                    name: 'Alice'
                }),
                {
                    status: 200,
                    headers: { 'Content-Type': 'application/json' }
                }
            )
        );

        await walter_sign_in(fetchImpl, 'alice', 'secret');
        walter_sign_out({
            successTitle: 'ok',
            failureTitle: 'fail',
            subtitleSuccess: () => 'ok',
            subtitleFailure: () => 'fail'
        });

        expect(getAccessToken()).toBeUndefined();
        expect(localStorage.getItem('auth-state')).toBeNull();
        expect(addToastMock).toHaveBeenCalledWith(
            expect.objectContaining({ successTitle: 'ok' }),
            true,
            'Abmelden erfolgreich'
        );
    });
});
