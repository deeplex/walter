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
 * Die eine Stelle für die Anzeige von Beträgen, Daten, Monaten und Zahlen — überall
 * deutsch formatiert. Komponenten importieren von hier, statt eigene Kopien zu definieren.
 */

/** Cent-Toleranz: kleinere Beträge gelten als null. */
const TOLERANZ = 0.005;

const MONATE = [
    'Jan',
    'Feb',
    'Mär',
    'Apr',
    'Mai',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Okt',
    'Nov',
    'Dez'
];

/** `1.234,56 €`; fehlend als `0,00 €`. */
export function formatEuro(v: number | null | undefined): string {
    return (v ?? 0).toLocaleString('de-DE', {
        style: 'currency',
        currency: 'EUR'
    });
}

/** Wie `formatEuro`, aber `—` für null bzw. fehlend (z. B. „nichts offen“). */
export function formatEuroOderStrich(v: number | null | undefined): string {
    return v == null || Math.abs(v) < TOLERANZ ? '—' : formatEuro(v);
}

/**
 * `02.01.2026`. Ein reines Datum (`yyyy-mm-dd`) gilt als Kalendertag, nicht als
 * Mitternacht UTC — sonst könnte es in einer anderen Zeitzone auf den Vortag rutschen.
 */
export function formatDatum(d: string | Date): string {
    const tag =
        typeof d === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(d)
            ? new Date(
                  Number(d.slice(0, 4)),
                  Number(d.slice(5, 7)) - 1,
                  Number(d.slice(8, 10))
              )
            : new Date(d);
    return tag.toLocaleDateString('de-DE', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
}

/** `Jan 2026` (Monat 1–12). */
export function formatMonat(jahr: number, monat: number): string {
    return `${MONATE[monat - 1]} ${jahr}`;
}

/** `1.234,5` — bis zu `nachkomma` Stellen, ohne überflüssige Nullen. */
export function formatZahl(
    v: number | null | undefined,
    nachkomma = 3
): string {
    return (v ?? 0).toLocaleString('de-DE', {
        maximumFractionDigits: nachkomma
    });
}

/** Anteil `0.1234` als `12,34 %`. */
export function formatProzent(anteil: number | null | undefined): string {
    return (anteil ?? 0).toLocaleString('de-DE', {
        style: 'percent',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

/** `42,00 m²`. */
export function formatQuadratmeter(v: number | null | undefined): string {
    return `${(v ?? 0).toLocaleString('de-DE', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    })} m²`;
}
