// Copyright (C) 2023-2026  Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

/** Cent-Toleranz für Vergleiche von Geldbeträgen. */
const TOLERANZ = 0.005;

export type WalterZahlungsStatusArt =
    | 'offen'
    | 'teilweise'
    | 'bezahlt'
    | 'guthaben'
    | 'rueckgaengig'
    | 'nichtVersendet'
    | 'storniert';

/** Status-Tag im Format, das `WalterDataTable` als Zelle darstellt. */
export type WalterZahlungsStatus = {
    art: WalterZahlungsStatusArt;
    text: string;
    tag: 'red' | 'purple' | 'green' | 'blue' | 'gray';
};

/** Die Status nach Art — für Stellen, die nur wissen, *ob* bezahlt ist, nicht wie viel. */
export const ZAHLUNGS_STATUS: Readonly<
    Record<WalterZahlungsStatusArt, WalterZahlungsStatus>
> = {
    offen: { art: 'offen', text: 'Offen', tag: 'red' },
    teilweise: { art: 'teilweise', text: 'Teilweise bezahlt', tag: 'purple' },
    bezahlt: { art: 'bezahlt', text: 'Bezahlt', tag: 'green' },
    guthaben: { art: 'guthaben', text: 'Guthaben', tag: 'blue' },
    rueckgaengig: {
        art: 'rueckgaengig',
        text: 'Rückgängig gemacht',
        tag: 'gray'
    },
    nichtVersendet: {
        art: 'nichtVersendet',
        text: 'Nicht versendet',
        tag: 'gray'
    },
    storniert: { art: 'storniert', text: 'Storniert', tag: 'gray' }
};

/**
 * Status einer Forderung in der Fachsicht: was fällig war gegen das, was bezahlt
 * wurde. Farben einheitlich: rot = offen, lila = teilweise bezahlt, grün = bezahlt,
 * blau = Guthaben, grau = rückgängig gemacht.
 */
export function zahlungsStatus(
    faellig: number,
    bezahlt: number,
    optionen: { rueckgaengig?: boolean } = {}
): WalterZahlungsStatus {
    if (optionen.rueckgaengig) return ZAHLUNGS_STATUS.rueckgaengig;

    const offen = faellig - bezahlt;
    if (offen < -TOLERANZ) return ZAHLUNGS_STATUS.guthaben;
    if (offen <= TOLERANZ) return ZAHLUNGS_STATUS.bezahlt;
    if (bezahlt > TOLERANZ) return ZAHLUNGS_STATUS.teilweise;
    return ZAHLUNGS_STATUS.offen;
}
