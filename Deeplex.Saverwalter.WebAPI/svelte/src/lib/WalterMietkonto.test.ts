import { describe, expect, it } from 'vitest';

import {
    mietkontoZeilen,
    zahlungsEintrag,
    type WalterMietkonto,
    type WalterMietkontoMonat,
    type WalterMietkontoZahlung
} from './WalterMietkonto';

const zahlung = (
    part: Partial<WalterMietkontoZahlung>
): WalterMietkontoZahlung => ({
    buchungssatzId: 's1',
    transaktionId: 't1',
    datum: '2024-02-02',
    betrag: 500,
    zahler: 'Müller',
    art: 'Kaltmiete',
    ...part
});

const ohneTransaktionen: Pick<
    WalterMietkontoMonat,
    'transaktionen' | 'transaktionenBetrag' | 'nkVorauszahlung'
> = { transaktionen: [], transaktionenBetrag: 0, nkVorauszahlung: 0 };

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
            zahlungen: [zahlung({})],
            transaktionen: [
                {
                    transaktionId: 't1',
                    datum: '2024-02-02',
                    betrag: 700,
                    zahler: 'Müller',
                    nkVorauszahlung: 150,
                    zuordnung: [
                        {
                            text: 'Kaltmiete Jan 2024',
                            betrag: 500,
                            storniert: false
                        },
                        {
                            text: 'NK-Vorauszahlung 2024',
                            betrag: 150,
                            storniert: false
                        },
                        { text: 'Kaution', betrag: 50, storniert: false }
                    ]
                }
            ],
            transaktionenBetrag: 700,
            nkVorauszahlung: 150
        },
        {
            jahr: 2024,
            monat: 2,
            faellig: 500,
            bezahlt: 0,
            offen: 500,
            kaltmiete: 500,
            garagen: [],
            zahlungen: [],
            ...ohneTransaktionen
        },
        {
            jahr: 2023,
            monat: 12,
            faellig: 500,
            bezahlt: 500,
            offen: 0,
            kaltmiete: 500,
            garagen: [],
            zahlungen: [],
            ...ohneTransaktionen
        }
    ],
    nichtZugeordnet: [],
    rueckgaengigGemacht: [],
    offen: 700
};

describe('mietkontoZeilen', () => {
    it('ordnet die Monate absteigend, ohne eigene NK-Zeile', () => {
        expect(mietkontoZeilen(konto).map((z) => z.periode)).toEqual([
            'Feb 2024',
            'Jan 2024',
            'Dez 2023'
        ]);
    });

    it('filtert auf ein Jahr', () => {
        expect(mietkontoZeilen(konto, 2023).map((z) => z.periode)).toEqual([
            'Dez 2023'
        ]);
    });

    it('zeigt die ganze Transaktion mit NK-Anteil und Zuordnung', () => {
        const januar = mietkontoZeilen(konto).find(
            (z) => z.periode === 'Jan 2024'
        )!;
        expect(januar).toMatchObject({
            transaktionen: 700,
            nkVorauszahlung: 150
        });
        expect(januar.zahlungen).toHaveLength(1);
        expect(januar.zahlungen[0]).toMatchObject({
            betrag: 700,
            text: 'Zahlung von Müller',
            href: '/transaktionen/t1',
            transaktionId: 't1'
        });
        expect(januar.zahlungen[0].parts).toEqual([
            {
                text: 'Kaltmiete Jan 2024',
                betrag: 500,
                href: undefined,
                dimmed: false
            },
            {
                text: 'NK-Vorauszahlung 2024',
                betrag: 150,
                href: undefined,
                dimmed: false
            },
            { text: 'Kaution', betrag: 50, href: undefined, dimmed: false }
        ]);
    });

    it('schlüsselt Monate mit Garage in Kaltmiete und Garage auf', () => {
        const januar = mietkontoZeilen(konto).find(
            (z) => z.periode === 'Jan 2024'
        )!;
        expect(januar.breakdown).toEqual([
            { text: 'Kaltmiete', faellig: 500, bezahlt: 500 },
            { text: 'Garage G1', faellig: 50, bezahlt: 0 }
        ]);
    });

    it('schlüsselt Monate ohne Garage nicht auf', () => {
        const februar = mietkontoZeilen(konto).find(
            (z) => z.periode === 'Feb 2024'
        )!;
        expect(februar.breakdown).toEqual([]);
    });

    it('schlüsselt Monate des Garagenmietkontos nicht auf', () => {
        const garagenkonto: WalterMietkonto = {
            monate: [
                {
                    jahr: 2024,
                    monat: 1,
                    faellig: 50,
                    bezahlt: 20,
                    offen: 30,
                    kaltmiete: 0,
                    garagen: [
                        {
                            garageVertragId: 1,
                            kennung: 'G1',
                            faellig: 50,
                            bezahlt: 20
                        }
                    ],
                    zahlungen: [],
                    ...ohneTransaktionen
                }
            ],
            nichtZugeordnet: [],
            rueckgaengigGemacht: [],
            offen: 30
        };
        const [januar] = mietkontoZeilen(garagenkonto);
        expect(januar).toMatchObject({
            periode: 'Jan 2024',
            offen: 30,
            breakdown: [],
            erfassenFuer: '2024-01-01'
        });
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
            zahlungsEintrag(zahlung({ transaktionId: null, zahler: null }))
        ).toMatchObject({
            text: 'Kaltmiete',
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
