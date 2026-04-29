import { beforeEach, describe, expect, it, vi } from 'vitest';

const walterGotoMock = vi.fn();

vi.mock('./utils', () => ({
    walter_goto: walterGotoMock
}));

describe('navigation service', () => {
    beforeEach(() => {
        vi.resetModules();
        walterGotoMock.mockReset();
    });

    it('routes every entity helper to the correct page path', async () => {
        const { navigation } = await import('./navigation');

        const expectations: Array<
            [keyof typeof navigation, string | number, string]
        > = [
            ['abrechnungsresultat', 'abc', '/abrechnungsresultate/abc'],
            ['account', 1, '/accounts/1'],
            ['adresse', 2, '/adressen/2'],
            ['betriebskostenrechnung', 3, '/betriebskostenrechnungen/3'],
            ['erhaltungsaufwendung', 4, '/erhaltungsaufwendungen/4'],
            ['kontakt', 5, '/kontakte/5'],
            ['miete', 6, '/mieten/6'],
            ['mietminderung', 7, '/mietminderungen/7'],
            ['transaktion', 8, '/transaktionen/8'],
            ['umlage', 9, '/umlagen/9'],
            ['umlagetyp', 10, '/umlagetypen/10'],
            ['vertrag', 11, '/vertraege/11'],
            ['vertragversion', 12, '/vertragversionen/12'],
            ['wohnung', 13, '/wohnungen/13'],
            ['zaehler', 14, '/zaehler/14'],
            ['zaehlerstand', 15, '/zaehlerstaende/15']
        ];

        for (const [method, id] of expectations) {
            await navigation[method](id as never);
        }

        expect(walterGotoMock.mock.calls.map((call) => call[0])).toEqual(
            expectations.map(([, , path]) => path)
        );
    });
});
