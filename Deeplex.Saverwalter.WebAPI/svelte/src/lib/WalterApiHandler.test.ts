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

    it('unwraps a paged {items, totalCount} envelope and maps the items', async () => {
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

        walterGetMock.mockResolvedValue({
            items: [{ value: 'one' }, { value: 'two' }],
            totalCount: 17
        });

        const result = await DemoEntry.GetPaged<DemoEntry>(fetch);

        expect(walterGetMock).toHaveBeenCalledWith('/api/demo', fetch);
        expect(result.totalCount).toBe(17);
        expect(result.items.map((e) => e.value)).toEqual(['ONE', 'TWO']);
    });

    it('builds the query string from paging params', async () => {
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

        walterGetMock.mockResolvedValue({ items: [], totalCount: 0 });

        await DemoEntry.GetPaged<DemoEntry>(fetch, {
            search: 'foo',
            sortBy: 'name',
            sortDir: 'desc',
            skip: 20,
            take: 10
        });

        const calledUrl = walterGetMock.mock.calls[0][0] as string;
        const [base, query] = calledUrl.split('?');
        expect(base).toBe('/api/demo');
        const qs = new URLSearchParams(query);
        expect(qs.get('search')).toBe('foo');
        expect(qs.get('sortBy')).toBe('name');
        expect(qs.get('sortDir')).toBe('desc');
        expect(qs.get('skip')).toBe('20');
        expect(qs.get('take')).toBe('10');
    });

    it('rejects a non-paged response shape', async () => {
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

        // A plain array (the pre-migration shape) is no longer accepted.
        walterGetMock.mockResolvedValue([{ value: 'one' }]);

        await expect(DemoEntry.GetPaged<DemoEntry>(fetch)).rejects.toThrow(
            'Expected paged response with items and totalCount.'
        );
    });
});
