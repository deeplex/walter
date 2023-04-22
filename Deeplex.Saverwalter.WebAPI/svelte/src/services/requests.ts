import type { WalterSelectionEntry, WalterToastContent } from '$WalterLib';
import { addToast } from '$WalterStore';
import { goto } from '$app/navigation';
import { getAccessToken } from './auth';

export const walter_selection = {
  betriebskostentypen(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/betriebskostentypen', f);
  },
  umlagen(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/umlagen', f);
  },
  kontakte(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/kontakte', f);
  },
  juristischePersonen(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/juristischepersonen', f);
  },
  wohnungen(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/wohnungen', f);
  },
  umlageschluessel(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/umlageschluessel', f);
  },
  zaehler(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/zaehler', f);
  },
  zaehlertypen(f: fetchType): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/zaehlertypen', f);
  }
};

export async function walter_fetch(
  fetchImpl: typeof fetch,
  url: string,
  init: RequestInit = {}
): Promise<Response> {
  const headers = new Headers(init.headers);
  headers.set('Content-Type', 'application/json');
  const accessToken = getAccessToken();
  if (accessToken != null) {
    headers.set('Authorization', `X-WalterToken ${accessToken}`);
  }

  init.headers = headers;
  const response = await fetchImpl(url, init);
  if (response.status === 401) {
    await goto('/login');
    throw new Error('Unauthorized access. Redirecting to login page.');
  }
  return response;
}

// =================================== GET ====================================

// Get fetch from svelte:
type fetchType = (
  input: RequestInfo | URL,
  init?: RequestInit | undefined
) => Promise<Response>;
export const walter_get = (url: string, fetch: fetchType): Promise<any> =>
  walter_fetch(fetch, url, { method: 'GET' }).then((e) => e.json());

// =================================== PUT ===================================

export const walter_put = (
  url: string,
  body: any,
  toast?: WalterToastContent
) =>
  walter_fetch(fetch, url, { method: 'PUT', body: JSON.stringify(body) }).then(
    (e) => finishPut(e, toast)
  );

async function finishPut(e: Response, toast?: WalterToastContent) {
  const j = await e.json();

  toast && addToast(toast, e.status === 200, j);

  return j;
}

// =================================== POST ====================================

export async function walter_post<
  Result extends object = Record<string, string>
>(url: string, body: any, toast?: WalterToastContent): Promise<Result> {
  const response = await walter_fetch(fetch, url, {
    method: 'POST',
    body: JSON.stringify(body)
  });
  const parsed = await response.json();

  toast && addToast(toast, response.ok, parsed);
  return parsed;
}

// =================================== DELETE =================================

export const walter_delete = (url: string, toast?: WalterToastContent) =>
  walter_fetch(fetch, url, { method: 'DELETE' }).then((e) =>
    finishDelete(e, toast)
  );

function finishDelete(e: Response, toast?: WalterToastContent) {
  toast && addToast(toast, e.status === 200);
  return e;
}
