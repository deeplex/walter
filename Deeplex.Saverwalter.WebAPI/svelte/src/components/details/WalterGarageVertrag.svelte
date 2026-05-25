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
        WalterComboBox,
        WalterDatePicker,
        WalterMultiSelectKontakt,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import WalterComboBoxGarage from '$walter/components/elements/WalterComboBoxGarage.svelte';
    import WalterComboBoxVertrag from '$walter/components/elements/WalterComboBoxVertrag.svelte';
    import type { WalterGarageVertragEntry } from '$walter/lib';
    import { convertDateGerman } from '$walter/services/utils';
    import { Row, TextInput } from 'carbon-components-svelte';

    export let entry: Partial<WalterGarageVertragEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <TextInput
        placeholder="Wird aus Nachtrag genommen"
        required
        readonly
        value={entry.beginn && convertDateGerman(new Date(entry.beginn))}
        labelText="Beginn (aus Nachtrag)"
    />
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
        placeholder="Offen"
    />
</Row>
<Row>
    <WalterComboBoxGarage
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.garage}
    />
    <WalterComboBoxVertrag {fetchImpl} {readonly} bind:value={entry.vertrag} />
</Row>
<Row>
    <WalterMultiSelectKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedMieter}
        titleText="Mieter (leer = aus Wohnungsvertrag)"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
