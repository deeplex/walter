import { expect, test, type Page } from '@playwright/test';
import { devPassword } from './credentials';

async function signIn(page: Page, username: string) {
    await page.goto('/login');
    await page.getByLabel('Nutzername').fill(username);
    await page.getByLabel('Passwort').fill(devPassword);
    await page.getByRole('button', { name: 'Anmelden' }).click();
    await expect(page).not.toHaveURL(/\/login$/);
}

test('admin user can access admin area', async ({ page }) => {
    await signIn(page, 'admin.dev');

    await page.goto('/admin');

    await expect(page.getByText('Adminbereich', { exact: true })).toBeVisible();
    await expect(page.getByText('Nutzeraccounts', { exact: true })).toBeVisible();
});

test('non-admin user receives error view in admin area', async ({ page }) => {
    await signIn(page, 'viewer.dev');

    await page.goto('/admin');

    await expect(page.getByText('Fehler', { exact: true })).toBeVisible();
    await expect(page.getByText('Nutzeraccounts', { exact: true })).toHaveCount(0);
});
