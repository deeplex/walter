import { describe, expect, it } from 'vitest';

import {
    mietkontoZeilen,
    zahlungsEintrag,
    type WalterMietkonto,
    type WalterMietkontoZahlung
} from './WalterMietkonto';

const zahlung = (
    teil: Partial<WalterMietkontoZahlung>
): WalterMietkontoZahlung => ({
    buchungssatzId: 's1',
    transaktionId: 't1',
    datum: '2024-02-02',
    betrag: 500,
    zahler: 'Müller',
    art: 'Kaltmiete',
    ...teil
});

const konto: WalterMietkonto = {
    monate: [
        {
            jahr: 2024,
            monat: 1,
            faellig: 550,
            bezahlt: 500,
            offen: 50,
            kaltmiete: 500,
            garagen: [
                { garageVertragId: 1, kennung: 'G1', faellig: 50, bezahlt: 0 }
            ],
            zahlungen: [zahlung({})]
        },
        {
            jahr: 2024,
            monat: 2,
            faellig: 500,
            bezahlt: 0,
            offen: 500,
            kaltmiete: 500,
            garagen: [],
            zahlungen: []
        },
        {
            jahr: 2023,
            monat: 12,
            faellig: 500,
            bezahlt: 500,
            offen: 0,
            kaltmiete: 500,
            garagen: [],
            zahlungen: []
        }
    ],
    nkVorauszahlungen: [
        { jahr: 2024, faellig: 300, bezahlt: 150, offen: 150, zahlungen: [] },
        { jahr: 2023, faellig: 150, bezahlt: 150, offen: 0, zahlungen: [] }
    ],
    nichtZugeordnet: [],
    rueckgaengigGemacht: [],
    offen: 700
};

describe('mietkontoZeilen', () => {
    it('ordnet je Jahr die Monate absteigend, danach die NK-Vorauszahlungen', () => {
        expect(mietkontoZeilen(konto).map((z) => z.periode)).toEqual([
            'Feb 2024',
            'Jan 2024',
            'NK-Vorauszahlungen 2024',
            'Dez 2023',
            'NK-Vorauszahlungen 2023'
        ]);
    });

    it('filtert auf ein Jahr', () => {
        expect(mietkontoZeilen(konto, 2023).map((z) => z.periode)).toEqual([
            'Dez 2023',
            'NK-Vorauszahlungen 2023'
        ]);
    });

    it('schlüsselt Monate mit Garage in Kaltmiete und Garage auf', () => {
        const januar = mietkontoZeilen(konto).find(
            (z) => z.periode === 'Jan 2024'
        )!;
        expect(januar.aufschluesselung).toEqual([
            { text: 'Kaltmiete', faellig: 500, bezahlt: 500 },
            { text: 'Garage G1', faellig: 50, bezahlt: 0 }
        ]);
    });

    it('schlüsselt Monate ohne Garage nicht auf', () => {
        const februar = mietkontoZeilen(konto).find(
            (z) => z.periode === 'Feb 2024'
        )!;
        expect(februar.aufschluesselung).toEqual([]);
    });
});

describe('zahlungsEintrag', () => {
    it('beschreibt die Zahlung aus Art und Zahler und verlinkt die Transaktion', () => {
        expect(zahlungsEintrag(zahlung({}))).toMatchObject({
            text: 'Kaltmiete – von Müller',
            href: '/transaktionen/t1'
        });
    });

    it('verlinkt Buchungen ohne Transaktion auf den Buchungssatz', () => {
        expect(
            zahlungsEintrag(
                zahlung({
                    art: 'MietKorrektur',
                    transaktionId: null,
                    zahler: null,
                    betrag: -100
                })
            )
        ).toMatchObject({
            text: 'Miet-Korrektur (zur Kaltmiete umgebucht)',
            href: '/buchungssaetze/s1'
        });
    });

    it('nennt die Garage', () => {
        expect(
            zahlungsEintrag(
                zahlung({ art: 'Garage', garage: 'G1', zahler: null })
            ).text
        ).toBe('Garage G1');
    });
});
