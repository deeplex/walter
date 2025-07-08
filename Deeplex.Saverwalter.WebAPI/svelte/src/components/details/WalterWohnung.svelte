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
        WalterAdresse,
        WalterComboBoxKontakt,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterWohnungEntry } from '$walter/lib';

    export let entry: Partial<WalterWohnungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <WalterComboBoxKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.besitzer}
        title="Besitzer"
    />
</Row>
<WalterAdresse required {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung"
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
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
