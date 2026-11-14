import { describe, expect, it } from 'vitest';

import { zuordnungAlsEintraege, zuordnungAlsTeile } from './WalterEintrag';

const zuordnung = [
    {
        text: 'Kaltmiete Jan 2026 – EG links',
        href: '/vertraege/1#mietkonto',
        betrag: 500,
        storniert: false
    },
    {
        text: 'NK-Vorauszahlung 2026 – EG links',
        href: null,
        betrag: 150,
        storniert: true
    }
];

describe('zuordnungAlsEintraege', () => {
    it('macht je Teil einen Eintrag, rückgängig gemachte mit grauem Tag', () => {
        expect(zuordnungAlsEintraege(zuordnung)).toEqual([
            {
                id: 'zuordnung-0',
                betrag: 500,
                text: 'Kaltmiete Jan 2026 – EG links',
                href: '/vertraege/1#mietkonto',
                tag: undefined
            },
            {
                id: 'zuordnung-1',
                betrag: 150,
                text: 'NK-Vorauszahlung 2026 – EG links',
                href: undefined,
                tag: { text: 'Rückgängig gemacht', tag: 'gray' }
            }
        ]);
    });
});

describe('zuordnungAlsTeile', () => {
    it('dimmt rückgängig gemachte Teile', () => {
        expect(zuordnungAlsTeile(zuordnung).map((t) => t.gedimmt)).toEqual([
            false,
            true
        ]);
    });
});
