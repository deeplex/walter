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
    import type {
        WalterVertragEntry,
        WalterMieteEntry,
        TransaktionsInput
    } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import {
        months,
        walter_data_miettabelle,
        type WalterDataConfigType,
        type WalterDataPoint
    } from '../data/WalterData';
    import WalterDataHeatmapChart from '../data/WalterDataHeatmapChart.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { invalidateAll } from '$app/navigation';
    import { WalterTransaktion } from '..';

    export let config: WalterDataConfigType;
    export let vertraege: WalterVertragEntry[];
    export let year: number;
    export let mieten: WalterMieteEntry[];
    export let fetchImpl: typeof fetch;

    let addModalOpen = false;
    let modalTitle = 'Mietzahlung';
    let buchungsInput: TransaktionsInput = emptyTransaktionsInput();

    function click(e: CustomEvent, config: WalterDataConfigType) {
        const targetWithData = e.target as { __data__?: WalterDataPoint };
        const data = targetWithData?.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        // Because of that all the configs with different years are filtered here.
        if (!data) return;

        let monthIndex = months.findIndex((month) => month === data.key);
        if (monthIndex === -1) return;

        const thisEntry = config.data.find(
            (entry) => entry.key === data.key && entry.year === year
        );

        const vertragId = thisEntry?.id;
        if (vertragId) {
            modalTitle = data.group!;
            buchungsInput = {
                ...emptyTransaktionsInput(),
                mieten: [
                    { kaltmiete: 0, nkVorauszahlung: 0, vertragId: +vertragId }
                ]
            };
            addModalOpen = true;
        }
    }

    async function onSubmit() {
        config = walter_data_miettabelle(vertraege, year);
        await invalidateAll();
    }
</script>

<WalterDataWrapperQuickAdd
    title={modalTitle}
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={buchungsInput}
    bind:addModalOpen
    {onSubmit}
>
    <WalterTransaktion {fetchImpl} bind:buchung={buchungsInput} />
</WalterDataWrapperQuickAdd>

<div style="left: 0; min-height: 30em; display: block; width: 100%;">
    <h3>Miettabelle</h3>
    {#if mieten.length === 0}
        <p>Keine Mieten vorhanden</p>
    {:else}
        <div id="miettabelle">
            <WalterDataHeatmapChart {click} bind:config />
        </div>
    {/if}
</div>
