import { expect, test, type APIRequestContext } from '@playwright/test';
import {
    authHeader,
    authenticatePage,
    signInApi,
    withFreshApiContext
} from './auth';
import { unwrapList } from './api';

/**
 * Mietkonto am Vertrag: Miete über „+ Zahlung“ erfassen → im Mietkonto als bezahlt
 * sichtbar → über „Rückgängig machen“ zurücknehmen → der Monat ist wieder offen und
 * die Zahlung steht unter „Rückgängig gemachte Zahlungen“. Außerdem: Miete von der
 * Startseite aus erfassen; Garagenmiete im Garagenmietkonto erfassen und zurücknehmen.
 */

type Monat = {
    jahr: number;
    monat: number;
    faellig: number;
    bezahlt: number;
    offen: number;
    kaltmiete: number;
    garagen: unknown[];
};

type Mietkonto = {
    monate: Monat[];
    rueckgaengigGemacht: { betrag: number; art: string }[];
};

const KURZ = [
    'Jan',
    'Feb',
    'Mär',
    'Apr',
    'Mai',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Okt',
    'Nov',
    'Dez'
];
const LANG = [
    'Januar',
    'Februar',
    'März',
    'April',
    'Mai',
    'Juni',
    'Juli',
    'August',
    'September',
    'Oktober',
    'November',
    'Dezember'
];

async function mietkonto(
    api: APIRequestContext,
    token: string,
    vertragId: number
): Promise<Mietkonto> {
    const r = await api.get(`/api/vertraege/${vertragId}/mietkonto`, {
        headers: authHeader(token)
    });
    expect(r.ok()).toBeTruthy();
    return (await r.json()) as Mietkonto;
}

function monatVon(konto: Mietkonto, jahr: number, monat: number) {
    return konto.monate.find((m) => m.jahr === jahr && m.monat === monat);
}

/** Ein Vertrag mit einem komplett offenen Monat des laufenden Jahres ohne Garage. */
async function findeOffenenMonat(): Promise<
    { vertragId: number; monat: Monat } | undefined
> {
    return withFreshApiContext(async (api) => {
        const login = await signInApi(api, 'admin.dev');
        const r = await api.get('/api/vertraege', {
            headers: authHeader(login.token)
        });
        expect(r.ok()).toBeTruthy();
        const vertraege = unwrapList<{ id: number }>(await r.json());
        const jahr = new Date().getFullYear();

        for (const v of vertraege) {
            const konto = await mietkonto(api, login.token, v.id);
            const monat = konto.monate.find(
                (m) =>
                    m.jahr === jahr &&
                    m.bezahlt === 0 &&
                    m.kaltmiete > 0 &&
                    m.garagen.length === 0
            );
            if (monat) return { vertragId: v.id, monat };
        }
        return undefined;
    });
}

// Beide Tests buchen auf den Dev-Daten; nacheinander, damit sie nicht denselben Monat wählen.
test.describe.configure({ mode: 'serial' });

