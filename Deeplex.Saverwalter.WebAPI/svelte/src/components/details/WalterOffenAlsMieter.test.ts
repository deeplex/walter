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

import { afterEach, describe, expect, it } from 'vitest';
import { cleanup, render, waitFor } from '@testing-library/svelte';
import { createRouteFetch } from '../../test-helpers/route-fetch';
import WalterOffenAlsMieter from './WalterOffenAlsMieter.svelte';

const konto = (offen: number) => ({
    monate: [],
    nichtZugeordnet: [],
    rueckgaengigGemacht: [],
    offen
});

const abrechnung = (
    status: string,
    saldo: number,
    offen: number,
    id = 'a1'
) => ({
    abrechnungsresultatId: id,
    jahr: 2023,
    status,
    saldo,
    betrag: Math.abs(saldo),
    bezahlt: 0,
    offen,
    kannAusgleichen: true,
    zahlungen: []
});

const abrechnungen = (...liste: ReturnType<typeof abrechnung>[]) => ({
    abrechnungen: liste,
    rueckgaengigGemacht: []
});

describe('WalterOffenAlsMieter', () => {
    afterEach(cleanup);

    it('rendert ohne Verträge nichts', () => {
        const { container } = render(WalterOffenAlsMieter, {
            vertraege: [],
            fetchImpl: createRouteFetch({})
        });

        expect(container.querySelector('#offen-als-mieter')).toBeNull();
    });

    it('zeigt je Vertrag offene Miete und Nachzahlung mit der Summe', async () => {
        const fetchImpl = createRouteFetch({
            '/api/vertraege/1/mietkonto': konto(100),
            '/api/vertraege/1/abrechnungen': abrechnungen(
                abrechnung('Abgesendet', 80, 50)
            ),
            '/api/vertraege/2/mietkonto': konto(25),
            '/api/vertraege/2/abrechnungen': abrechnungen()
        });
        const { findByText, container } = render(WalterOffenAlsMieter, {
            vertraege: [
                { id: 1, text: 'Vertrag Eins' },
                { id: 2, text: 'Vertrag Zwei' }
            ],
            fetchImpl
        });

        // 100 + 50 + 25
        expect(await findByText(/175,00\s*€ offen/)).toBeTruthy();
        const zeilen = Array.from(
            container.querySelectorAll('.bx--structured-list-row')
        ).filter((z) => !z.className.includes('head'));
        expect(zeilen).toHaveLength(2);
        expect(zeilen[0].textContent).toContain('Vertrag Eins');
        expect(zeilen[0].textContent).toMatch(/100,00\s*€/);
        expect(zeilen[0].textContent).toMatch(/50,00\s*€/);
        expect(zeilen[1].textContent).toMatch(/25,00\s*€/);
        expect(zeilen[1].textContent).toContain('—');
    });

    it('verlinkt Miete und Nachzahlung auf die Abschnitte des Vertrags', async () => {
        const { findByText, container } = render(WalterOffenAlsMieter, {
            vertraege: [{ id: 7, text: 'V7' }],
            fetchImpl: createRouteFetch({
                '/api/vertraege/7/mietkonto': konto(10),
                '/api/vertraege/7/abrechnungen': abrechnungen(
                    abrechnung('Abgesendet', 30, 30)
                )
            })
        });

        await findByText(/40,00\s*€ offen/);
        const hrefs = Array.from(container.querySelectorAll('a')).map((a) =>
            a.getAttribute('href')
        );
        expect(
            hrefs.some(
                (h) => h?.includes('/vertraege/7') && h.endsWith('#mietkonto')
            )
        ).toBe(true);
        expect(
            hrefs.some(
                (h) =>
                    h?.includes('/vertraege/7') && h.endsWith('#abrechnungen')
            )
        ).toBe(true);
    });

    it('zählt nur versendete Nachzahlungen, keine Entwürfe, Erstattungen oder Stornos', async () => {
        const { findByText } = render(WalterOffenAlsMieter, {
            vertraege: [{ id: 3, text: 'V3' }],
            fetchImpl: createRouteFetch({
                '/api/vertraege/3/mietkonto': konto(0),
                '/api/vertraege/3/abrechnungen': abrechnungen(
                    abrechnung('Entwurf', 90, 90, 'e'),
                    abrechnung('Storniert', 90, 90, 's'),
                    abrechnung('Abgesendet', -60, 60, 'r'),
                    abrechnung('Abgesendet', 20, 20, 'n')
                )
            })
        });

        expect(await findByText(/20,00\s*€ offen/)).toBeTruthy();
    });

    it('rechnet ein Guthaben im Mietkonto nicht gegen andere Posten', async () => {
        const { findByText } = render(WalterOffenAlsMieter, {
            vertraege: [{ id: 4, text: 'V4' }],
            fetchImpl: createRouteFetch({
                '/api/vertraege/4/mietkonto': konto(-200),
                '/api/vertraege/4/abrechnungen': abrechnungen(
                    abrechnung('Abgesendet', 30, 30)
                )
            })
        });

        expect(await findByText(/30,00\s*€ offen/)).toBeTruthy();
    });

    it('zeigt „Nichts offen“ und behandelt fehlgeschlagene Anfragen als nichts offen', async () => {
        const { findByText } = render(WalterOffenAlsMieter, {
            vertraege: [{ id: 5, text: 'V5' }],
            fetchImpl: createRouteFetch({})
        });

        expect(await findByText('Nichts offen')).toBeTruthy();
    });

    it('lädt neu, wenn sich die Verträge ändern', async () => {
        const fetchImpl = createRouteFetch({
            '/api/vertraege/1/mietkonto': konto(10),
            '/api/vertraege/1/abrechnungen': abrechnungen(),
            '/api/vertraege/2/mietkonto': konto(40),
            '/api/vertraege/2/abrechnungen': abrechnungen()
        });
        const { component, findByText } = render(WalterOffenAlsMieter, {
            vertraege: [{ id: 1, text: 'V1' }],
            fetchImpl
        });
        await findByText(/10,00\s*€ offen/);

        component.$set({ vertraege: [{ id: 2, text: 'V2' }] });

        await waitFor(async () =>
            expect(await findByText(/40,00\s*€ offen/)).toBeTruthy()
        );
    });
});
