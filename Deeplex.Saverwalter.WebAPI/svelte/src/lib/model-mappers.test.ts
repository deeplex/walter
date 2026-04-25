import { describe, expect, it } from 'vitest';

import { WalterAbrechnungsresultatEntry } from './WalterAbrechnungsresultat';
import { WalterAccountEntry } from './WalterAccount';
import { WalterHKVOEntry } from './WalterHKVO';
import { WalterMieteEntry } from './WalterMiete';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVerwalterEntry } from './WalterVerwalter';

describe('model mapper helpers', () => {
    it('maps account JSON including nested role and verwalter entries', () => {
        const result = WalterAccountEntry.fromJson({
            id: 1,
            username: 'admin.dev',
            name: 'Admin',
            role: { id: 2, text: 'Admin', filter: 'admin' },
            createdAt: new Date('2026-04-25T10:00:00Z'),
            lastModified: new Date('2026-04-25T11:00:00Z'),
            resetToken: 'token',
            resetTokenExpires: new Date('2026-04-26T11:00:00Z'),
            verwalter: [
                {
                    id: 10,
                    rolle: { id: 1, text: 'Eigentuemer' },
                    wohnung: { id: 5, text: 'Wohnung 5' },
                    createdAt: new Date('2026-04-20T10:00:00Z'),
                    lastModified: new Date('2026-04-21T10:00:00Z'),
                    permissions: { read: true, update: true, remove: false }
                }
            ]
        } as never);

        expect(result.role).toBeInstanceOf(WalterSelectionEntry);
        expect(result.verwalter[0]).toBeInstanceOf(WalterVerwalterEntry);
        expect(result.verwalter[0].rolle.text).toBe('Eigentuemer');
        expect(result.verwalter[0].permissions).toBeInstanceOf(WalterPermissions);
    });

    it('maps abrechnungsresultat JSON and builds its id URL', () => {
        const result = WalterAbrechnungsresultatEntry.fromJson({
            id: 'abr-1',
            vertrag: { id: 9, text: 'Vertrag 9' },
            jahr: 2025,
            kaltmiete: 100,
            vorauszahlung: 20,
            rechnungsbetrag: 30,
            minderung: 0,
            abgesendet: true,
            saldo: 10,
            notiz: 'ready',
            createdAt: new Date('2026-04-25T10:00:00Z'),
            lastModified: new Date('2026-04-25T11:00:00Z'),
            permissions: { read: true, update: false, remove: false }
        } as never);

        expect(WalterAbrechnungsresultatEntry.ApiURLId('abr-1')).toBe(
            '/api/abrechnungsresultate/abr-1'
        );
        expect(result.vertrag).toBeInstanceOf(WalterSelectionEntry);
        expect(result.permissions).toBeInstanceOf(WalterPermissions);
        expect(result.abgesendet).toBe(true);
    });

    it('maps miete JSON with nested vertrag and permissions', () => {
        const result = WalterMieteEntry.fromJson({
            id: 22,
            betreffenderMonat: '2026-04',
            zahlungsdatum: '2026-04-02',
            betrag: 750,
            notiz: 'paid',
            repeat: 1,
            createdAt: new Date('2026-04-01T10:00:00Z'),
            lastModified: new Date('2026-04-02T10:00:00Z'),
            vertrag: { id: 4, text: 'Vertrag 4' },
            permissions: { read: true, update: true, remove: false }
        } as never);

        expect(result.vertrag).toBeInstanceOf(WalterSelectionEntry);
        expect(result.permissions).toBeInstanceOf(WalterPermissions);
        expect(result.repeat).toBe(1);
    });

    it('maps hkvo JSON including optional selections and permissions', () => {
        const result = WalterHKVOEntry.fromJson({
            id: 7,
            notiz: 'hkvo',
            hkvO_P7: 50,
            hkvO_P8: 70,
            hkvO_P9: { id: 11, text: 'Sonderfall' },
            strompauschale: 25,
            stromrechnung: { id: 12, text: 'Rechnung 12' },
            createdAt: new Date('2026-04-25T10:00:00Z'),
            lastModified: new Date('2026-04-25T11:00:00Z'),
            permissions: { read: true, update: false, remove: false }
        } as never);

        expect(result.hkvO_P9).toBeInstanceOf(WalterSelectionEntry);
        expect(result.stromrechnung).toBeInstanceOf(WalterSelectionEntry);
        expect(result.permissions).toBeInstanceOf(WalterPermissions);
    });

    it('maps direct selection, permissions and verwalter entries', () => {
        const selection = WalterSelectionEntry.fromJson({
            id: 'sel-1',
            text: 'Selection',
            filter: 'filter'
        } as never);
        const permissions = WalterPermissions.fromJson({
            read: true,
            update: false,
            remove: true
        } as never);
        const verwalter = WalterVerwalterEntry.fromJson({
            id: 2,
            rolle: selection,
            wohnung: { id: 3, text: 'Wohnung 3' },
            createdAt: new Date('2026-04-25T10:00:00Z'),
            lastModified: new Date('2026-04-25T11:00:00Z'),
            permissions
        } as never);

        expect(selection.filter).toBe('filter');
        expect(permissions.remove).toBe(true);
        expect(verwalter.rolle).toBeInstanceOf(WalterSelectionEntry);
        expect(verwalter.wohnung.text).toBe('Wohnung 3');
        expect(verwalter.permissions).toBeInstanceOf(WalterPermissions);
    });
});