test('Miete im Mietkonto erfassen und rückgängig machen', async ({ page }) => {
    const gefunden = await findeOffenenMonat();
    expect(
        gefunden,
        'erwartet einen Vertrag mit offenem Monat im Dev-Seed'
    ).toBeDefined();
    const { vertragId, monat } = gefunden!;
    const periode = `${KURZ[monat.monat - 1]} ${monat.jahr}`;
    const kaltmieteText = `Kaltmiete ${LANG[monat.monat - 1]} ${monat.jahr}`;

    await authenticatePage(page, 'admin.dev');
    // Das Mietkonto ist ein aufklappbarer Abschnitt; der Anker öffnet ihn.
    await page.goto(`/vertraege/${vertragId}#mietkonto`);

    // ---- erfassen -----------------------------------------------------
    const zeile = page.getByRole('row').filter({ hasText: periode }).first();
    await zeile.getByRole('button', { name: 'Zahlung' }).click();

    const dialog = page.locator('.bx--modal.is-visible');
    await expect(
        dialog.getByText(`Zahlung erfassen – ${periode}`)
    ).toBeVisible();
    // Vorbelegt mit dem offenen Betrag; die Vorschau beschreibt die Zahlung.
    await expect(dialog.getByText(kaltmieteText)).toBeVisible();
    await expect(dialog.getByText(/danach offen: 0,00\s€/)).toBeVisible();

    const buchen = dialog.getByRole('button', { name: 'Buchen' });
    await expect(buchen).toBeEnabled();
    await buchen.click();
    await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);

    await expect
        .poll(() =>
            withFreshApiContext(async (api) => {
                const login = await signInApi(api, 'admin.dev');
                const konto = await mietkonto(api, login.token, vertragId);
                return monatVon(konto, monat.jahr, monat.monat)?.offen;
            })
        )
        .toBe(0);
    await expect(zeile.getByText('Bezahlt', { exact: true })).toBeVisible();

    // ---- rückgängig machen ---------------------------------------------
    await zeile.getByRole('button', { name: /expand/i }).click();
    await page
        .locator('[data-child-row]')
        .getByRole('button', { name: 'Rückgängig machen' })
        .first()
        .click();

    const undo = page.locator('.bx--modal.is-visible');
    // Die Zahlung ist mit der Sollstellung des Monats ausgeglichen → Storno mit Grund.
    await expect(undo.getByText(/Gegenbuchung/)).toBeVisible();
    await undo.getByLabel('Grund (Pflicht)').fill('E2E-Test');
    await undo.getByRole('button', { name: 'Rückgängig machen' }).click();
    await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);

    await expect
        .poll(() =>
            withFreshApiContext(async (api) => {
                const login = await signInApi(api, 'admin.dev');
                const konto = await mietkonto(api, login.token, vertragId);
                return {
                    offen: monatVon(konto, monat.jahr, monat.monat)?.offen,
                    rueckgaengig: konto.rueckgaengigGemacht.some(
                        (z) => z.art === 'Kaltmiete' && z.betrag === monat.offen
                    )
                };
            })
        )
        .toEqual({ offen: monat.offen, rueckgaengig: true });
    await expect(page.getByText('Rückgängig gemachte Zahlungen')).toBeVisible();
});

type OffeneMiete = {
    vertragId: number;
    vertragBezeichnung: string;
    monat: string;
    offen: number;
    garagen: unknown[];
};

async function offeneMieten(jahr: number): Promise<OffeneMiete[]> {
    return withFreshApiContext(async (api) => {
        const login = await signInApi(api, 'admin.dev');
        const r = await api.get(`/api/offene-forderungen/${jahr}`, {
            headers: authHeader(login.token)
        });
        expect(r.ok()).toBeTruthy();
        return (await r.json()) as OffeneMiete[];
    });
}

test('Miete von der Startseite aus erfassen', async ({ page }) => {
    const jahr = new Date().getFullYear();
    const forderung = (await offeneMieten(jahr)).find(
        (f) => f.garagen.length === 0
    );
    expect(
        forderung,
        'erwartet eine offene Miete ohne Garage im Dev-Seed'
    ).toBeDefined();
    const { vertragId, vertragBezeichnung, monat } = forderung!;
    const [j, m] = monat.split('-').map(Number);
    const monatLang = `${LANG[m - 1]} ${j}`;

    await authenticatePage(page, 'admin.dev');
    await page.goto(`/?jahr=${jahr}`);

    const zeile = page
        .getByRole('row')
        .filter({ hasText: monatLang })
        .filter({ hasText: vertragBezeichnung })
        .first();
    // Die Zeile verlinkt auf das Mietkonto des Vertrags.
    await expect(zeile.getByRole('link').first()).toHaveAttribute(
        'href',
        `/vertraege/${vertragId}#mietkonto`
    );
    await zeile.getByRole('button', { name: 'Zahlung erfassen' }).click();

    const dialog = page.locator('.bx--modal.is-visible');
    await expect(dialog.getByText(`Kaltmiete ${monatLang}`)).toBeVisible();
    const buchen = dialog.getByRole('button', { name: 'Buchen' });
    await expect(buchen).toBeEnabled();
    await buchen.click();
    await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);

    await expect
        .poll(async () =>
            (await offeneMieten(jahr)).some(
                (f) => f.vertragId === vertragId && f.monat === monat
            )
        )
        .toBe(false);
});

