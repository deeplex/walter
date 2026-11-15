import { describe, expect, it } from 'vitest';

import { zahlungsStatus } from './WalterZahlungsStatus';

describe('zahlungsStatus', () => {
    it('ist offen, wenn nichts bezahlt wurde', () => {
        expect(zahlungsStatus(800, 0)).toMatchObject({
            art: 'offen',
            text: 'Offen',
            tag: 'red'
        });
    });

    it('ist teilweise bezahlt bei einer Teilzahlung', () => {
        expect(zahlungsStatus(800, 650)).toMatchObject({
            art: 'teilweise',
            tag: 'purple'
        });
    });

    it('ist bezahlt, wenn Fällig und Bezahlt übereinstimmen', () => {
        expect(zahlungsStatus(800, 800)).toMatchObject({
            art: 'bezahlt',
            tag: 'green'
        });
    });

    it('ignoriert Rundungsdifferenzen unter einem Cent', () => {
        expect(zahlungsStatus(800, 799.996).art).toBe('bezahlt');
        expect(zahlungsStatus(800, 800.004).art).toBe('bezahlt');
    });

    it('ist Guthaben bei einer Überzahlung', () => {
        expect(zahlungsStatus(800, 900)).toMatchObject({
            art: 'guthaben',
            tag: 'blue'
        });
    });

    it('ist bezahlt, wenn nichts fällig war und nichts kam', () => {
        expect(zahlungsStatus(0, 0).art).toBe('bezahlt');
    });

    it('ist rückgängig gemacht unabhängig von den Beträgen', () => {
        expect(zahlungsStatus(800, 0, { rueckgaengig: true })).toMatchObject({
            art: 'rueckgaengig',
            text: 'Rückgängig gemacht',
            tag: 'gray'
        });
    });
});
