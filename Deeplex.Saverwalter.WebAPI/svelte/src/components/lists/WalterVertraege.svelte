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
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterDataWrapper,
        WalterVertrag
    } from '$walter/components';
    import { WalterVertragEntry, type TransaktionsInput } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterBuchung from '../details/WalterBuchung.svelte';
    import { invalidateAll } from '$app/navigation';

    const headers = [
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'mieterAuflistung', value: 'Mieter' },
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' },
        { key: 'button', value: 'Miete hinzufügen' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.vertrag(e.detail.id);
    const rowHref = (row: DataTableRow) => `/vertraege/${row.id}`;

    export let rows: WalterVertragEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterVertragEntry> | undefined = undefined;

    let modalOpen = false;
    let modalTitle = 'Mietzahlung';
    let buchungsInput: TransaktionsInput = emptyTransaktionsInput();

    function add(e: CustomEvent, vertrag: WalterVertragEntry) {
        e.stopPropagation();
        modalTitle = vertrag.wohnung?.text || `Vertrag ${vertrag.id}`;
        buchungsInput = {
            ...emptyTransaktionsInput(),
            mieten: [{ kaltmiete: 0, nkVorauszahlung: 0, vertragId: vertrag.id as number }]
        };
        modalOpen = true;
    }

    const rowsAdd = rows.map((row) => ({
        ...row,
        button: (e: CustomEvent) => add(e, row)
    }));

    async function onSubmit() {
        await invalidateAll();
    }
</script>

<WalterDataWrapperQuickAdd
    title={modalTitle}
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={buchungsInput}
    bind:addModalOpen={modalOpen}
    onSubmit={onSubmit}
>
    <WalterBuchung {fetchImpl} bind:buchung={buchungsInput} />
</WalterDataWrapperQuickAdd>

<WalterDataWrapper
    addUrl={WalterVertragEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rowHref}
    rows={rowsAdd}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterVertrag {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