/** Ein Garagenvertrag ohne Wohnungsvertrag mit einem offenen Monat im laufenden Jahr. */
async function findeOffeneGarage(): Promise<
    { garageVertragId: number; monat: Monat } | undefined
> {
    return withFreshApiContext(async (api) => {
        const login = await signInApi(api, 'admin.dev');
        const r = await api.get('/api/garage-vertraege', {
            headers: authHeader(login.token)
        });
        expect(r.ok()).toBeTruthy();
        const vertraege = unwrapList<{ id: number; vertrag?: unknown }>(
            await r.json()
        ).filter((g) => !g.vertrag);
        const jahr = new Date().getFullYear();

        for (const g of vertraege) {
            const k = await api.get(`/api/garage-vertraege/${g.id}/mietkonto`, {
                headers: authHeader(login.token)
            });
            expect(k.ok()).toBeTruthy();
            const konto = (await k.json()) as Mietkonto;
            const monat = konto.monate.find(
                (m) => m.jahr === jahr && m.bezahlt === 0 && m.offen > 0
            );
            if (monat) return { garageVertragId: g.id, monat };
        }
        return undefined;
    });
}

test('Garagenmiete im Garagenmietkonto erfassen und rückgängig machen', async ({
    page
}) => {
    const gefunden = await findeOffeneGarage();
    expect(
        gefunden,
        'erwartet einen Garagenvertrag mit offenem Monat im Dev-Seed'
    ).toBeDefined();
    const { garageVertragId, monat } = gefunden!;
    const periode = `${KURZ[monat.monat - 1]} ${monat.jahr}`;
    const offen = () =>
        withFreshApiContext(async (api) => {
            const login = await signInApi(api, 'admin.dev');
            const r = await api.get(
                `/api/garage-vertraege/${garageVertragId}/mietkonto`,
                { headers: authHeader(login.token) }
            );
            const konto = (await r.json()) as Mietkonto;
            return monatVon(konto, monat.jahr, monat.monat)?.offen;
        });

    await authenticatePage(page, 'admin.dev');
    await page.goto(`/garage-vertraege/${garageVertragId}#mietkonto`);
    await expect(page.getByText('Garagenmietkonto')).toBeVisible();

    const zeile = page.getByRole('row').filter({ hasText: periode }).first();
    await zeile.getByRole('button', { name: 'Zahlung' }).click();

    const dialog = page.locator('.bx--modal.is-visible');
    await expect(
        dialog.getByText(`Zahlung erfassen – ${periode}`)
    ).toBeVisible();
    await expect(
        dialog.getByText(
            new RegExp(`Garagenmiete .* ${LANG[monat.monat - 1]} ${monat.jahr}`)
        )
    ).toBeVisible();
    const buchen = dialog.getByRole('button', { name: 'Buchen' });
    await expect(buchen).toBeEnabled();
    await buchen.click();
    await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);

    await expect.poll(offen).toBe(0);
    await expect(zeile.getByText('Bezahlt', { exact: true })).toBeVisible();

    await zeile.getByRole('button', { name: /expand/i }).click();
    await page
        .locator('[data-child-row]')
        .getByRole('button', { name: 'Rückgängig machen' })
        .first()
        .click();
    const undo = page.locator('.bx--modal.is-visible');
    await expect(undo.getByText(/Gegenbuchung/)).toBeVisible();
    await undo.getByLabel('Grund (Pflicht)').fill('E2E-Test');
    await undo.getByRole('button', { name: 'Rückgängig machen' }).click();
    await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);

    await expect.poll(offen).toBe(monat.offen);
});
