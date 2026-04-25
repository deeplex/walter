import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import { authHeader, devPassword } from './credentials';

type AuthState = {
    userId: string;
    token: string;
    role: number;
    name: string;
};

type WohnungListEntry = {
    id: number;
    permissions: {
        read: boolean;
        update: boolean;
        remove: boolean;
    };
};

type AccountEntry = {
    id: string;
    username: string;
};

async function signInApi(
    api: APIRequestContext,
    username: string
): Promise<AuthState> {
    const response = await api.post('/api/user/sign-in', {
        data: {
            username,
            password: devPassword
        }
    });

    expect(response.ok()).toBeTruthy();
    return (await response.json()) as AuthState;
}

async function authenticatePage(page: Page, username: string): Promise<AuthState> {
    const authState = await signInApi(page.request, username);
    await page.addInitScript((state: AuthState) => {
        window.localStorage.setItem('auth-state', JSON.stringify(state));
    }, authState);

    return authState;
}

async function getWohnungen(
    api: APIRequestContext,
    username: string
): Promise<WohnungListEntry[]> {
    const authState = await signInApi(api, username);
    const response = await api.get('/api/wohnungen', {
        headers: authHeader(authState.token)
    });

    expect(response.ok()).toBeTruthy();
    return (await response.json()) as WohnungListEntry[];
}

async function getAccounts(
    api: APIRequestContext,
    username: string
): Promise<AccountEntry[]> {
    const authState = await signInApi(api, username);
    const response = await api.get('/api/accounts', {
        headers: authHeader(authState.token)
    });

    expect(response.ok()).toBeTruthy();
    return (await response.json()) as AccountEntry[];
}

test('viewer can open allowed wohnung detail but not forbidden wohnung detail', async ({ page, request }) => {
    const adminRows = await getWohnungen(request, 'admin.dev');
    const viewerRows = await getWohnungen(request, 'viewer.dev');

    const allowedId = viewerRows[0]?.id;
    const forbiddenId = adminRows.find(
        (row) => !viewerRows.some((visibleRow) => visibleRow.id === row.id)
    )?.id;

    expect(allowedId).toBeTruthy();
    expect(forbiddenId).toBeTruthy();

    await authenticatePage(page, 'viewer.dev');

    await page.goto(`/wohnungen/${allowedId}`);
    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);
    await expect(page.getByText('Verträge', { exact: true })).toBeVisible();

    await page.goto(`/wohnungen/${forbiddenId}`);
    await expect(page.getByText('Fehler', { exact: true })).toBeVisible();
});

test('owner can open wohnung creation page', async ({ page }) => {
    await authenticatePage(page, 'owner.dev');

    await page.goto('/wohnungen/new');

    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);
    await expect(page.getByText('Neue Wohnung', { exact: true })).toBeVisible();
});

test('non-owner users are blocked from wohnung creation page', async ({ page }) => {
    for (const username of ['manager.dev', 'viewer.dev', 'limited.dev']) {
        await authenticatePage(page, username);
        await page.goto('/wohnungen/new');
        await expect(page.getByText('Fehler', { exact: true })).toBeVisible();
        await page.evaluate(() => window.localStorage.clear());
    }
});

test('admin can open account detail and account creation pages', async ({ page, request }) => {
    const accounts = await getAccounts(request, 'admin.dev');
    const targetAccount = accounts.find((entry) => entry.username === 'viewer.dev');

    expect(targetAccount?.id).toBeTruthy();

    await authenticatePage(page, 'admin.dev');

    await page.goto('/accounts/new');
    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);
    await expect(page.getByText('Neuen Nutzer anlegen', { exact: true })).toBeVisible();

    await page.goto(`/accounts/${targetAccount!.id}`);
    await expect(page.getByText('Passwortlink anfordern', { exact: true })).toBeVisible();
});

test('non-admin users are blocked from account management pages', async ({ page, request }) => {
    const accounts = await getAccounts(request, 'admin.dev');
    const targetAccount = accounts.find((entry) => entry.username === 'viewer.dev');

    expect(targetAccount?.id).toBeTruthy();

    for (const username of ['owner.dev', 'manager.dev', 'viewer.dev', 'limited.dev']) {
        await authenticatePage(page, username);

        await page.goto('/accounts/new');
        await expect(page.getByText('Fehler', { exact: true })).toBeVisible();

        await page.goto(`/accounts/${targetAccount!.id}`);
        await expect(page.getByText('Fehler', { exact: true })).toBeVisible();

        await page.evaluate(() => window.localStorage.clear());
    }
});