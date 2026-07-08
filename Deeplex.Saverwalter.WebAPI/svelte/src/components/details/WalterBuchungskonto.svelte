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
    import {
        kontoAnzeigeSaldo,
        kontoStatusTag,
        type WalterBuchungskontoEntry
    } from '$walter/lib';
    import { convertEuro } from '$walter/services/utils';
    import { Row, Tag } from 'carbon-components-svelte';

    export let entry: Partial<WalterBuchungskontoEntry> = {};
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    $: statusTag = kontoStatusTag(entry);
</script>

<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung"
    />
    {#if entry.funktion}
        <WalterTextInput readonly value={entry.funktion} labelText="Funktion" />
    {/if}
    <WalterTextInput readonly value={entry.kontotyp} labelText="Kontotyp" />
</Row>
<Row>
    <WalterTextInput
        readonly
        value={convertEuro(entry.sollSumme ?? 0)}
        labelText="Soll gesamt"
    />
    <WalterTextInput
        readonly
        value={convertEuro(entry.habenSumme ?? 0)}
        labelText="Haben gesamt"
    />
    <WalterTextInput
        readonly
        value={convertEuro(kontoAnzeigeSaldo(entry))}
        labelText="Saldo"
    />
</Row>
{#if statusTag}
    <Row>
        <div class="konto-status">
            <Tag type={statusTag.tag}>{statusTag.text}</Tag>
        </div>
    </Row>
{/if}
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>

<style>
    .konto-status {
        padding: 0 1rem 0.5rem;
    }
</style>
