import { describe, expect, it } from 'vitest';

import { zuordnungText, zuordnungZelle } from './WalterTransaktion';

describe('zuordnungZelle', () => {
    it('ohne Eintrag: nicht zugeordnet', () => {
        expect(zuordnungZelle([])).toEqual({
            text: 'Nicht zugeordnet',
            tag: 'warm-gray'
        });
        expect(zuordnungZelle(undefined)).toEqual({
            text: 'Nicht zugeordnet',
            tag: 'warm-gray'
        });
    });

    it('verbindet die Einträge und markiert stornierte', () => {
        expect(
            zuordnungZelle([
                {
                    text: 'Kaltmiete Mär 2026 – 3 OG',
                    storniert: false,
                    betrag: 1
                },
                {
                    text: 'NK-Vorauszahlung 2026 – 3 OG',
                    storniert: true,
                    betrag: 1
                }
            ])
        ).toBe(
            'Kaltmiete Mär 2026 – 3 OG; NK-Vorauszahlung 2026 – 3 OG (rückgängig gemacht)'
        );
    });

    it('zuordnungText ohne Storno ist der Text', () => {
        expect(
            zuordnungText({
                text: 'Kaution – 3 OG',
                storniert: false,
                betrag: 1
            })
        ).toBe('Kaution – 3 OG');
    });
});
