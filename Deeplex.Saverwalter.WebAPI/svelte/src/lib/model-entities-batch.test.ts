import { describe, expect, it } from 'vitest';

import { WalterAdresseEntry } from './WalterAdresse';
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterErhaltungsaufwendungEntry } from './WalterErhaltungsaufwendung';
import { WalterKontaktEntry } from './WalterKontakt';
import { WalterMiettabelleEntry } from './WalterMiettabelle';
import { WalterMietminderungEntry } from './WalterMietminderung';
import { WalterToastContent } from './WalterToastContent';
import { WalterTransaktionEntry } from './WalterTransaktion';
import { WalterUmlageEntry } from './WalterUmlage';
import { WalterUmlagetypEntry } from './WalterUmlagetyp';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterVertragVersionEntry } from './WalterVertragVersion';
import { WalterWohnungEntry } from './WalterWohnung';
import { WalterZaehlerEntry } from './WalterZaehler';
import { WalterZaehlerstandEntry } from './WalterZaehlerstand';

describe('lib entity mappers batch', () => {
    it('maps adresse, kontakt, zaehler and wohnung models', () => {
        const adresse = WalterAdresseEntry.fromJson({
            id: 1,
            strasse: 'Teststrasse',
            hausnummer: '1',
            postleitzahl: '10115',
            stadt: 'Berlin',
            anschrift: 'Teststrasse 1',
            notiz: 'Adresse',
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            wohnungen: [],
            kontakte: [],
            zaehler: [],
            permissions: { read: true, update: true, remove: false }
        } as never);

        const kontakt = WalterKontaktEntry.fromJson({
            id: 2,
            email: 'a@example.com',
            telefon: '123',
            fax: '456',
            mobil: '789',
            notiz: 'Kontakt',
            rechtsform: { id: 1, text: 'natuerlich' },
            bezeichnung: 'Kontakt 1',
            name: 'Max',
            vorname: 'Mustermann',
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            anrede: { id: 1, text: 'Herr' },
            selectedJuristischePersonen: [],
            adresse,
            juristischePersonen: [],
            wohnungen: [],
            vertraege: [],
            selectedMitglieder: [],
            mitglieder: [],
            transaktionen: [],
            permissions: { read: true, update: false, remove: false }
        } as never);

        const zaehler = WalterZaehlerEntry.fromJson({
            id: 3,
            kennnummer: 'Z-1',
            ende: '2026-12-31',
            notiz: 'Zaehler',
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            adresse,
            typ: { id: 1, text: 'Wasser' },
            wohnung: { id: 1, text: 'W1' },
            umlagen: [],
            selectedUmlagen: [],
            staende: [],
            lastZaehlerstand: undefined,
            permissions: { read: true, update: true, remove: false }
        } as never);

        const wohnung = WalterWohnungEntry.fromJson({
            adresse,
            id: 4,
            bezeichnung: 'W1',
            wohnflaeche: 70,
            nutzflaeche: 55,
            miteigentumsanteile: 100,
            einheiten: 1,
            notiz: 'Wohnung',
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            besitzer: { id: 1, text: 'Besitzer' },
            haus: [],
            zaehler: [zaehler],
            vertraege: [],
            betriebskostenrechnungen: [],
            erhaltungsaufwendungen: [],
            umlagen: [],
            bewohner: '1',
            permissions: { read: true, update: true, remove: false }
        } as never);

        expect(adresse.stadt).toBe('Berlin');
        expect(kontakt.adresse?.id).toBe(1);
        expect(zaehler.typ?.text).toBe('Wasser');
        expect(wohnung.bezeichnung).toBe('W1');
    });

    it('maps contract and rent-related entities', () => {
        const version = WalterVertragVersionEntry.fromJson({
            id: 5,
            beginn: '2026-01-01',
            personenzahl: 2,
            notiz: 'Version',
            grundmiete: 700,
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            vertrag: { id: 1, text: 'Vertrag 1' },
            permissions: { read: true, update: true, remove: false }
        } as never);

        const mietminderung = WalterMietminderungEntry.fromJson({
            id: 6,
            beginn: '2026-01-01',
            ende: undefined,
            minderung: 20,
            notiz: 'Minderung',
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            vertrag: { id: 1, text: 'Vertrag 1' },
            permissions: { read: true, update: false, remove: false }
        } as never);

        const vertrag = WalterVertragEntry.fromJson({
            id: 7,
            beginn: '2026-01-01',
            ende: undefined,
            notiz: 'Vertrag',
            mieterAuflistung: 'Mieter A',
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            wohnung: { id: 1, text: 'W1' },
            ansprechpartner: { id: 2, text: 'Kontakt A' },
            selectedMieter: [],
            versionen: [version],
            mieter: [],
            mieten: [],
            mietminderungen: [mietminderung],
            betriebskostenrechnungen: [],
            abrechnungsresultate: [],
            permissions: { read: true, update: true, remove: false }
        } as never);

        const miettabelle = WalterMiettabelleEntry.fromJson({
            id: 8,
            bezeichnung: 'Tabelle',
            createdAt: new Date('2026-01-01T10:00:00Z'),
            lastModified: new Date('2026-01-01T11:00:00Z'),
            mieten: [
                {
                    id: 9,
                    betreffenderMonat: '2026-01-01',
                    zahlungsdatum: '2026-01-03',
                    betrag: 700,
                    notiz: 'Miete',
                    repeat: 1,
                    createdAt: new Date('2026-01-01T10:00:00Z'),
                    lastModified: new Date('2026-01-01T11:00:00Z'),
                    vertrag: { id: 7, text: 'Vertrag 7' },
                    permissions: { read: true, update: true, remove: false }
                }
            ]
        } as never);

        expect(version.grundmiete).toBe(700);
        expect(mietminderung.minderung).toBe(20);
        expect(vertrag.versionen[0].id).toBe(5);
        expect(miettabelle.mieten[0].betrag).toBe(700);
    });

    it('maps utility and accounting entities', () => {
        const betriebskostenrechnung = WalterBetriebskostenrechnungEntry.fromJson({
            id: 10,
            betrag: 120,
            betreffendesJahr: 2026,
            datum: '2026-02-01',
            notiz: 'BK',
            createdAt: new Date('2026-02-01T10:00:00Z'),
            lastModified: new Date('2026-02-01T11:00:00Z'),
            typ: { id: 1, text: 'Wasser' },
            umlage: { id: 2, text: 'Umlage A' },
            wohnungen: [],
            betriebskostenrechnungen: [],
            permissions: { read: true, update: false, remove: false }
        } as never);

        const umlage = WalterUmlageEntry.fromJson({
            id: 11,
            notiz: 'Umlage',
            beschreibung: 'Beschreibung',
            wohnungenBezeichnung: 'W1',
            hkvo: undefined,
            createdAt: new Date('2026-02-01T10:00:00Z'),
            lastModified: new Date('2026-02-01T11:00:00Z'),
            zaehler: [],
            selectedZaehler: [],
            typ: { id: 3, text: 'Typ' },
            schluessel: { id: 4, text: 'Schluessel' },
            selectedWohnungen: [],
            wohnungen: [],
            betriebskostenrechnungen: [betriebskostenrechnung],
            permissions: { read: true, update: true, remove: false }
        } as never);

        const umlagetyp = WalterUmlagetypEntry.fromJson({
            id: 12,
            notiz: 'Umlagetyp',
            bezeichnung: 'Typ A',
            createdAt: new Date('2026-02-01T10:00:00Z'),
            lastModified: new Date('2026-02-01T11:00:00Z'),
            umlagen: [umlage],
            permissions: { read: true, update: false, remove: false }
        } as never);

        const erhaltungsaufwendung = WalterErhaltungsaufwendungEntry.fromJson({
            id: 13,
            betrag: 300,
            datum: '2026-03-01',
            notiz: 'Erhaltung',
            bezeichnung: 'Fenster',
            createdAt: new Date('2026-03-01T10:00:00Z'),
            lastModified: new Date('2026-03-01T11:00:00Z'),
            wohnung: { id: 1, text: 'W1' },
            aussteller: { id: 2, text: 'Firma A' },
            permissions: { read: true, update: false, remove: false }
        } as never);

        expect(WalterErhaltungsaufwendungEntry.ApiURLId(13)).toBe(
            '/api/erhaltungsaufwendungen/13'
        );
        expect(betriebskostenrechnung.typ.text).toBe('Wasser');
        expect(umlage.betriebskostenrechnungen[0].id).toBe(10);
        expect(umlagetyp.umlagen[0].id).toBe(11);
        expect(erhaltungsaufwendung.aussteller.text).toBe('Firma A');
    });

    it('maps transaction and meter reading entities', () => {
        const zaehlerstand = WalterZaehlerstandEntry.fromJson({
            id: 14,
            stand: 123.45,
            datum: '2026-04-01',
            einheit: 'kWh',
            zaehler: { id: 3, text: 'Z-1' },
            notiz: 'Stand',
            createdAt: new Date('2026-04-01T10:00:00Z'),
            lastModified: new Date('2026-04-01T11:00:00Z'),
            permissions: { read: true, update: true, remove: false }
        } as never);

        const transaktion = WalterTransaktionEntry.fromJson({
            id: 'trx-1',
            zahler: { id: 1, text: 'Mieter' },
            zahlungsempfaenger: { id: 2, text: 'Hausverwaltung' },
            zahlungsdatum: '2026-04-02',
            betrag: 999,
            verwendungszweck: 'Miete April',
            notiz: 'Transaktion',
            permissions: { read: true, update: false, remove: false },
            createdAt: new Date('2026-04-01T10:00:00Z'),
            lastModified: new Date('2026-04-01T11:00:00Z')
        } as never);

        expect(zaehlerstand.zaehler.text).toBe('Z-1');
        expect(transaktion.zahler.text).toBe('Mieter');
        expect(transaktion.betrag).toBe(999);
    });

    it('supports WalterToastContent defaults and explicit subtitle functions', () => {
        const defaults = new WalterToastContent('ok', 'fail');
        const explicit = new WalterToastContent(
            'ok',
            'fail',
            (...args: unknown[]) => `${String(args[0] ?? '')} done`,
            (...args: unknown[]) => `${String(args[0] ?? '')} failed`
        );

        expect(defaults.subtitleSuccess()).toBe('');
        expect(defaults.subtitleFailure()).toBe('');
        expect(explicit.subtitleSuccess('Save')).toBe('Save done');
        expect(explicit.subtitleFailure('Save')).toBe('Save failed');
    });
});