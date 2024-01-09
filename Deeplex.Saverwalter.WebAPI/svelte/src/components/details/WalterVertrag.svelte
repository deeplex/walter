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
        WalterTextArea,
        WalterComboBoxKontakt,
        WalterComboBoxWohnung,
        WalterMultiSelectKontakt
    } from '$walter/components';
    import type { WalterVertragEntry } from '$walter/lib';
    import { Row, TextInput } from 'carbon-components-svelte';
    import { convertDateGerman } from '$walter/services/utils';

    export let entry: Partial<WalterVertragEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <TextInput
        placeholder="Wird aus Vertragsversion genommen"
        required
        readonly
        value={entry.beginn && convertDateGerman(new Date(entry.beginn))}
        labelText="Beginn (aus Vertragsversion)"
    />
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
        placeholder="Offen"
    />
</Row>
<Row>
    <WalterComboBoxWohnung
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.wohnung}
    />
    <TextInput labelText="Vermieter" readonly value={entry.wohnung?.filter} />
    <WalterComboBoxKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.ansprechpartner}
        title="Ansprechpartner"
    />
</Row>
<Row>
    <WalterMultiSelectKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedMieter}
        titleText="Mieter"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
