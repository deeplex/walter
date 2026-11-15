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

import type { WalterZuordnungEintrag } from './WalterTransaktion';
import { zuordnungText } from './WalterTransaktion';
import { entityRefHref } from './WalterEntityRef';

/** Farbe eines Tags an einem Eintrag. */
export type WalterListItemTagColor =
    | 'green'
    | 'warm-gray'
    | 'red'
    | 'blue'
    | 'gray'
    | 'purple'
    | 'teal';

/** Teil eines Eintrags, eingerückt darunter — z. B. wofür eine Zahlung gebucht wurde. */
export interface WalterListItemPart {
    text: string;
    betrag?: number;
    href?: string | null;
    /** Abgeschwächt, z. B. ein rückgängig gemachter Teil. */
    dimmed?: boolean;
}

/**
 * Ein Eintrag der einheitlichen Liste (WalterItemList), z. B. in einer aufgeklappten
 * Tabellenzeile: Datum, Betrag, Text mit Link, optional ein Tag und Teile darunter.
 */
export interface WalterListItem {
    id: string;
    datum?: string;
    betrag?: number;
    /** Statt eines Betrags ein anderer Wert in derselben Spalte, z. B. ein Zählerstand. */
    wert?: string;
    text: string;
    href?: string;
    tag?: { text: string; tag: WalterListItemTagColor };
    parts?: WalterListItemPart[];
}

/** Zuordnung einer Transaktion als Teile eines Eintrags. */
export function zuordnungAsParts(
    zuordnung: WalterZuordnungEintrag[]
): WalterListItemPart[] {
    return zuordnung.map((z) => ({
        text: zuordnungText(z),
        betrag: z.betrag,
        href: entityRefHref(z.ref),
        dimmed: z.storniert
    }));
}

/**
 * Zuordnung einer Transaktion als eigene Einträge (z. B. aufgeklappt in der
 * Transaktionsliste): Betrag und wofür, rückgängig gemachte Teile mit grauem Tag.
 */
export function zuordnungAsItems(
    zuordnung: WalterZuordnungEintrag[]
): WalterListItem[] {
    return zuordnung.map((z, i) => ({
        id: `zuordnung-${i}`,
        betrag: z.betrag,
        text: z.text,
        href: entityRefHref(z.ref),
        tag: z.storniert
            ? { text: 'Rückgängig gemacht', tag: 'gray' as const }
            : undefined
    }));
}
