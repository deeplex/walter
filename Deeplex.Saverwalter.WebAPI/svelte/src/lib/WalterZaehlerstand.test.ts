import { describe, expect, it } from 'vitest';

import { zaehlerstandItems } from './WalterZaehlerstand';

const stand = (id: number, datum: string, wert: number, notiz = '') => ({
    id,
    datum,
    stand: wert,
    einheit: 'm³',
    notiz
});

describe('zaehlerstandItems', () => {
    it('zeigt den neuesten Stand zuerst, mit Verbrauch seit dem vorigen', () => {
        const items = zaehlerstandItems([
            stand(1, '2024-01-01', 100),
            stand(3, '2026-01-01', 1234.5, 'Ablesung durch Mieter'),
            stand(2, '2025-01-01', 1000)
        ]);

        expect(items.map((e) => e.id)).toEqual(['3', '2', '1']);
        expect(items[0]).toMatchObject({
            datum: '2026-01-01',
            wert: '1.234,5 m³',
            text: '+234,5 m³ seit 01.01.2025 – Ablesung durch Mieter',
            href: '/zaehlerstaende/3'
        });
        expect(items[2].text).toBe('Erster Stand');
    });

    it('zeigt einen gesunkenen Stand (z. B. Zählertausch) mit Minus', () => {
        const [neu] = zaehlerstandItems([
            stand(1, '2024-01-01', 500),
            stand(2, '2025-01-01', 20)
        ]);
        expect(neu.text).toBe('−480 m³ seit 01.01.2024');
    });
});
