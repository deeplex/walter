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
        WalterDataWrapper,
        WalterVertragVersion
    } from '$walter/components';
    import { WalterVertragVersionEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    const headers = [
        { key: 'beginn', value: 'Beginn' },
        { key: 'personenzahl', value: 'Personenzahl' },
        { key: 'grundmiete', value: 'Grundmiete' },
        { key: 'notiz', value: 'Notiz' }
    ];

    export let rows: WalterVertragVersionEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.vertragversion(e.detail.id);

    export let entry: Partial<WalterVertragVersionEntry> | undefined =
        undefined;
</script>

<WalterDataWrapper
    addUrl={WalterVertragVersionEntry.ApiURL}
    {on_click_row}
    addEntry={entry}
    {title}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterVertragVersion {entry} />
    {/if}
</WalterDataWrapper>
