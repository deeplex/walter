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
    import { Row } from 'carbon-components-svelte';

    import type { WalterAdresseEntry } from '$walter/lib';
    import { WalterTextInput } from '$walter/components';

    export let entry: Partial<WalterAdresseEntry> | undefined = {};
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
    export let required = false;

    let fallback: Partial<WalterAdresseEntry> = entry || {};

    const change = () => {
        entry = fallback;
    };
</script>

<Row>
    {#if entry}
        <WalterTextInput
            {required}
            {readonly}
            labelText="Straße"
            bind:value={entry.strasse}
        />
        <WalterTextInput
            {required}
            {readonly}
            labelText="Hausnr."
            bind:value={entry.hausnummer}
        />
        <WalterTextInput
            {required}
            {readonly}
            labelText="Postleitzahl"
            bind:value={entry.postleitzahl}
        />
        <WalterTextInput
            {required}
            {readonly}
            labelText="Stadt"
            bind:value={entry.stadt}
        />
    {:else}
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Straße"
            bind:value={fallback.strasse}
        />
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Hausnr."
            bind:value={fallback.hausnummer}
        />
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Postleitzahl"
            bind:value={fallback.postleitzahl}
        />
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Stadt"
            bind:value={fallback.stadt}
        />
    {/if}
</Row>
