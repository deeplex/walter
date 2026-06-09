import { expect, test, type Page } from '@playwright/test';
import { devUsers } from './credentials';
import { authenticatePage } from './auth';

type RouteExpectation = {
    path: string;
    expectedText: string;
};

const mainRoutes: RouteExpectation[] = [
    { path: '/', expectedText: 'Walter' },
    { path: '/abrechnungslauf', expectedText: 'Abrechnungslauf' },
    { path: '/kontakte', expectedText: 'Kontakte' },
    { path: '/wohnungen', expectedText: 'Wohnungen' },
    { path: '/vertraege', expectedText: 'Verträge' },
    { path: '/transaktionen', expectedText: 'Transaktionen' },
    { path: '/umlagen', expectedText: 'Umlagen' },
    { path: '/umlagetypen', expectedText: 'Umlagetypen' },
    { path: '/zaehler', expectedText: 'Zähler' },
    { path: '/adressen', expectedText: 'Adressen' },
    { path: '/stack', expectedText: 'Ablagestapel' },
    { path: '/user', expectedText: 'Nutzereinstellungen' }
];

test.describe.configure({ timeout: 120_000 });

async function visitMainRoutes(page: Page, username: string): Promise<void> {
    for (const route of mainRoutes) {
        await test.step(`${username} opens ${route.path}`, async () => {
            await page.goto(route.path);
            // Some pages append query state (e.g. the dashboard adds
            // `?jahr=YYYY`), so assert on the pathname and ignore any query.
            await expect
                .poll(async () => new URL(page.url()).pathname, {
                    timeout: 20_000
                })
                .toBe(route.path);
            await expect(page.getByText('Fehler', { exact: true })).toHaveCount(
                0
            );
            await expect
                .poll(
                    async () =>
                        (await page.getByRole('banner').textContent()) ?? '',
                    { timeout: 20_000 }
                )
                .toContain(route.expectedText);
        });
    }
}

for (const user of devUsers) {
    test(`${user.username} can access all main app pages`, async ({ page }) => {
        await authenticatePage(page, user.username);
        await visitMainRoutes(page, user.username);
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
            await authenticatePage(page, username);
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
    await authenticatePage(page, 'admin.dev');

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
