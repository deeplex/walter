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
    import type { WalterSelectionEntry } from '$walter/lib';
    import {
        walter_subscribe_reset_changeTracker,
        walter_update_value
    } from '$walter/services/utils';
    import { MultiSelect, TextInputSkeleton } from 'carbon-components-svelte';
    import { onMount } from 'svelte';
    import { entriesToString } from './WalterMultiSelect';

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let entries: Promise<WalterSelectionEntry[]>;
    export let disabled = false;
    export let hideLabel = false;

    let lastSavedValue: string | undefined;
    function updateLastSavedValue() {
        lastSavedValue = entriesToString(value);
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    function select(e: CustomEvent) {
        const old_values = entriesToString(value);
        const new_values = entriesToString(e.detail.selected);
        walter_update_value(lastSavedValue, old_values, new_values);
        value = e.detail.selected;
    }

    // svelte things item should be MultiSelectItem but importing is squiggly.
    function filterItem(item: { text: string } & unknown, value: string) {
        if (!value) return true;

        const text = item.text.toLowerCase();
        const values = `${value}`
            .toLowerCase()
            .split(';')
            .map((e) => e.trim());
        return values.every((val) => text.includes(val)); // svelte thinks this should be string...
    }
</script>

{#await entries}
    <TextInputSkeleton />
{:then resolvedEntries}
    {#await value}
        <TextInputSkeleton />
    {:then x}
        <MultiSelect
            {hideLabel}
            {disabled}
            label={value?.map((e) => e.text).join(', ')}
            placeholder={value?.map((e) => e.text).join(', ')}
            selectedIds={x?.map((e) => e.id)}
            style="padding-right: 1rem"
            items={resolvedEntries}
            on:select={select}
            {titleText}
            filterable={!disabled}
            {filterItem}
        />
    {/await}
{/await}
