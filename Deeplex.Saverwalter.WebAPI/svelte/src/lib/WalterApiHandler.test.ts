import { beforeEach, describe, expect, it, vi } from 'vitest';

const walterGetMock = vi.fn();

vi.mock('$walter/services/requests', () => ({
    walter_get: walterGetMock
}));

describe('WalterApiHandler', () => {
    beforeEach(() => {
        walterGetMock.mockReset();
    });

    it('throws for the abstract default fromJson implementation', async () => {
        const { WalterApiHandler } = await import('./WalterApiHandler');

        expect(() => WalterApiHandler.fromJson({})).toThrow(
            'WalterApiHandler.fromJson not implemented.'
        );
    });

    it('maps arrays through GetAll using the subclass fromJson implementation', async () => {
        const { WalterApiHandler } = await import('./WalterApiHandler');

        class DemoEntry extends WalterApiHandler {
            protected static ApiURL = '/api/demo';

            constructor(public value: string) {
                super();
            }

            static fromJson(json: { value: string }) {
                return new DemoEntry(json.value.toUpperCase());
            }
        }

        walterGetMock.mockResolvedValue([{ value: 'one' }, { value: 'two' }]);

        const result = await DemoEntry.GetAll<DemoEntry>(fetch);

        expect(walterGetMock).toHaveBeenCalledWith('/api/demo', fetch);
        expect(result.map((entry) => entry.value)).toEqual(['ONE', 'TWO']);
    });

    it('rejects non-array GetAll responses and supports GetOne', async () => {
        const { WalterApiHandler } = await import('./WalterApiHandler');

        class DemoEntry extends WalterApiHandler {
            protected static ApiURL = '/api/demo';

            constructor(public value: string) {
                super();
            }

            static fromJson(json: { value: string }) {
                return new DemoEntry(json.value);
            }
        }

        walterGetMock.mockResolvedValueOnce({ value: 'wrong-shape' });

        await expect(DemoEntry.GetAll<DemoEntry>(fetch)).rejects.toThrow(
            'Expected response to be an array.'
        );

        walterGetMock.mockResolvedValueOnce({ value: 'single' });

        const result = await DemoEntry.GetOne<DemoEntry>('42', fetch);

        expect(walterGetMock).toHaveBeenLastCalledWith('/api/demo/42', fetch);
        expect(result.value).toBe('single');
    });
});
