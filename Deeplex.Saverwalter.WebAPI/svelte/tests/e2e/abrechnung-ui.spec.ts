import { expect, test, type APIRequestContext } from '@playwright/test';
import { promises as fs } from 'fs';
import { authHeader, authenticatePage, signInApi } from './auth';

type VertragSummary = {
    id: number;
    permissions: { update: boolean; read: boolean };
    beginn?: string;
    ende?: string | null;
};

async function findRenderableAbrechnung(
    api: APIRequestContext,
    token: string
): Promise<{ vertragId: number; jahr: number }> {
    const vertraegeResp = await api.get('/api/vertraege', {
        headers: authHeader(token)
    });
    expect(vertraegeResp.ok()).toBeTruthy();
    const vertraege = (await vertraegeResp.json()) as VertragSummary[];

    const editable = vertraege.filter((v) => v.permissions.update).slice(0, 12);
    for (const vertrag of editable) {
        const beginn = vertrag.beginn ? new Date(vertrag.beginn) : null;
        const ende = vertrag.ende ? new Date(vertrag.ende) : new Date();
        if (!beginn) continue;
        const startYear = beginn.getFullYear();
        const endYear = ende.getFullYear();
        for (let jahr = endYear; jahr >= startYear; jahr--) {
            const url = `/api/betriebskostenabrechnung/${vertrag.id}/${jahr}/pdf_document`;
            const r = await api.get(url, { headers: authHeader(token) });
            if (r.ok()) {
                return { vertragId: vertrag.id, jahr };
            }
        }
    }
    throw new Error('No contract with renderable Betriebskostenabrechnung in dev seed');
}

test.describe('Abrechnung UI download flow', () => {
    test('owner can open the abrechnung page and download a PDF via the UI', async ({
        page,
        request
    }) => {
        const ownerLogin = await signInApi(request, 'owner.dev');
        const candidate = await findRenderableAbrechnung(request, ownerLogin.token);

        await authenticatePage(page, 'owner.dev');
        await page.goto(
            `/abrechnung?vertrag=${candidate.vertragId}&jahr=${candidate.jahr}`
        );

        // Wait until the abrechnung area finishes loading. The page either
        // renders the abrechnung details or surfaces an inline notification.
        await expect(page.getByText('Lade Abrechnung...')).toHaveCount(0, {
            timeout: 30_000
        });

        // The header panel (Anhänge) is open by default and overlays the
        // download menu. Close it before attempting to interact with the
        // OverflowMenu in the page body.
        const panelToggle = page.locator('button.bx--header__action').first();
        const isPanelActive = await panelToggle.evaluate((el) =>
            el.classList.contains('bx--header__action--active')
        );
        if (isPanelActive) {
            await panelToggle.click();
        }

        // Carbon's OverflowMenu renders a button with the .bx--overflow-menu class.
        const menuTrigger = page.locator('main button.bx--overflow-menu').first();
        await menuTrigger.click();

        const downloadPromise = page.waitForEvent('download', { timeout: 60_000 });
        await page
            .getByRole('menuitem', { name: 'PDF-Dokument erstellen' })
            .click();

        // If a previous run already produced a resultat for the same
        // contract+year, the page asks for confirmation before overwriting.
        const overrideButton = page
            .getByRole('button', { name: 'Bestätigen' })
            .first();
        if (await overrideButton.isVisible().catch(() => false)) {
            await page
                .getByLabel('Abrechnungsergebnis überschreiben')
                .check({ force: true })
                .catch(() => undefined);
            await overrideButton.click();
        }

        const download = await downloadPromise;
        const downloadPath = await download.path();
        expect(downloadPath).toBeTruthy();

        // Note: suggestedFilename() can be the browser default ("download")
        // because the anchor click is dispatched from an async continuation
        // after user activation has expired. We verify the payload is a PDF
        // instead, and rely on the API check below to confirm the resultat
        // was persisted under the expected name in S3.
        const buf = await fs.readFile(downloadPath!);
        expect(buf.subarray(0, 5).toString('latin1')).toBe('%PDF-');

        // Saving the file twice (once via UI, once via API to S3) is a useful
        // smoke test. Confirm the abrechnungsresultat now exists for the
        // contract+year and that S3 holds the generated artefact.
        const resultatResp = await request.get(
            `/api/abrechnungsresultate/vertrag/${candidate.vertragId}/jahr/${candidate.jahr}`,
            { headers: authHeader(ownerLogin.token) }
        );
        expect(resultatResp.ok()).toBeTruthy();
        const resultat = (await resultatResp.json()) as { id: string };
        expect(resultat.id).toBeTruthy();

        const filesResp = await request.get(
            `/api/abrechnungsresultate/${resultat.id}/files`,
            { headers: authHeader(ownerLogin.token) }
        );
        expect(filesResp.ok()).toBeTruthy();
        const files = (await filesResp.json()) as Array<{ fileName: string }>;
        expect(files.length).toBeGreaterThan(0);
        expect(files.some((f) => /\.pdf$/.test(f.fileName))).toBeTruthy();
    });
});
