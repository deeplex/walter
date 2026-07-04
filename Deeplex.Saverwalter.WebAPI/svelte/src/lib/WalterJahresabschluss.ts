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

/** Ein Konto in der Jahressicht der Jahresabschlusskontrolle. */
export type WalterKontoJahres = {
    kontoId: number;
    kontonummer: string;
    bezeichnung: string;
    kontotyp: string;
    funktion: string | undefined;
    ausgleichbar: boolean;
    verknuepfungTyp: string | undefined;
    verknuepfungId: string | undefined;
    verknuepfungText: string | undefined;
    sollJahr: number;
    habenJahr: number;
    saldovortrag: number;
    endsaldo: number;
    offenePostenAnzahl: number;
    offenePostenBetrag: number;
    ausgeglichen: boolean;
};

/** Status der Betriebskostenabrechnung eines Vertrags für ein Jahr. */
export type WalterAbrechnungsstatus = {
    vertragId: number;
    bezeichnung: string;
    resultatVorhanden: boolean;
    abgesendet: boolean;
    /** Saldo per OPOS gedeckt (oder 0). Nur aussagekräftig wenn resultatVorhanden. */
    ausgeglichen: boolean;
    buchungssatzId: string | undefined;
};

export type WalterJahresUebersicht = {
    jahr: number;
    kontenOffen: number;
    abrechnungenOffen: number;
    abgeschlossen: boolean;
};

export type WalterJahresabschluss = {
    jahr: number;
    konten: WalterKontoJahres[];
    abrechnungen: WalterAbrechnungsstatus[];
    kontenOffen: number;
    abrechnungenGesamt: number;
    abrechnungenFertig: number;
    jahrAbgeschlossen: boolean;
};

const ApiURL = '/api/jahresabschluss';

export async function getJahresUebersicht(
    fetchImpl: typeof fetch
): Promise<WalterJahresUebersicht[]> {
    return (await walter_get(ApiURL, fetchImpl)) as WalterJahresUebersicht[];
}

export async function getJahresabschluss(
    jahr: number,
    fetchImpl: typeof fetch
): Promise<WalterJahresabschluss> {
    return (await walter_get(
        `${ApiURL}/${jahr}`,
        fetchImpl
    )) as WalterJahresabschluss;
}
