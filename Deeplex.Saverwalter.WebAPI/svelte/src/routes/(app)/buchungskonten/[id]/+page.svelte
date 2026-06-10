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
        WalterBuchungskonto,
        WalterBuchungssaetze,
        WalterDataBarChartGrouped,
        WalterGrid,
        WalterHeaderDetail,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import { walter_data_soll_haben_monate } from '$walter/components/data/WalterData';
    import { kontoVerknuepfungHref } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;

    let title = data.entry.funktion
        ? `${data.entry.bezeichnung} (${data.entry.funktion})`
        : data.entry.bezeichnung;
    $: {
        title = data.entry.funktion
            ? `${data.entry.bezeichnung} (${data.entry.funktion})`
            : data.entry.bezeichnung;
    }
</script>

<WalterHeaderDetail entry={data.entry} apiURL={data.apiURL} {title} />

<WalterGrid>
    <WalterBuchungskonto bind:entry={data.entry} />

    <WalterLinks>
        <WalterBuchungssaetze
            fetchImpl={data.fetchImpl}
            title="Buchungssätze"
            kontoId={data.entry.id}
        />

        {#each data.entry.verknuepfungen as verknuepfung}
            <WalterLinkTile
                fileref=""
                name={`${verknuepfung.typ}: ${verknuepfung.text} (${verknuepfung.funktion})`}
                href={kontoVerknuepfungHref(verknuepfung)}
            />
        {/each}
    </WalterLinks>

    {#if data.entry.monatsSummen.length > 0}
        <Row>
            <WalterDataBarChartGrouped
                config={walter_data_soll_haben_monate(
                    'Soll und Haben je Monat',
                    data.entry.monatsSummen
                )}
            />
        </Row>
    {/if}
</WalterGrid>
