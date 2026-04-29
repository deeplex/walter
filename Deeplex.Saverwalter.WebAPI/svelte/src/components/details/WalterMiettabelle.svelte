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
    import { type WalterVertragEntry, WalterMieteEntry } from '$walter/lib';
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
    import { openModal } from '$walter/store';

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
            betrag: betrag
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

    let addEntry: Partial<WalterMieteEntry> = {};
    let addModalOpen = false;
    let title = 'Unbekannter Vertrag';

    function onSubmit(new_value: unknown) {
        const value = new_value as WalterMieteEntry;
        const vertrag = vertraege.find((e) => e.id === +value.vertrag?.id);
        if (!vertrag) {
            console.warn('Vertrag not found: ', value.vertrag?.id);
            return;
        }
        mieten.push(value);
        vertrag!.mieten.push(value);
        config = walter_data_miettabelle(vertraege, year);
    }

    function onDeleteMonthMiete(value: WalterMieteEntry) {
        const vertrag = vertraege.find(
            (entry) => entry.id === +value.vertrag?.id
        );
        if (vertrag) {
            vertrag.mieten = (vertrag.mieten || []).filter(
                (miete) => miete.id !== value.id
            );
        }

        mieten = mieten.filter((miete) => miete.id !== value.id);
        config = walter_data_miettabelle(vertraege, year);
    }

    function dateToMonthKey(value: string | undefined) {
        if (!value) {
            return undefined;
        }

        const parsed = new Date(value);
        if (Number.isNaN(parsed.getTime())) {
            return undefined;
        }

        return `${parsed.getFullYear()}-${`${parsed.getMonth() + 1}`.padStart(2, '0')}`;
    }

    function confirmDuplicateRentMonth(entryToCheck: unknown) {
        const miete = entryToCheck as Partial<WalterMieteEntry>;
        const vertragId = +(miete.vertrag?.id || 0);
        const monthKey = dateToMonthKey(miete.betreffenderMonat);

        const hasDuplicate =
            !miete.id &&
            vertragId > 0 &&
            !!monthKey &&
            mieten.some(
                (row) =>
                    +row.vertrag?.id === vertragId &&
                    dateToMonthKey(row.betreffenderMonat) === monthKey
            );

        if (!hasDuplicate) {
            return Promise.resolve(true);
        }

        return new Promise<boolean>((resolve) => {
            openModal({
                modalHeading: 'Miete bereits vorhanden',
                content:
                    'Für diesen Vertrag gibt es im ausgewählten Monat bereits mindestens eine Miete. Möchtest du trotzdem speichern?',
                primaryButtonText: 'Trotzdem speichern',
                submit: async () => {
                    resolve(true);
                    return true;
                },
                cancel: () => resolve(false),
                danger: false
            });
        });
    }
</script>

<WalterDataWrapperQuickAdd
    {onSubmit}
    bind:addEntry
    addUrl={WalterMieteEntry.ApiURL}
    bind:addModalOpen
    beforeSubmit={confirmDuplicateRentMonth}
    {title}
>
    <WalterMiete
        entry={addEntry}
        {mieten}
        {vertraege}
        {onDeleteMonthMiete}
        onRequestCloseModal={() => (addModalOpen = false)}
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
