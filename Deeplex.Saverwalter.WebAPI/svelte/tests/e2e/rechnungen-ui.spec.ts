import {
    expect,
    test,
    type APIRequestContext,
    type Page
} from '@playwright/test';
import { getList } from './api';
import {
    authHeader,
    authenticatePage,
    signInApi,
    withFreshApiContext
} from './auth';

/**
 * Rechnungen in der Fachsicht: auf der Wohnung eine Rechnung für Erhaltungsaufwand und auf
 * der Umlage eine Betriebskostenrechnung über „+ Zahlung“ bezahlen → bezahlt → rückgängig
 * machen → wieder offen. Die Betriebskostenrechnung wird dabei über „+ Rechnung“ erfasst.
 * Zum Schluss werden die Rechnungen wieder gelöscht.
 */

type Rechnung = {
    buchungssatzId: string;
    jahr: number;
    offen: number;
};

type Eintrag = { id: number; text?: string };

async function zahlungBuchen(page: Page) {
    const dialog = page.locator('.bx--modal.is-visible');
    await expect(dialog.getByText(/danach offen: 0,00\s€/)).toBeVisible();
    const buchen = dialog.getByRole('button', { name: 'Buchen' });
    await expect(buchen).toBeEnabled();
    await buchen.click();
    await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);
}

/** Erste Zahlung im aufgeklappten Abschnitt rückgängig machen (Löschen oder Storno). */
async function zahlungZuruecknehmen(page: Page, abschnitt: string) {
    await page
        .locator(`${abschnitt} [data-child-row]`)
        .getByRole('button', { name: 'Rückgängig machen' })
        .first()
        .click();
    const undo = page.locator('.bx--modal.is-visible');
    const grund = undo.getByLabel('Grund (Pflicht)');
    // Warten, bis feststeht, ob gelöscht oder per Gegenbuchung storniert wird.
    await expect(
        undo.getByText(/vollständig entfernt|Gegenbuchung/)
    ).toBeVisible();
    if (await grund.isVisible()) await grund.fill('E2E-Test');
    await undo.getByRole('button', { name: 'Rückgängig machen' }).click();
    await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);
}

async function erhaltungsaufwand(
    api: APIRequestContext,
    token: string,
    wohnungId: number
) {
    const r = await api.get(`/api/wohnungen/${wohnungId}/erhaltungsaufwand`, {
        headers: authHeader(token)
    });
    expect(r.ok()).toBeTruthy();
    return (await r.json()) as {
        rechnungen: Rechnung[];
        vermieterBankkontoId?: number | null;
    };
}

test('Rechnung für Erhaltungsaufwand bezahlen und rückgängig machen', async ({
    page
}) => {
    const betrag = 123.45;
    const bezeichnung = `E2E-Reparatur ${Date.now()}`;

    // Eine Wohnung mit Bankkonto des Eigentümers (Vorbelegung des Zahlers).
    const angelegt = await withFreshApiContext(async (api) => {
        const { token } = await signInApi(api, 'admin.dev');
        const wohnungen = await getList<Eintrag>(api, token, '/api/wohnungen');
        let wohnungId: number | undefined;
        for (const w of wohnungen) {
            if (
                (await erhaltungsaufwand(api, token, w.id)).vermieterBankkontoId
            ) {
                wohnungId = w.id;
                break;
            }
        }
        expect(
            wohnungId,
            'erwartet eine Wohnung mit Bankkonto des Eigentümers im Dev-Seed'
        ).toBeDefined();
        const [kontakt] = await getList<Eintrag>(api, token, '/api/kontakte');
        const post = await api.post('/api/erhaltungsaufwendungen', {
            headers: authHeader(token),
            data: {
                wohnung: { id: wohnungId, text: '' },
                aussteller: { id: kontakt.id, text: '' },
                betrag,
                datum: new Date().toISOString().slice(0, 10),
                bezeichnung
            }
        });
        expect(post.ok()).toBeTruthy();
        return {
            wohnungId: wohnungId!,
            id: ((await post.json()) as { id: string }).id
        };
    });
    const offen = () =>
        withFreshApiContext(async (api) => {
            const { token } = await signInApi(api, 'admin.dev');
            return (
                await erhaltungsaufwand(api, token, angelegt.wohnungId)
            ).rechnungen.find((r) => r.buchungssatzId === angelegt.id)?.offen;
        });

    try {
        await authenticatePage(page, 'admin.dev');
        await page.goto(`/wohnungen/${angelegt.wohnungId}#erhaltungsaufwand`);

        const zeile = page
            .locator('#erhaltungsaufwand')
            .getByRole('row')
            .filter({ hasText: bezeichnung })
            .first();
        await expect(zeile.getByRole('link').first()).toHaveAttribute(
            'href',
            `/buchungssaetze/${angelegt.id}`
        );
        await zeile.getByRole('button', { name: 'Zahlung' }).click();
        await zahlungBuchen(page);

        await expect.poll(offen).toBe(0);
        await expect(zeile.getByText('Bezahlt', { exact: true })).toBeVisible();

        await zeile.getByRole('button', { name: /expand/i }).click();
        await zahlungZuruecknehmen(page, '#erhaltungsaufwand');
        await expect.poll(offen).toBe(betrag);
    } finally {
        await withFreshApiContext(async (api) => {
            const { token } = await signInApi(api, 'admin.dev');
            await api.delete(`/api/erhaltungsaufwendungen/${angelegt.id}`, {
                headers: authHeader(token)
            });
        });
    }
});

