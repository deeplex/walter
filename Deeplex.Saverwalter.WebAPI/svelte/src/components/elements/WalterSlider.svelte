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
    import { Slider } from 'carbon-components-svelte';
    import {
        walter_subscribe_reset_changeTracker,
        walter_update_value
    } from '$walter/services/utils';
    import { onMount } from 'svelte';

    export let value: number | undefined = undefined;
    export let min: number = 0;
    export let max: number = 100;
    export let step: number = 1;
    export let fullWidth: boolean = false;
    export let labelText: string = '';
    export let disabled: boolean = false;

    let lastSavedValue: number = value ?? 0;
    let committed: number = value ?? 0;
    let _internal: number = value ?? 0;
    $: _internal = value ?? 0;

    function updateLastSavedValue() {
        lastSavedValue = value ?? 0;
        committed = value ?? 0;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    function change(e: CustomEvent<number>) {
        value = walter_update_value(lastSavedValue, committed, e.detail);
        committed = e.detail;
    }
</script>

<Slider
    {min}
    {max}
    {step}
    {fullWidth}
    {labelText}
    {disabled}
    bind:value={_internal}
    on:change={change}
/>
