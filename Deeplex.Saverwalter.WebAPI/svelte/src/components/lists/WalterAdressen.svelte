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

    import { WalterAdresse, WalterDataWrapper } from '$walter/components';
    import { WalterAdresseEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'strasse', value: 'Straße' },
        { key: 'hausnummer', value: 'Hausnummer' },
        { key: 'postleitzahl', value: 'Postleitzahl' },
        { key: 'stadt', value: 'Stadt' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.adresse(e.detail.id);

    export let fullHeight = false;
    export let rows: WalterAdresseEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterAdresseEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterAdresseEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterAdresse {entry} />
    {/if}
</WalterDataWrapper>
