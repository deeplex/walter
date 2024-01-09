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
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterVertragVersionEntry } from '$walter/lib';

    export let entry: Partial<WalterVertragVersionEntry> = {};
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.beginn}
        labelText="Beginn"
    />
    <WalterNumberInput
        required
        {readonly}
        hideSteppers
        bind:value={entry.grundmiete}
        label="Grundmiete"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.personenzahl}
        label="Personenzahl"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
