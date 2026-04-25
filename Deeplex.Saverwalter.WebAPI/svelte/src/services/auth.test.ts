import { beforeEach, describe, expect, it, vi } from 'vitest';
const addToastMock = vi.fn();

vi.mock('$app/environment', () => ({
    browser: true
}));

vi.mock('$walter/store', () => ({
    addToast: addToastMock
}));

describe('auth service', () => {
    beforeEach(() => {
        vi.resetModules();
        addToastMock.mockReset();
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
        expect(addToastMock).toHaveBeenCalled();
    });
});
