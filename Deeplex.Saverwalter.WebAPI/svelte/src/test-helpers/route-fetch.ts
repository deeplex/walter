import { vi } from 'vitest';

type Antwort = unknown | { status: number; body: string };

/**
 * `fetch`-Ersatz, der je URL eine feste Antwort liefert (JSON, oder `{ status, body }` für
 * Fehler). Auswahllisten (`/api/selection/…`) sind leer, sofern nicht angegeben; andere
 * unbekannte URLs antworten mit 404. Die Aufrufe lassen sich über `.mock.calls` prüfen.
 */
export function createRouteFetch(routes: Record<string, Antwort>) {
    return vi.fn(async (input: RequestInfo | URL) => {
        const url =
            typeof input === 'string'
                ? input
                : input instanceof URL
                  ? input.toString()
                  : input.url;
        if (!(url in routes) && url.startsWith('/api/selection/'))
            return new Response('[]', { status: 200 });
        if (!(url in routes))
            return new Response('nicht gefunden', { status: 404 });
        const antwort = routes[url];
        if (
            antwort &&
            typeof antwort === 'object' &&
            'status' in antwort &&
            'body' in antwort
        ) {
            const fehler = antwort as { status: number; body: string };
            return new Response(fehler.body, { status: fehler.status });
        }
        return new Response(JSON.stringify(antwort), { status: 200 });
    }) as unknown as typeof fetch & ReturnType<typeof vi.fn>;
}