test('Betriebskostenrechnung erfassen, bezahlen und rückgängig machen', async ({
    page
}) => {
    // Ein Jahr, das niemand abrechnet: die Rechnung beeinflusst keine anderen Tests.
    const jahr = 2099;
    const betrag = 321;

    const umlageId = await withFreshApiContext(async (api) => {
        const { token } = await signInApi(api, 'admin.dev');
        const [umlage] = await getList<Eintrag>(api, token, '/api/umlagen');
        return umlage.id;
    });
    const rechnung = () =>
        withFreshApiContext(async (api) => {
            const { token } = await signInApi(api, 'admin.dev');
            const r = await api.get(`/api/umlagen/${umlageId}/rechnungen`, {
                headers: authHeader(token)
            });
            expect(r.ok()).toBeTruthy();
            return (
                (await r.json()) as { rechnungen: Rechnung[] }
            ).rechnungen.find((x) => x.jahr === jahr);
        });

    try {
        await authenticatePage(page, 'admin.dev');
        await page.goto(`/umlagen/${umlageId}#rechnungen`);
        const abschnitt = page.locator('#rechnungen');

        await abschnitt
            .getByRole('button', { name: 'Rechnung', exact: true })
            .click();
        const dialog = page.locator('.bx--modal.is-visible');
        await dialog.getByLabel('Betreffendes Jahr').fill(`${jahr}`);
        await dialog.getByLabel('Betrag', { exact: true }).fill(`${betrag}`);
        await dialog.getByLabel('Betrag', { exact: true }).blur();
        await dialog.getByRole('button', { name: 'Speichern' }).click();
        await expect(page.locator('.bx--modal.is-visible')).toHaveCount(0);
        await expect.poll(async () => (await rechnung())?.offen).toBe(betrag);

        const zeile = abschnitt
            .getByRole('row')
            .filter({ hasText: `${jahr} – Rechnung vom` })
            .first();
        await expect(zeile.getByText('Nicht verteilt')).toBeVisible();
        await zeile.getByRole('button', { name: 'Zahlung' }).click();
        await zahlungBuchen(page);
        await expect.poll(async () => (await rechnung())?.offen).toBe(0);

        await zeile.getByRole('button', { name: /expand/i }).click();
        await zahlungZuruecknehmen(page, '#rechnungen');
        await expect.poll(async () => (await rechnung())?.offen).toBe(betrag);
    } finally {
        await withFreshApiContext(async (api) => {
            const { token } = await signInApi(api, 'admin.dev');
            const r = await api.get(`/api/umlagen/${umlageId}/rechnungen`, {
                headers: authHeader(token)
            });
            const { rechnungen } = (await r.json()) as {
                rechnungen: Rechnung[];
            };
            for (const x of rechnungen.filter((x) => x.jahr === jahr))
                await api.delete(
                    `/api/betriebskostenrechnungen/${x.buchungssatzId}`,
                    {
                        headers: authHeader(token)
                    }
                );
        });
    }
});
