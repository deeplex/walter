import { expect, test, type APIRequestContext } from '@playwright/test';
import { authHeader, devPassword, devUsers } from './credentials';

const apiBaseUrl =
    process.env.PLAYWRIGHT_API_BASE_URL ?? 'http://localhost:5254';

type LoginResult = {
    token: string;
    role: string;
};

type EntityPermissions = {
    read: boolean;
    update: boolean;
    remove: boolean;
};

type WohnungListEntry = {
    id: number;
    permissions: EntityPermissions;
};

type WohnungDetailEntry = {
    id: number;
    permissions: EntityPermissions;
    [key: string]: unknown;
};

async function signIn(
    api: APIRequestContext,
    username: string
): Promise<LoginResult> {
    const response = await api.post('/api/user/sign-in', {
        data: {
            username,
            password: devPassword
        }
    });

    expect(response.ok()).toBeTruthy();

    const body = (await response.json()) as LoginResult;
    expect(body.token).toBeTruthy();

    return body;
}

async function getWohnungsCount(
    api: APIRequestContext,
    username: string
): Promise<number> {
    return (await getWohnungen(api, username)).length;
}

async function getWohnungen(
    api: APIRequestContext,
    username: string
): Promise<WohnungListEntry[]> {
    const login = await signIn(api, username);
    const response = await api.get('/api/wohnungen', {
        headers: authHeader(login.token)
    });

    expect(response.ok()).toBeTruthy();

    return (await response.json()) as WohnungListEntry[];
}

async function getWohnungDetail(
    api: APIRequestContext,
    username: string,
    id: number
): Promise<{ responseStatus: number; body?: WohnungDetailEntry }> {
    const login = await signIn(api, username);
    const response = await api.get(`/api/wohnungen/${id}`, {
        headers: authHeader(login.token)
    });

    if (!response.ok()) {
        return { responseStatus: response.status() };
    }

    return {
        responseStatus: response.status(),
        body: (await response.json()) as WohnungDetailEntry
    };
}

async function putWohnungDetail(
    api: APIRequestContext,
    username: string,
    entry: WohnungDetailEntry
): Promise<number> {
    const login = await signIn(api, username);
    const response = await api.put(`/api/wohnungen/${entry.id}`, {
        headers: authHeader(login.token),
        data: entry
    });

    return response.status();
}

