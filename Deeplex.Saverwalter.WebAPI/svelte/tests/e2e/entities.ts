import type { APIRequestContext } from '@playwright/test';
import { authHeader, signInApi } from './auth';
import { unwrapList, type EntityPermissions } from './api';

export type ListEntry = {
    id: number;
    permissions?: EntityPermissions;
    [key: string]: unknown;
};

/**
 * Visibility model of an API collection, used to drive the data-driven
 * permission matrix in `entity-authz.spec.ts`.
 *
 *   - `scoped`    – rows are filtered to the wohnungen a user manages,
 *   - `global`    – every authenticated user sees the full collection,
 *   - `adminOnly` – only the Admin role may access the collection.
 */
export type Visibility = 'scoped' | 'global' | 'adminOnly';

export type EntitySpec = {
    /** Human-readable name used in test titles. */
    name: string;
    /** Collection endpoint, e.g. `/api/wohnungen`. */
    listUrl: string;
    visibility: Visibility;
    /** Builds the detail endpoint for an id; null when there is none to probe. */
    detailUrl?: (id: number) => string;
    /**
     * Scoped only: a GET on a detail outside the user's scope returns 403.
     * Some collections (e.g. adressen) narrow the list but answer detail reads
     * permissively, so this is false for them.
     */
    enforcesDetailReadScope?: boolean;
    /**
     * Scoped only: viewer/limited (no Vollmacht) must see every visible row as
     * read-only. False for zaehler, where meters without a Wohnung are editable
     * by anyone.
     */
    readonlyForNonPrivileged?: boolean;
    /** Scoped only: the Vollmacht role (manager) can edit at least one row. */
    privilegedCanEdit?: boolean;
    /** The seed may contain no rows for this collection (e.g. garagen). */
    mayBeEmpty?: boolean;
    /**
     * Very large collection (e.g. transaktionen): fetching every row for every
     * role is too slow, so the "no over-visibility" check uses cheap
     * totalCount-narrowing instead of a full id-subset scan. Per-row enforcement
     * is still covered by the detail-403 test.
     */
    large?: boolean;
    /**
     * Global only: the collection is readable by everyone, but writing requires
     * management authority — so read-only/guest roles must see rows as
     * non-editable (e.g. the shared Kontakte address book).
     */
    globalWriteRequiresManagement?: boolean;
    /** A detail endpoint to attempt a (denied) write against, for write-gating checks. */
    detailUrlForWrite?: (id: number) => string;
};

/**
 * The full collection surface. Behaviour encoded here was verified empirically
 * against the seeded dev database (see the permission matrix test).
 */
export const entitySpecs: EntitySpec[] = [
    {
        name: 'wohnungen',
        listUrl: '/api/wohnungen',
        visibility: 'scoped',
        detailUrl: (id) => `/api/wohnungen/${id}`,
        enforcesDetailReadScope: true,
        readonlyForNonPrivileged: true,
        privilegedCanEdit: true
    },
    {
        name: 'vertraege',
        listUrl: '/api/vertraege',
        visibility: 'scoped',
        detailUrl: (id) => `/api/vertraege/${id}`,
        enforcesDetailReadScope: true,
        readonlyForNonPrivileged: true,
        privilegedCanEdit: true
    },
    {
        name: 'umlagen',
        listUrl: '/api/umlagen',
        visibility: 'scoped',
        detailUrl: (id) => `/api/umlagen/${id}`,
        enforcesDetailReadScope: true,
        readonlyForNonPrivileged: true,
        privilegedCanEdit: true
    },
    {
        name: 'umlagetypen',
        listUrl: '/api/umlagetypen',
        visibility: 'scoped',
        detailUrl: (id) => `/api/umlagetypen/${id}`,
        // owner relation of its own — it's visible if unused anywhere, or if any
        // single Umlage usage (across all owners) touches the caller's scope.
        // That makes "an instance outside the viewer's scope" structurally
        // unlikely to exist, so the detail-read-scope test doesn't apply here.
        enforcesDetailReadScope: false,
        readonlyForNonPrivileged: true,
        privilegedCanEdit: true
    },
    {
        name: 'transaktionen',
        listUrl: '/api/transaktionen',
        visibility: 'scoped',
        detailUrl: (id) => `/api/transaktionen/${id}`,
        enforcesDetailReadScope: true,
        readonlyForNonPrivileged: true,
        privilegedCanEdit: true,
        large: true
    },
    {
        name: 'zaehler',
        listUrl: '/api/zaehler',
        visibility: 'scoped',
        detailUrl: (id) => `/api/zaehler/${id}`,
        enforcesDetailReadScope: true,
        // After the auth fix, meters without a Wohnung also require management
        // authority to edit, so viewers/limited are read-only throughout.
        readonlyForNonPrivileged: true,
        privilegedCanEdit: true
    },
    {
        name: 'adressen',
        listUrl: '/api/adressen',
        visibility: 'scoped',
        detailUrl: (id) => `/api/adressen/${id}`,
        // The list narrows by managed wohnungen, but detail reads are permissive.
        enforcesDetailReadScope: false,
        readonlyForNonPrivileged: true
    },
    {
        name: 'garagen',
        listUrl: '/api/garagen',
        visibility: 'scoped',
        readonlyForNonPrivileged: false,
        mayBeEmpty: true
    },
    {
        name: 'kontakte',
        listUrl: '/api/kontakte',
        visibility: 'global',
        globalWriteRequiresManagement: true,
        detailUrlForWrite: (id) => `/api/kontakte/${id}`
    },
    {
        name: 'accounts',
        listUrl: '/api/accounts',
        visibility: 'adminOnly'
    }
];

/** Fetches an entire collection (defeating server-side paging) for `username`. */
export async function getFullListAs(
    api: APIRequestContext,
    username: string,
    listUrl: string
): Promise<ListEntry[]> {
    const login = await signInApi(api, username);
    const sep = listUrl.includes('?') ? '&' : '?';
    const response = await api.get(`${listUrl}${sep}take=100000`, {
        headers: authHeader(login.token)
    });
    if (!response.ok()) {
        throw new Error(
            `GET ${listUrl} for ${username} failed with ${response.status()}`
        );
    }
    return unwrapList<ListEntry>(await response.json());
}

/**
 * Cheap row count for a collection: the paged `totalCount` when present,
 * otherwise the array length. Avoids fetching every row for large collections.
 */
export async function getCountAs(
    api: APIRequestContext,
    username: string,
    listUrl: string
): Promise<number> {
    const login = await signInApi(api, username);
    const sep = listUrl.includes('?') ? '&' : '?';
    const response = await api.get(`${listUrl}${sep}take=1`, {
        headers: authHeader(login.token)
    });
    if (!response.ok()) {
        throw new Error(
            `GET ${listUrl} for ${username} failed with ${response.status()}`
        );
    }
    const body = (await response.json()) as unknown;
    if (Array.isArray(body)) {
        return body.length;
    }
    return (body as { totalCount: number }).totalCount;
}
