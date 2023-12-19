import { fileURL } from '$walter/services/files';
import { WalterFileHandle } from './WalterFileHandle';

export class WalterFileWrapper {
    handles: WalterFileHandle[] = [];

    constructor(public fetchImpl: typeof fetch) {}

    // Always register for the next to last position; the last is always the stack.
    register(name: string, fileURL: string) {
        const handle = new WalterFileHandle(name, fileURL, this.fetchImpl);
        this.handles.splice(this.handles.length - 1, 0, handle);
    }

    registerStack() {
        this.register('Ablagestapel', fileURL.stack);
    }
}
