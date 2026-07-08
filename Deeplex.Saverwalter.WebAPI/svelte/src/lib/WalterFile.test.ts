import { describe, expect, it } from 'vitest';

import { WalterFile } from './WalterFile';

describe('WalterFile', () => {
    it('creates a WalterFile from a browser File', () => {
        const file = new File(['hello'], 'greeting.txt', {
            type: 'text/plain',
            lastModified: 1234
        });

        const result = WalterFile.fromFile(file, '/api/files/greeting.txt');

        expect(result).toBeInstanceOf(WalterFile);
        expect(result).toMatchObject({
            fileName: 'greeting.txt',
            key: '/api/files/greeting.txt',
            lastModified: 1234,
            size: 5,
            blob: file,
            type: 'text/plain'
        });
    });
});
