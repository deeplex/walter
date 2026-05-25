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

{#if entry.buchungszeilen && entry.buchungszeilen.length > 0}
    <Row style="margin-top: 1rem;">
        <div style="width: 100%; overflow-x: auto;">
            <table style="width: 100%; border-collapse: collapse; font-size: 0.875rem;">
                <thead>
                    <tr style="border-bottom: 1px solid var(--cds-ui-03);">
                        <th style="text-align: left; padding: 0.5rem; color: var(--cds-text-secondary);">Konto</th>
                        <th style="text-align: left; padding: 0.5rem; color: var(--cds-text-secondary);">Soll/Haben</th>
                        <th style="text-align: right; padding: 0.5rem; color: var(--cds-text-secondary);">Betrag</th>
                    </tr>
                </thead>
                <tbody>
                    {#each entry.buchungszeilen as z}
                        <tr style="border-bottom: 1px solid var(--cds-ui-01);">
                            <td style="padding: 0.5rem;">{z.konto}</td>
                            <td style="padding: 0.5rem; color: {z.sollHaben === 'Soll' ? 'var(--cds-support-error)' : 'var(--cds-support-success)'};">{z.sollHaben}</td>
                            <td style="text-align: right; padding: 0.5rem;">{z.betrag.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' })}</td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    </Row>
{/if}
