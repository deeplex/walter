import { expect, test } from '@playwright/test';
import { getList } from './api';
import {
    authHeader,
    authenticatePage,
    signInApi,
    withFreshApiContext
} from './auth';
import {
    entityRefHref,
    type WalterEntityRef
} from '../../src/lib/WalterEntityRef';

/**
 * „Zuordnung“ einer Zahlung: in der Transaktionsliste als Spalte, auf der Transaktion oben mit
 * Link zum Fachobjekt. Auf dem Kontakt eines Mieters: was über alle Verträge offen ist.
 */

type ZuordnungEintrag = {
    text: string;
    ref?: WalterEntityRef | null;
    storniert: boolean;
};
type Transaktion = { id: string; zuordnung: ZuordnungEintrag[] };

test('Zuordnung auf der Transaktion und in der Liste', async ({ page }) => {
    const transaktion = await withFreshApiContext(async (api) => {
        const { token } = await signInApi(api, 'admin.dev');
        const liste = await getList<Transaktion>(
            api,
            token,
            '/api/transaktionen?take=50'
        );
        return liste.find((t) => t.zuordnung.some((w) => w.ref));
    });
    expect(
        transaktion,
        'erwartet eine zugeordnete Transaktion im Dev-Seed'
    ).toBeDefined();
    const item = transaktion!.zuordnung.find((w) => w.ref)!;

    await authenticatePage(page, 'admin.dev');
    await page.goto(`/transaktionen/${transaktion!.id}`);
    const link = page.locator('#zuordnung').getByRole('link').first();
    await expect(link).toHaveAttribute('href', entityRefHref(item.ref)!);
    await expect(link).toContainText(item.text);

    await page.goto('/transaktionen');
    await expect(
        page.getByRole('columnheader', { name: 'Zuordnung' })
    ).toBeVisible();
});

test('Kontakt eines Mieters zeigt, was offen ist', async ({ page }) => {
    const kontaktId = await withFreshApiContext(async (api) => {
        const { token } = await signInApi(api, 'admin.dev');
        const kontakte = await getList<{ id: number }>(
            api,
            token,
            '/api/kontakte?take=50'
        );
        for (const k of kontakte) {
            const r = await api.get(`/api/kontakte/${k.id}`, {
                headers: authHeader(token)
            });
            const entry = (await r.json()) as { mietvertragIds?: number[] };
            if ((entry.mietvertragIds ?? []).length > 0) return k.id;
        }
        return undefined;
    });
    expect(kontaktId, 'erwartet einen Mieter im Dev-Seed').toBeDefined();

    await authenticatePage(page, 'admin.dev');
    await page.goto(`/kontakte/${kontaktId}`);
    const section = page.locator('#offen-als-mieter');
    await expect(section.getByText('Offen als Mieter')).toBeVisible();
    // Summen-Tag: „… offen“ oder „Nichts offen“, sobald die Mietkonten geladen sind.
    await expect(section.locator('.bx--tag').first()).toBeVisible();
    await expect(
        section.locator('a[href*="#mietkonto"]').first()
    ).toBeVisible();
});

test('Strukturprüfung: Dev-Daten ohne Auffälligkeiten', async ({ page }) => {
    // Der Seed bucht Zahlungen über Buchungszeile.Zuordnen — keine Zahlung ohne Zuordnung,
    // jedes Konto mit genau einem Besitzer.
    const ergebnis = await withFreshApiContext(async (api) => {
        const { token } = await signInApi(api, 'admin.dev');
        const r = await api.get('/api/jahresabschluss/struktur', {
            headers: authHeader(token)
        });
        expect(r.ok()).toBeTruthy();
        return (await r.json()) as {
            mehrdeutigeKonten: unknown[];
            zahlungenOhneZuordnung: unknown[];
        };
    });
    expect(ergebnis.mehrdeutigeKonten).toEqual([]);
    expect(ergebnis.zahlungenOhneZuordnung).toEqual([]);

    await authenticatePage(page, 'admin.dev');
    await page.goto('/jahresabschlusskontrolle');
    await expect(
        page.locator('#strukturpruefung').getByText('Keine Auffälligkeiten')
    ).toBeVisible();
});
