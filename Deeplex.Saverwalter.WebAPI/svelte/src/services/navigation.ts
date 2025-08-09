// Copyright (c) 2023-2024 Kai Lawrence
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

import { walter_goto } from './utils';

export const navigation = {
    abrechnungsresultat: (id: string) => walter_goto(`/abrechnungsresultate/${id}`),
    account: (id: number) => walter_goto(`/accounts/${id}`),
    adresse: (id: number) => walter_goto(`/adressen/${id}`),
    betriebskostenrechnung: (id: number) =>
        walter_goto(`/betriebskostenrechnungen/${id}`),
    erhaltungsaufwendung: (id: number) =>
        walter_goto(`/erhaltungsaufwendungen/${id}`),
    kontakt: (id: number) => walter_goto(`/kontakte/${id}`),
    miete: (id: number) => walter_goto(`/mieten/${id}`),
    mietminderung: (id: number) => walter_goto(`/mietminderungen/${id}`),
    transaktion: (id: number) => walter_goto(`/transaktionen/${id}`),
    umlage: (id: number) => walter_goto(`/umlagen/${id}`),
    umlagetyp: (id: number) => walter_goto(`/umlagetypen/${id}`),
    vertrag: (id: number) => walter_goto(`/vertraege/${id}`),
    vertragversion: (id: number) => walter_goto(`/vertragversionen/${id}`),
    wohnung: (id: number) => walter_goto(`/wohnungen/${id}`),
    zaehler: (id: number) => walter_goto(`/zaehler/${id}`),
    zaehlerstand: (id: number) => walter_goto(`/zaehlerstaende/${id}`)
};
