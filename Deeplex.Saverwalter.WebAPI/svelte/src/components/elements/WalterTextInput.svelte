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
        walter_subscribe_reset_changeTracker,
        walter_update_value
    } from '$walter/services/utils';
    import { TextInput, TextInputSkeleton } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let value: string | undefined = undefined;
    export let labelText: string | undefined;
    export let readonly = false;
    export let required = false;

    let lastSavedValue: string | undefined;
    function updateLastSavedValue() {
        lastSavedValue = value || undefined;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    export let change = (e: CustomEvent<string | number | null>) => {
        value = walter_update_value(
            lastSavedValue,
            value || undefined,
            '' + e.detail || undefined
        );
    };
</script>

{#await value}
    <TextInputSkeleton />
{:then x}
    <TextInput
        invalid={required && !value}
        invalidText={`${labelText} ist ein notwendiges Feld.`}
        {readonly}
        on:change={change}
        {labelText}
        value={x}
    />
{/await}
