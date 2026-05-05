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
        type WalterVertragEntry,
        WalterMieteEntry,
        WalterMietzahlungApiURL,
        type WalterMietzahlungInput
    } from '$walter/lib';
    import {
        months,
        walter_data_miettabelle,
        type WalterDataConfigType,
        type WalterDataPoint
    } from '../data/WalterData';
    import WalterDataHeatmapChart from '../data/WalterDataHeatmapChart.svelte';
    import { convertDateCanadian } from '$walter/services/utils';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { WalterMiete } from '..';

    export let config: WalterDataConfigType;
    export let vertraege: WalterVertragEntry[];
    export let year: number;
    export let mieten: WalterMieteEntry[];

    function updateEntry(
        vertragId: string,
        monthIndex: number,
        wohnung: string,
        betrag: number
    ) {
        addEntry = {
            zahlungsdatum: convertDateCanadian(new Date()),
            betreffenderMonat: convertDateCanadian(
                new Date(year, monthIndex, 1)
            ),
            vertrag: {
                id: vertragId,
                text: wohnung
            },
            kaltmieteZahlung: betrag,
            nkZahlung: 0
        };

        title = `${wohnung}`;
    }

    function click(e: CustomEvent, config: WalterDataConfigType) {
        const targetWithData = e.target as { __data__?: WalterDataPoint };
        const data = targetWithData?.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        // Because of that all the configs with different years are filtered here.
        if (!data) return;

        let monthIndex = months.findIndex((month) => month === data.key);

        if (monthIndex === -1) return;

        let lastMonthIndex =
            monthIndex === 0 ? months.length - 1 : monthIndex - 1;

        const group = config.data.filter((entry) => entry.group === data.group);

        const lastEntry = group.find(
            (entry) =>
                entry.key === months[lastMonthIndex] &&
                entry.year === (monthIndex === 0 ? year - 1 : year)
        );

        const thisEntry = group.find(
            (entry) => entry.key === data.key && entry.year === year
        );

        const vertragId = thisEntry?.id;
        if (vertragId) {
            const wohnung = data.group!;
            const lastValue = (lastEntry?.value as number) || 0;

            updateEntry(vertragId, monthIndex, wohnung, lastValue);
            addModalOpen = true;
        }
    }

    let addEntry: Partial<WalterMietzahlungInput> = {};
    let addModalOpen = false;
    let title = 'Unbekannter Vertrag';

    function onSubmit(new_value: unknown) {
        const ergebnis = new_value as { betreffenderMonat: string; kaltmieteZahlung: number };
        const vertrag = vertraege.find((e) => e.id === +(addEntry.vertrag?.id || 0));
        if (!vertrag) return;

        const synthetic = {
            id: -Date.now(),
            betreffenderMonat: ergebnis.betreffenderMonat || addEntry.betreffenderMonat || '',
            zahlungsdatum: addEntry.zahlungsdatum || '',
            betrag: (addEntry.kaltmieteZahlung || 0) + (addEntry.nkZahlung || 0),
            notiz: '',
            repeat: 0,
            createdAt: new Date(),
            lastModified: new Date(),
            vertrag: addEntry.vertrag!,
            permissions: vertrag.permissions
        } as WalterMieteEntry;
        mieten.push(synthetic);
        vertrag.mieten.push(synthetic);
        config = walter_data_miettabelle(vertraege, year);
    }
</script>

<WalterDataWrapperQuickAdd
    {onSubmit}
    bind:addEntry
    addUrl={WalterMietzahlungApiURL}
    bind:addModalOpen
    {title}
>
    <WalterMiete
        entry={addEntry}
        vertrag={vertraege.find((v) => v.id === +(addEntry.vertrag?.id || 0))}
    />
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
