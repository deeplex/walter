import { describe, expect, it } from 'vitest';

import { entityRefHref } from './WalterEntityRef';

describe('entityRefHref', () => {
    it('baut Route und Abschnitt', () => {
        expect(
            entityRefHref({ type: 'Vertrag', id: 3, section: 'mietkonto' })
        ).toBe('/vertraege/3#mietkonto');
        expect(
            entityRefHref({ type: 'Vertrag', id: 3, section: 'kaution' })
        ).toBe('/vertraege/3/kaution');
        expect(entityRefHref({ type: 'GarageVertrag', id: '7' })).toBe(
            '/garage-vertraege/7'
        );
        expect(
            entityRefHref({ type: 'Buchungskonto', id: '12', section: null })
        ).toBe('/buchungskonten/12');
    });

    it('ohne Referenz kein Link', () => {
        expect(entityRefHref(undefined)).toBeUndefined();
        expect(entityRefHref(null)).toBeUndefined();
    });
});
