// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';

/** Verknüpfung eines Buchungskontos zu seiner Besitzer-Entität. */
export type WalterKontoVerknuepfung = {
    typ:
        | 'Vertrag'
        | 'Wohnung'
        | 'Umlage'
        | 'Kontakt'
        | 'Garage'
        | 'GarageVertrag';
    id: string;
    text: string;
    funktion: string;
    kontoId: number;
};

export type WalterKontoMonatsSumme = {
    jahr: number;
    monat: number;
    soll: number;
    haben: number;
};

export type WalterKontoStatusTag = {
    text: string;
    tag: 'green' | 'red' | 'teal';
};

/**
 * Saldo aus Sicht der natürlichen Kontoseite: Aktiv-/Aufwandskonten führen
 * Soll, Passiv-/Ertragskonten Haben. So ist der angezeigte Saldo immer
 * "positiv = das, was das Konto sammelt".
 */
export function kontoAnzeigeSaldo(
    konto: Partial<Pick<WalterBuchungskontoEntry, 'kontotyp' | 'saldo'>>
): number {
    const saldo = konto.saldo ?? 0;
    return konto.kontotyp === 'Passiv' || konto.kontotyp === 'Ertrag'
        ? -saldo
        : saldo;
}

/**
 * Status für Ausgleichskonten (Forderungen/Verbindlichkeiten): Saldo 0 ist
 * ausgeglichen, ein Saldo auf der natürlichen Seite ist ein offener Posten.
 * Summenkonten (Erträge, Aufwendungen, Zahlungseingänge) haben keinen Status.
 */
export function kontoStatusTag(
    konto: Partial<
        Pick<WalterBuchungskontoEntry, 'kontotyp' | 'saldo' | 'ausgleichbar'>
    >
): WalterKontoStatusTag | undefined {
    if (!konto.ausgleichbar) {
        return undefined;
    }
    const saldo = kontoAnzeigeSaldo(konto);
    if (saldo === 0) {
        return { text: 'Ausgeglichen', tag: 'green' };
    }
    if (saldo > 0) {
        return { text: 'Offen', tag: 'red' };
    }
    return { text: 'Guthaben', tag: 'teal' };
}

export function kontoVerknuepfungHref(v: WalterKontoVerknuepfung): string {
    switch (v.typ) {
        case 'Vertrag':
            return `/vertraege/${v.id}`;
        case 'Wohnung':
            return `/wohnungen/${v.id}`;
        case 'Umlage':
            return `/umlagen/${v.id}`;
        case 'Kontakt':
            return `/kontakte/${v.id}`;
        case 'Garage':
            return `/garagen/${v.id}`;
        case 'GarageVertrag':
            return `/garage-vertraege/${v.id}`;
    }
}

export class WalterBuchungskontoEntry extends WalterApiHandler {
    public static ApiURL = `/api/buchungskonten`;

    constructor(
        public id: number,
        public kontonummer: string,
        public bezeichnung: string,
        public kontotyp: string,
        public notiz: string | undefined,
        public anzahlBuchungszeilen: number,
        public saldo: number,
        // Funktion des Kontos bei seiner Besitzer-Entität, z.B. "Mietforderungen".
        public funktion: string | undefined,
        // Ob das Konto sich ausgleichen soll — ein Saldo ist dann ein offener Posten.
        public ausgleichbar: boolean,
        // Nur im Detail (GET /api/buchungskonten/{id}) gefüllt.
        public verknuepfungen: WalterKontoVerknuepfung[],
        public sollSumme: number,
        public habenSumme: number,
        public forderungSumme: number,
        public forderungAusgeglichen: number,
        public monatsSummen: WalterKontoMonatsSumme[],
        public createdAt: Date,
        public lastModified: Date,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterBuchungskontoEntry): WalterBuchungskontoEntry {
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterBuchungskontoEntry(
            json.id,
            json.kontonummer,
            json.bezeichnung,
            json.kontotyp,
            json.notiz,
            json.anzahlBuchungszeilen,
            json.saldo,
            json.funktion,
            json.ausgleichbar ?? false,
            json.verknuepfungen ?? [],
            json.sollSumme ?? 0,
            json.habenSumme ?? 0,
            json.forderungSumme ?? 0,
            json.forderungAusgeglichen ?? 0,
            json.monatsSummen ?? [],
            new Date(json.createdAt),
            new Date(json.lastModified),
            permissions
        );
    }
}
