import { beforeEach, describe, expect, it, vi } from 'vitest';

const walterPostMock = vi.fn();
const addToastMock = vi.fn();

vi.mock('$walter/services/requests', () => ({
    walter_post: walterPostMock
}));

vi.mock('$walter/store', () => ({
    addToast: addToastMock
}));

describe('WalterDataWrapper helper', () => {
    beforeEach(() => {
        walterPostMock.mockReset();
        addToastMock.mockReset();
    });

    it('posts data, returns parsed payload and reports success toast state', async () => {
        walterPostMock.mockResolvedValue(
            new Response(JSON.stringify({ id: 3, status: 'ok' }), {
                status: 200,
                headers: { 'Content-Type': 'application/json' }
            })
        );

        const { handle_save } = await import('./WalterDataWrapper');
        const parsed = await handle_save('/api/test', { title: 'Demo' }, 'Eintrag');

        expect(walterPostMock).toHaveBeenCalledWith('/api/test', {
            title: 'Demo'
        });
        expect(parsed).toEqual({ id: 3, status: 'ok' });
        expect(addToastMock).toHaveBeenCalledWith(
            expect.anything(),
            true,
            { id: 3, status: 'ok' }
        );
    });

    it('reports failure toast state for non-200 responses', async () => {
        walterPostMock.mockResolvedValue(
            new Response(JSON.stringify('Fehler'), {
                status: 500,
                headers: { 'Content-Type': 'application/json' }
            })
        );

        const { handle_save } = await import('./WalterDataWrapper');
        await handle_save('/api/test', { title: 'Demo' }, 'Eintrag');

        expect(addToastMock).toHaveBeenCalledWith(
            expect.anything(),
            false,
            'Fehler'
        );
    });
});