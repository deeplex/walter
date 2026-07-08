import { expect, test, type APIRequestContext } from '@playwright/test';
import { authHeader, signInApi } from './auth';
import {
    entitySpecs,
    getCountAs,
    getFullListAs,
    type ListEntry
} from './entities';

/**
 * Data-driven permission/visibility matrix covering every API collection.
 *
 * For each entity it asserts the invariants of the role model:
 *   - scoped collections never leak rows outside a user's managed scope,
 *   - non-privileged roles (viewer/limited) see their rows read-only,
 *   - a privileged role (manager, Vollmacht) can edit,
 *   - detail reads are denied (403) outside scope where the API enforces it,
 *   - global collections are identical for every role,
 *   - admin-only collections reject every non-admin role.
 *
 * The dev roles (see credentials.ts): admin.dev (Admin, global access),
 * owner.dev (Owner/Eigentuemer), manager.dev (Vollmacht), viewer.dev and
 * limited.dev (Keine — read-only).
 */

// Some collections (transaktionen) are large and have per-row permission
// computation; each test fetches the full list for several roles, so allow
// generous time.
test.describe.configure({ timeout: 120_000 });

const NON_PRIVILEGED = ['viewer.dev', 'limited.dev'] as const;

function idSet(rows: ListEntry[]): Set<number> {
    return new Set(rows.map((r) => r.id));
}

