import {
    expect,
    test,
    type APIRequestContext,
    type Page
} from '@playwright/test';
import {
    authHeader,
    authenticatePage,
    signInApi,
    withFreshApiContext
} from './auth';

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

test('viewer can open allowed wohnung detail but not forbidden wohnung detail', async ({
    page
}) => {
    const adminRows = await withFreshApiContext((api) =>
        getWohnungen(api, 'admin.dev')
    );
    const viewerRows = await withFreshApiContext((api) =>
        getWohnungen(api, 'viewer.dev')
    );

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

test('non-owner users are blocked from wohnung creation page', async ({
    page
}) => {
    for (const username of ['manager.dev', 'viewer.dev', 'limited.dev']) {
        await authenticatePage(page, username);
        await page.goto('/wohnungen/new');
        await expectBlocked(page);
        await page.evaluate(() => window.localStorage.clear());
    }
});

test('admin can open account detail and account creation pages', async ({
    page
}) => {
    const accounts = await withFreshApiContext((api) =>
        getAccounts(api, 'admin.dev')
    );
    const targetAccount = accounts.find(
        (entry) => entry.username === 'viewer.dev'
    );

    expect(targetAccount?.id).toBeTruthy();

    await authenticatePage(page, 'admin.dev');

    await page.goto('/accounts/new');
    await expect(page).toHaveURL(/\/accounts\/new$/);
    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);

    await page.goto(`/accounts/${targetAccount!.id}`);
    await expect(page).toHaveURL(new RegExp(`/accounts/${targetAccount!.id}$`));
    await expect(page.getByText('Fehler', { exact: true })).toHaveCount(0);
});

test('non-admin users are blocked from account management pages', async ({
    page
}) => {
    const accounts = await withFreshApiContext((api) =>
        getAccounts(api, 'admin.dev')
    );
    const targetAccount = accounts.find(
        (entry) => entry.username === 'viewer.dev'
    );

    expect(targetAccount?.id).toBeTruthy();

    for (const username of [
        'owner.dev',
        'manager.dev',
        'viewer.dev',
        'limited.dev'
    ]) {
        await authenticatePage(page, username);

        await page.goto('/accounts/new');
        await expectBlocked(page);

        await page.goto(`/accounts/${targetAccount!.id}`);
        await expectBlocked(page);

        await page.evaluate(() => window.localStorage.clear());
    }
});
