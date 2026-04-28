import { expect, request, test, type APIRequestContext, type Page } from '@playwright/test';
import { authHeader, devPassword } from './credentials';

const uiBaseUrl = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5173';
const apiBaseUrl = process.env.PLAYWRIGHT_API_BASE_URL ?? 'http://localhost:5254';

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

async function withFreshApiContext<T>(
    baseURL: string,
    run: (api: APIRequestContext) => Promise<T>
): Promise<T> {
    const api = await request.newContext({ baseURL });

    try {
        return await run(api);
    } finally {
        await api.dispose();
    }
}

async function authenticatePage(page: Page, username: string): Promise<AuthState> {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('auth-state'));
    await page.goto('/login');
    await page.getByLabel('Nutzername').fill(username);
    await page.getByLabel('Passwort').fill(devPassword);
    await page.getByRole('button', { name: 'Anmelden' }).click();

    await expect(page.getByText('Anmeldung fehlgeschlagen', { exact: true })).toHaveCount(0);
    await expect
        .poll(async () => page.evaluate(() => window.localStorage.getItem('auth-state')), {
            timeout: 30000
        })
        .not.toBeNull();
    await expect(page).toHaveURL(/\/(?!login$)/, { timeout: 30000 });

    const authStateRaw = await page.evaluate(() => window.localStorage.getItem('auth-state'));
    expect(authStateRaw).toBeTruthy();

    return JSON.parse(authStateRaw!) as AuthState;
}

async function expectBlocked(page: Page): Promise<void> {
    if (/\/login$/.test(page.url())) {
        await expect(page).toHaveURL(/\/login$/);
        return;
    }

    await expect(page.getByText('Fehler', { exact: true })).toBeVisible();
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

test('viewer can open allowed wohnung detail but not forbidden wohnung detail', async ({ page }) => {
    const adminRows = await withFreshApiContext(apiBaseUrl, (api) => getWohnungen(api, 'admin.dev'));
    const viewerRows = await withFreshApiContext(apiBaseUrl, (api) => getWohnungen(api, 'viewer.dev'));

    const allowedId = viewerRows[0]?.id;
    const forbiddenId = adminRows.find(
        (row) => !viewerRows.some((visibleRow) => visibleRow.id === row.id)
    )?.id;

    expect(allowedId).toBeTruthy();
    expect(forbiddenId).toBeTruthy();

    await authenticatePage(page, 'viewer.dev');

    await page.goto(`/wohnungen/${allowedId}`);
    await expect(page).toHaveURL(new RegExp(`/wohnungen/${allowedId}$`));
    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);

    await page.goto(`/wohnungen/${forbiddenId}`);
    await expectBlocked(page);
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
        await expectBlocked(page);
        await page.evaluate(() => window.localStorage.clear());
    }
});

test('admin can open account detail and account creation pages', async ({ page }) => {
    const accounts = await withFreshApiContext(apiBaseUrl, (api) => getAccounts(api, 'admin.dev'));
    const targetAccount = accounts.find((entry) => entry.username === 'viewer.dev');

    expect(targetAccount?.id).toBeTruthy();

    await authenticatePage(page, 'admin.dev');

    await page.goto('/accounts/new');
    await expect(page).toHaveURL(/\/accounts\/new$/);
    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);

    await page.goto(`/accounts/${targetAccount!.id}`);
    await expect(page).toHaveURL(new RegExp(`/accounts/${targetAccount!.id}$`));
    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);
});

test('non-admin users are blocked from account management pages', async ({ page }) => {
    const accounts = await withFreshApiContext(apiBaseUrl, (api) => getAccounts(api, 'admin.dev'));
    const targetAccount = accounts.find((entry) => entry.username === 'viewer.dev');

    expect(targetAccount?.id).toBeTruthy();

    for (const username of ['owner.dev', 'manager.dev', 'viewer.dev', 'limited.dev']) {
        await authenticatePage(page, username);

        await page.goto('/accounts/new');
        await expectBlocked(page);

        await page.goto(`/accounts/${targetAccount!.id}`);
        await expectBlocked(page);

        await page.evaluate(() => window.localStorage.clear());
    }
});