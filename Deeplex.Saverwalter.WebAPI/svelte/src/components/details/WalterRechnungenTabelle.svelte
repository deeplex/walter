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
    import type { WalterUmlageEntry, TransaktionsInput } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import {
        walter_data_rechnungentabelle,
        type WalterDataConfigType,
        type WalterDataPoint
    } from '../data/WalterData';
    import WalterDataHeatmapChart from '../data/WalterDataHeatmapChart.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterBuchung from './WalterBuchung.svelte';
    import { invalidateAll } from '$app/navigation';

    export let config: WalterDataConfigType;
    export let umlagen: WalterUmlageEntry[];
    export let fetchImpl: typeof fetch;
    export let year: number;

    let addModalOpen = false;
    let modalTitle = 'Betriebskostenrechnung';
    let buchungsInput: TransaktionsInput = emptyTransaktionsInput();

    function click(e: CustomEvent, config: WalterDataConfigType) {
        const targetWithData = e.target as { __data__?: WalterDataPoint };
        const data = targetWithData?.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        if (!data) return;

        const group = config.data.filter((entry) => entry.group === data.group);
        const thisEntry = group.find((entry) => entry.key === data.key);

        if (thisEntry?.id && thisEntry.key) {
            modalTitle = thisEntry.key;
            buchungsInput = {
                ...emptyTransaktionsInput(),
                betriebskostenEingaenge: [{ betrag: 0, umlageId: +thisEntry.id, betreffendesJahr: year }]
            };
            addModalOpen = true;
        }
    }

    async function onSubmit() {
        config = walter_data_rechnungentabelle(umlagen, year);
        await invalidateAll();
    }
</script>

<WalterDataWrapperQuickAdd
    title={modalTitle}
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={buchungsInput}
    bind:addModalOpen
    onSubmit={onSubmit}
>
    <WalterBuchung {fetchImpl} bind:buchung={buchungsInput} />
</WalterDataWrapperQuickAdd>

<div style="left: 0; min-height: 30em; display: block; width: 100%;">
    <h3>Umlagentabelle</h3>
    {#if config.data.length === 0}
        <p>Keine Daten vorhanden</p>
    {:else}
        <div id="umlagentabelle">
            <WalterDataHeatmapChart {click} bind:config />
        </div>
    {/if}
</div>
