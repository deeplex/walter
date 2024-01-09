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
        WalterDropdown,
        WalterMultiSelectJuristischePerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterKontaktEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row } from 'carbon-components-svelte';
    import WalterMultiSelectKontakt from '../elements/WalterMultiSelectKontakt.svelte';

    export let entry: Partial<WalterKontaktEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
    export let juristisch = false;

    const anreden = walter_selection.anreden(fetchImpl);
    const rechtsformen = walter_selection
        .rechtsformen(fetchImpl)
        .then((res) => {
            if (juristisch) {
                // Disable selection of natuerliche Person
                (res[0] as any).disabled = true;
                if (entry.rechtsform?.id === 0) {
                    entry.rechtsform = res[1];
                }
            }
            return res;
        });
</script>

<Row>
    <WalterDropdown
        entries={rechtsformen}
        bind:value={entry.rechtsform}
        titleText="Rechtsform"
    />
    {#if entry.rechtsform?.id === 0}
        <WalterDropdown
            entries={anreden}
            bind:value={entry.anrede}
            titleText="Anrede"
        />
        <WalterTextInput
            {readonly}
            bind:value={entry.vorname}
            labelText="Vorname"
        />
    {/if}
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.name}
        labelText="Name"
    />
</Row>

<WalterAdresse {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterTextInput {readonly} bind:value={entry.email} labelText="E-Mail" />
    <WalterTextInput {readonly} bind:value={entry.fax} labelText="Fax" />
</Row>
<Row>
    <WalterTextInput
        {readonly}
        bind:value={entry.telefon}
        labelText="Telefon"
    />
    <WalterTextInput {readonly} bind:value={entry.mobil} labelText="Mobil" />
</Row>

<Row>
    <WalterMultiSelectJuristischePerson
        {readonly}
        {fetchImpl}
        titleText="Juristische Personen"
        bind:value={entry.selectedJuristischePersonen}
    />
    {#if entry.rechtsform?.id !== 0}
        <WalterMultiSelectKontakt
            {readonly}
            {fetchImpl}
            titleText="Mitglieder"
            bind:value={entry.selectedMitglieder}
        />
    {/if}
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
