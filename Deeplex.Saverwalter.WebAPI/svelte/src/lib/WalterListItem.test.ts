import { describe, expect, it } from 'vitest';

import { zuordnungAsItems, zuordnungAsParts } from './WalterListItem';
import type { WalterZuordnungEintrag } from './WalterTransaktion';

const zuordnung: WalterZuordnungEintrag[] = [
    {
        text: 'Kaltmiete Jan 2026 – EG links',
        ref: { type: 'Vertrag', id: '1', section: 'mietkonto' },
        betrag: 500,
        storniert: false
    },
    {
        text: 'NK-Vorauszahlung 2026 – EG links',
        ref: null,
        betrag: 150,
        storniert: true
    }
];

describe('zuordnungAsItems', () => {
    it('macht je Teil einen Eintrag, rückgängig gemachte mit grauem Tag', () => {
        expect(zuordnungAsItems(zuordnung)).toEqual([
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

describe('zuordnungAsParts', () => {
    it('dimmt rückgängig gemachte Teile', () => {
        expect(zuordnungAsParts(zuordnung).map((t) => t.dimmed)).toEqual([
            false,
            true
        ]);
    });
});
