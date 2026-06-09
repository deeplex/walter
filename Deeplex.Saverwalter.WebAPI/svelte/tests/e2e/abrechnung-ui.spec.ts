import { expect, test } from '@playwright/test';
import { promises as fs } from 'fs';
import { authenticatePage, signInApi, withFreshApiContext } from './auth';
import { findPrintableAbrechnung, isPdfOrZip } from './abrechnung';

test.describe('Abrechnungslauf UI download flow', () => {
    test('owner can run a billing group and download the result via the UI', async ({
        page
    }) => {
        // Find a billing group + year that renders, using the API directly.
        const printable = await withFreshApiContext(async (api) => {
            const login = await signInApi(api, 'owner.dev');
            return findPrintableAbrechnung(api, login.token);
        });
        expect(
            printable,
            'expected the dev seed to contain a renderable billing group'
        ).toBeDefined();

        await authenticatePage(page, 'owner.dev');
        await page.goto('/abrechnungslauf');

        await expect(
            page
                .getByRole('banner')
                .getByText('Abrechnungslauf', { exact: true })
        ).toBeVisible();

        // Set the year that we know renders.
        const jahrInput = page.getByLabel('Jahr');
        await jahrInput.fill(String(printable!.jahr));

        // Open the Abrechnungsgruppen multi-select and pick the matching group.
        await page.getByText('Abrechnungsgruppen').click();
        await page
            .getByRole('option', { name: printable!.gruppe.bezeichnung })
            .click();
        // Close the dropdown so it does not overlay the action buttons.
        await page.keyboard.press('Escape');

        await page.getByRole('button', { name: 'Vorschau zeigen' }).click();

        // The preview finished once the calculation note disappears and the
        // download button is rendered.
        await expect(page.getByText('Wird berechnet…')).toHaveCount(0, {
            timeout: 30_000
        });

        const downloadButton = page.getByRole('button', {
            name: /runterladen/
        });
        await expect(downloadButton).toBeEnabled({ timeout: 30_000 });

        const downloadPromise = page.waitForEvent('download', {
            timeout: 60_000
        });
        await downloadButton.click();
        const download = await downloadPromise;

        const downloadPath = await download.path();
        expect(downloadPath).toBeTruthy();
        const buf = await fs.readFile(downloadPath!);
        // A one-contract group downloads a PDF; a multi-contract group a ZIP.
        expect(isPdfOrZip(buf)).toBeTruthy();
    });
});
