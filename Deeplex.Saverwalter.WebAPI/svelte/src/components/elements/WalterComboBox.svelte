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
    import { ComboBox, TextInputSkeleton } from 'carbon-components-svelte';

    import type { WalterSelectionEntry } from '$walter/lib';
    import { shouldFilterItem } from './WalterComboBox';
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
    /** Pre-select an item by id when entries load, without requiring a full entry object */
    export let initialId: number | string | undefined = undefined;

    let lastSavedValue: string | number | undefined;
    function updateLastSavedValue() {
        lastSavedValue = value?.id;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    function select(e: CustomEvent) {
        walter_update_value(lastSavedValue, value?.id, e.detail.selectedItem);
        value = e.detail.selectedItem;
    }

    function sanitizeEntries(
        resolvedEntries: WalterSelectionEntry[],
        selectedEntry: WalterSelectionEntry | undefined
    ) {
        if (!selectedEntry) {
            return resolvedEntries;
        }
        return resolvedEntries.some((entry) => entry.id === selectedEntry.id)
            ? resolvedEntries
            : [selectedEntry];
    }
</script>

{#await entries}
    <TextInputSkeleton />
{:then resolvedEntries}
    {@const selected = value ?? (initialId != null ? resolvedEntries?.find((e) => +e.id === +initialId) : undefined)}
    <ComboBox
        invalid={required && !selected}
        invalidText={`${titleText} ist ein notwendiges Feld.`}
        disabled={readonly}
        selectedId={selected?.id}
        on:select={select}
        style="padding-right: 1rem"
        items={sanitizeEntries(resolvedEntries, selected)}
        value={selected?.text}
        {titleText}
        {shouldFilterItem}
    />
{/await}
