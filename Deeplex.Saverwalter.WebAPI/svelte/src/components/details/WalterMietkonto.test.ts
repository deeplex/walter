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

import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, waitFor } from '@testing-library/svelte';
import { createRouteFetch } from '../../test-helpers/route-fetch';
import { oeffneAbschnitt, tabellenZeilen } from '../../test-helpers/accordion';
import WalterMietkonto from './WalterMietkonto.svelte';

vi.mock('$app/stores', async () => {
    const { readable } = await import('svelte/store');
    return {
        page: readable({ url: new URL('http://localhost/mock'), params: {} })
    };
});

const monat = (
    jahr: number,
    m: number,
    faellig: number,
    bezahlt: number,
    extra: Record<string, unknown> = {}
) => ({
    jahr,
    monat: m,
    faellig,
    bezahlt,
    offen: faellig - bezahlt,
    kaltmiete: faellig,
    garagen: [],
    zahlungen: [],
    transaktionen: [],
    transaktionenBetrag: bezahlt,
    nkVorauszahlung: 0,
    ...extra
});

const konto = (
    monate: ReturnType<typeof monat>[],
    extra: Record<string, unknown> = {}
) => ({
    monate,
    nichtZugeordnet: [],
    rueckgaengigGemacht: [],
    offen: monate.reduce((s, m) => s + m.offen, 0),
    ...extra
});

const zahlung = (id: string, datum: string, betrag: number) => ({
    buchungssatzId: id,
    transaktionId: null,
    datum,
    betrag,
    zahler: 'Mieter',
    art: 'Kaltmiete'
});

/** Die Miet-Korrektur unter dem Mietkonto lädt ihre Liste mit. */
const MIET_KORREKTUR = {
    '/api/vertraege/1/miet-korrektur': [],
    '/api/vertraege/2/miet-korrektur': []
};

async function zeige(
    routes: Record<string, unknown>,
    props: Record<string, unknown> = {}
) {
    const fetchImpl = createRouteFetch({ ...MIET_KORREKTUR, ...routes });
    const result = render(WalterMietkonto, {
        vertragId: 1,
        fetchImpl,
        ...props
    });
    return { ...result, fetchImpl };
}

