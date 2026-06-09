import { expect, type APIRequestContext } from '@playwright/test';
import { authHeader, signInApi } from './auth';

/**
 * Shape of a paged list response. The backend migrated most list endpoints to
 * return `{ items, totalCount }` (consumed by `WalterApiHandler.GetPaged` in the
 * frontend), while a handful of small lists still return a plain array
 * (consumed by `GetAll`). Tests should not care which shape an endpoint uses,
 * so everything funnels through {@link unwrapList}.
 */
export type PagedResponse<T> = { items: T[]; totalCount: number };

export function isPaged<T>(body: unknown): body is PagedResponse<T> {
    return (
        typeof body === 'object' &&
        body !== null &&
        'items' in body &&
        'totalCount' in body &&
        Array.isArray((body as PagedResponse<T>).items)
    );
}

/** Normalises a list response into a plain array regardless of paging shape. */
export function unwrapList<T>(body: unknown): T[] {
    if (Array.isArray(body)) {
        return body as T[];
    }
    if (isPaged<T>(body)) {
        return body.items;
    }
    throw new Error(
        `Expected an array or { items, totalCount } list response, got: ${JSON.stringify(
            body
        ).slice(0, 200)}`
    );
}

/** GETs a list endpoint with the given token and returns its entries as an array. */
export async function getList<T>(
    api: APIRequestContext,
    token: string,
    url: string
): Promise<T[]> {
    const response = await api.get(url, { headers: authHeader(token) });
    expect(response.ok(), `GET ${url} should succeed`).toBeTruthy();
    return unwrapList<T>(await response.json());
}

/** Signs in as `username`, GETs the list endpoint, and returns its entries. */
export async function getListAs<T>(
    api: APIRequestContext,
    username: string,
    url: string
): Promise<T[]> {
    const login = await signInApi(api, username);
    return getList<T>(api, login.token, url);
}

export type EntityPermissions = {
    read: boolean;
    update: boolean;
    remove: boolean;
};
