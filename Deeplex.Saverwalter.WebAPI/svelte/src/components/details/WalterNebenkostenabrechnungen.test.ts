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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, waitFor } from '@testing-library/svelte';
import { createRouteFetch } from '../../test-helpers/route-fetch';
import { oeffneAbschnitt, tabellenZeilen } from '../../test-helpers/accordion';
import WalterNebenkostenabrechnungen from './WalterNebenkostenabrechnungen.svelte';

vi.mock('$app/stores', async () => {
    const { readable } = await import('svelte/store');
    return {
        page: readable({ url: new URL('http://localhost/mock'), params: {} })
    };
});

const ENDPUNKT = '/api/vertraege/1/abrechnungen';

const abrechnung = (
    id: string,
    jahr: number,
    status: 'Entwurf' | 'Abgesendet' | 'Storniert',
    saldo: number,
    offen: number,
    kannAusgleichen = status === 'Abgesendet' && offen !== 0
) => ({
    abrechnungsresultatId: id,
    jahr,
    status,
    saldo,
    betrag: Math.abs(saldo),
    bezahlt: Math.abs(saldo) - Math.abs(offen),
    offen,
    kannAusgleichen,
    zahlungen: []
});

function antwort(
    abrechnungen: ReturnType<typeof abrechnung>[],
    extra: Record<string, unknown> = {}
) {
    return {
        [ENDPUNKT]: { abrechnungen, rueckgaengigGemacht: [], ...extra },
        '/api/selection/bankkontos': []
    };
}

async function zeige(
    routes: Record<string, unknown>,
    props: Record<string, unknown> = {}
) {
    const fetchImpl = createRouteFetch(routes);
    const result = render(WalterNebenkostenabrechnungen, {
        vertragId: 1,
        fetchImpl,
        ...props
    });
    await oeffneAbschnitt(result.container);
    return { ...result, fetchImpl };
}

describe('WalterNebenkostenabrechnungen', () => {
    afterEach(cleanup);

    it('zeigt Ladezustand und dann je Abrechnung eine Zeile', async () => {
        const { getByText, container, findByText } = await zeige(
            antwort([
                abrechnung('a2', 2023, 'Abgesendet', 80, 80),
                abrechnung('a1', 2022, 'Abgesendet', -40, 40)
            ])
        );

        expect(getByText('Nebenkostenabrechnungen (0)')).toBeTruthy();
        await findByText('Nebenkostenabrechnungen (2)');
        await waitFor(() => expect(tabellenZeilen(container)).toHaveLength(2));
        expect(tabellenZeilen(container)[0].textContent).toContain(
            'Abrechnung 2023 – Nachzahlung'
        );
        expect(tabellenZeilen(container)[1].textContent).toContain(
            'Abrechnung 2022 – Erstattung'
        );
    });

    it('zeigt offene Nachzahlungen und Erstattungen getrennt im Kopf', async () => {
        const { findByText } = await zeige(
            antwort([
                abrechnung('a2', 2023, 'Abgesendet', 80, 80),
                abrechnung('a1', 2022, 'Abgesendet', -40, 40)
            ])
        );

        expect(await findByText(/80,00\s*€ Nachzahlung offen/)).toBeTruthy();
        expect(await findByText(/40,00\s*€ Erstattung offen/)).toBeTruthy();
    });

    it('meldet „Alles ausgeglichen“, wenn nichts offen ist, und zählt Entwürfe nicht', async () => {
        const { findByText, queryByText } = await zeige(
            antwort([
                abrechnung('a1', 2022, 'Abgesendet', 50, 0),
                abrechnung('a2', 2023, 'Entwurf', 99, 99)
            ])
        );

        expect(await findByText('Alles ausgeglichen')).toBeTruthy();
        expect(queryByText(/Nachzahlung offen/)).toBeNull();
    });

    it('zeigt ohne Abrechnung den Leertext und kein Tag', async () => {
        const { findByText, queryByText } = await zeige(antwort([]));

        expect(await findByText('Noch keine Abrechnung.')).toBeTruthy();
        expect(queryByText('Alles ausgeglichen')).toBeNull();
    });

    it('zeigt den Fehler des Servers', async () => {
        const { findByText } = await zeige({
            [ENDPUNKT]: { status: 500, body: 'Abrechnungen kaputt' }
        });

        expect(await findByText('Abrechnungen kaputt')).toBeTruthy();
    });

    it('bietet „Zahlung erfassen“ nur bei bearbeitbar und nur für ausgleichbare Abrechnungen', async () => {
        const routes = antwort([
            abrechnung('a1', 2023, 'Abgesendet', 80, 80),
            abrechnung('a2', 2022, 'Entwurf', 80, 80)
        ]);

        const lesend = await zeige(routes);
        await waitFor(() =>
            expect(tabellenZeilen(lesend.container)).toHaveLength(2)
        );
        expect(lesend.queryByText('Zahlung erfassen')).toBeNull();
        lesend.unmount();

        const schreibend = await zeige(routes, { bearbeitbar: true });
        await waitFor(() =>
            expect(tabellenZeilen(schreibend.container)).toHaveLength(2)
        );
        expect(schreibend.getAllByText('Zahlung erfassen')).toHaveLength(1);
    });

    it('öffnet für eine Nachzahlung den Dialog mit dem offenen Betrag', async () => {
        const { findByText, getByLabelText } = await zeige(
            antwort([abrechnung('a1', 2023, 'Abgesendet', 80, 80)], {
                vermieterBankkontoId: 7
            }),
            { bearbeitbar: true }
        );

        await fireEvent.click(await findByText('Zahlung erfassen'));

        expect(
            await findByText('Zahlung erfassen – Nachzahlung Abrechnung 2023')
        ).toBeTruthy();
        expect((getByLabelText('Betrag (€)') as HTMLInputElement).value).toBe(
            '80'
        );
    });

    it('benennt den Dialog bei einer Erstattung entsprechend', async () => {
        const { findByText } = await zeige(
            antwort([abrechnung('a1', 2023, 'Abgesendet', -40, 40)]),
            { bearbeitbar: true }
        );

        await fireEvent.click(await findByText('Zahlung erfassen'));

        expect(
            await findByText('Zahlung erfassen – Erstattung Abrechnung 2023')
        ).toBeTruthy();
    });
});
