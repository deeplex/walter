import {
    expect,
    request,
    type APIRequestContext,
    type Page
} from '@playwright/test';
import { devPassword } from './credentials';

export type AuthState = {
    userId: string;
    token: string;
    role: number;
    name: string;
};

const baseUrl = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5254';

export function authHeader(token: string): Record<string, string> {
    return { Authorization: `X-WalterToken ${token}` };
}

export async function signInApi(
    api: APIRequestContext,
    username: string
): Promise<AuthState> {
    const response = await api.post('/api/user/sign-in', {
        data: { username, password: devPassword }
    });
    expect(response.ok()).toBeTruthy();
    return (await response.json()) as AuthState;
}

export async function withFreshApiContext<T>(
    run: (api: APIRequestContext) => Promise<T>
): Promise<T> {
    const api = await request.newContext({ baseURL: baseUrl });

    try {
        return await run(api);
    } finally {
        await api.dispose();
    }
}

export async function authenticatePage(
    page: Page,
    username: string
): Promise<AuthState> {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('auth-state'));
    await page.goto('/login');
    await page.getByLabel('Nutzername').fill(username);
    await page.getByLabel('Passwort').fill(devPassword);
    await page.getByRole('button', { name: 'Anmelden' }).click();

    await expect(
        page.getByText('Anmeldung fehlgeschlagen', { exact: true })
    ).toHaveCount(0);
    await expect
        .poll(async () =>
            page.evaluate(() => window.localStorage.getItem('auth-state'))
        )
        .not.toBeNull();
    await expect(page).toHaveURL(/\/(?!login$)/);

    const authStateRaw = await page.evaluate(() =>
        window.localStorage.getItem('auth-state')
    );
    return JSON.parse(authStateRaw!) as AuthState;
}
