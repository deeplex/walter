<!-- Copyright (C) 2023-2025  Kai Lawrence -->
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
    import { createEventDispatcher } from 'svelte';
    import {
        WalterComboBoxBankkonto,
        WalterTextArea,
        WalterNumberInput,
        WalterDatePicker
    } from '$walter/components';

    import { InlineNotification, Row } from 'carbon-components-svelte';
    import type {
        WalterSelectionEntry,
        WalterTransaktionEntry
    } from '$walter/lib';

    export let entry: Partial<WalterTransaktionEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    export let initialZahlerId: number | undefined = undefined;
    export let initialZahlungsempfaengerId: number | undefined = undefined;
    export let betrag: number | undefined = undefined;
    export let zahlungsdatum: string | undefined = undefined;
    $: {
        readonly = entry?.permissions?.update === false;
    }
    // Gebucht: Betrag, Datum und ein erfasster Zahler bzw. Empfänger passen zu den
    // Buchungen und bleiben fest (der Server lehnt Änderungen ab); Memo und Notiz nicht.
    // Ein beim Laden fehlender Zahler bzw. Empfänger darf nachgetragen werden.
    $: gebucht = (entry?.buchungssaetze?.length ?? 0) > 0;
    const zahlerAnfangs = !!entry?.zahler;
    const empfaengerAnfangs = !!entry?.zahlungsempfaenger;
    $: zahlerFest = gebucht && zahlerAnfangs;
    $: empfaengerFest = gebucht && empfaengerAnfangs;

    const dispatch = createEventDispatcher<{
        zahlerChange: WalterSelectionEntry | undefined;
        zahlungsempfaengerChange: WalterSelectionEntry | undefined;
    }>();
</script>

{#if gebucht && !readonly}
    <Row>
        <InlineNotification
            kind="info"
            lowContrast
            hideCloseButton
            title="Gebucht:"
            subtitle="Betrag, Zahlungsdatum, Zahler und Zahlungsempfänger lassen sich nur über „Rückgängig machen“ und neu buchen ändern. Memo und Notiz bleiben änderbar."
        />
    </Row>
{/if}

<Row>
    <WalterComboBoxBankkonto
        {fetchImpl}
        readonly={readonly || zahlerFest}
        bind:value={entry.zahler}
        title="Zahler"
        initialId={initialZahlerId}
        on:select={(e) => dispatch('zahlerChange', e.detail.selectedItem)}
    />

    <WalterComboBoxBankkonto
        {fetchImpl}
        readonly={readonly || empfaengerFest}
        bind:value={entry.zahlungsempfaenger}
        title="Zahlungsempfänger"
        initialId={initialZahlungsempfaengerId}
        on:select={(e) =>
            dispatch('zahlungsempfaengerChange', e.detail.selectedItem)}
    />
</Row>

<Row>
    {#if betrag !== undefined}
        <WalterNumberInput
            required
            {readonly}
            bind:value={betrag}
            label="Betrag"
        />
    {:else}
        <WalterNumberInput
            required
            readonly={readonly || gebucht}
            bind:value={entry.betrag}
            label="Betrag"
        />
    {/if}
    {#if zahlungsdatum !== undefined}
        <WalterDatePicker
            required
            disabled={readonly}
            bind:value={zahlungsdatum}
            labelText="Zahlungsdatum"
        />
    {:else}
        <WalterDatePicker
            required
            disabled={readonly || gebucht}
            bind:value={entry.zahlungsdatum}
            labelText="Zahlungsdatum"
        />
    {/if}
</Row>
<Row>
    <WalterTextArea
        {readonly}
        labelText="Memo"
        bind:value={entry.verwendungszweck}
    />
</Row>

<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
