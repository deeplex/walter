// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, waitFor } from '@testing-library/svelte';
import { createRouteFetch } from '../../test-helpers/route-fetch';
import {
    knopf,
    oeffneAbschnitt,
    tabellenZeilen
} from '../../test-helpers/accordion';
import WalterUmlageRechnungen from './WalterUmlageRechnungen.svelte';

const mocks = vi.hoisted(() => ({ walter_post: vi.fn() }));

vi.mock('$walter/services/requests', async (importOriginal) => {
    const original =
        await importOriginal<typeof import('$walter/services/requests')>();
    return { ...original, walter_post: mocks.walter_post };
});

vi.mock('$app/stores', async () => {
    const { readable } = await import('svelte/store');
    return {
        page: readable({ url: new URL('http://localhost/mock'), params: {} })
    };
});

const ENDPUNKT = '/api/umlagen/5/rechnungen';

const rechnung = (
    id: string,
    jahr: number,
    betrag: number,
    bezahlt: number,
    verteilt: number,
    extra: Record<string, unknown> = {}
) => ({
    buchungssatzId: id,
    jahr,
    datum: `${jahr + 1}-01-15`,
    betrag,
    bezahlt,
    verteilt,
    offen: betrag - bezahlt,
    storniert: false,
    kannBezahlen: betrag - bezahlt > 0,
    zahlungen: [],
    verteilung: [],
    ...extra
});

const daten = (
    rechnungen: ReturnType<typeof rechnung>[],
    extra: Record<string, unknown> = {}
) => ({ rechnungen, rueckgaengigGemacht: [], ...extra });

/** Eine vollständige Vorbelegung: „+ Rechnung“ ist ohne weitere Eingabe speicherbar. */
const neueRechnung = {
    betreffendesJahr: 2024,
    betrag: 120,
    datum: '2025-01-10',
    typ: { id: 1, text: 'Grundsteuer' },
    umlage: { id: 5, text: 'Grundsteuer' }
};

async function zeige(
    routes: Record<string, unknown>,
    props: Record<string, unknown> = {}
) {
    const fetchImpl = createRouteFetch(routes);
    const result = render(WalterUmlageRechnungen, {
        umlageId: 5,
        bezeichnung: 'Grundsteuer',
        neueRechnung,
        fetchImpl,
        ...props
    });
    await oeffneAbschnitt(result.container);
    return { ...result, fetchImpl };
}