test.describe('API authentication and authorization matrix', () => {
    test.use({ baseURL: apiBaseUrl });

    test('all seeded users can sign in with shared dev password', async ({
        request
    }) => {
        for (const user of devUsers) {
            const login = await signIn(request, user.username);
            expect(login.role).toBe(user.expectedRole);
        }
    });

    test('accounts endpoint is restricted to admins', async ({ request }) => {
        const adminLogin = await signIn(request, 'admin.dev');
        const adminResponse = await request.get('/api/accounts', {
            headers: authHeader(adminLogin.token)
        });
        expect(adminResponse.ok()).toBeTruthy();

        const ownerLogin = await signIn(request, 'owner.dev');
        const ownerResponse = await request.get('/api/accounts', {
            headers: authHeader(ownerLogin.token)
        });
        expect(ownerResponse.status()).toBe(403);

        const managerLogin = await signIn(request, 'manager.dev');
        const managerResponse = await request.get('/api/accounts', {
            headers: authHeader(managerLogin.token)
        });
        expect(managerResponse.status()).toBe(403);
    });

    test('wohnung list visibility narrows by role assignment', async ({
        request
    }) => {
        const managerCount = await getWohnungsCount(request, 'manager.dev');
        const viewerCount = await getWohnungsCount(request, 'viewer.dev');
        const limitedCount = await getWohnungsCount(request, 'limited.dev');

        expect(managerCount).toBeGreaterThan(0);
        expect(viewerCount).toBeGreaterThan(0);
        expect(limitedCount).toBeGreaterThan(0);

        expect(managerCount).toBeGreaterThan(viewerCount);
        expect(viewerCount).toBeGreaterThan(limitedCount);
    });

    test('wohnung permissions reflect role capabilities', async ({
        request
    }) => {
        const adminRows = await getWohnungen(request, 'admin.dev');
        const ownerRows = await getWohnungen(request, 'owner.dev');
        const managerRows = await getWohnungen(request, 'manager.dev');
        const viewerRows = await getWohnungen(request, 'viewer.dev');
        const limitedRows = await getWohnungen(request, 'limited.dev');

        expect(adminRows.length).toBeGreaterThan(0);
        expect(ownerRows.length).toBeGreaterThan(0);
        expect(managerRows.length).toBeGreaterThan(0);
        expect(viewerRows.length).toBeGreaterThan(0);
        expect(limitedRows.length).toBeGreaterThan(0);

        expect(adminRows.every((row) => row.permissions.update)).toBeTruthy();
        expect(ownerRows.every((row) => row.permissions.update)).toBeTruthy();
        expect(managerRows.every((row) => row.permissions.update)).toBeTruthy();
        expect(
            viewerRows.every(
                (row) => !row.permissions.update && !row.permissions.remove
            )
        ).toBeTruthy();
        expect(
            limitedRows.every(
                (row) => !row.permissions.update && !row.permissions.remove
            )
        ).toBeTruthy();
    });

    test('users cannot read wohnung details outside their visibility scope', async ({
        request
    }) => {
        const adminRows = await getWohnungen(request, 'admin.dev');
        const viewerRows = await getWohnungen(request, 'viewer.dev');
        const limitedRows = await getWohnungen(request, 'limited.dev');

        const viewerAllowedId = viewerRows[0]?.id;
        const limitedAllowedId = limitedRows[0]?.id;
        const viewerForbiddenId = adminRows.find(
            (row) => !viewerRows.some((visibleRow) => visibleRow.id === row.id)
        )?.id;
        const limitedForbiddenId = adminRows.find(
            (row) => !limitedRows.some((visibleRow) => visibleRow.id === row.id)
        )?.id;

        expect(viewerAllowedId).toBeTruthy();
        expect(limitedAllowedId).toBeTruthy();
        expect(viewerForbiddenId).toBeTruthy();
        expect(limitedForbiddenId).toBeTruthy();

        const viewerAllowed = await getWohnungDetail(
            request,
            'viewer.dev',
            viewerAllowedId!
        );
        const viewerForbidden = await getWohnungDetail(
            request,
            'viewer.dev',
            viewerForbiddenId!
        );
        const limitedAllowed = await getWohnungDetail(
            request,
            'limited.dev',
            limitedAllowedId!
        );
        const limitedForbidden = await getWohnungDetail(
            request,
            'limited.dev',
            limitedForbiddenId!
        );

        expect(viewerAllowed.responseStatus).toBe(200);
        expect(limitedAllowed.responseStatus).toBe(200);
        expect(viewerForbidden.responseStatus).toBe(403);
        expect(limitedForbidden.responseStatus).toBe(403);
    });

    test('only users with update rights can save visible wohnung details', async ({
        request
    }) => {
        const managerRows = await getWohnungen(request, 'manager.dev');
        const viewerRows = await getWohnungen(request, 'viewer.dev');

        const managerEditableId = managerRows.find(
            (row) => row.permissions.update
        )?.id;
        const viewerReadonlyId = viewerRows.find(
            (row) => !row.permissions.update
        )?.id;

        expect(managerEditableId).toBeTruthy();
        expect(viewerReadonlyId).toBeTruthy();

        const managerDetail = await getWohnungDetail(
            request,
            'manager.dev',
            managerEditableId!
        );
        const viewerDetail = await getWohnungDetail(
            request,
            'viewer.dev',
            viewerReadonlyId!
        );

        expect(managerDetail.responseStatus).toBe(200);
        expect(viewerDetail.responseStatus).toBe(200);

        const managerPutStatus = await putWohnungDetail(
            request,
            'manager.dev',
            managerDetail.body!
        );
        const viewerPutStatus = await putWohnungDetail(
            request,
            'viewer.dev',
            viewerDetail.body!
        );

        expect(managerPutStatus).toBe(200);
        expect(viewerPutStatus).toBe(403);
    });

    test('owner-only wohnung creation policy is enforced', async ({
        request
    }) => {
        const ownerLogin = await signIn(request, 'owner.dev');
        const ownerResponse = await request.post('/api/wohnungen', {
            headers: authHeader(ownerLogin.token),
            data: {}
        });
        expect(ownerResponse.status()).not.toBe(401);
        expect(ownerResponse.status()).not.toBe(403);

        const managerLogin = await signIn(request, 'manager.dev');
        const managerResponse = await request.post('/api/wohnungen', {
            headers: authHeader(managerLogin.token),
            data: {}
        });
        expect(managerResponse.status()).toBe(403);
    });
});
