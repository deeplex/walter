import { describe, expect, it } from 'vitest';

import {
    formatDatum,
    formatEuro,
    formatEuroOderStrich,
    formatMonat,
    formatProzent,
    formatQuadratmeter,
    formatZahl
} from './format';

// toLocaleString trennt Zahl und Einheit mit einem geschützten Leerzeichen.
const nb = (s: string) => s.replace(/\u00a0/g, ' ');

describe('format', () => {
    it('formatiert Beträge deutsch', () => {
        expect(nb(formatEuro(1234.5))).toBe('1.234,50 €');
        expect(nb(formatEuro(-3))).toBe('-3,00 €');
        expect(nb(formatEuro(undefined))).toBe('0,00 €');
    });

    it('zeigt null als Strich', () => {
        expect(formatEuroOderStrich(0)).toBe('—');
        expect(formatEuroOderStrich(0.004)).toBe('—');
        expect(formatEuroOderStrich(undefined)).toBe('—');
        expect(nb(formatEuroOderStrich(12))).toBe('12,00 €');
    });

    it('formatiert Daten zweistellig und ohne Zeitzonen-Verschiebung', () => {
        expect(formatDatum('2026-01-02')).toBe('02.01.2026');
        expect(formatDatum(new Date(2023, 4, 24))).toBe('24.05.2023');
    });

    it('formatiert Monate, Zahlen, Prozent und Fläche', () => {
        expect(formatMonat(2026, 3)).toBe('Mär 2026');
        expect(formatZahl(1234.5)).toBe('1.234,5');
        expect(formatZahl(812)).toBe('812');
        expect(nb(formatProzent(0.1234))).toBe('12,34 %');
        expect(nb(formatQuadratmeter(42))).toBe('42,00 m²');
    });
});