describe('WalterUmlageRechnungen', () => {
    beforeEach(() => mocks.walter_post.mockReset());
    afterEach(cleanup);

    it('lädt die Rechnungen der Umlage und zeigt je Rechnung eine Zeile', async () => {
        const { fetchImpl, container, findByText } = await zeige({
            [ENDPUNKT]: daten([
                rechnung('r2', 2024, 300, 0, 0),
                rechnung('r1', 2023, 200, 200, 200)
            ])
        });

        expect(await findByText('Rechnungen (2)')).toBeTruthy();
        expect(fetchImpl).toHaveBeenCalledWith(ENDPUNKT, expect.anything());
        await waitFor(() => expect(tabellenZeilen(container)).toHaveLength(2));
        expect(tabellenZeilen(container)[0].textContent).toContain('2024');
    });

    it('zeigt je Rechnung, ob sie verteilt ist', async () => {
        const { findByText } = await zeige({
            [ENDPUNKT]: daten([
                rechnung('r1', 2022, 100, 0, 100),
                rechnung('r2', 2023, 100, 0, 0),
                rechnung('r3', 2024, 100, 0, 40)
            ])
        });

        expect(await findByText('Verteilt')).toBeTruthy();
        expect(await findByText('Nicht verteilt')).toBeTruthy();
        expect(await findByText(/40,00\s*€ verteilt/)).toBeTruthy();
    });

    it('zeigt den offenen Betrag im Kopf, solange etwas offen ist', async () => {
        const { findByText, queryByText } = await zeige({
            [ENDPUNKT]: daten([
                rechnung('r1', 2023, 200, 50, 0),
                rechnung('r2', 2024, 100, 100, 0)
            ])
        });

        expect(await findByText(/150,00\s*€ offen/)).toBeTruthy();
        cleanup();
        const alles = await zeige({
            [ENDPUNKT]: daten([rechnung('r2', 2024, 100, 100, 0)])
        });
        await alles.findByText('Rechnungen (1)');
        expect(queryByText(/€ offen/)).toBeNull();
    });

    it('zeigt den Fehler des Servers', async () => {
        const { findByText } = await zeige({
            [ENDPUNKT]: { status: 500, body: 'Rechnungen kaputt' }
        });

        expect(await findByText('Rechnungen kaputt')).toBeTruthy();
    });

    it('bietet „Rechnung“ und „Zahlung erfassen“ nur bei bearbeitbar an', async () => {
        const routes = { [ENDPUNKT]: daten([rechnung('r1', 2024, 300, 0, 0)]) };

        const lesend = await zeige(routes);
        await waitFor(() =>
            expect(tabellenZeilen(lesend.container)).toHaveLength(1)
        );
        expect(knopf(lesend.container, 'Rechnung')).toBeUndefined();
        expect(lesend.queryByText('Zahlung erfassen')).toBeNull();
        lesend.unmount();

        const schreibend = await zeige(routes, { bearbeitbar: true });
        await waitFor(() =>
            expect(tabellenZeilen(schreibend.container)).toHaveLength(1)
        );
        expect(knopf(schreibend.container, 'Rechnung')).toBeDefined();
        expect(schreibend.getByText('Zahlung erfassen')).toBeTruthy();
    });

    it('öffnet die Zahlung mit der Bezeichnung und dem offenen Rest', async () => {
        const { findByText, getByLabelText } = await zeige(
            { [ENDPUNKT]: daten([rechnung('r1', 2024, 300, 100, 0)]) },
            { bearbeitbar: true }
        );

        await fireEvent.click(await findByText('Zahlung erfassen'));

        expect(
            await findByText('Zahlung erfassen – Grundsteuer 2024')
        ).toBeTruthy();
        expect((getByLabelText('Betrag (€)') as HTMLInputElement).value).toBe(
            '200'
        );
    });

    it('speichert eine neue Rechnung, lädt die Liste neu und meldet die Änderung', async () => {
        mocks.walter_post.mockResolvedValue(
            new Response('{}', { status: 200 })
        );
        const { fetchImpl, findByText, getByText, component, container } =
            await zeige({ [ENDPUNKT]: daten([]) }, { bearbeitbar: true });
        const geaendert = vi.fn();
        component.$on('geaendert', geaendert);
        await findByText('Noch keine Rechnung.');
        const ladenVorher = fetchImpl.mock.calls.filter(
            (c) => c[0] === ENDPUNKT
        ).length;

        await fireEvent.click(knopf(container, 'Rechnung')!);
        expect(
            await findByText('Rechnung erfassen – Grundsteuer')
        ).toBeTruthy();
        const speichern = getByText('Speichern').closest(
            'button'
        ) as HTMLButtonElement;
        await waitFor(() => expect(speichern.disabled).toBe(false));
        await fireEvent.click(speichern);

        await waitFor(() => expect(geaendert).toHaveBeenCalledTimes(1));
        expect(mocks.walter_post).toHaveBeenCalledWith(
            '/api/betriebskostenrechnungen',
            expect.objectContaining({ betrag: 120, betreffendesJahr: 2024 })
        );
        expect(
            fetchImpl.mock.calls.filter((c) => c[0] === ENDPUNKT).length
        ).toBe(ladenVorher + 1);
    });

    it('zeigt den Fehler beim Speichern und meldet keine Änderung', async () => {
        mocks.walter_post.mockResolvedValue(
            new Response('Rechnung existiert bereits', { status: 400 })
        );
        const { findByText, getByText, component, container } = await zeige(
            { [ENDPUNKT]: daten([]) },
            { bearbeitbar: true }
        );
        const geaendert = vi.fn();
        component.$on('geaendert', geaendert);

        await findByText('Noch keine Rechnung.');
        await fireEvent.click(knopf(container, 'Rechnung')!);
        const speichern = getByText('Speichern').closest(
            'button'
        ) as HTMLButtonElement;
        await waitFor(() => expect(speichern.disabled).toBe(false));
        await fireEvent.click(speichern);

        expect(await findByText('Rechnung existiert bereits')).toBeTruthy();
        expect(geaendert).not.toHaveBeenCalled();
    });

    it('speichert nichts, wenn die Vorbelegung unvollständig ist', async () => {
        const { findByText, getByText, container } = await zeige(
            { [ENDPUNKT]: daten([]) },
            { bearbeitbar: true, neueRechnung: { betreffendesJahr: 2024 } }
        );

        await findByText('Noch keine Rechnung.');
        await fireEvent.click(knopf(container, 'Rechnung')!);

        expect(
            (getByText('Speichern').closest('button') as HTMLButtonElement)
                .disabled
        ).toBe(true);
        expect(mocks.walter_post).not.toHaveBeenCalled();
    });
});
