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
        WalterAdresse,
        WalterComboBoxKontakt,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterGarageEntry } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    export let entry: Partial<WalterGarageEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <WalterTextInput
        {readonly}
        required
        labelText="Kennung"
        bind:value={entry.kennung}
    />
    <WalterComboBoxKontakt
        {fetchImpl}
        {readonly}
        required
        bind:value={entry.besitzer}
        title="Besitzer"
    />
</Row>
<Row>
    <WalterAdresse {readonly} bind:entry={entry.adresse} />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
