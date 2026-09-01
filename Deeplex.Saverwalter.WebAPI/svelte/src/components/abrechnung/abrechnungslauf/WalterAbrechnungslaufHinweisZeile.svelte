<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    export let text: string;
    /** Kennnummer → ZaehlerId, für das Verlinken erwähnter Zähler. */
    export let zaehlerIds: Map<string, number> = new Map();

    const fehlerPräfix = /^Fehler:\s*/;

    $: istFehler = fehlerPräfix.test(text);
    $: anzeigeText = text.replace(fehlerPräfix, '');
    $: segments = linkify(anzeigeText, zaehlerIds);

    type Segment = { text: string; href?: string };

    function linkify(input: string, map: Map<string, number>): Segment[] {
        const kennnummern = [...map.keys()]
            // Kurze/generische Kennnummern (z.B. "1") würden sonst überall im
            // Fließtext anschlagen — daher eine Mindestlänge verlangen.
            .filter((k) => k.length >= 3)
            .sort((a, b) => b.length - a.length);
        if (kennnummern.length === 0) return [{ text: input }];

        const result: Segment[] = [];
        let remaining = input;

        while (remaining.length > 0) {
            let bestIndex = -1;
            let bestKennnummer = '';
            for (const k of kennnummern) {
                const idx = remaining.indexOf(k);
                if (idx !== -1 && (bestIndex === -1 || idx < bestIndex)) {
                    bestIndex = idx;
                    bestKennnummer = k;
                }
            }
            if (bestIndex === -1) {
                result.push({ text: remaining });
                break;
            }
            if (bestIndex > 0) {
                result.push({ text: remaining.slice(0, bestIndex) });
            }
            result.push({
                text: bestKennnummer,
                href: `/zaehler/${map.get(bestKennnummer)}`
            });
            remaining = remaining.slice(bestIndex + bestKennnummer.length);
        }

        return result;
    }
</script>

<li class="hinweis-zeile" class:hinweis-fehler={istFehler}>
    {#each segments as segment}{#if segment.href}<a href={segment.href}
                >{segment.text}</a
            >{:else}{segment.text}{/if}{/each}
</li>

<style>
    .hinweis-zeile {
        margin: 0.15rem 0;
    }

    .hinweis-fehler {
        color: var(--cds-support-error);
    }
</style>
