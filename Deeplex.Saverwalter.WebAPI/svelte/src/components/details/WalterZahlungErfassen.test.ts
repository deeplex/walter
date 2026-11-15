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

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, waitFor } from '@testing-library/svelte';
import type { WalterZahlungsZiel } from '$walter/lib';
import { createMockFetch } from '../../test-helpers/mock-fetch';
import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';

const mocks = vi.hoisted(() => ({
    walter_post: vi.fn(),
    bankkontos: vi.fn()
}));

vi.mock('$walter/services/requests', async (importOriginal) => {
    const original =
        await importOriginal<typeof import('$walter/services/requests')>();
    return {
        ...original,
        walter_post: mocks.walter_post,
        walter_selection: {
            ...original.walter_selection,
            bankkontos: mocks.bankkontos
        }
    };
});

const VORSCHAU = '/api/transaktionen/buchen/vorschau';
const BUCHEN = '/api/transaktionen/buchen';

const vorschauAntwort = {
    betrag: 500,
    zahler: null,
    positionen: [{ text: 'Kaltmiete Januar 2024', betrag: 500 }],
    saetze: []
};

function antwort(ok: boolean, body: unknown) {
    return new Response(
        typeof body === 'string' ? body : JSON.stringify(body),
        {
            status: ok ? 200 : 400
        }
    );
}

/** Antwortet auf Vorschau und Buchen; je Endpunkt überschreibbar. */
function server(
    handlers: { vorschau?: () => Response; buchen?: () => Response } = {}
) {
    mocks.walter_post.mockImplementation(async (url: string) => {
        if (url === VORSCHAU)
            return handlers.vorschau?.() ?? antwort(true, vorschauAntwort);
        if (url === BUCHEN) return handlers.buchen?.() ?? antwort(true, {});
        throw new Error(`Unerwartete Anfrage ${url}`);
    });
}

function aufrufe(url: string) {
    return mocks.walter_post.mock.calls.filter((c) => c[0] === url);
}

const miete: WalterZahlungsZiel = {
    art: 'miete',
    vertragId: 42,
    monat: '2024-01-01'
};

function oeffne(props: Record<string, unknown> = {}) {
    const result = render(WalterZahlungErfassen, {
        fetchImpl: vi.fn() as unknown as typeof fetch,
        ziel: miete,
        titel: 'Jan 2024',
        vorschlag: 500,
        open: true,
        ...props
    });
    const gebucht = vi.fn();
    result.component.$on('gebucht', gebucht);
    return { ...result, gebucht };
}

function buchenKnopf(container: HTMLElement) {
    const knopf = Array.from(
        container.ownerDocument.querySelectorAll('button')
    ).find((b) => b.textContent?.trim() === 'Buchen');
    expect(knopf).toBeDefined();
    return knopf as HTMLButtonElement;
}

