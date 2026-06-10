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
    import {
        WalterBuchungssatz,
        WalterGrid,
        WalterHeaderDetail,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import { kontoVerknuepfungHref, type WalterAusgleich } from '$walter/lib';
    import { convertEuro } from '$walter/services/utils';
    import { formatToTableDate } from '$walter/components/elements/WalterDataTable';
    import type { PageData } from './$types';

    export let data: PageData;

    let title = `Buchungssatz ${data.entry.nummer}`;
    $: {
        title = `Buchungssatz ${data.entry.nummer}`;
    }

    // OPOS-Ausgleiche aller Zeilen, pro Partner-Satz zusammengefasst —
    // so folgt man von der Forderung zur Zahlung und umgekehrt.
    $: ausgleiche = [
        ...data.entry.zeilen
            .flatMap((zeile) => zeile.ausgleiche)
            .reduce((map, ausgleich) => {
                const existing = map.get(ausgleich.buchungssatzId);
                if (existing) {
                    existing.betrag += ausgleich.betrag;
                } else {
                    map.set(ausgleich.buchungssatzId, { ...ausgleich });
                }
                return map;
            }, new Map<string, WalterAusgleich>())
            .values()
    ];

    // Eine Entität kann über mehrere Konten beteiligt sein — pro Entität
    // ein Link-Tile mit allen Funktionen.
    $: verknuepfungen = [
        ...data.entry.verknuepfungen
            .reduce((map, verknuepfung) => {
                const key = `${verknuepfung.typ}:${verknuepfung.id}`;
                const existing = map.get(key);
                if (existing) {
                    if (!existing.funktionen.includes(verknuepfung.funktion)) {
                        existing.funktionen.push(verknuepfung.funktion);
                    }
                } else {
                    map.set(key, {
                        ...verknuepfung,
                        funktionen: [verknuepfung.funktion]
                    });
                }
                return map;
            }, new Map<string, (typeof data.entry.verknuepfungen)[number] & { funktionen: string[] }>())
            .values()
    ];
</script>

<WalterHeaderDetail entry={data.entry} apiURL={data.apiURL} {title} />

<WalterGrid>
    <WalterBuchungssatz entry={data.entry} />

    <WalterLinks>
        {#if data.entry.stornoNach}
            <WalterLinkTile
                fileref=""
                name={`Storniert durch: Buchungssatz ${data.entry.stornoNach.buchungsjahr}-${data.entry.stornoNach.buchungsnummer}`}
                href={`/buchungssaetze/${data.entry.stornoNach.id}`}
            />
        {/if}
        {#if data.entry.stornoVon}
            <WalterLinkTile
                fileref=""
                name={`Storno von: Buchungssatz ${data.entry.stornoVon.buchungsjahr}-${data.entry.stornoVon.buchungsnummer}`}
                href={`/buchungssaetze/${data.entry.stornoVon.id}`}
            />
        {/if}
        {#if data.entry.transaktion}
            <WalterLinkTile
                fileref=""
                name={`Transaktion: ${data.entry.transaktion.text}`}
                href={`/transaktionen/${data.entry.transaktion.id}`}
            />
        {/if}
        {#each ausgleiche as ausgleich}
            <WalterLinkTile
                fileref=""
                name={`OPOS-Ausgleich: ${ausgleich.buchungsjahr}-${ausgleich.buchungsnummer} | ${formatToTableDate(ausgleich.buchungsdatum)} | ${ausgleich.beschreibung} — ${convertEuro(ausgleich.betrag)}`}
                href={`/buchungssaetze/${ausgleich.buchungssatzId}`}
            />
        {/each}
        {#each verknuepfungen as verknuepfung}
            <WalterLinkTile
                fileref=""
                name={`${verknuepfung.typ}: ${verknuepfung.text} (${verknuepfung.funktionen.join(', ')})`}
                href={kontoVerknuepfungHref(verknuepfung)}
            />
        {/each}
    </WalterLinks>
</WalterGrid>