for (const entity of entitySpecs) {
    test.describe(`permissions: ${entity.name}`, () => {
        if (entity.visibility === 'adminOnly') {
            test('admin can list, every other role is denied', async ({
                request
            }) => {
                const adminRows = await getFullListAs(
                    request,
                    'admin.dev',
                    entity.listUrl
                );
                expect(adminRows.length).toBeGreaterThan(0);

                for (const username of [
                    'owner.dev',
                    'manager.dev',
                    ...NON_PRIVILEGED
                ]) {
                    const login = await signInApi(request, username);
                    const response = await request.get(entity.listUrl, {
                        headers: authHeader(login.token)
                    });
                    expect(
                        response.status(),
                        `${username} must be denied ${entity.listUrl}`
                    ).toBe(403);
                }
            });
            return;
        }

        if (entity.visibility === 'global') {
            test('every role sees the same full collection', async ({
                request
            }) => {
                const adminRows = await getFullListAs(
                    request,
                    'admin.dev',
                    entity.listUrl
                );
                expect(adminRows.length).toBeGreaterThan(0);
                const adminIds = idSet(adminRows);

                for (const username of [
                    'owner.dev',
                    'manager.dev',
                    ...NON_PRIVILEGED
                ]) {
                    const rows = await getFullListAs(
                        request,
                        username,
                        entity.listUrl
                    );
                    expect(
                        rows.length,
                        `${username} should see all ${entity.name}`
                    ).toBe(adminRows.length);
                    for (const row of rows) {
                        expect(adminIds.has(row.id)).toBeTruthy();
                    }
                }
            });

            if (
                entity.globalWriteRequiresManagement &&
                entity.detailUrlForWrite
            ) {
                test('is readable by all but only writable with management authority', async ({
                    request
                }) => {
                    // Read-only/guest roles: rows are non-editable and a write is denied.
                    for (const username of NON_PRIVILEGED) {
                        const rows = await getFullListAs(
                            request,
                            username,
                            entity.listUrl
                        );
                        for (const row of rows) {
                            if (row.permissions) {
                                expect(
                                    row.permissions.update,
                                    `${username} must not edit ${entity.name} #${row.id}`
                                ).toBeFalsy();
                                expect(row.permissions.remove).toBeFalsy();
                            }
                        }

                        // A concrete write attempt must be rejected (sending the
                        // unchanged detail body back; 403 means nothing changes).
                        const login = await signInApi(request, username);
                        const target = rows[0];
                        expect(target).toBeTruthy();
                        const detailUrl = entity.detailUrlForWrite!(target.id);
                        const current = await request.get(detailUrl, {
                            headers: authHeader(login.token)
                        });
                        expect(current.ok()).toBeTruthy();
                        const put = await request.put(detailUrl, {
                            headers: authHeader(login.token),
                            data: await current.json()
                        });
                        expect(
                            put.status(),
                            `${username} must not be able to PUT ${entity.name}`
                        ).toBe(403);
                    }

                    // Management roles see rows as editable.
                    for (const username of ['admin.dev', 'manager.dev']) {
                        const rows = await getFullListAs(
                            request,
                            username,
                            entity.listUrl
                        );
                        expect(
                            rows.some((r) => r.permissions?.update),
                            `${username} should be able to edit some ${entity.name}`
                        ).toBeTruthy();
                    }
                });
            }
            return;
        }

        // ---- scoped collections ------------------------------------------

        async function adminRows(api: APIRequestContext): Promise<ListEntry[]> {
            return getFullListAs(api, 'admin.dev', entity.listUrl);
        }

        test('admin (global access) can see the collection', async ({
            request
        }) => {
            const rows = await adminRows(request);
            if (entity.mayBeEmpty) {
                expect(Array.isArray(rows)).toBeTruthy();
            } else {
                expect(rows.length).toBeGreaterThan(0);
            }
        });

        if (entity.large) {
            // Full id-subset would mean fetching every row for every role; for a
            // large collection that is too slow. Admin (global) sees everything,
            // so "no over-visibility" reduces to narrowing: no role sees more
            // rows than admin. Per-row enforcement is covered by the detail-403
            // test below.
            test('no role sees more rows than the admin (global) scope', async ({
                request
            }) => {
                const adminCount = await getCountAs(
                    request,
                    'admin.dev',
                    entity.listUrl
                );
                for (const username of [
                    'owner.dev',
                    'manager.dev',
                    ...NON_PRIVILEGED
                ]) {
                    const count = await getCountAs(
                        request,
                        username,
                        entity.listUrl
                    );
                    expect(
                        count,
                        `${username} should not see more ${entity.name} than admin`
                    ).toBeLessThanOrEqual(adminCount);
                }
            });
        } else {
            test('no role sees rows outside the admin (global) scope', async ({
                request
            }) => {
                const adminIds = idSet(await adminRows(request));
                for (const username of [
                    'owner.dev',
                    'manager.dev',
                    ...NON_PRIVILEGED
                ]) {
                    const rows = await getFullListAs(
                        request,
                        username,
                        entity.listUrl
                    );
                    for (const row of rows) {
                        expect(
                            adminIds.has(row.id),
                            `${username} sees ${entity.name} #${row.id} that admin cannot`
                        ).toBeTruthy();
                    }
                }
            });
        }

        if (entity.readonlyForNonPrivileged) {
            test('viewer and limited see their rows as read-only', async ({
                request
            }) => {
                let sawAny = false;
                for (const username of NON_PRIVILEGED) {
                    const rows = await getFullListAs(
                        request,
                        username,
                        entity.listUrl
                    );
                    for (const row of rows) {
                        if (!row.permissions) continue;
                        sawAny = true;
                        expect(
                            row.permissions.update,
                            `${username} must not update ${entity.name} #${row.id}`
                        ).toBeFalsy();
                        expect(row.permissions.remove).toBeFalsy();
                    }
                }
                expect(
                    sawAny,
                    `expected ${entity.name} to expose permission flags on some visible row`
                ).toBeTruthy();
            });
        }

        if (entity.privilegedCanEdit) {
            test('a Vollmacht role (manager) can edit at least one row', async ({
                request
            }) => {
                const rows = await getFullListAs(
                    request,
                    'manager.dev',
                    entity.listUrl
                );
                const editable = rows.filter((r) => r.permissions?.update);
                expect(
                    editable.length,
                    `manager should be able to edit some ${entity.name}`
                ).toBeGreaterThan(0);
            });
        }

        if (entity.enforcesDetailReadScope && entity.detailUrl) {
            test('detail reads are denied outside the viewer scope', async ({
                request
            }) => {
                const admin = await adminRows(request);
                const viewer = await getFullListAs(
                    request,
                    'viewer.dev',
                    entity.listUrl
                );
                const viewerIds = idSet(viewer);
                const forbidden = admin.find((r) => !viewerIds.has(r.id));
                const allowed = viewer[0];

                expect(
                    forbidden,
                    `expected an ${entity.name} outside the viewer scope`
                ).toBeTruthy();
                expect(
                    allowed,
                    `expected the viewer to see at least one ${entity.name}`
                ).toBeTruthy();

                const viewerLogin = await signInApi(request, 'viewer.dev');
                const forbiddenResp = await request.get(
                    entity.detailUrl!(forbidden!.id),
                    { headers: authHeader(viewerLogin.token) }
                );
                expect(forbiddenResp.status()).toBe(403);

                const allowedResp = await request.get(
                    entity.detailUrl!(allowed!.id),
                    { headers: authHeader(viewerLogin.token) }
                );
                expect(allowedResp.status()).toBe(200);
            });
        }
    });
}
