import { expect, test } from '@playwright/test';
import {
    authHeader,
    authenticatePage,
    signInApi,
    withFreshApiContext
} from './auth';
import { getFullListAs } from './entities';

/**
 * End-to-end CRUD through the real UI, plus permission-gating of the edit
 * affordances. Adresse is used because it has the simplest create/detail form
 * (four text fields) and a /new page, exercising the same WalterSimpleList →
 * WalterHeaderNew/WalterHeaderDetail machinery every entity shares.
 */

async function getAdresseStatus(id: number): Promise<number> {
    return withFreshApiContext(async (api) => {
        const login = await signInApi(api, 'admin.dev');
        const r = await api.get(`/api/adressen/${id}`, {
            headers: authHeader(login.token)
        });
        return r.status();
    });
}

test('admin can create, edit and delete an Adresse through the UI', async ({
    page
}) => {
    const marker = `E2E-${Date.now()}`;

    await authenticatePage(page, 'admin.dev');

    // ---- create -------------------------------------------------------
    await page.goto('/adressen/new');
    await expect(
        page.getByRole('banner').getByText('Neue Adresse', { exact: true })
    ).toBeVisible();

    await page
        .getByLabel('Straße', { exact: true })
        .first()
        .fill(`${marker}-Strasse`);
    await page.getByLabel('Hausnr.', { exact: true }).first().fill('1');
    await page
        .getByLabel('Postleitzahl', { exact: true })
        .first()
        .fill('12345');
    await page.getByLabel('Stadt', { exact: true }).first().fill('Teststadt');
    // WalterTextInput commits its value on `change` (blur), not on input, so
    // blur the last field to flush it before the save button re-validates.
    await page.getByLabel('Stadt', { exact: true }).first().blur();

    // The new page exposes exactly one header action: save.
    await expect(page.locator('.bx--header__action').first()).toBeEnabled();
    await page.locator('.bx--header__action').first().click();

    // Saving navigates to the freshly created detail page.
    await expect(page).toHaveURL(/\/adressen\/\d+$/, { timeout: 15_000 });
    const id = Number(page.url().match(/\/adressen\/(\d+)/)![1]);
    await expect(
        page.getByLabel('Straße', { exact: true }).first()
    ).toHaveValue(`${marker}-Strasse`);

    // ---- edit ---------------------------------------------------------
    await page.getByLabel('Stadt', { exact: true }).first().fill('Editstadt');
    await page.getByLabel('Stadt', { exact: true }).first().blur();
    // On the detail page the header actions are: [save, delete, ...].
    await page.locator('.bx--header__action').first().click();
    await expect.poll(() => getAdresseStatus(id)).toBe(200);
    const editedStadt = await withFreshApiContext(async (api) => {
        const login = await signInApi(api, 'admin.dev');
        const r = await api.get(`/api/adressen/${id}`, {
            headers: authHeader(login.token)
        });
        return ((await r.json()) as { stadt: string }).stadt;
    });
    expect(editedStadt).toBe('Editstadt');

    // ---- delete -------------------------------------------------------
    await page.locator('.bx--header__action').nth(1).click();
    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();
    await dialog.getByRole('button', { name: 'Löschen' }).click();

    await expect.poll(() => getAdresseStatus(id)).not.toBe(200);
});

test('the detail UI gates editing by permission', async ({ page }) => {
    const { viewerAdresseId, ownerEditableId } = await withFreshApiContext(
        async (api) => {
            const viewer = await getFullListAs(
                api,
                'viewer.dev',
                '/api/adressen'
            );
            const owner = await getFullListAs(
                api,
                'owner.dev',
                '/api/adressen'
            );
            return {
                viewerAdresseId: viewer[0]?.id,
                ownerEditableId: owner.find((a) => a.permissions?.update)?.id
            };
        }
    );

    expect(viewerAdresseId).toBeTruthy();
    expect(ownerEditableId).toBeTruthy();

    // A read-only user sees the fields disabled and no save action.
    await authenticatePage(page, 'viewer.dev');
    await page.goto(`/adressen/${viewerAdresseId}`);
    await expect(
        page.getByLabel('Straße', { exact: true }).first()
    ).toHaveJSProperty('readOnly', true);

    // An owner editing one of their own addresses can edit the fields.
    await authenticatePage(page, 'owner.dev');
    await page.goto(`/adressen/${ownerEditableId}`);
    await expect(
        page.getByLabel('Straße', { exact: true }).first()
    ).toHaveJSProperty('readOnly', false);
});