describe('WalterZahlungErfassen', () => {
    beforeEach(() => {
        mocks.walter_post.mockReset();
        mocks.bankkontos.mockReset();
        mocks.bankkontos.mockResolvedValue([
            { id: 7, text: 'Konto Vermieter' },
            { id: 9, text: 'Konto Mieter' }
        ]);
        server();
    });
    afterEach(cleanup);

    it('zeigt Überschrift und den vorbelegten Betrag', () => {
        const { getByText, getByLabelText } = oeffne({ vorschlag: 123.456 });

        expect(getByText('Zahlung erfassen – Jan 2024')).toBeTruthy();
        const betrag = getByLabelText('Betrag (€)') as HTMLInputElement;
        expect(betrag.value).toBe('123.46');
    });

    it('fragt ohne Betrag nichts an und lässt nicht buchen', async () => {
        const { container, getByText } = oeffne({ vorschlag: 0 });

        expect(getByText('Datum und Betrag eingeben.')).toBeTruthy();
        expect(buchenKnopf(container).disabled).toBe(true);
        await new Promise((r) => setTimeout(r, 450));
        expect(mocks.walter_post).not.toHaveBeenCalled();
    });

    it('holt die Vorschau einer Miete mit Gesamtbetrag und gibt Buchen frei', async () => {
        const { container, findByText } = oeffne();

        await findByText(/Kaltmiete Januar 2024/);
        await waitFor(() =>
            expect(buchenKnopf(container).disabled).toBe(false)
        );
        const [, input] = aufrufe(VORSCHAU)[0];
        expect(input).toMatchObject({
            betrag: 500,
            mieten: [
                {
                    vertragId: 42,
                    betreffenderMonat: '2024-01-01',
                    kaltmiete: 0,
                    nkVorauszahlung: 0,
                    gesamtbetrag: 500
                }
            ]
        });
    });

    it('bucht über den Endpunkt, schließt den Dialog und meldet gebucht', async () => {
        const { container, gebucht, findByText } = oeffne();
        await findByText(/Kaltmiete Januar 2024/);
        await waitFor(() =>
            expect(buchenKnopf(container).disabled).toBe(false)
        );

        await fireEvent.click(buchenKnopf(container));

        await waitFor(() => expect(gebucht).toHaveBeenCalledTimes(1));
        expect(aufrufe(BUCHEN)).toHaveLength(1);
        expect(aufrufe(BUCHEN)[0][1]).toMatchObject({ betrag: 500 });
        await waitFor(() =>
            expect(
                container.ownerDocument.querySelector('.bx--modal.is-visible')
            ).toBeNull()
        );
    });

    it('zeigt den Fehler des Servers beim Buchen und meldet nicht gebucht', async () => {
        server({
            buchen: () => antwort(false, 'Kaltmiete übersteigt Forderung')
        });
        const { container, gebucht, findByText } = oeffne();
        await findByText(/Kaltmiete Januar 2024/);
        await waitFor(() =>
            expect(buchenKnopf(container).disabled).toBe(false)
        );

        await fireEvent.click(buchenKnopf(container));

        expect(await findByText('Kaltmiete übersteigt Forderung')).toBeTruthy();
        expect(gebucht).not.toHaveBeenCalled();
    });

    it('zeigt eine abgelehnte Vorschau und sperrt Buchen', async () => {
        server({ vorschau: () => antwort(false, 'Jahr bereits abgerechnet') });
        const { container, findByText } = oeffne();

        expect(await findByText('Jahr bereits abgerechnet')).toBeTruthy();
        expect(buchenKnopf(container).disabled).toBe(true);
        expect(aufrufe(BUCHEN)).toHaveLength(0);
    });

    it('verteilt eine Miete über den Server, die anderen Ziele tragen den Betrag selbst', async () => {
        const faelle: [WalterZahlungsZiel, Record<string, unknown>][] = [
            [
                {
                    art: 'garage',
                    garageVertragId: 3,
                    monat: '2024-01-01',
                    kennung: 'G1'
                },
                {
                    garagenEingaenge: [
                        {
                            garageVertragId: 3,
                            garageKennung: 'G1',
                            betreffenderMonat: '2024-01-01',
                            betrag: 500
                        }
                    ]
                }
            ],
            [
                { art: 'bkRechnung', buchungssatzId: 'abc' },
                {
                    betriebskostenEingaenge: [
                        { existingBuchungssatzId: 'abc', betrag: 500 }
                    ]
                }
            ],
            [
                { art: 'bkNeu', umlageId: 5, jahr: 2023 },
                {
                    betriebskostenEingaenge: [
                        { umlageId: 5, betreffendesJahr: 2023, betrag: 500 }
                    ]
                }
            ],
            [
                {
                    art: 'abrechnung',
                    abrechnungsresultatId: 'r1',
                    erstattung: false
                },
                {
                    abrechnungsAusgleiche: [
                        { abrechnungsresultatId: 'r1', betrag: 500 }
                    ]
                }
            ]
        ];

        for (const [ziel, erwartet] of faelle) {
            mocks.walter_post.mockClear();
            const { unmount } = oeffne({ ziel });
            await waitFor(() => expect(aufrufe(VORSCHAU)).toHaveLength(1));
            expect(aufrufe(VORSCHAU)[0][1]).toMatchObject(erwartet);
            expect(aufrufe(VORSCHAU)[0][1].mieten).toEqual([]);
            unmount();
        }
    });

    it('verlangt bei Erhaltungsaufwand ein Zahlerkonto und belegt es mit dem Vermieter vor', async () => {
        const ziel: WalterZahlungsZiel = {
            art: 'erhaltungsaufwand',
            buchungssatzId: 'ea1',
            wohnungId: 11
        };

        // Ohne bekanntes Konto: keine Anfrage, Buchen gesperrt.
        const ohne = oeffne({ ziel });
        await new Promise((r) => setTimeout(r, 450));
        expect(mocks.walter_post).not.toHaveBeenCalled();
        expect(buchenKnopf(ohne.container).disabled).toBe(true);
        ohne.unmount();

        // Mit Vermieterkonto: der Vermieter zahlt.
        oeffne({ ziel, vermieterBankkontoId: 7 });
        await waitFor(() => expect(aufrufe(VORSCHAU)).toHaveLength(1));
        const [, input] = aufrufe(VORSCHAU)[0];
        expect(input).toMatchObject({
            zahlerId: 7,
            erhaltungsaufwendungen: [
                { existingBuchungssatzId: 'ea1', wohnungId: 11, betrag: 500 }
            ]
        });
        expect(input.zahlungsempfaengerId).toBeUndefined();
    });

    it('trägt bei einer Miete den Vermieter als Empfänger ein, bei einer Erstattung als Zahler', async () => {
        const mietzahlung = oeffne({ vermieterBankkontoId: 7 });
        await waitFor(() => expect(aufrufe(VORSCHAU)).toHaveLength(1));
        expect(aufrufe(VORSCHAU)[0][1]).toMatchObject({
            zahlungsempfaengerId: 7
        });
        expect(aufrufe(VORSCHAU)[0][1].zahlerId).toBeUndefined();
        mietzahlung.unmount();

        mocks.walter_post.mockClear();
        const erstattung = oeffne({
            ziel: {
                art: 'abrechnung',
                abrechnungsresultatId: 'r1',
                erstattung: true
            },
            vermieterBankkontoId: 7
        });
        await waitFor(() => expect(aufrufe(VORSCHAU)).toHaveLength(1));
        expect(aufrufe(VORSCHAU)[0][1]).toMatchObject({ zahlerId: 7 });
        expect(aufrufe(VORSCHAU)[0][1].zahlungsempfaengerId).toBeUndefined();
        expect(
            erstattung.getByText('Bankkonto des Empfängers (optional)')
        ).toBeTruthy();
    });

    it('wechselt mit „Erweitert …“ ins volle Formular und behält die Werte', async () => {
        const { getByText, container } = oeffne({
            fetchImpl: createMockFetch()
        });
        await waitFor(() => expect(aufrufe(VORSCHAU)).toHaveLength(1));

        await fireEvent.click(getByText('Erweitert …'));

        await waitFor(() =>
            expect(
                container.ownerDocument.querySelector(
                    '.bx--modal-container--lg'
                )
            ).not.toBeNull()
        );
    });
});
