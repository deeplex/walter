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

import { walter_get, walter_post, walter_delete } from '$walter/services/requests';

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
    /** Konto eines Vertrags mit Abrechnungsverzicht in diesem Jahr. */
    verzichtet: boolean;
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
    /** Dokumentierter Abrechnungsverzicht — gilt als erledigt, ohne Buchung. */
    verzichtet: boolean;
    verzichtGrund: string | undefined;
    verzichtId: string | undefined;
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

export type WalterPruefStatus =
    | 'Bestanden'
    | 'NichtBestanden'
    | 'Fehlt'
    | 'VerzichtBestanden'
    | 'VerzichtNichtBestanden';

/** Eine geprüfte Position (Vertrag oder Eigenanteil) der Jahresabschluss-Kontrolle. */
export type WalterPruefPosition = {
    vertragId: number | undefined;
    bezeichnung: string;
    gruppe: string;
    status: WalterPruefStatus;
    gebuchterSaldo: number | undefined;
    neuerSaldo: number | undefined;
    detail: string | undefined;
};

/** Ergebnis der Jahresabschluss-Kontrolle: würde eine Neuabrechnung dasselbe ergeben? */
export type WalterJahresabschlussKontrolle = {
    jahr: number;
    positionen: WalterPruefPosition[];
    bestanden: number;
    nichtBestanden: number;
    fehlt: number;
    verzichtBestanden: number;
    verzichtNichtBestanden: number;
    gesamt: number;
};

/** Läuft alle Abrechnungsgruppen des Jahres durch (rein prüfend, keine Buchung). */
export async function getJahresabschlussKontrolle(
    jahr: number,
    fetchImpl: typeof fetch
): Promise<WalterJahresabschlussKontrolle> {
    return (await walter_get(
        `/api/abrechnungslauf/kontrolle/${jahr}`,
        fetchImpl
    )) as WalterJahresabschlussKontrolle;
}

const VerzichtURL = '/api/abrechnungsverzicht';

/** Setzt einen dokumentierten Abrechnungsverzicht (Grund Pflicht). */
export async function setzeAbrechnungsverzicht(
    vertragId: number,
    jahr: number,
    grund: string
): Promise<Response> {
    return await walter_post(VerzichtURL, { vertragId, jahr, grund });
}

/** Hebt den Abrechnungsverzicht für Vertrag + Jahr wieder auf. */
export async function hebeAbrechnungsverzichtAuf(
    vertragId: number,
    jahr: number
): Promise<Response> {
    return await walter_delete(`${VerzichtURL}/${vertragId}/${jahr}`);
}
