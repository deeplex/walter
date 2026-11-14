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
import WalterStrukturpruefung from './WalterStrukturpruefung.svelte';

const ENDPUNKT = '/api/jahresabschluss/struktur';

describe('WalterStrukturpruefung', () => {
    afterEach(cleanup);

    it('zeigt den Ladezustand, bis die Antwort da ist', async () => {
        const { getByText, findByText } = render(WalterStrukturpruefung, {
            fetchImpl: createRouteFetch({
                [ENDPUNKT]: {
                    mehrdeutigeKonten: [],
                    zahlungenOhneZuordnung: [],
                    auffaellig: false
                }
            })
        });

        expect(getByText('Prüfe…')).toBeTruthy();
        expect(await findByText('Keine Auffälligkeiten')).toBeTruthy();
    });

    it('meldet, wenn alles in Ordnung ist', async () => {
        const { findByText } = render(WalterStrukturpruefung, {
            fetchImpl: createRouteFetch({
                [ENDPUNKT]: {
                    mehrdeutigeKonten: [],
                    zahlungenOhneZuordnung: [],
                    auffaellig: false
                }
            })
        });

        expect(
            await findByText(/Alle Konten haben genau einen Besitzer/)
        ).toBeTruthy();
    });

    it('listet Konten mit mehreren Besitzern und Zahlungen ohne Zuordnung', async () => {
        const { findByText, getByText, container } = render(
            WalterStrukturpruefung,
            {
                fetchImpl: createRouteFetch({
                    [ENDPUNKT]: {
                        mehrdeutigeKonten: [
                            {
                                kontoId: 5,
                                kontonummer: '1200',
                                bezeichnung: 'Bank',
                                besitzer: ['Anna', 'Ben']
                            }
                        ],
                        zahlungenOhneZuordnung: [
                            {
                                transaktionId: 't1',
                                datum: '2026-07-05',
                                betrag: 283.48,
                                text: 'Erstattung Vertrag 42',
                                storniert: false
                            }
                        ],
                        auffaellig: true
                    }
                })
            }
        );

        expect(await findByText('2 Auffälligkeiten')).toBeTruthy();
        expect(getByText('Konten mit mehreren Besitzern')).toBeTruthy();
        expect(getByText('Anna, Ben')).toBeTruthy();
        expect(
            container.querySelector('a[href="/buchungskonten/5"]')?.textContent
        ).toContain('1200 Bank');
        expect(getByText('Zahlungen ohne Zuordnung')).toBeTruthy();
        expect(
            container.querySelector('a[href="/transaktionen/t1"]')?.textContent
        ).toBe('Erstattung Vertrag 42');
    });

    it('zählt eine Auffälligkeit im Singular', async () => {
        const { findByText } = render(WalterStrukturpruefung, {
            fetchImpl: createRouteFetch({
                [ENDPUNKT]: {
                    mehrdeutigeKonten: [
                        {
                            kontoId: 1,
                            kontonummer: '1',
                            bezeichnung: 'K',
                            besitzer: ['A', 'B']
                        }
                    ],
                    zahlungenOhneZuordnung: [],
                    auffaellig: true
                }
            })
        });

        expect(await findByText('1 Auffälligkeit')).toBeTruthy();
    });

    it('zählt stornierte Zahlungen nicht mit und zeigt sie getrennt', async () => {
        const { findByText, getByText, queryByText } = render(
            WalterStrukturpruefung,
            {
                fetchImpl: createRouteFetch({
                    [ENDPUNKT]: {
                        mehrdeutigeKonten: [],
                        zahlungenOhneZuordnung: [
                            {
                                transaktionId: 't9',
                                datum: '2026-01-02',
                                betrag: 10,
                                text: 'Storniert',
                                storniert: true
                            }
                        ],
                        auffaellig: false
                    }
                })
            }
        );

        expect(await findByText('Keine Auffälligkeiten')).toBeTruthy();
        expect(
            getByText('Rückgängig gemachte Zahlungen ohne Zuordnung')
        ).toBeTruthy();
        expect(queryByText('Zahlungen ohne Zuordnung')).toBeNull();
        expect(
            queryByText(/Alle Konten haben genau einen Besitzer/)
        ).toBeNull();
    });

    it('zeigt den Fehler des Servers', async () => {
        const { findByText, queryByText } = render(WalterStrukturpruefung, {
            fetchImpl: createRouteFetch({
                [ENDPUNKT]: { status: 500, body: 'Datenbank nicht erreichbar' }
            })
        });

        expect(await findByText('Datenbank nicht erreichbar')).toBeTruthy();
        await waitFor(() => expect(queryByText('Prüfe…')).toBeNull());
    });
});
