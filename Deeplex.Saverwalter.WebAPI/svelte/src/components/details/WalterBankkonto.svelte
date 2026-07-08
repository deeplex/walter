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
    import { WalterTextArea, WalterTextInput } from '$walter/components';
    import type { WalterBankkontoEntry } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';
    import WalterMultiSelectKontakt from '../elements/WalterMultiSelectKontakt.svelte';

    export let entry: Partial<WalterBankkontoEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<!-- At least one of IBAN or Bank must be filled -->
<Row>
    <WalterTextInput
        required={!entry.iban && !entry.bank}
        {readonly}
        bind:value={entry.iban}
        labelText="IBAN"
    />
    <WalterTextInput
        required={!entry.iban && !entry.bank}
        {readonly}
        bind:value={entry.bank}
        labelText="Bank"
    />
</Row>
<Row>
    <WalterMultiSelectKontakt
        {readonly}
        {fetchImpl}
        titleText="Besitzer"
        bind:value={entry.selectedBesitzer}
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
