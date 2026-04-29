import { expect, test, type Page } from '@playwright/test';
import { devPassword, devUsers } from './credentials';

type RouteExpectation = {
    path: string;
    expectedText: string;
};

const mainRoutes: RouteExpectation[] = [
    { path: '/', expectedText: 'Walter' },
    { path: '/abrechnung', expectedText: 'Wähle einen Vertrag aus' },
    { path: '/kontakte', expectedText: 'Kontakte' },
    { path: '/wohnungen', expectedText: 'Wohnungen' },
    { path: '/vertraege', expectedText: 'Verträge' },
    { path: '/transaktionen', expectedText: 'Transaktionen' },
    {
        path: '/betriebskostenrechnungen',
        expectedText: 'Betriebskostenrechnung'
    },
    { path: '/umlagen', expectedText: 'Umlagen' },
    { path: '/umlagetypen', expectedText: 'Umlagetypen' },
    { path: '/erhaltungsaufwendungen', expectedText: 'Erhaltungsaufwendungen' },
    { path: '/zaehler', expectedText: 'Zähler' },
    { path: '/adressen', expectedText: 'Adressen' },
    { path: '/stack', expectedText: 'Ablagestapel' },
    { path: '/user', expectedText: 'Nutzereinstellungen' }
];

async function signIn(page: Page, username: string) {
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
}

for (const user of devUsers) {
    test(`${user.username} can access all main app pages`, async ({ page }) => {
        await signIn(page, user.username);

        for (const route of mainRoutes) {
            await test.step(`${user.username} opens ${route.path}`, async () => {
                await page.goto(route.path);
                await expect(page).toHaveURL(
                    new RegExp(
                        `${route.path === '/' ? '/$' : `${route.path}$`}`
                    )
                );
                await expect(
                    page.getByText('Fehler', { exact: true })
                ).toHaveCount(0);
                await expect
                    .poll(
                        async () =>
                            (await page.getByRole('banner').textContent()) ??
                            '',
                        { timeout: 30000 }
                    )
                    .toContain(route.expectedText);
            });
        }
    });
}

test('admin-only pages are locked for non-admin users', async ({ page }) => {
    for (const username of [
        'owner.dev',
        'manager.dev',
        'viewer.dev',
        'limited.dev'
    ]) {
        await test.step(`${username} cannot open /admin`, async () => {
            await signIn(page, username);
            await page.goto('/admin');
            await expect(
                page.getByText('Fehler', { exact: true })
            ).toBeVisible();
        });

        await test.step(`${username} cannot open /accounts`, async () => {
            await page.goto('/accounts');
            await expect(
                page.getByText('Fehler', { exact: true })
            ).toBeVisible();
        });
    }
});

test('admin can open admin pages', async ({ page }) => {
    await signIn(page, 'admin.dev');

    await page.goto('/admin');
    await expect(
        page.getByRole('banner').getByText('Adminbereich', { exact: true })
    ).toBeVisible();

    await page.goto('/accounts');
    await expect(
        page
            .getByRole('banner')
            .getByText('Nutzereinstellungen', { exact: true })
    ).toBeVisible();
});
