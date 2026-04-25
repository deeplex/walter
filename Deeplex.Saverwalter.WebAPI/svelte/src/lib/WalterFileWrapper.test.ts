import { beforeEach, describe, expect, it, vi } from 'vitest';

const handleConstructorMock = vi.fn();

vi.mock('$walter/services/files', () => ({
    fileURL: {
        stack: '/api/user/files'
    }
}));

vi.mock('./WalterFileHandle', () => ({
    WalterFileHandle: class WalterFileHandleMock {
        constructor(
            public name: string,
            public fileURL: string,
            public fetchImpl: typeof fetch
        ) {
            handleConstructorMock(name, fileURL, fetchImpl);
        }
    }
}));

describe('WalterFileWrapper', () => {
    beforeEach(() => {
        handleConstructorMock.mockReset();
    });

    it('registers file handles before the stack entry and clears them again', async () => {
        const { WalterFileWrapper } = await import('./WalterFileWrapper');
        const fetchImpl = vi.fn() as unknown as typeof fetch;
        const wrapper = new WalterFileWrapper(fetchImpl);

        wrapper.registerStack();
        wrapper.register('Wohnung', '/api/wohnungen/1/files');
        wrapper.register('Vertrag', '/api/vertraege/2/files');

        expect(wrapper.handles.map((handle) => handle.name)).toEqual([
            'Wohnung',
            'Vertrag',
            'Ablagestapel'
        ]);
        expect(wrapper.handles.map((handle) => handle.fileURL)).toEqual([
            '/api/wohnungen/1/files',
            '/api/vertraege/2/files',
            '/api/user/files'
        ]);
        expect(handleConstructorMock).toHaveBeenCalledTimes(3);

        wrapper.clear();

        expect(wrapper.handles).toEqual([]);
    });
});