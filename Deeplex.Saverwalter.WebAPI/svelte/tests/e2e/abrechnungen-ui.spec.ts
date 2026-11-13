import { expect, test, type APIRequestContext } from '@playwright/test';
import {
    authHeader,
    authenticatePage,
    signInApi,
    withFreshApiContext
} from './auth';
import { CANDIDATE_YEARS, getGruppen } from './abrechnung';

/**
 * Nebenkostenabrechnungen am Vertrag: eine Abrechnung buchen und versenden (über die
 * API), die Nachzahlung bzw. Erstattung im Abschnitt „Nebenkostenabrechnungen“ über
 * „+ Zahlung“ erfassen → ausgeglichen → rückgängig machen → wieder offen. Zum Schluss wird die
 * Abrechnung storniert, damit das Jahr wieder abgerechnet werden kann.
 */

type Abrechnung = {
    abrechnungsresultatId: string;
    jahr: number;
    status: string;
    saldo: number;
    offen: number;
};

type Abrechnungen = {
    abrechnungen: Abrechnung[];
    rueckgaengigGemacht: { betrag: number }[];
};

async function abrechnungen(
    api: APIRequestContext,
    token: string,
    vertragId: number
): Promise<Abrechnungen> {
    const r = await api.get(`/api/vertraege/${vertragId}/abrechnungen`, {
        headers: authHeader(token)
    });
    expect(r.ok()).toBeTruthy();
    return (await r.json()) as Abrechnungen;
}

type Gebucht = {
    wohnungIds: number[];
    jahr: number;
    vertragId: number;
    abrechnung: Abrechnung;
};

/**
 * Bucht die erste abrechenbare Gruppe (kleinste zuerst), deren Abrechnung für einen
 * Vertrag einen Saldo hat, und versendet diese Abrechnung.
 */
async function bucheAbrechnung(): Promise<Gebucht | undefined> {
    return withFreshApiContext(async (api) => {
        const { token } = await signInApi(api, 'admin.dev');
        const gruppen = (await getGruppen(api, token)).sort(
            (a, b) => a.wohnungIds.length - b.wohnungIds.length
        );
        for (const jahr of [...CANDIDATE_YEARS].reverse()) {
            for (const gruppe of gruppen) {
                const book = await api.post('/api/abrechnungslauf/book', {
                    headers: authHeader(token),
                    data: { jahr, gruppen: [{ wohnungIds: gruppe.wohnungIds }] }
                });
                if (!book.ok()) continue;
                const vertragIds = (
                    (await book.json()) as {
                        gruppen: { resultate: { vertragId: number }[] }[];
                    }[]
                ).flatMap((r) =>
                    r.gruppen.flatMap((g) =>
                        g.resultate.map((x) => x.vertragId)
                    )
                );

                for (const vertragId of vertragIds) {
                    const abrechnung = (
                        await abrechnungen(api, token, vertragId)
                    ).abrechnungen.find(
                        (a) =>
                            a.jahr === jahr &&
                            a.status === 'Entwurf' &&
                            Math.abs(a.saldo) > 0.005
                    );
                    if (!abrechnung) continue;

                    const url = `/api/abrechnungsresultate/${abrechnung.abrechnungsresultatId}`;
                    const entry = await (
                        await api.get(url, { headers: authHeader(token) })
                    ).json();
                    const put = await api.put(url, {
                        headers: authHeader(token),
                        data: { ...entry, abgesendet: true }
                    });
                    expect(put.ok()).toBeTruthy();
                    return {
                        wohnungIds: gruppe.wohnungIds,
                        jahr,
                        vertragId,
                        abrechnung
                    };
                }
                // Kein Saldo in dieser Gruppe: Buchung zurücknehmen.
                await api.post('/api/abrechnungslauf/rueckabwicklung', {
                    headers: authHeader(token),
                    data: { wohnungIds: gruppe.wohnungIds, jahr }
                });
            }
        }
        return undefined;
    });
}

test('Ausgleich einer Nebenkostenabrechnung erfassen und rückgängig machen', async ({
    page
}) => {
    const gebucht = await bucheAbrechnung();
    expect(
        gebucht,
        'erwartet eine abrechenbare Gruppe mit Saldo im Dev-Seed'
    ).toBeDefined();
    const { wohnungIds, jahr, vertragId, abrechnung } = gebucht!;
    const art = abrechnung.saldo < 0 ? 'Erstattung' : 'Nachzahlung';
    const offen = () =>
        withFreshApiContext(async (api) => {
            const { token } = await signInApi(api, 'admin.dev');
            return (
                await abrechnungen(api, token, vertragId)
            ).abrechnungen.find(
                (a) =>
                    a.abrechnungsresultatId === abrechnung.abrechnungsresultatId
            )?.offen;
        });

    try {
        await authenticatePage(page, 'admin.dev');
        await page.goto(`/vertraege/${vertragId}`);

        const abschnitt = page.locator('#abrechnungen');
        const zeile = abschnitt
            .getByRole('row')
            .filter({ hasText: `Abrechnung ${jahr} – ${art}` })
            .first();
        await expect(zeile.getByRole('link')).toHaveAttribute(
            'href',
            `/abrechnungsresultate/${abrechnung.abrechnungsresultatId}`
        );
        await zeile.getByRole('button', { name: 'Zahlung' }).click();

        const dialog = page.locator('.bx--modal.is-visible');
        await expect(
            dialog.getByText(`${art} NK-Abrechnung ${jahr}`)
        ).toBeVisible();
        await expect(dialog.getByText(/danach offen: 0,00\s€/)).toBeVisible();
        const buchen = dialog.getByRole('button', { name: 'Buchen' });
        await expect(buchen).toBeEnabled();
        await buchen.click();
        await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);

        await expect.poll(offen).toBe(0);
        await expect(zeile.getByText('Bezahlt', { exact: true })).toBeVisible();

        await zeile.getByRole('button', { name: /expand/i }).click();
        await abschnitt
            .locator('[data-child-row]')
            .getByRole('button', { name: 'Rückgängig machen' })
            .first()
            .click();
        const undo = page.locator('.bx--modal.is-visible');
        await expect(undo.getByText(/Gegenbuchung/)).toBeVisible();
        await undo.getByLabel('Grund (Pflicht)').fill('E2E-Test');
        await undo.getByRole('button', { name: 'Rückgängig machen' }).click();
        await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);

        await expect.poll(offen).toBe(abrechnung.offen);
        await expect(
            abschnitt.getByText('Rückgängig gemachte Zahlungen')
        ).toBeVisible();
    } finally {
        await withFreshApiContext(async (api) => {
            const { token } = await signInApi(api, 'admin.dev');
            const storno = await api.post('/api/abrechnungslauf/storno', {
                headers: authHeader(token),
                data: { wohnungIds, jahr, grund: 'E2E-Test aufräumen' }
            });
            expect(storno.ok()).toBeTruthy();
        });
    }
});
