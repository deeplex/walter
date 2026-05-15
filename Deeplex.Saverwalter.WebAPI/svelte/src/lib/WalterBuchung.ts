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

export interface MietzahlungsInput {
    vertragId?: number;
    betreffenderMonat?: string;
    kaltmiete: number;
    nkVorauszahlung: number;
}

export interface BetriebskostenEingangInput {
    umlageId?: number;
    betreffendesJahr?: number;
    rechnungsDatum?: string;
    betrag: number;
    notiz?: string;
}

export interface ErhaltungsaufwendungsInput {
    wohnungId?: number;
    habenKontoId?: number;
    betrag: number;
    beschreibung?: string;
}

export interface SonstigerBuchungssatzInput {
    sollKontoId?: number;
    habenKontoId?: number;
    betrag: number;
    beschreibung?: string;
}

export interface TransaktionsInput {
    zahlungsdatum: string;
    zahlerId?: number;
    zahlungsempfaengerId?: number;
    verwendungszweck: string;
    notiz?: string;
    mieten: MietzahlungsInput[];
    betriebskostenEingaenge: BetriebskostenEingangInput[];
    erhaltungsaufwendungen: ErhaltungsaufwendungsInput[];
    sonstige: SonstigerBuchungssatzInput[];
}

export function emptyTransaktionsInput(): TransaktionsInput {
    return {
        zahlungsdatum: new Date().toISOString().slice(0, 10),
        verwendungszweck: '',
        mieten: [],
        betriebskostenEingaenge: [],
        erhaltungsaufwendungen: [],
        sonstige: []
    };
}
