import { vi } from 'vitest';

/**
 * Returns a `fetch` stub suitable for component tests. Many detail/list form
 * components fetch their combobox/select options on mount via `fetchImpl`.
 * Without a real implementation those calls reject and surface as unhandled
 * rejections (which fail the whole Vitest run). This stub resolves every
 * request with an empty, well-formed payload:
 *
 *   - `/api/selection/*` endpoints return a plain `[]` (array shape),
 *   - everything else returns the paged `{ items: [], totalCount: 0 }` envelope.
 *
 * That covers both `WalterApiHandler.GetAll` and `GetPaged` consumers so no
 * parser throws.
 */
export function createMockFetch(): typeof fetch {
    return vi.fn(async (input: RequestInfo | URL) => {
        const url =
            typeof input === 'string'
                ? input
                : input instanceof URL
                  ? input.toString()
                  : input.url;

        const body = /\/selection\//.test(url)
            ? '[]'
            : '{"items":[],"totalCount":0}';

        return new Response(body, {
            status: 200,
            headers: { 'Content-Type': 'application/json' }
        });
    }) as unknown as typeof fetch;
}
