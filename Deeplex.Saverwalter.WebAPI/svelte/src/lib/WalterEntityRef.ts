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
 * Reference to a domain object as delivered by the backend (`EntityRef` in `Kontorollen.cs`):
 * type and id, optionally a section of the page. The backend knows no routes — URLs are built
 * only here, in `entityRefHref`. The values are domain terms or anchors and therefore German.
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

/** Collapsible sections of a page (their `id` is the anchor). */
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

/** URL of a reference, e.g. `/vertraege/3#mietkonto`; `undefined` without a reference. */
export function entityRefHref(
    ref: WalterEntityRef | null | undefined
): string | undefined {
    if (!ref) return undefined;
    const path = `/${ROUTES[ref.type]}/${ref.id}`;
    return ref.section ? `${path}#${ref.section}` : path;
}
