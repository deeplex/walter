import type { WalterSelectionEntry, WalterToastContent } from '$WalterLib';
import { addToast } from '$WalterStore';
import { goto } from '$app/navigation';
import { getAccessToken } from './auth';

export const walter_selection = {
  betriebskostentypen(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/betriebskostentypen', f);
  },
  umlagen(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/umlagen', f);
  },
  umlagen_verbrauch(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/umlagen_verbrauch', f);
  },
  kontakte(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/kontakte', f);
  },
  juristischePersonen(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/juristischepersonen', f);
  },
  wohnungen(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/wohnungen', f);
  },
  umlageschluessel(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/umlageschluessel', f);
  },
  zaehler(f: typeof fetch): Promise<WalterSelectionEntry[]> {
    return walter_get('/api/selection/zaehler', f);
  },
  zaehlertypen(f: typeof fetch): Promise<WalterSelectionEntry[]> {
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

export const walter_get = (url: string, f: typeof fetch): Promise<any> =>
  walter_fetch(f, url, { method: 'GET' }).then((e) => e.json());

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

export async function walter_post(url: string, body: unknown): Promise<Response> {
  const response = await walter_fetch(fetch, url, {
    method: 'POST',
    body: JSON.stringify(body)
  });

  return response;
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
