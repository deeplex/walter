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
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterWohnungVersionEntry } from '$walter/lib';

    export let entry: Partial<WalterWohnungVersionEntry> = {};
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
        bind:value={entry.wohnflaeche}
        label="Wohnfläche"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.nutzflaeche}
        label="Nutzfläche"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.miteigentumsanteile}
        label="Miteigentumsanteile"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.einheiten}
        label="Einheiten"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
