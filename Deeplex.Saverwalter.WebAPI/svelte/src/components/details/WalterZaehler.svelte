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
        WalterComboBox,
        WalterComboBoxWohnung,
        WalterMultiSelectUmlage,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterZaehlerEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row } from 'carbon-components-svelte';
    import WalterDatePicker from '../elements/WalterDatePicker.svelte';

    export let entry: Partial<WalterZaehlerEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    const zaehlertypen = walter_selection.zaehlertypen(fetchImpl);
</script>

<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.kennnummer}
        labelText="Kennnummer"
    />
    <WalterComboBox
        required
        {readonly}
        bind:value={entry.typ}
        titleText="Typ"
        entries={zaehlertypen}
    />
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
    />
</Row>
<WalterAdresse {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterComboBoxWohnung {fetchImpl} {readonly} bind:value={entry.wohnung} />
</Row>
<Row>
    <WalterMultiSelectUmlage
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedUmlagen}
        titleText="Umlagen"
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
