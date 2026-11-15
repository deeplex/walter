import { describe, expect, it } from 'vitest';

import {
    abrechnungsZeilen,
    type WalterNebenkostenabrechnung,
    type WalterNebenkostenabrechnungen
} from './WalterNebenkostenabrechnungen';
import { ZAHLUNGS_STATUS } from './WalterZahlungsStatus';

const abrechnung = (
    part: Partial<WalterNebenkostenabrechnung>
): WalterNebenkostenabrechnung => ({
    abrechnungsresultatId: 'r1',
    jahr: 2024,
    status: 'Abgesendet',
    saldo: 120,
    betrag: 120,
    bezahlt: 0,
    offen: 120,
    kannAusgleichen: true,
    zahlungen: [],
    ...part
});

const daten = (
    ...abrechnungen: WalterNebenkostenabrechnung[]
): WalterNebenkostenabrechnungen => ({
    abrechnungen,
    rueckgaengigGemacht: []
});

describe('abrechnungsZeilen', () => {
    it('beschreibt eine offene Nachzahlung und erlaubt eine Zahlung', () => {
        const [zeile] = abrechnungsZeilen(daten(abrechnung({})));
        expect(zeile).toMatchObject({
            periode: 'Abrechnung 2024 – Nachzahlung',
            href: '/abrechnungsresultate/r1',
            faellig: 120,
            offen: 120,
            erfassenFuer: 'r1',
            status: undefined
        });
    });

    it('nennt Erstattungen und ihre Empfänger', () => {
        const [zeile] = abrechnungsZeilen(
            daten(
                abrechnung({
                    saldo: -80,
                    betrag: 80,
                    bezahlt: 80,
                    offen: 0,
                    kannAusgleichen: false,
                    zahlungen: [
                        {
                            buchungssatzId: 's1',
                            transaktionId: 't1',
                            datum: '2025-03-05',
                            betrag: 80,
                            zahler: 'Vermieter',
                            empfaenger: 'Müller',
                            erstattung: true
                        }
                    ]
                })
            )
        );
        expect(zeile.periode).toBe('Abrechnung 2024 – Erstattung');
        expect(zeile.erfassenFuer).toBeUndefined();
        expect(zeile.zahlungen).toEqual([
            {
                id: 's1',
                datum: '2025-03-05',
                betrag: 80,
                text: 'Erstattung – an Müller',
                href: '/transaktionen/t1',
                transaktionId: 't1',
                buchungssatzId: 's1'
            }
        ]);
    });

    it('zeigt Entwurf und Storno als eigenen Status', () => {
        const zeilen = abrechnungsZeilen(
            daten(
                abrechnung({ status: 'Entwurf', kannAusgleichen: false }),
                abrechnung({
                    abrechnungsresultatId: 'r0',
                    status: 'Storniert',
                    offen: 0,
                    kannAusgleichen: false
                })
            )
        );
        expect(zeilen.map((z) => z.status)).toEqual([
            ZAHLUNGS_STATUS.nichtVersendet,
            ZAHLUNGS_STATUS.storniert
        ]);
        expect(zeilen.every((z) => z.erfassenFuer === undefined)).toBe(true);
    });
});
