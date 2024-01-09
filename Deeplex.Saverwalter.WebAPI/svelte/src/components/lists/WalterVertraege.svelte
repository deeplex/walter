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
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import {
        WalterDataWrapper,
        WalterMiete,
        WalterVertrag
    } from '$walter/components';
    import { WalterVertragEntry, type WalterMieteEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'mieterAuflistung', value: 'Mieter' },
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' },
        { key: 'button', value: 'Miete hinzufügen' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.vertrag(e.detail.id);

    export let rows: WalterVertragEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterVertragEntry> | undefined = undefined;

    let earliest: Date = new Date();
    let quickAddEntry: Partial<WalterMieteEntry> = {};

    function add(e: CustomEvent, vertrag: WalterVertragEntry) {
        e.stopPropagation();

        const mieten = vertrag.mieten
            .map((miete) => new Date(miete.betreffenderMonat))
            .sort((a, b) => b.getTime() - a.getTime());
        const dateMiete = mieten[0] || new Date();
        dateMiete.setDate(
            dateMiete.getDate() +
                new Date(
                    dateMiete.getFullYear(),
                    dateMiete.getMonth(),
                    0
                ).getDate()
        );
        if (dateMiete < earliest) {
            dateMiete.setDate(earliest.getDate());
        }
        quickAddEntry = {
            vertrag: vertrag.mieten[0]?.vertrag || {
                id: `${vertrag.id}`,
                text: vertrag.wohnung.text
            },
            zahlungsdatum: convertDateCanadian(new Date()),
            betrag: vertrag.mieten[0]?.betrag,
            betreffenderMonat: convertDateCanadian(dateMiete)
        };

        open = true;
    }
    const rowsAdd = rows.map((row) => ({
        ...row,
        button: (e: CustomEvent) => add(e, row)
    }));

    let open = false;
</script>

<WalterDataWrapperQuickAdd
    title={quickAddEntry.vertrag?.text || 'Vertrag'}
    addEntry={quickAddEntry}
    addUrl={WalterVertragEntry.ApiURL}
    bind:addModalOpen={open}
>
    <WalterMiete entry={quickAddEntry} />
</WalterDataWrapperQuickAdd>

<WalterDataWrapper
    addUrl={WalterVertragEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    rows={rowsAdd}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterVertrag {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
