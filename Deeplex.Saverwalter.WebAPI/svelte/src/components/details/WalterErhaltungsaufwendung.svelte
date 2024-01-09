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
        WalterTextArea,
        WalterTextInput,
        WalterComboBoxKontakt,
        WalterComboBoxWohnung
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterErhaltungsaufwendungEntry } from '$walter/lib';

    export let entry: Partial<WalterErhaltungsaufwendungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung"
    />
    <WalterComboBoxKontakt
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.aussteller}
        title="Aussteller"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Datum"
    />
</Row>
<Row>
    <WalterComboBoxWohnung
        required
        {readonly}
        {fetchImpl}
        bind:value={entry.wohnung}
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag"
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
