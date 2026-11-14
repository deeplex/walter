import { describe, expect, it } from 'vitest';

import {
    erhaltungsaufwandZeilen,
    rechnungsZahlungsEintrag,
    umlageRechnungZeilen,
    type WalterErhaltungsaufwandRechnung,
    type WalterUmlageRechnung
} from './WalterRechnungen';
import { ZAHLUNGS_STATUS } from './WalterZahlungsStatus';

const ea = (
    teil: Partial<WalterErhaltungsaufwandRechnung>
): WalterErhaltungsaufwandRechnung => ({
    buchungssatzId: 's1',
    datum: '2024-03-01',
    jahr: 2024,
    bezeichnung: 'Heizungsreparatur',
    aussteller: 'Handwerker GmbH',
    betrag: 500,
    bezahlt: 200,
    offen: 300,
    storniert: false,
    direktBezahlt: false,
    kannBezahlen: true,
    zahlungen: [],
    ...teil
});

const bk = (teil: Partial<WalterUmlageRechnung>): WalterUmlageRechnung => ({
    buchungssatzId: 'b1',
    jahr: 2024,
    datum: '2024-11-01',
    betrag: 1000,
    bezahlt: 0,
    verteilt: 0,
    offen: 1000,
    storniert: false,
    kannBezahlen: true,
    zahlungen: [],
    ...teil
});

describe('erhaltungsaufwandZeilen', () => {
    it('beschreibt eine offene Rechnung und erlaubt eine Zahlung', () => {
        const [zeile] = erhaltungsaufwandZeilen({
            rechnungen: [ea({})],
            jahre: []
        });
        expect(zeile).toMatchObject({
            periode: `${new Date('2024-03-01').toLocaleDateString('de-DE')} – Heizungsreparatur – Handwerker GmbH`,
            href: '/buchungssaetze/s1',
            faellig: 500,
            bezahlt: 200,
            offen: 300,
            erfassenFuer: 's1',
            status: undefined
        });
    });

    it('stornierte Rechnungen sind storniert und nicht bezahlbar', () => {
        const [zeile] = erhaltungsaufwandZeilen({
            rechnungen: [
                ea({ storniert: true, offen: 0, kannBezahlen: false })
            ],
            jahre: []
        });
        expect(zeile.status).toEqual(ZAHLUNGS_STATUS.storniert);
        expect(zeile.erfassenFuer).toBeUndefined();
    });
});

describe('umlageRechnungZeilen', () => {
    it('zeigt, wie viel verteilt ist', () => {
        const zusatz = umlageRechnungZeilen({
            rechnungen: [
                bk({ verteilt: 1000 }),
                bk({ buchungssatzId: 'b2', verteilt: 0 }),
                bk({ buchungssatzId: 'b3', verteilt: 400 }),
                bk({ buchungssatzId: 'b4', storniert: true })
            ],
            rueckgaengigGemacht: []
        }).map((z) => z.zusatz);
        expect(zusatz).toEqual([
            { text: 'Verteilt', tag: 'green' },
            { text: 'Nicht verteilt', tag: 'warm-gray' },
            {
                text: `${(400).toLocaleString('de-DE', { style: 'currency', currency: 'EUR' })} verteilt`,
                tag: 'warm-gray'
            },
            undefined
        ]);
    });

    it('nennt Jahr und Datum und erlaubt eine Zahlung nur, solange offen', () => {
        const [offen, bezahlt] = umlageRechnungZeilen({
            rechnungen: [
                bk({}),
                bk({ buchungssatzId: 'b2', offen: 0, kannBezahlen: false })
            ],
            rueckgaengigGemacht: []
        });
        expect(offen.periode).toBe(
            `2024 – Rechnung vom ${new Date('2024-11-01').toLocaleDateString('de-DE')}`
        );
        expect(offen.erfassenFuer).toBe('b1');
        expect(bezahlt.erfassenFuer).toBeUndefined();
    });
});

describe('rechnungsZahlungsEintrag', () => {
    it('nennt Zahler und Empfänger und verlinkt die Transaktion', () => {
        expect(
            rechnungsZahlungsEintrag({
                buchungssatzId: 'z1',
                transaktionId: 't1',
                datum: '2024-12-01',
                betrag: 400,
                zahler: 'Vermieter',
                empfaenger: 'Stadtwerke'
            })
        ).toMatchObject({
            text: 'Zahlung von Vermieter an Stadtwerke',
            href: '/transaktionen/t1',
            transaktionId: 't1'
        });
    });

    it('ohne Transaktion verlinkt den Buchungssatz', () => {
        expect(
            rechnungsZahlungsEintrag({
                buchungssatzId: 'z1',
                datum: '2024-12-01',
                betrag: 400
            })
        ).toMatchObject({ text: 'Zahlung', href: '/buchungssaetze/z1' });
    });
});
