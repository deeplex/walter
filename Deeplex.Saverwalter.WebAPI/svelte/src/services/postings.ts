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

export interface MonatsPosten {
    id: number;
    bezeichnung: string;
    /** ISO-Datum des Monatsersten, z.B. "2026-03-01" */
    monat: string;
    betrag: number;
}

export interface PostenGruppe {
    id: number;
    bezeichnung: string;
    anzahl: number;
    summe: number;
    aeltesterMonat: string;
    neuesterMonat: string;
}

/**
 * Fasst Monatsposten pro Vertrag/Garage zusammen. Sortiert nach ältestem
 * Monat, damit die am längsten offenen Einträge oben stehen.
 */
export function gruppiereNachId(posten: MonatsPosten[]): PostenGruppe[] {
    const gruppen = new Map<number, PostenGruppe>();

    for (const p of posten) {
        const g = gruppen.get(p.id);
        if (!g) {
            gruppen.set(p.id, {
                id: p.id,
                bezeichnung: p.bezeichnung,
                anzahl: 1,
                summe: p.betrag,
                aeltesterMonat: p.monat,
                neuesterMonat: p.monat
            });
            continue;
        }
        g.anzahl += 1;
        g.summe += p.betrag;
        if (p.monat < g.aeltesterMonat) g.aeltesterMonat = p.monat;
        if (p.monat > g.neuesterMonat) g.neuesterMonat = p.monat;
    }

    return [...gruppen.values()].sort(
        (a, b) =>
            a.aeltesterMonat.localeCompare(b.aeltesterMonat) ||
            a.bezeichnung.localeCompare(b.bezeichnung, 'de')
    );
}

export const euro = (betrag: number) =>
    betrag.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

const monatKurz = (monat: string) =>
    new Date(monat + 'T00:00:00').toLocaleDateString('de-DE', {
        month: 'short',
        year: 'numeric'
    });

export function zeitraumText(g: PostenGruppe): string {
    return g.anzahl === 1 || g.aeltesterMonat === g.neuesterMonat
        ? monatKurz(g.aeltesterMonat)
        : `${monatKurz(g.aeltesterMonat)} – ${monatKurz(g.neuesterMonat)}`;
}

export const plural = (n: number, einzahl: string, mehrzahl: string) =>
    `${n} ${n === 1 ? einzahl : mehrzahl}`;
