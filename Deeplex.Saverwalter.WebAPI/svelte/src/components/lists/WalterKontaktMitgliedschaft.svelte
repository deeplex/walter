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
        WalterComboBoxKontakt,
        WalterDataTable,
        WalterDatePicker,
        WalterNumberInput
    } from '$walter/components';
    import {
        WalterKontaktMitgliedschaftEntry,
        type WalterMitgliedschaftEntry
    } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    export let rows: WalterMitgliedschaftEntry[] = [];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterKontaktMitgliedschaftEntry> = {};
    export let fetchImpl: typeof fetch;
    export let mode: 'alsMitglied' | 'alsJuristischePerson';

    const headersAlsMitglied = [
        { key: 'juristischePerson.text', value: 'Juristische Person' },
        { key: 'von', value: 'Von' },
        { key: 'bis', value: 'Bis' },
        { key: 'anteil', value: 'Anteil' }
    ];

    const headersAlsJuristischePerson = [
        { key: 'mitglied.text', value: 'Mitglied' },
        { key: 'von', value: 'Von' },
        { key: 'bis', value: 'Bis' },
        { key: 'anteil', value: 'Anteil' }
    ];

    $: headers =
        mode === 'alsMitglied'
            ? headersAlsMitglied
            : headersAlsJuristischePerson;
    $: submitDisabled =
        !entry.von ||
        (mode === 'alsMitglied'
            ? !entry.juristischePerson?.id
            : !entry.mitglied?.id);
</script>

<WalterDataTable
    addUrl={WalterKontaktMitgliedschaftEntry.ApiURL}
    addEntry={entry}
    {submitDisabled}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    {rows}
    {headers}
    {fullHeight}
>
    <Row>
        {#if mode === 'alsMitglied'}
            <WalterComboBoxKontakt
                {fetchImpl}
                bind:value={entry.juristischePerson}
                title="Juristische Person"
            />
        {:else}
            <WalterComboBoxKontakt
                {fetchImpl}
                bind:value={entry.mitglied}
                title="Mitglied"
            />
        {/if}
        <WalterDatePicker required bind:value={entry.von} labelText="Von" />
        <WalterDatePicker bind:value={entry.bis} labelText="Bis" />
        <WalterNumberInput bind:value={entry.anteil} label="Anteil" />
    </Row>
</WalterDataTable>
