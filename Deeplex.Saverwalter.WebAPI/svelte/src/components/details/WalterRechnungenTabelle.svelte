<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
        WalterBetriebskostenrechnungEntry,
        type WalterUmlageEntry
    } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        walter_data_rechnungentabelle,
        type WalterDataConfigType,
        type WalterDataPoint
    } from '../data/WalterData';
    import WalterDataHeatmapChart from '../data/WalterDataHeatmapChart.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterBetriebskostenrechnung from './WalterBetriebskostenrechnung.svelte';

    export let config: WalterDataConfigType;
    export let umlagen: WalterUmlageEntry[];
    export let fetchImpl: typeof fetch;
    export let year: number;
    let addEntry: Partial<WalterBetriebskostenrechnungEntry> = {};

    function updateEntry(
        umlageId: number,
        umlageTyp: string,
        rechnungTyp: number
    ) {
        addEntry = {
            datum: convertDateCanadian(new Date()),
            betreffendesJahr: year,
            typ: {
                id: rechnungTyp,
                text: umlageTyp
            },
            umlage: {
                id: umlageId,
                text: umlageTyp
            }
        };
    }

    function click(e: CustomEvent, config: WalterDataConfigType) {
        const targetWithData = e.target as { __data__?: WalterDataPoint };
        const data = targetWithData?.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        // Because of that all the configs with different years are filtered here.
        if (!data) return;

        const group = config.data.filter((entry) => entry.group === data.group);

        const thisEntry = group.find((entry) => entry.key === data.key);
        console.log(thisEntry);

        if (thisEntry?.id && thisEntry.id2 && thisEntry.key) {
            const umlageTyp = thisEntry.key;
            const rechnungTyp = +thisEntry.id2;
            const umlageId = +thisEntry.id;

            updateEntry(umlageId, umlageTyp, rechnungTyp);
            addModalOpen = true;
        }
    }

    let addModalOpen = false;
    let title = 'Umlage';
    let rechnungen = umlagen.flatMap((e) => e.betriebskostenrechnungen);

    function onSubmit(new_value: unknown) {
        const value = new_value as WalterBetriebskostenrechnungEntry;
        const umlage = umlagen.find((e) => e.id === +value.umlage?.id);
        if (!umlage) {
            console.warn('Umlage not found: ', value.umlage.id);
            return;
        }
        umlage!.betriebskostenrechnungen.push(value);
        rechnungen.push(value);
        config = walter_data_rechnungentabelle(umlagen, year);
    }
</script>

<WalterDataWrapperQuickAdd
    {onSubmit}
    bind:addEntry
    addUrl={WalterBetriebskostenrechnungEntry.ApiURL}
    bind:addModalOpen
    {title}
>
    <WalterBetriebskostenrechnung
        {fetchImpl}
        bind:entry={addEntry}
        {rechnungen}
    />
</WalterDataWrapperQuickAdd>

<div style="left: 0; min-height: 30em; display: block; min-width: 60em;">
    <h3>Umlagentabelle</h3>
    {#if config.data.length === 0}
        <p>Keine Daten vorhanden</p>
    {:else}
        <div id="umlagentabelle">
            <WalterDataHeatmapChart {click} bind:config />
        </div>
    {/if}
</div>
