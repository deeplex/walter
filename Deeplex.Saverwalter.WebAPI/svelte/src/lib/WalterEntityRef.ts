// Copyright (C) 2023-2026  Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

/**
 * Referenz auf ein Fachobjekt, wie sie das Backend liefert (`EntityRef` in `Kontorollen.cs`):
 * Typ und Id, optional ein Abschnitt der Seite. Das Backend kennt keine Routen — die URL
 * entsteht nur hier, in `entityRefHref`. Die Werte sind Fachbegriffe bzw. Anker und daher deutsch.
 */
export type WalterEntityRefType =
    | 'Vertrag'
    | 'GarageVertrag'
    | 'Wohnung'
    | 'Umlage'
    | 'Kontakt'
    | 'Garage'
    | 'Bankkonto'
    | 'Buchungskonto';

/** Aufklappbare Abschnitte einer Seite (ihre `id` ist das Sprungziel). */
export type WalterEntityRefSection =
    | 'mietkonto'
    | 'abrechnungen'
    | 'kaution'
    | 'rechnungen'
    | 'erhaltungsaufwand';

export interface WalterEntityRef {
    type: WalterEntityRefType;
    id: string | number;
    section?: WalterEntityRefSection | null;
}

const ROUTES: Record<WalterEntityRefType, string> = {
    Vertrag: 'vertraege',
    GarageVertrag: 'garage-vertraege',
    Wohnung: 'wohnungen',
    Umlage: 'umlagen',
    Kontakt: 'kontakte',
    Garage: 'garagen',
    Bankkonto: 'bankkontos',
    Buchungskonto: 'buchungskonten'
};

/** Die URL einer Referenz, z. B. `/vertraege/3#mietkonto` oder `/vertraege/3/kaution`; ohne Referenz `undefined`. */
export function entityRefHref(
    ref: WalterEntityRef | null | undefined
): string | undefined {
    if (!ref) return undefined;
    const path = `/${ROUTES[ref.type]}/${ref.id}`;
    // Die Kaution hat eine eigene Seite statt eines Abschnitts.
    if (ref.type === 'Vertrag' && ref.section === 'kaution') {
        return `${path}/kaution`;
    }
    return ref.section ? `${path}#${ref.section}` : path;
}
