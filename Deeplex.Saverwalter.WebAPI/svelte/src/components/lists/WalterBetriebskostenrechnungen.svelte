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

    import {
        WalterBetriebskostenrechnung,
        WalterDataWrapper
    } from '$walter/components';
    import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fullHeight = false;
    export let rows: WalterBetriebskostenrechnungEntry[];
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;

    const headers = [
        { key: 'typ.text', value: 'Typ' },
        { key: 'umlage.text', value: 'Wohnungen' },
        { key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'datum', value: 'Datum' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.betriebskostenrechnung(e.detail.id);

    export let entry: Partial<WalterBetriebskostenrechnungEntry> | undefined =
        undefined;
</script>

<WalterDataWrapper
    addUrl={WalterBetriebskostenrechnungEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterBetriebskostenrechnung {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
