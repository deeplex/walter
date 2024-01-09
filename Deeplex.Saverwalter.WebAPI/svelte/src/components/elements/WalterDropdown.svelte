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
    import { Dropdown, TextInputSkeleton } from 'carbon-components-svelte';

    import type { WalterSelectionEntry } from '$walter/lib';
    import {
        walter_subscribe_reset_changeTracker,
        walter_update_value
    } from '$walter/services/utils';
    import { onMount } from 'svelte';

    export let value: WalterSelectionEntry | undefined;
    export let titleText: string;
    export let entries: Promise<WalterSelectionEntry[]>;
    export let readonly = false;
    export let required = false;
    export let hideLabel = false;

    let lastSavedValue: string | number | undefined;
    function updateLastSavedValue() {
        lastSavedValue = value?.id;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
        if (value === undefined) {
            entries.then((res) => (value = res[0]));
        }
    });

    function select(e: CustomEvent) {
        walter_update_value(lastSavedValue, value?.id, e.detail.selectedItem);
        value = e.detail.selectedItem;
    }
</script>

{#await entries}
    <TextInputSkeleton />
{:then resolvedEntries}
    {#await value}
        <TextInputSkeleton />
    {:then x}
        <Dropdown
            {hideLabel}
            invalid={required && !value}
            invalidText={`${titleText} ist ein notwendiges Feld.`}
            disabled={readonly}
            selectedId={x?.id}
            on:select={select}
            style="padding-right: 1rem"
            items={resolvedEntries}
            {titleText}
        />
    {/await}
{/await}
