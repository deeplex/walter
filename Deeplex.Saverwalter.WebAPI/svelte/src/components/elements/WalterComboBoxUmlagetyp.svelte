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
        WalterUmlagetypEntry,
        type WalterSelectionEntry
    } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { WalterComboBox, WalterUmlagetyp } from '..';
    import WalterQuickAddButton from './WalterQuickAddButton.svelte';

    export let readonly: boolean = false;
    export let required: boolean = false;
    export let value: WalterSelectionEntry | undefined;
    export let fetchImpl: typeof fetch;
    export let title: string | undefined = undefined;

    let entries = walter_selection.umlagetypen(fetchImpl);
    function onSubmit() {
        entries = walter_selection.umlagetypen(fetchImpl);
    }

    let addEntry = {};
</script>

<div
    style="
    flex: 1 1 auto !important;
    display: flex !important;
    flex-wrap: wrap !important"
>
    <WalterComboBox
        {required}
        {readonly}
        bind:value
        titleText={title || 'Person'}
        {entries}
    />

    {#if !readonly}
        <WalterQuickAddButton
            title="Umlagetypen"
            bind:addEntry
            addUrl={WalterUmlagetypEntry.ApiURL}
            {onSubmit}
        >
            <WalterUmlagetyp entry={addEntry} {fetchImpl} />
        </WalterQuickAddButton>
    {/if}
</div>