describe('WalterMietkonto', () => {
    afterEach(cleanup);

    it('lädt das Mietkonto des Vertrags', async () => {
        const { fetchImpl, findByText } = await zeige({
            '/api/vertraege/1/mietkonto': konto([monat(2024, 1, 500, 500)])
        });

        expect(await findByText('Mietkonto (1)')).toBeTruthy();
        expect(fetchImpl).toHaveBeenCalledWith(
            '/api/vertraege/1/mietkonto',
            expect.anything()
        );
    });

    it('lädt das Garagenmietkonto, wenn nur eine Garagenvertrag-Id gegeben ist', async () => {
        const { fetchImpl, findByText } = await zeige(
            {
                '/api/garage-vertraege/9/mietkonto': konto([
                    monat(2024, 1, 50, 50)
                ])
            },
            { vertragId: undefined, garageVertragId: 9 }
        );

        expect(await findByText('Mietkonto (1)')).toBeTruthy();
        expect(fetchImpl).toHaveBeenCalledWith(
            '/api/garage-vertraege/9/mietkonto',
            expect.anything()
        );
    });

    it('zeigt die Monate, neuester zuerst', async () => {
        const { container, findByText } = await zeige({
            '/api/vertraege/1/mietkonto': konto([
                monat(2024, 1, 500, 500),
                monat(2024, 3, 500, 0),
                monat(2024, 2, 500, 200)
            ])
        });
        await findByText('Mietkonto (3)');
        await oeffneAbschnitt(container);

        await waitFor(() => expect(tabellenZeilen(container)).toHaveLength(3));
        const text = tabellenZeilen(container).map((z) => z.textContent ?? '');
        expect(text[0]).toContain('2024');
        expect(text[0]).toMatch(/Mär/);
        expect(text[2]).toMatch(/Jan/);
    });

    it.each([
        {
            stand: 'offen',
            monate: [monat(2024, 1, 500, 300)],
            erwartet: /200,00\s*€ offen/,
            extra: {}
        },
        {
            stand: 'alles bezahlt',
            monate: [monat(2024, 1, 500, 500)],
            erwartet: /Alles bezahlt/,
            extra: {}
        },
        {
            stand: 'Guthaben',
            monate: [monat(2024, 1, 500, 500)],
            erwartet: /50,00\s*€ Guthaben/,
            extra: { nichtZugeordnet: [zahlung('g1', '2024-02-01', 50)] }
        }
    ])('zeigt den Stand: $stand', async ({ monate, erwartet, extra }) => {
        const { findByText } = await zeige({
            '/api/vertraege/1/mietkonto': konto(monate, extra)
        });

        expect(await findByText(erwartet)).toBeTruthy();
    });

    it('rechnet ein Guthaben gegen das Offene', async () => {
        const { findByText } = await zeige({
            '/api/vertraege/1/mietkonto': konto([monat(2024, 1, 500, 300)], {
                nichtZugeordnet: [zahlung('g1', '2024-02-01', 50)]
            })
        });

        expect(await findByText(/150,00\s*€ offen/)).toBeTruthy();
    });

    it('zeigt mit `jahr` nur dieses Jahr als Kachel statt als Abschnitt', async () => {
        const { container, findByText, queryByText } = await zeige(
            {
                '/api/vertraege/1/mietkonto': konto([
                    monat(2023, 12, 500, 0),
                    monat(2024, 1, 500, 500)
                ])
            },
            { jahr: 2024, title: 'Mietkonto 2024' }
        );

        expect(await findByText(/Alles bezahlt/)).toBeTruthy();
        expect(container.querySelector('.bx--accordion')).toBeNull();
        expect(container.querySelector('.bx--tile')).not.toBeNull();
        await waitFor(() => expect(tabellenZeilen(container)).toHaveLength(1));
        expect(queryByText(/500,00\s*€ offen/)).toBeNull();
    });

    it('ordnet Guthaben und rückgängig gemachte Zahlungen nach dem Jahr', async () => {
        const { findByText, queryByText } = await zeige(
            {
                '/api/vertraege/1/mietkonto': konto(
                    [monat(2024, 1, 500, 500)],
                    {
                        nichtZugeordnet: [
                            zahlung('g1', '2023-05-01', 70),
                            zahlung('g2', '2024-05-01', 30)
                        ]
                    }
                )
            },
            { jahr: 2024 }
        );

        expect(await findByText(/30,00\s*€ Guthaben/)).toBeTruthy();
        expect(queryByText(/100,00\s*€ Guthaben/)).toBeNull();
    });

    it('zeigt den Fehler des Servers', async () => {
        const { findByText, container } = await zeige({
            '/api/vertraege/1/mietkonto': {
                status: 500,
                body: 'Mietkonto kaputt'
            }
        });
        await oeffneAbschnitt(container);

        expect(await findByText('Mietkonto kaputt')).toBeTruthy();
    });

    it('lädt neu, wenn sich der Vertrag ändert', async () => {
        const fetchImpl = createRouteFetch({
            ...MIET_KORREKTUR,
            '/api/vertraege/1/mietkonto': konto([monat(2024, 1, 500, 500)]),
            '/api/vertraege/2/mietkonto': konto([
                monat(2024, 1, 500, 500),
                monat(2024, 2, 500, 500)
            ])
        });
        const { component, findByText } = render(WalterMietkonto, {
            vertragId: 1,
            fetchImpl
        });
        await findByText('Mietkonto (1)');

        component.$set({ vertragId: 2 });

        expect(await findByText('Mietkonto (2)')).toBeTruthy();
    });

    it('bietet „Zahlung erfassen“ nur bei bearbeitbar an und öffnet den Dialog für den Monat', async () => {
        const routes = {
            '/api/vertraege/1/mietkonto': konto([monat(2024, 1, 500, 300)])
        };

        const lesend = await zeige(routes);
        await oeffneAbschnitt(lesend.container);
        await waitFor(() =>
            expect(tabellenZeilen(lesend.container)).toHaveLength(1)
        );
        expect(lesend.queryByText('Zahlung erfassen')).toBeNull();
        lesend.unmount();

        const schreibend = await zeige(routes, { bearbeitbar: true });
        await oeffneAbschnitt(schreibend.container);
        await fireEvent.click(await schreibend.findByText('Zahlung erfassen'));

        expect(await schreibend.findByText(/Zahlung erfassen – /)).toBeTruthy();
        expect(
            (schreibend.getByLabelText('Betrag (€)') as HTMLInputElement).value
        ).toBe('200');
    });
});
