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

import { walter_get } from '$walter/services/requests';
import { WalterApiHandler } from './WalterApiHandler';
import type { WalterKontoVerknuepfung } from './WalterBuchungskonto';

export type WalterBuchungszeile = {
    id: string;
    kontoId: number;
    kontonummer: string;
    kontobezeichnung: string;
    sollHaben: 'Soll' | 'Haben';
    betrag: number;
};

export type WalterBuchungssatzLink = {
    id: string;
    buchungsnummer: number;
    buchungsjahr: number;
};

export type WalterTransaktionLink = {
    id: string;
    text: string;
};

export class WalterBuchungssatzEntry extends WalterApiHandler {
    public static ApiURL = `/api/buchungssaetze`;

    constructor(
        public id: string,
        public buchungsnummer: number,
        public buchungsjahr: number,
        public buchungsdatum: string,
        public beschreibung: string,
        public betrag: number,
        public sollKonten: string,
        public habenKonten: string,
        public istStorno: boolean,
        public istStorniert: boolean,
        // Soll-/Haben-Anteil eines Kontos an diesem Satz — nur gefüllt,
        // wenn die Liste nach kontoId gefiltert ist (Kontoblatt).
        public kontoSoll: number | undefined,
        public kontoHaben: number | undefined,
        // Nur im Detail (GET /api/buchungssaetze/{id}) gefüllt.
        public notiz: string | undefined,
        public belegpfad: string | undefined,
        public zeilen: WalterBuchungszeile[],
        public transaktion: WalterTransaktionLink | undefined,
        public stornoVon: WalterBuchungssatzLink | undefined,
        public stornoNach: WalterBuchungssatzLink | undefined,
        public verknuepfungen: WalterKontoVerknuepfung[],
        public createdAt: Date,
        public lastModified: Date
    ) {
        super();
    }

    /** Lückenlose Belegnummer im Format Jahr-Nummer (§ 239 HGB). */
    get nummer(): string {
        return `${this.buchungsjahr}-${this.buchungsnummer}`;
    }

    get status(): string {
        if (this.istStorno) return 'Storno';
        if (this.istStorniert) return 'Storniert';
        return '';
    }

    static fromJson(json: WalterBuchungssatzEntry): WalterBuchungssatzEntry {
        return new WalterBuchungssatzEntry(
            json.id,
            json.buchungsnummer,
            json.buchungsjahr,
            json.buchungsdatum,
            json.beschreibung,
            json.betrag,
            json.sollKonten,
            json.habenKonten,
            json.istStorno,
            json.istStorniert,
            json.kontoSoll ?? undefined,
            json.kontoHaben ?? undefined,
            json.notiz,
            json.belegpfad,
            json.zeilen ?? [],
            json.transaktion,
            json.stornoVon,
            json.stornoNach,
            json.verknuepfungen ?? [],
            new Date(json.createdAt),
            new Date(json.lastModified)
        );
    }

    /** Wie GetPaged, zusätzlich gefiltert auf Sätze mit Zeilen auf dem Konto. */
    public static async GetPagedByKonto(
        fetchImpl: typeof fetch,
        params: {
            search?: string;
            sortBy?: string;
            sortDir?: 'asc' | 'desc';
            skip?: number;
            take?: number;
        },
        kontoId: number
    ): Promise<{ items: WalterBuchungssatzEntry[]; totalCount: number }> {
        const qs = new URLSearchParams();
        qs.set('kontoId', `${kontoId}`);
        if (params.search) qs.set('search', params.search);
        if (params.sortBy) qs.set('sortBy', params.sortBy);
        if (params.sortDir) qs.set('sortDir', params.sortDir);
        if (params.skip !== undefined) qs.set('skip', `${params.skip}`);
        if (params.take !== undefined) qs.set('take', `${params.take}`);

        const paged = (await walter_get(
            `${this.ApiURL}?${qs.toString()}`,
            fetchImpl
        )) as { items: WalterBuchungssatzEntry[]; totalCount: number };

        return {
            items: paged.items.map(WalterBuchungssatzEntry.fromJson),
            totalCount: paged.totalCount
        };
    }
}
