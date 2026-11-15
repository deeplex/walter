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
import WalterErhaltungsaufwand from './WalterErhaltungsaufwand.svelte';

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

const ENDPUNKT = '/api/wohnungen/3/erhaltungsaufwand';
const wohnung = { id: 3, text: 'Wohnung 3' };

const rechnung = (
    id: string,
    bezeichnung: string,
    betrag: number,
    bezahlt: number,
    extra: Record<string, unknown> = {}
) => ({
    buchungssatzId: id,
    datum: '2024-03-01',
    jahr: 2024,
    bezeichnung,
    aussteller: 'Handwerker GmbH',
    betrag,
    bezahlt,
    offen: betrag - bezahlt,
    storniert: false,
    direktBezahlt: false,
    kannBezahlen: betrag - bezahlt > 0,
    zahlungen: [],
    ...extra
});

const daten = (
    rechnungen: ReturnType<typeof rechnung>[],
    extra: Record<string, unknown> = {}
) => ({ rechnungen, jahre: [], vermieterBankkontoId: 7, ...extra });

async function zeige(
    routes: Record<string, unknown>,
    props: Record<string, unknown> = {}
) {
    const fetchImpl = createRouteFetch(routes);
    const result = render(WalterErhaltungsaufwand, {
        wohnung,
        fetchImpl,
        ...props
    });
    await oeffneAbschnitt(result.container);
    return { ...result, fetchImpl };
}

describe('WalterErhaltungsaufwand', () => {
    beforeEach(() => mocks.walter_post.mockReset());
    afterEach(cleanup);

    it('lädt den Erhaltungsaufwand der Wohnung und zeigt je Rechnung eine Zeile', async () => {
        const { fetchImpl, container, findByText } = await zeige({
            [ENDPUNKT]: daten([
                rechnung('r1', 'Dach', 1000, 400),
                rechnung('r2', 'Heizung', 500, 500)
            ])
        });

        expect(await findByText('Erhaltungsaufwand (2)')).toBeTruthy();
        expect(fetchImpl).toHaveBeenCalledWith(ENDPUNKT, expect.anything());
        await waitFor(() => expect(tabellenZeilen(container)).toHaveLength(2));
        expect(tabellenZeilen(container)[0].textContent).toContain('Dach');
        expect(tabellenZeilen(container)[0].textContent).toContain(
            'Handwerker GmbH'
        );
    });

    it('zeigt den offenen Betrag nur für offene Rechnungen im Kopf', async () => {
        const { findByText, queryByText } = await zeige({
            [ENDPUNKT]: daten([
                rechnung('r1', 'Dach', 1000, 400),
                rechnung('r2', 'Heizung', 500, 500)
            ])
        });

        expect(await findByText(/600,00\s*€ offen/)).toBeTruthy();
        cleanup();
        const bezahlt = await zeige({
            [ENDPUNKT]: daten([rechnung('r2', 'Heizung', 500, 500)])
        });
        await bezahlt.findByText('Erhaltungsaufwand (1)');
        expect(queryByText(/€ offen/)).toBeNull();
    });

    it('zeigt den Jahresüberblick, wenn es Jahre gibt', async () => {
        const { findByText, queryByText } = await zeige({
            [ENDPUNKT]: daten([rechnung('r1', 'Dach', 1000, 0)], {
                jahre: [
                    {
                        wert: 2024,
                        mieteinnahmen: 6000,
                        erhaltungsaufwand: 1000,
                        betriebskostenEigenanteil: 120
                    }
                ]
            })
        });

        expect(await findByText('Jahresüberblick')).toBeTruthy();
        expect(await findByText(/6\.000,00\s*€/)).toBeTruthy();
        cleanup();
        const ohne = await zeige({ [ENDPUNKT]: daten([]) });
        await ohne.findByText('Noch keine Rechnung.');
        expect(queryByText('Jahresüberblick')).toBeNull();
    });

    it('zeigt den Fehler des Servers', async () => {
        const { findByText } = await zeige({
            [ENDPUNKT]: { status: 500, body: 'Erhaltungsaufwand kaputt' }
        });

        expect(await findByText('Erhaltungsaufwand kaputt')).toBeTruthy();
    });

    it('bietet „Rechnung“ und „Zahlung erfassen“ nur bei bearbeitbar an', async () => {
        const routes = { [ENDPUNKT]: daten([rechnung('r1', 'Dach', 1000, 0)]) };

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

    it('bietet keine Zahlung für bezahlte Rechnungen an', async () => {
        const { container, queryByText } = await zeige(
            { [ENDPUNKT]: daten([rechnung('r1', 'Dach', 1000, 1000)]) },
            { bearbeitbar: true }
        );

        await waitFor(() => expect(tabellenZeilen(container)).toHaveLength(1));
        expect(queryByText('Zahlung erfassen')).toBeNull();
    });

    it('öffnet die Zahlung mit dem offenen Rest der gewählten Rechnung', async () => {
        const { findByText, getByLabelText } = await zeige(
            { [ENDPUNKT]: daten([rechnung('r1', 'Dach', 1000, 400)]) },
            { bearbeitbar: true }
        );

        await fireEvent.click(await findByText('Zahlung erfassen'));

        expect(await findByText('Zahlung erfassen – Dach')).toBeTruthy();
        expect((getByLabelText('Betrag (€)') as HTMLInputElement).value).toBe(
            '600'
        );
    });

    it('öffnet „Rechnung“, sperrt Speichern bei unvollständigen Angaben und schließt mit Abbrechen', async () => {
        const { findByText, getByText, queryByText, container } = await zeige(
            { [ENDPUNKT]: daten([]) },
            { bearbeitbar: true }
        );

        await findByText('Noch keine Rechnung.');
        await fireEvent.click(knopf(container, 'Rechnung')!);

        expect(
            await findByText('Rechnung für Erhaltungsaufwand erfassen')
        ).toBeTruthy();
        const speichern = getByText('Speichern').closest('button');
        expect(speichern?.disabled).toBe(true);

        const dialog = container.ownerDocument.querySelector(
            '.bx--modal.is-visible'
        ) as HTMLElement;
        // Das Formular enthält eigene Dialoge mit „Abbrechen“; gemeint ist der Fußbereich dieses Dialogs.
        const fuss = Array.from(
            dialog.querySelector('.bx--modal-container')!.children
        ).find((c) => c.classList.contains('bx--modal-footer')) as HTMLElement;
        await fireEvent.click(knopf(fuss, 'Abbrechen')!);
        await waitFor(() =>
            expect(
                container.ownerDocument.querySelector('.bx--modal.is-visible')
            ).toBeNull()
        );
        expect(mocks.walter_post).not.toHaveBeenCalled();
        expect(queryByText('Speichern')).not.toBeNull();
    });
});
